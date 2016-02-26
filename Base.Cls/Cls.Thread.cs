//-----------------------------------------------------------------------
// Cls.Impl.cs - описание вспомогательных классов для работы с потоками
//
// Created by *** 08.09.2010
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Lib.Base
{
	/// <summary>
	/// Template общего класса событий потока
	/// </summary>
	/// <typeparam name="ClsMain">Класс, в котором используются события</typeparam>
	public abstract class ObjThreadEvents<ClsMain>
	{
		// --- Обработчики события и уведомления ---
		/// <summary>
		/// Уведомление о начале работы
		/// </summary>
		public EventBase.Handler<ClsMain> Start;
		/// <summary>
		/// Уведомление о завершении работы
		/// </summary>
		public EventBase.Handler<ClsMain> Stop;
		/// <summary>
		/// Уведомление о возникновении ошибки
		/// </summary>
		public EventBase.Handler<ClsMain> Error;

		/// <summary>
		/// Событие начала работы
		/// </summary>
		public readonly ManualResetEvent Started = new ManualResetEvent(false);
		/// <summary>
		/// Событие завершения работы
		/// </summary>
		public readonly ManualResetEvent Completed = new ManualResetEvent(false);
		/// <summary>
		/// Событие начала запуска
		/// </summary>
		public readonly ManualResetEvent Starting = new ManualResetEvent(false);
		/// <summary>
		/// Событие начала остановки
		/// </summary>
		public readonly ManualResetEvent Stopping = new ManualResetEvent(false);

		/// <summary>
		/// Ожидание событий или таймера
		/// В базовой реализации ожидается только событие остановки (Stopping),
		/// в производных реализациях могут использоваться другие события
		/// </summary>
		/// <param name="millisecsTimeout">время [ms], после которого выход происходит без возникновения событий</param>
		public virtual void Wait( int millisecsTimeout ) { Stopping.WaitOne( millisecsTimeout ); }
		/// <summary>
		/// Ожидание событий
		/// В базовой реализации ожидается только событие остановки (Stopping),
		/// в производных реализациях могут использоваться другие события
		/// </summary>
		public virtual void Wait() { Stopping.WaitOne(); }

		/// <summary>
		/// Доступ к EventLog
		/// </summary>
		/// <remarks>Устанавливается снаружи для предоставления функций записи в EventLog</remarks>
		public EventLog Log { get; set; }

		/// <summary>
		/// Выводить в консоль, если не установлен Log,
		/// по умолчанию true
		/// </summary>
		public bool ConsoleWriteEnable = true;

		/// <summary>
		/// Записать в сообщение в журнал
		/// </summary>
		/// <param name="msg">Сообщение</param>
		public void WriteLog( string msg )
		{
			if (Log != null) Log.WriteEntry(msg);
			else if (ConsoleWriteEnable) Console.WriteLine(msg);
		}
		/// <summary>
		/// Записать сообщение указанного типа в журнал
		/// </summary>
		/// <param name="msg">Сообщение</param>
		/// <param name="type">Тип сообщения</param>
		public void WriteLog( string msg,EventLogEntryType type )
		{
			if(Log != null) Log.WriteEntry( msg,type );
			else if(ConsoleWriteEnable) Console.WriteLine( "{0}: {1}",type,msg );
		}
		/// <summary>
		/// Записать сообщение указанного типа с идентификатором события в журнал
		/// </summary>
		/// <param name="msg">Сообщение</param>
		/// <param name="type">Тип сообщения</param>
		/// <param name="idEvent">Идентификатор события</param>
		public void WriteLog( string msg,EventLogEntryType type,int idEvent )
		{
			if(Log != null) Log.WriteEntry( msg,type,idEvent );
			else if(ConsoleWriteEnable) Console.WriteLine( "{0}({2}): {1}",type,msg,idEvent );
		}
		/// <summary>
		/// Записать сообщение указанного типа с идентификатором события в журнал указанной категории
		/// </summary>
		/// <param name="msg">Сообщение</param>
		/// <param name="type">Тип сообщения</param>
		/// <param name="idEvent">Идентификатор события</param>
		/// <param name="category">Идентификатор категории</param>
		public void WriteLog( string msg,EventLogEntryType type,int idEvent,short category )
		{
			if(Log != null) Log.WriteEntry( msg,type,idEvent,category );
			else if (ConsoleWriteEnable) Console.WriteLine("[{3}]{0}({2}): {1}", type, msg, idEvent, category);
		}
		/// <summary>
		/// Записать сообщение указанного типа с идентификатором события и raw-данными в журнал указанной категории
		/// </summary>
		/// <param name="msg">Сообщение</param>
		/// <param name="type">Тип сообщения</param>
		/// <param name="idEvent">Идентификатор события</param>
		/// <param name="category">Идентификатор категории</param>
		/// <param name="rawData">raw-данные</param>
		public void WriteLog( string msg,EventLogEntryType type,int idEvent,short category,byte[] rawData )
		{
			if (Log != null) Log.WriteEntry(msg, type, idEvent, category, rawData);
			else if(ConsoleWriteEnable) Console.WriteLine( "[{3}]{0}({2}): {1}",type,msg,idEvent,category );
		}
	}
		
	/// <summary>
	/// Template общего класса потоков
	/// </summary>
	/// <remarks>
	/// Класс оперирует собственным набором состояний и событий отличными от стандартого потока. Их состав
	/// представлен в свойстве Event. Класс для этого свойства может быть переопределён.
	/// Для реализации неоходимых действий потока, в производном классе нужно переопределить метод DoWork.
	/// Исключения возникающие в методе DoWork обрабатываются автоматически. При возникновении исключения,
	/// поток переводится в состояние окончания работы. Последнее исключение доступно через свойство Error.
	/// Для реализации собственной обработки исключения, в производном классе нужно переопределить метод DoError.
	/// </remarks>
	/// <typeparam name="ClsDerived">Класс производный от этого Template</typeparam>
	/// <typeparam name="ClsEvents">Класс производный от _Events этого Template</typeparam>
	public abstract class ObjThread<ClsDerived, ClsEvents> : IDisposableImpl
		where ClsDerived : class
		where ClsEvents : ObjThread<ClsDerived, ClsEvents>.BaseEvents, new()
	{
		/// <summary>
		/// Класс событий потока
		/// </summary>
		public class BaseEvents : ObjThreadEvents<ClsDerived>
		{
		}

		/// <summary>
		/// События потока
		/// </summary>
		public readonly ClsEvents Event = new ClsEvents();

		private Exception _err = null;
		/// <summary>
		/// Последнее произошедшее в потоке исключение
		/// </summary>
		public Exception Error // readonly
		{
			get { lock(this) return _err; }
			protected set { lock(this) _err = value; }
		}

		#region Методы управления потоком

		/// <summary>
		/// Запуск потока
		/// </summary>
		/// <param name="p">Произвольный объект с параметрами для конечного класса реализации работы потока</param>
		public virtual void Start( object p = null )
		{
			bool start = false;
			lock(Event)
			{
				if(Event.Started.IsReset() && Event.Starting.IsReset())
				{
					Event.Starting.Set();
					Event.Stopping.Reset();
					Event.Completed.Reset();
					start = true;
				}
			}
			if(start)
				ThreadPool.QueueUserWorkItem( new WaitCallback(Working),p );
		}

		/// <summary>
		/// Прекращение выполнения потока
		/// </summary>
		/// <param name="wait">
		/// Флаг ожидания завершения работы,
		/// если false - будет только взведён флаг необходимости остановки Event.Stopping,
		/// иначе - после взведения флага ждёт взведения флага окончания Event.Completed
		/// </param>
		public virtual void Stop( bool wait = true )
		{
			lock(Event)
			{
				if(Event.Started.IsSet() || Event.Starting.IsSet())
					Event.Stopping.Set();
				else if(wait)
					wait = Event.Stopping.IsSet();
			}
			if(wait) Wait();
		}

		/// <summary>
		/// Ожидание завершения потока
		/// </summary>
		/// <remarks>
		/// Признаком окончания выполнения потока считается взведение флага Event.Completed
		/// </remarks>
		public virtual void Wait()
		{
			Event.Completed.WaitOne();
		}

		/// <summary>
		/// Извещение о необходимости завершить поток
		/// </summary>
		/// <remarks>
		/// Делегат.
		/// В стандартой реализации просто взводит флаг Event.Stopping, если поток находится в состоянии выполнения.
		/// В случае перегрузки метода необходимо позаботиться, чтобы возврат из него происходил настолько быстро,
		/// насколько это возможно. Не нужно выполнять в нём действия по остановке, для это есть метод Stop.
		/// </remarks>
		/// <param name="sender">Отправивший событие объект</param>
		/// <param name="eArgs">Дополнительные параметры</param>
		public virtual void OnNotifyStop( object sender,EventArgs eArgs )
		{
			Stop( false );
		}

		/// <summary>
		/// Метод выполняемый в созданном потоке
		/// </summary>
		/// <param name="p">Произвольный объект с параметрами для конечного класса реализации работы потока</param>
		protected virtual void Working( object p )
		{
			Error = null;

			try
			{
				bool start = false;

				lock(Event)
				{
					if(Event.Stopping.IsReset())
					{
						//Event.Completed.Reset(); - перенесён в Start
						Event.Started.Set();
						Event.Starting.Reset();
						start = true;
					}
				}

				if(start)
				{
					Event.Start.Run( this as ClsDerived );
					DoWork( p );
				}
			}
			catch( Exception err )
			{
				DoError( err,p );
				Event.Error.Run( this as ClsDerived );
			}
			finally
			{
				lock(Event)
				{
					Event.Starting.Reset();
					Event.Started.Reset();
					Event.Stopping.Reset();
					Event.Completed.Set();
				}
				Event.Stop.Run( this as ClsDerived );
			}
		}

		#endregion Методы управления потоком

		/// <summary>
		/// Собственно действия, которые выполняются потоком
		/// </summary>
		/// <remarks>
		/// Поскольку управление потоком в штатном режиме предполагается только через собственные события из Event,
		/// необходимо проверять значение флага Event.Stopping и, в случае если он установлен, завершать работу.
		/// </remarks>
		/// <param name="p">Произвольный объект с параметрами для конечного класса реализации работы потока</param>
		protected virtual void DoWork( object p ) {}

		/// <summary>
		/// Обработка исключения возникшего при выполнении метода DoWork
		/// </summary>
		/// <param name="err">Исключение</param>
		/// <param name="p">Произвольный объект с параметрами для конечного класса реализации работы потока</param>
		protected virtual void DoError( Exception err,object p ) { Error = err; }

		#region Реализация интерфейса IDisposable

		/// <summary>
		/// Освобожнение управляемых ресурсов
		/// </summary>
		protected override void DisposeManaged()
		{
			Stop( true );
			base.DisposeManaged();
		}

		#endregion Реализация интерфейса IDisposable
	}

	/// <summary>
	/// Template общего класса потоков
	/// </summary>
	/// <remarks>
	/// Этот Template отличается от предыдущего тем, что не даёт возможность переопределить
	/// класс для свойства Event.
	/// Используется в тех случаях, когда удобнее использовать делигаты с параментром sender того же типа,
	/// что и производный от этого Template класс.
	/// </remarks>
	/// <typeparam name="ClsDerived">Класс производный от этого Template</typeparam>
	public abstract class ObjThread<ClsDerived> : ObjThread<ClsDerived, ObjThread<ClsDerived>.Events>
		where ClsDerived : class
	{
		/// <summary>
		/// Класс событий потока
		/// </summary>
		public class Events : BaseEvents
		{
		}
	}

	/// <summary>
	/// Общий класс потоков
	/// </summary>
	/// <remarks>
	/// Отличается от Template-ов тем, что не даёт переопределять никаких типов, используется только
	/// для переопределения методов.
	/// </remarks>
	public abstract class ObjThread : ObjThread<ObjThread>
	{
	}

	/// <summary>
	/// Template пула объектов
	/// </summary>
	/// <typeparam name="ClsItem">Класс объектов пула</typeparam>
	public sealed class Pool<ClsItem>
	where ClsItem : class, new()
	{
		/// <summary>
		/// Делегат создания объекта
		/// </summary>
		/// <returns>Созданный экземпляр объекта</returns>
		public delegate ClsItem NewItem();

		private Stack<ClsItem> _pool = new Stack<ClsItem>();
		/// <summary>
		/// Кол-во объектов в пуле
		/// </summary>
		public int Count { get { lock(_pool) return _pool.Count; } }

		/// <summary>
		/// Получить объект из пула или создать новый, если пул пуст
		/// В случае отсутствия делегата для создания объекта, возвращает null
		/// </summary>
		/// <param name="newItem">Делегат для создания объекта</param>
		/// <returns>Полученный экземпляр объекта или null</returns>
		public ClsItem Get( NewItem newItem = null )
		{
			lock(_pool)
				return _pool.Count > 0 ? _pool.Pop() : newItem != null ? newItem() : new ClsItem();
		}
		/// <summary>
		/// Добавить объект в пул
		/// </summary>
		/// <param name="item">Добавляемый объект</param>
		/// <returns>Добавленный объект</returns>
		public ClsItem Add( ClsItem item )
		{
			lock(_pool) _pool.Push( item );
			return item;
		}
		/// <summary>
		/// Очистить пул
		/// </summary>
		public void Clear()
		{
			lock(_pool) _pool.Clear();
		}
	}

	/// <summary>
	/// Ограничения на кол-во потоков
	/// </summary>
	public sealed class ThreadLimits
	{
		/// <summary>
		/// Делегат вычисления объекта
		/// </summary>
		/// <param name="worker">Кол-во рабочих потоков</param>
		/// <param name="completionPort">Кол-во потоков асинхронного ожидания</param>
		public delegate void Query( out int worker,out int completionPort );

		/// <summary>
		/// Кол-во "обычных" потоков
		/// </summary>
		public int Worker = 0;
		/// <summary>
		/// Кол-во потоков асинхронного ожидания I/O
		/// </summary>
		public int CompletionPort = 0;

		/// <summary>
		/// Конструктор
		/// </summary>
		public ThreadLimits() {}

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="query">Метод позволяющий получить значения пары</param>
		public ThreadLimits( Query query )
		{
			if(query != null)
				query( out Worker,out CompletionPort );
		}

		/// <summary>
		/// Сумма ограничений на кол-во потоков
		/// </summary>
		/// <param name="lim1">Первый набор ограничений</param>
		/// <param name="lim2">Второй набор ограничений</param>
		/// <returns>Суммарный набор ограничений</returns>
		public static ThreadLimits operator + ( ThreadLimits lim1,ThreadLimits lim2 )
		{
			return new ThreadLimits() {
				Worker = (lim1 == null ? 0 : lim1.Worker) + (lim2 == null ? 0 : lim2.Worker),
				CompletionPort = (lim1 == null ? 0 : lim1.CompletionPort) + (lim2 == null ? 0 : lim2.CompletionPort)
			};
		}
		/// <summary>
		/// Разница ограничений на кол-во потоков
		/// </summary>
		/// <param name="lim1">Первый набор ограничений</param>
		/// <param name="lim2">Второй набор ограничений</param>
		/// <returns>Разностный набор ограничений</returns>
		public static ThreadLimits operator - ( ThreadLimits lim1,ThreadLimits lim2 )
		{
			return new ThreadLimits() {
				Worker = (lim1 == null ? 0 : lim1.Worker) - (lim2 == null ? 0 : lim2.Worker),
				CompletionPort = (lim1 == null ? 0 : lim1.CompletionPort) - (lim2 == null ? 0 : lim2.CompletionPort)
			};
		}
		/// <summary>
		/// Произведение ограничений на кол-во потоков и числа
		/// </summary>
		/// <param name="lim">Набор ограничений</param>
		/// <param name="multiplier">Число</param>
		/// <returns>Результирующий набор ограничений</returns>
		public static ThreadLimits operator * ( ThreadLimits lim,int multiplier )
		{
			return new ThreadLimits() {
				Worker = lim == null ? 0 : lim.Worker * multiplier,
				CompletionPort = lim == null ? 0 : lim.CompletionPort * multiplier
			};
		}

		/// <summary>
		/// Получить максимальные значения ограничений
		/// </summary>
		/// <param name="lim1">Первый набор ограничений</param>
		/// <param name="lim2">Второй набор ограничений</param>
		/// <returns>Составленный из двух набор максимальных ограничений</returns>
		public static ThreadLimits Max( ThreadLimits lim1,ThreadLimits lim2 )
		{
			return new ThreadLimits() {
				Worker = Math.Max( lim1 == null ? 0 : lim1.Worker,lim2 == null ? 0 : lim2.Worker ),
				CompletionPort = Math.Max( lim1 == null ? 0 : lim1.CompletionPort,lim2 == null ? 0 : lim2.CompletionPort )
			};
		}
	}

	/// <summary>
	/// Класс настроек потоков
	/// </summary>
	public sealed class ThreadSettings
	{
		private ThreadLimits _min = null;
		/// <summary>
		/// Минимальное кол-во потоков
		/// </summary>
		public ThreadLimits Min { get { return _min ?? (_min = new ThreadLimits(ThreadPool.GetMinThreads)); } }

		private ThreadLimits _max = null;
		/// <summary>
		/// Максимальное кол-во потоков
		/// </summary>
		public ThreadLimits Max { get { return _max ?? (_max = new ThreadLimits(ThreadPool.GetMaxThreads)); } }

		private ThreadLimits _available = null;
		/// <summary>
		/// Кол-во доступных потоков
		/// </summary>
		public ThreadLimits Available { get { return _available ?? (_available = new ThreadLimits(ThreadPool.GetAvailableThreads)); } }

		/// <summary>
		/// Установить минимальное кол-во потов пула
		/// </summary>
		/// <param name="worker">Кол-во "обычных" потоков,
		/// если > 0, устанавливается значение не меньшее кол-ва процессоров (ядер),
		/// иначе значение не меняется</param>
		/// <param name="completionPort">Кол-во потоков асинхронного ожидания I/O,
		/// если > 0, устанавливается значение не меньшее кол-ва процессоров (ядер),
		/// иначе значение не меняется</param>
		/// <param name="autoGrowthMax">Флаг автоматического увеличения максимального кол-ва потоков,
		/// если заданы превышающие их значения</param>
		/// <returns>Флаг успешного выполнения</returns>
		public bool SetMinPoolLimits( int worker,int completionPort,bool autoGrowthMax = true )
		{
			bool result = false;
			int processors = Environment.ProcessorCount;

			worker = worker <= 0 ? Min.Worker : worker < processors ? processors : worker;
			completionPort = completionPort <= 0 ? Min.CompletionPort : completionPort < processors ? processors : completionPort;
			if(worker != Min.Worker || completionPort != Min.CompletionPort)
			{
				if(	worker <= Max.Worker && completionPort <= Max.CompletionPort ||
					autoGrowthMax && ThreadPool.SetMaxThreads( Math.Max( worker,Max.Worker ),Math.Max( completionPort,Max.CompletionPort ) ) )
				{
					if(result = ThreadPool.SetMinThreads( worker,completionPort ))
					{
						_min = null;
						_max = null;
						_available = null;
					}
				}
			}
			else
				result = true;

			return result;
		}
		/// <summary>
		/// Установить максимальное кол-во потоков
		/// </summary>
		/// <param name="worker">Кол-во "обычных" потоков,
		/// если > 0, устанавливается значение не меньшее кол-ва процессоров (ядер),
		/// иначе значение не меняется</param>
		/// <param name="completionPort">Кол-во потоков асинхронного ожидания I/O,
		/// если > 0, устанавливается значение не меньшее кол-ва процессоров (ядер),
		/// иначе значение не меняется</param>
		/// <returns>Флаг успешного выполнения</returns>
		public bool SetMaxPoolLimits( int worker,int completionPort )
		{
			bool result = false;
			int processors = Environment.ProcessorCount;

			worker = worker <= 0 ? Max.Worker : worker < processors ? processors : worker;
			completionPort = completionPort <= 0 ? Max.CompletionPort : completionPort < processors ? processors : completionPort;
			if(worker != Max.Worker || completionPort != Max.CompletionPort)
			{
				if(result = ThreadPool.SetMaxThreads( worker,completionPort ))
				{
					_max = null;
					_available = null;
				}
			}
			else
				result = true;

			return result;
		}
	}

	/// <summary>
	/// Вспомогательные методы при работе с потоками
	/// </summary>
	public static class ThreadHelper
	{
		/// <summary>
		/// Текущие значения настроек потоков
		/// </summary>
		public static ThreadSettings Settings { get { return new ThreadSettings(); } }

		/// <summary>
		/// Установить минимальное кол-во потоков пула
		/// </summary>
		/// <param name="limits">Ограничение на кол-во потоков</param>
		/// <param name="autoGrowthMax">Флаг автоматического увеличения максимального кол-ва потоков,
		/// если заданы превышающие их значения</param>
		/// <returns>Флаг успешного выполнения</returns>
		public static bool SetMinPoolLimits( this ThreadLimits limits,bool autoGrowthMax = true )
		{
			return limits == null ? false : Settings.SetMinPoolLimits( limits.Worker,limits.CompletionPort,autoGrowthMax );
		}
		/// <summary>
		/// Установить максимальное кол-во потоков
		/// </summary>
		/// <param name="limits">Ограничение на кол-во потоков</param>
		/// <returns>Флаг успешного выполнения</returns>
		public static bool SetMaxPoolLimits( this ThreadLimits limits )
		{
			return limits == null ? false : Settings.SetMaxPoolLimits( limits.Worker,limits.CompletionPort );
		}
	}
}
