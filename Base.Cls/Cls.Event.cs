//-----------------------------------------------------------------------
// Cls.Event.cs - описание классов событий и уведомлений
//
// Created by *** 29.11.2011
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Threading;


namespace Lib.Base
{
	/// <summary>
	/// Базовые объявления и методы событий
	/// </summary>
	public static class EventBase
	{
		/// <summary>
		/// Прототип делегата события
		/// </summary>
		/// <typeparam name="T">Тип инициатора события</typeparam>
		/// <param name="sender">Инициатор события</param>
		/// <param name="args">Аргументы</param>
		public delegate void Handler<T>( T sender,EventArgs args );
		/// <summary>
		/// Прототип делегата события, возвращающего результат обработки
		/// </summary>
		/// <typeparam name="T">Тип инициатора события</typeparam>
		/// <typeparam name="R">Тип результата обработки события</typeparam>
		/// <param name="sender">Инициатор события</param>
		/// <param name="args">Аргументы</param>
		/// <returns>Результат обработки события</returns>
		public delegate R Handler<T,R>( T sender,EventArgs args );

		/// <summary>
		/// Запуск события прототипа Handler
		/// </summary>
		/// <typeparam name="T">Тип инициатора события</typeparam>
		/// <param name="hdlEvent">Событие</param>
		/// <param name="sender">Инициатор события</param>
		/// <param name="args">Аргументы</param>
		public static void Run<T>( this Handler<T> hdlEvent,T sender,EventArgs args = null )
		{
			if(hdlEvent != null) hdlEvent( sender,args ?? EventArgs.Empty );
		}
		/// <summary>
		/// Запуск события прототипа Handler, возвращающего результат обработки
		/// </summary>
		/// <typeparam name="T">Тип инициатора события</typeparam>
		/// <typeparam name="R">Тип результата обработки события</typeparam>
		/// <param name="hdlEvent">Событие</param>
		/// <param name="sender">Инициатор события</param>
		/// <param name="defResult">Результат возвращаемый по умолчанию, в случае отсутствия события (hdlEvent == null)</param>
		/// <param name="args">Аргументы</param>
		/// <returns>Результат обработки события</returns>
		public static R Run<T,R>( this Handler<T,R> hdlEvent,T sender,R defResult,EventArgs args = null )
		{
			return hdlEvent == null ? defResult : hdlEvent( sender,args ?? EventArgs.Empty );
		}
		/// <summary>
		/// Запуск события EventHandler с типизированым инициатором события
		/// </summary>
		/// <typeparam name="T">Тип инициатора события</typeparam>
		/// <param name="hdlEvent">Событие</param>
		/// <param name="sender">Инициатор события</param>
		/// <param name="args">Аргументы</param>
		public static void Run<T>( this EventHandler hdlEvent,T sender,EventArgs args = null )
		{
			if(hdlEvent != null) hdlEvent( sender,args ?? EventArgs.Empty );
		}
		/// <summary>
		/// Запуск события EventHandler
		/// </summary>
		/// <param name="hdlEvent">Событие</param>
		/// <param name="sender">Инициатор события</param>
		/// <param name="args">Аргументы</param>
		public static void Run( this EventHandler hdlEvent,object sender,EventArgs args = null )
		{
			if(hdlEvent != null) hdlEvent( sender,args ?? EventArgs.Empty );
		}

		/// <summary>
		/// Проверка взведённого состояния флага ожидания,
		/// если флаг взведён возвращает true, иначе - false
		/// </summary>
		/// <param name="hdl">Флаг ожидания</param>
		/// <returns>флаг взведён - true, иначе - false</returns>
		public static bool IsSet( this WaitHandle hdl )
		{
			return hdl == null ? false : hdl.WaitOne( 0,false );
		}
		/// <summary>
		/// Проверка сброшенного состояния флага ожидания,
		/// если флаг сброшен возвращает true, иначе - false
		/// </summary>
		/// <param name="hdl">Флаг ожидания</param>
		/// <returns>флаг сброшен - true, иначе - false</returns>
		public static bool IsReset( this WaitHandle hdl )
		{
			return hdl == null ? true : !hdl.WaitOne( 0,false );
		}

		/// <summary>
		/// Ожидание взведения флага с "минимальным" timeout-ом,
		/// время "минимального" timeout-а задано константой MinIdleTimeout = 100[ms],
		/// если флаг взведён возвращает true, иначе - false
		/// </summary>
		/// <param name="hdl">Флаг ожидания</param>
		/// <param name="exitContext">true to exit the synchronization domain for the context before the wait (if in a synchronized context), and reacquire it afterward; otherwise, false</param>
		/// <returns>Флаг взведён - true, иначе - false</returns>
		public static bool WaitIdle( this WaitHandle hdl,bool exitContext = false )
		{
			return hdl == null ? false : hdl.WaitOne( MinIdleTimeout,exitContext );
		}

		/// <summary>
		/// Минимальный интервал "простоя" [ms]
		/// </summary>
		public const int MinIdleTimeout = 100; // [ms]
	}

	/// <summary>
	/// Словарь событий (флагов)
	/// </summary>
	/// <typeparam name="TKey">тип идентифицирующего ключа</typeparam>
	public class EventDictionary<TKey>
	{
		private Dictionary<TKey,ManualResetEvent> _events = new Dictionary<TKey,ManualResetEvent>();

		/// <summary>
		/// Получить событие (флаг) по ключу
		/// Если события соответствующего ключу, будет создано новое и добавлено в список
		/// Получение/установка события выполняется с блокировкой доступа
		/// </summary>
		/// <param name="key">идентифицирующий ключ</param>
		/// <returns>Соответствующее ключу событие (флаг)</returns>
		public ManualResetEvent this[ TKey key ]
		{
			get
			{
				lock(_events)
					return _events.ContainsKey( key ) ? _events[key] : _events[key] = new ManualResetEvent(false);
			}
			set
			{
				lock(_events)
					_events[key] = value;
			}
		}
	}

	/// <summary>
	/// Класс события таймера
	/// </summary>
	public sealed class TimerEvent : EventWaitHandle
	{
		private Timer _timer;
		private readonly bool _initialState;

		/// <summary>
		/// "Бесконечное" время ожидания
		/// </summary>
		public static readonly TimeSpan TimeSpanInfinite = new TimeSpan(0,0,0,0,-1);

		/// <summary>
		/// Создание события таймера
		/// </summary>
		/// <param name="initialState">Начальное состояние события</param>
		public TimerEvent( bool initialState ) : base(initialState,EventResetMode.ManualReset)
		{
			_initialState = initialState;
			_timer = new Timer((x) => {
				lock(this) if(_timer != null)
				{
					if(_initialState) Reset(); else Set();
				}
			});
		}
		/// <summary>
		/// Создание события таймера, сбрасывающего значение через указанный промежуток времени
		/// </summary>
		/// <param name="initialState">Начальное состояние события</param>
		/// <param name="msTime">Время срабатывания события [ms]</param>
		public TimerEvent( bool initialState,long msTime ) : this(initialState)
		{
			_timer.Change( msTime,Timeout.Infinite );
		}
		/// <summary>
		/// Создание события таймера, сбрасывающего значение через указанный промежуток времени
		/// </summary>
		/// <param name="initialState">Начальное состояние события</param>
		/// <param name="time">Время срабатывания события</param>
		public TimerEvent( bool initialState,TimeSpan time ) : this(initialState)
		{
			_timer.Change( time,TimeSpanInfinite );
		}

		/// <summary>
		/// Активировать таймер, срабатывающий через указанный промежуток времени
		/// </summary>
		/// <param name="msTime">Время срабатывания события [ms]</param>
		/// <param name="reset">Флаг сброса события в начальное значение определённое в конструкторе при активации таймера</param>
		public void Activate( long msTime,bool reset = true )
		{
			lock(this)
			{
				if(reset) { if(_initialState) Set(); else Reset(); }
				if(_timer != null) _timer.Change( msTime,Timeout.Infinite );
			}
		}
		/// <summary>
		/// Активировать таймер, срабатывающий через указанный промежуток времени
		/// </summary>
		/// <param name="time">Время срабатывания события</param>
		/// <param name="reset">Флаг сброса события в начальное значение определённое в конструкторе при активации таймера</param>
		public void Activate( TimeSpan time,bool reset = true )
		{
			lock(this)
			{
				if(reset) { if(_initialState) Set(); else Reset(); }
				if(_timer != null) _timer.Change( time,TimeSpanInfinite );
			}
		}
		/// <summary>
		/// Деактивировать таймер
		/// </summary>
		public void Deactivate()
		{
			lock(this) if(_timer != null)
				_timer.Change( Timeout.Infinite,Timeout.Infinite );
		}

		#region IDisposable members

		/// <summary>
		/// Освободить ресурсы
		/// </summary>
		/// <param name="explicitDisposing">true - освободить managed и unmanaged ресурсы, false - только unmanaged</param>
		protected override void Dispose( bool explicitDisposing )
		{
			if(explicitDisposing) lock(this)
			{
				if(_timer != null)
				{
					_timer.Dispose();
					_timer = null;
				}
			}
			base.Dispose( explicitDisposing );
		}

		#endregion IDisposable members
	}
}
