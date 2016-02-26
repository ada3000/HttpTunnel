//-----------------------------------------------------------------------
// Cls.StreamProxy.cs - описание класса прототипа для реализации
//	проксирования потоков
//
// Created by *** 4.07.2013
//-----------------------------------------------------------------------
using System;
using System.IO;


namespace Lib.Base
{
	/// <summary>
	/// Класс прототипа реализации прокси потока
	/// </summary>
	public class StreamProxy : Stream
	{
		/// <summary>
		/// Оригинальный поток
		/// </summary>
		private Stream _stream;

		/// <summary>
		/// Метод вызываемый при закрытии потока прокси
		/// </summary>
		public Action CloseStream = null;

		/// <summary>
		/// Флаг возможности чтения
		/// </summary>
		public override bool CanRead { get { return _stream == null ? false : _stream.CanRead; } }
		/// <summary>
		/// Флаг возможности превышения времени ожидания потоком
		/// </summary>
		public override bool CanTimeout { get { return _stream == null ? base.CanTimeout : _stream.CanTimeout;; } }
		/// <summary>
		/// Флаг возможности позиционирования в потоке
		/// </summary>
		public override bool CanSeek { get { return _stream == null ? false : _stream.CanSeek; } }
		/// <summary>
		/// Флаг возможности записи
		/// </summary>
		public override bool CanWrite { get { return _stream == null ? false : _stream.CanWrite; } }
		/// <summary>
		/// Длина потока
		/// </summary>
		public override long Length { get { return _stream == null ? -1 : _stream.Length; } }
		/// <summary>
		/// Текущая позиция в потоке
		/// </summary>
		public override long Position
		{
			get { return _stream == null ? -1 : _stream.Position; }
			set { if(_stream != null) _stream.Position = value; }
		}
		/// <summary>
		/// Получить/установить время [ms] при операции чтения, после которого поток "отвалится"
		/// </summary>
		public override int ReadTimeout
		{
			get { return _stream == null ? base.ReadTimeout : _stream.ReadTimeout; }
			set { if(_stream == null) base.ReadTimeout = value; else _stream.ReadTimeout = value; }
		}
		/// <summary>
		/// Получить/установить время [ms] при операции записи, после которого поток "отвалится"
		/// </summary>
		public override int WriteTimeout
		{
			get { return _stream == null ? base.WriteTimeout : _stream.WriteTimeout; }
			set { if(_stream == null) base.WriteTimeout = value; else _stream.WriteTimeout = value; }
		}

		/// <summary>
		/// Создать объект прокси потока
		/// </summary>
		/// <param name="stream">Оригинальный поток</param>
		public StreamProxy( Stream stream ) { _stream = stream; }

		/// <summary>
		/// Установить длину потока
		/// </summary>
		/// <param name="value">Устанавливаемое значение длины потока</param>
		public override void SetLength( long value )
		{
			if(_stream != null) _stream.SetLength( value );
		}
		/// <summary>
		/// Установить позицию в потоке
		/// </summary>
		/// <param name="offset">Смещение в потоке</param>
		/// <param name="origin">Точка отсчёта, относительно которой устанавливается смещение</param>
		/// <returns>Установленное значение</returns>
		public override long Seek( long offset,SeekOrigin origin )
		{
			return _stream == null ? -1 : _stream.Seek( offset,origin );
		}
		/// <summary>
		/// Сбросить состояния внутреннего кэша (буфера)
		/// </summary>
		public override void Flush()
		{
			if(_stream != null) _stream.Flush();
		}
		/// <summary>
		/// Записать данные в поток
		/// </summary>
		/// <param name="buffer">Буфер данных</param>
		/// <param name="offset">Смещение в буфере</param>
		/// <param name="count">Кол-во записываемых из буфера байт</param>
		public override void Write( byte[] buffer,int offset,int count )
		{
			if(_stream != null) _stream.Write( buffer,offset,count );
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
			return _stream == null ? 0 : _stream.Read( buffer,offset,count );
		}
		/// <summary>
		/// Закрыть поток и освободить ресурсы
		/// </summary>
		public override void Close()
		{
			if(_stream != null)
			{
				_stream.Close();
				_stream = null;
				if(CloseStream != null) CloseStream();
			}
		}
	}
}
