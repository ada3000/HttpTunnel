//-----------------------------------------------------------------------
// Cls.StreamReader.cs - описание класса прототипа для реализации
//	потоковой загрузки
//
// Created by *** 21.02.2013
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;


namespace Lib.Base
{
	/// <summary>
	/// Класс прототипа реализации потоковой загрузки
	/// </summary>
	public class StreamLoader : Stream
	{
		/// <summary>
		/// Позиция в кэше
		/// </summary>
		private int _position = 0;
		/// <summary>
		/// Кэш данных для передачи
		/// </summary>
		private byte[] _cache = null;
		/// <summary>
		/// Флаг возможности чтения
		/// </summary>
		private bool _canRead = false;

		/// <summary>
		/// Метод выполняющий загрузку в кэш, вызываемый из метода Read, для получения фактических данных
		/// </summary>
		public Func<byte[]> LoadCache = null;

		/// <summary>
		/// Флаг возможности чтения
		/// </summary>
		public override bool CanRead { get { return _canRead; } }
		/// <summary>
		/// Флаг возможности записи
		/// </summary>
		public override bool CanWrite { get { return false; } }
		/// <summary>
		/// Флаг возможности позиционирования в потоке
		/// </summary>
		public override bool CanSeek { get { return false; } }
		//public override bool CanTimeout { get { return base.CanTimeout; } }
		/// <summary>
		/// Длина потока
		/// </summary>
		public override long Length { get { return -1; } } //???
		/// <summary>
		/// Текущая позиция в потоке
		/// </summary>
		public override long Position
		{
			get { return -1; } //??? 0 ???
			set { Seek( value,SeekOrigin.Begin ); }
		}

		/// <summary>
		/// Создание объекта потоковой загрузки
		/// </summary>
		/// <param name="canRead">Начальное значение флага возможности чтения</param>
		public StreamLoader( bool canRead = true )
		{
			_canRead = canRead;
		}
		/// <summary>
		/// Установить длину потока (метод не реализован)
		/// </summary>
		/// <param name="value">Устанавливаемое значение длины потока</param>
		public override void SetLength( long value )
		{
			throw new NotImplementedException();
		}
		/// <summary>
		/// Установить позицию в потоке (метод не реализован)
		/// </summary>
		/// <param name="offset">Смещение в потоке</param>
		/// <param name="origin">Точка отсчёта, относительно которой устанавливается смещение</param>
		/// <returns>Установленное значение</returns>
		public override long Seek( long offset,SeekOrigin origin )
		{
			throw new NotImplementedException();
			//return 0; //??? -1 ???
		}
		/// <summary>
		/// Сбросить состояния внутреннего кэша (буфера)
		/// </summary>
		public override void Flush()
		{
			_cache = null;
			_position = 0;
		}
		/// <summary>
		/// Записать данные в поток (метод не реализован)
		/// </summary>
		/// <param name="buffer">Буфер данных</param>
		/// <param name="offset">Смещение в буфере</param>
		/// <param name="count">Кол-во записываемых из буфера байт</param>
		public override void Write( byte[] buffer,int offset,int count )
		{
			throw new NotImplementedException();
		}
		/// <summary>
		/// Считать указанное кол-во байт из потока
		/// Возвращает кол-во фактически считанных байт, если 0 - поток закончился
		/// </summary>
		/// <param name="buffer">Буфер, в который читать данные</param>
		/// <param name="offset">Смещение в буфере</param>
		/// <param name="count">Максимальное кол-во запрашиаемых из потока байт</param>
		/// <returns>Кол-во фактически считанных байт</returns>
		public override int Read( byte[] buffer,int offset,int count )
		{
			int filled = 0;

			for(int len = _cache == null ? -1 : _cache.Length; _canRead && filled < count; )
			{
				if(_position >= len)
				{
					_position = 0;
					_cache = LoadCache == null ? null : LoadCache();
					len = _cache == null ? -1 : _cache.Length;
				}
				if(len > 0)
				{
					int size = Math.Min( len - _position,count - filled );
					Array.ConstrainedCopy( _cache,_position,buffer,offset + filled,size );
					_position += size;
					filled += size;
				}
				else
					_canRead = false;
			}

			return filled;
		}

		//public override int ReadByte() { return base.ReadByte(); }
	}

	/// <summary>
	/// Вспомогательные методы для работы с потоками
	/// </summary>
	public static class StreamHelper
	{
		/// <summary>
		/// Перобразовать в поток
		/// </summary>
		/// <param name="enumerator">Итератор XmlReader-ов</param>
		/// <param name="root">Название корневого элемента</param>
		/// <param name="bufferSize">Размер буфера потока [символы], по умолчанию 4К</param>
		/// <param name="encoding">Кодировка строки в потоке</param>
		/// <returns>Созданный поток</returns>
		public static Stream ToStream( this IEnumerable<XmlReader> enumerator,string root,int bufferSize = 0,Encoding encoding = null )
		{
			return CreateStream( enumerator == null ? null : enumerator.GetEnumerator(),root,null,null,bufferSize,encoding );
		}

		/// <summary>
		/// Перобразовать в поток
		/// </summary>
		/// <param name="enumerator">Итератор XmlReader-ов</param>
		/// <param name="root">Название корневого элемента</param>
		/// <param name="beforeMoveNext">Действие перед переходом на следующий элемент итератора</param>
		/// <param name="afterMoveNext">Действие после перехода на следующий элемент итератора</param>
		/// <param name="bufferSize">Размер буфера потока [символы], по умолчанию 4К</param>
		/// <param name="encoding">Кодировка строки в потоке</param>
		/// <returns>Созданный поток</returns>
		public static Stream ToStream( this IEnumerable<XmlReader> enumerator,string root,Action beforeMoveNext,Action afterMoveNext,int bufferSize = 0,Encoding encoding = null )
		{
			return CreateStream( enumerator == null ? null : enumerator.GetEnumerator(),root,beforeMoveNext,afterMoveNext,bufferSize,encoding );
		}

		/// <summary>
		/// Создать поток из итератора XmlReader-ов
		/// </summary>
		/// <param name="enumerator">Итератор XmlReader-ов</param>
		/// <param name="root">Название корневого элемента</param>
		/// <param name="beforeMoveNext">Действие перед переходом на следующий элемент итератора</param>
		/// <param name="afterMoveNext">Действие после перехода на следующий элемент итератора</param>
		/// <param name="bufferSize">Размер буфера потока [символы], по умолчанию 4К</param>
		/// <param name="encoding">Кодировка строки в потоке</param>
		/// <returns>Созданный поток</returns>
		private static Stream CreateStream( IEnumerator<XmlReader> enumerator,string root,Action beforeMoveNext,Action afterMoveNext,int bufferSize,Encoding encoding )
		{
			bufferSize /= sizeof(char);
			var buff = new StringBuilder(bufferSize > 0 ? bufferSize * 2 : 4 * 1024);
			bool fetch = enumerator != null;
			int count = 0;
			return new StreamLoader(fetch){
				LoadCache = () => {
					while(fetch && (bufferSize > 0 ? buff.Length < bufferSize : buff.Length == 0))
					{
						if(beforeMoveNext != null) beforeMoveNext();
						if(fetch = enumerator.MoveNext())
						{
							var item = enumerator.Current;
							item.MoveToContent();
							if(count == 0 && root.IsValue())
								buff.Append( '<' ).Append( root ).Append( '>' );
							buff.Append( item.ReadOuterXml() );
							count++;
							if(afterMoveNext != null) afterMoveNext();
						}
						else if(root.IsValue())
						{
							buff.Append( count > 0 ? "</" : "<" ).Append( root ).Append( count > 0 ? ">" : " />" );
						}
					}
					return buff.ToByteArray( true,encoding );
				},
			};
		}
	}
}
