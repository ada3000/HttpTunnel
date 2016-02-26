//-----------------------------------------------------------------------
// Cls.EnumeratorLoader.cs - описание класса прототипа для реализации
//	загрузки итератора по элементам
//
// Created by *** 12.06.2013
//-----------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;


namespace Lib.Base
{
	/// <summary>
	/// Класс прототипа реализации загрузки итератора
	/// </summary>
	/// <typeparam name="T">Тип объекта</typeparam>
	public class EnumeratorLoader<T> : IDisposableImpl,
		IEnumerable<T>,
		IEnumerator<T>
		where T : class
	{
		/// <summary>
		/// Текущий элемент
		/// </summary>
		private T _current = null;

		/// <summary>
		/// Метод устанавливающий в начальное положение (до первого элемента в коллекции), вызывается из метода IEnumerator.Reset
		/// </summary>
		public Action ResetEnumerator = null;
		/// <summary>
		/// Метод закрывающий итератор, вызывается из метода Close и IDisposable.DisposeManaged
		/// </summary>
		public Action CloseEnumerator = null;
		/// <summary>
		/// Метод возвращающий следующий элемент итератора, вызывается из метода IEnumerator.MoveNext,
		/// элемент должен быть не null, иначе воспринимается как конец списка
		/// </summary>
		public Func<T> GetNext = null;

		/// <summary>
		/// Закрыть итератор
		/// </summary>
		public void Close()
		{
			if(CloseEnumerator != null) CloseEnumerator();
			_current = null;
		}

		#region IEnumerator members

		/// <summary>
		/// Текущий элемент в коллекции
		/// </summary>
		public T Current { get { return _current; } }
		/// <summary>
		/// Текущий элемент в коллекции
		/// </summary>
		object IEnumerator.Current { get { return _current; } }
		/// <summary>
		/// Перейти на следующий элемент в коллекции
		/// </summary>
		/// <returns>Удалось перейти - true, иначе - false</returns>
		public bool MoveNext()
		{
			return (_current = GetNext == null ? null : GetNext()) != null;
		}
		/// <summary>
		/// Установить в начальное положение (до первого элемента в коллекции)
		/// </summary>
		public void Reset()
		{
			if(ResetEnumerator != null) ResetEnumerator();
			_current = null;
		}

		#endregion IEnumerator members

		#region IEnumerable members

		/// <summary>
		/// Получить итератор коллекции
		/// </summary>
		/// <returns>Итератор коллекции</returns>
		public IEnumerator<T> GetEnumerator() { return this; }
		/// <summary>
		/// Получить итератор коллекции
		/// </summary>
		/// <returns>Итератор коллекции</returns>
		IEnumerator IEnumerable.GetEnumerator() { return this; }

		#endregion IEnumerable members

		#region IDisposable members

		/// <summary>
		/// Освободить управляемые ресурсы
		/// </summary>
		protected override void DisposeManaged()
		{
			base.DisposeManaged();
			if(CloseEnumerator != null) CloseEnumerator();
			_current = null;
		}

		#endregion IDisposable members
	}
}
