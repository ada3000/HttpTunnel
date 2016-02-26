//-----------------------------------------------------------------------
// Cls.IDisposableImpl.cs - реализация интерфейса IDisposable
//
// Created by *** 08.09.2010
//-----------------------------------------------------------------------
using System;


namespace Lib.Base
{
	/// <summary>
	/// Стандартная реализация интерфейса IDisposable
	/// </summary>
	public class IDisposableImpl : IDisposable
	{
		/// <summary>
		/// Флаг факта выполнения метода Dispose
		/// </summary>
		protected bool _disposed = false;

		/// <summary>
		/// Конструктор
		/// </summary>
		public IDisposableImpl() {}

		#region IDisposable Members

		/// <summary>
		/// Метод интерфейса IDisposable
		/// </summary>
		public void Dispose()
		{
			Dispose( true );
			GC.SuppressFinalize( this );
		}

		/// <summary>
		/// Внутренняя реализация метода Dispose
		/// </summary>
		/// <param name="disposing"></param>
		private void Dispose( bool disposing )
		{
			lock(this)
			{
				if(!_disposed)
				{
					if(disposing)
						DisposeManaged();
					DisposeUnmanaged();

					_disposed = true;
				}
			}
		}

		/// <summary>
		/// Метод необходимо перегрузить для освобожнения управляемых ресурсов
		/// </summary>
		protected virtual void DisposeManaged() {}

		/// <summary>
		/// Метод необходимо перегрузить для освобожнения неуправляемых ресурсов
		/// </summary>
		protected virtual void DisposeUnmanaged() {}

		#endregion IDisposable Members

        /// <summary>
        /// Финализатор
        /// </summary>
	    ~IDisposableImpl()
	    {
	        Dispose(false);
	    }
	}
}
