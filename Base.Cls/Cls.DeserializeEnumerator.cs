//-----------------------------------------------------------------------
// Cls.DeserializeEnumerator.cs - описание класса итератора по элементам
//	XmlReader-а, выполняющего преобразование к типизированным объектам
//
// Created by *** 13.03.2013
//-----------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;


namespace Lib.Base
{
	/// <summary>
	/// Класс типизированного итератора по элемента XmlReader-а, выполняющий преобразование к объектам
	/// </summary>
	/// <typeparam name="T">Тип объекта</typeparam>
	public sealed class DeserializeEnumerator<T> : IDisposableImpl,
		IEnumerable<T>,
		IEnumerator<T>
		where T : class, new()
	{
		/// <summary>
		/// Используемый итератор по XmlReader-у
		/// </summary>
		private XmlReaderEnumerator _reader = null;
		/// <summary>
		/// Текущий элемент
		/// </summary>
		private T _current = null;
		/// <summary>
		/// Используемый для "распаковки" XmlSerializer
		/// </summary>
		private XmlSerializer _serializer = null;

		/// <summary>
		/// Создание итератора по элементам XmlReader соответствующих критерию
		/// </summary>
		/// <param name="reader">Используемый XmlReader</param>
		/// <param name="criteria">Метод фильтрации подходящих элементов bool Criteria( XmlReader element ),
		/// если не задан (null), все элементы первого уровня</param>
		/// <param name="end">Метод вызываемый по окончании выборки</param>
		public DeserializeEnumerator( XmlReader reader,Func<XmlReader,bool> criteria = null,Action end = null )
		{
			_reader = new XmlReaderEnumerator(reader,criteria,end);
		}
		/// <summary>
		/// Создание итератора по элементам XmlReader соответствующих критерию
		/// </summary>
		/// <param name="reader">Используемый XmlReader</param>
		/// <param name="serializer">Сконфигурированный XmlSerializer</param>
		/// <param name="criteria">Метод фильтрации подходящих элементов bool Criteria( XmlReader element ),
		/// если не задан (null), все элементы первого уровня</param>
		/// <param name="end">Метод вызываемый по окончании выборки</param>
		public DeserializeEnumerator( XmlReader reader,XmlSerializer serializer,Func<XmlReader,bool> criteria = null,Action end = null )
		{
			_serializer = serializer;
			_reader = new XmlReaderEnumerator(reader,criteria,end);
		}
		/// <summary>
		/// Создание итератора по элементам XmlReader с заданным названием
		/// </summary>
		/// <param name="reader">Используемый XmlReader</param>
		/// <param name="name">Название элементов</param>
		/// <param name="end">Метод вызываемый по окончании выборки</param>
		public DeserializeEnumerator( XmlReader reader,string name,Action end = null )
		{
			_reader = new XmlReaderEnumerator(reader,name,end);
		}
		/// <summary>
		/// Создание итератора по элементам XmlReader с заданным названием
		/// </summary>
		/// <param name="reader">Используемый XmlReader</param>
		/// <param name="serializer">Сконфигурированный XmlSerializer</param>
		/// <param name="name">Название элементов</param>
		/// <param name="end">Метод вызываемый по окончании выборки</param>
		public DeserializeEnumerator( XmlReader reader,XmlSerializer serializer,string name,Action end = null )
		{
			_serializer = serializer;
			_reader = new XmlReaderEnumerator(reader,name,end);
		}

		/// <summary>
		/// Закрыть используемый коллекцией XmlReader
		/// </summary>
		public void Close()
		{
			if(_reader != null) _reader.Close();
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
			bool moved = _reader != null && _reader.MoveNext();
			_current = !moved ? null :
				_serializer == null ? _reader.Current.Deserialize<T>() :
				_reader.Current.Deserialize<T>( _serializer );
			return moved;
		}
		/// <summary>
		/// Установить в начальное положение (до первого элемента в коллекции)
		/// </summary>
		public void Reset()
		{
			if(_reader != null) _reader.Reset();
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
			if(_reader != null)
			{
				_reader.Dispose();
				_reader = null;
			}
			_current = null;
		}

		#endregion IDisposable members
	}

	/// <summary>
	/// Класс вспомогательных методов для работы с DeserializeEnumerator-ом
	/// </summary>
	public static class DeserializeEnumeratorHelper
	{
		/// <summary>
		/// Создать типизированнный итератор по элементам XmlReader-а для заданного узла в качестве корневого,
		/// в котором будут элементы соответствующие указанному критерию
		/// </summary>
		/// <typeparam name="T">Тип объекта</typeparam>
		/// <param name="root">Используемый корневой элемент XmlReader-а</param>
		/// <param name="criteria">Метод фильтрации подходящих элементов bool Criteria( XmlReader element ),
		/// если не задан (null), все элементы первого уровня под корневым элементом</param>
		/// <param name="end">Метод вызываемый по окончании выборки</param>
		/// <returns>Типизированный итератор элементов XmlReader-а</returns>
		public static DeserializeEnumerator<T> ToSubtreeDeserializeEnumerator<T>( this XmlReader root,Func<XmlReader,bool> criteria = null,Action end = null )
			where T : class, new()
		{
			var subtree = root.ReadSubtree();
			subtree.MoveToContent();
			subtree.Read();
			return new DeserializeEnumerator<T>(subtree,criteria,() => {
					subtree.Close();
					root.Skip();
					if(end != null) end();
				});
		}
		/// <summary>
		/// Создать типизированнный итератор по элементам XmlReader-а для заданного узла в качестве корневого,
		/// в котором будут элементы соответствующие указанному критерию
		/// </summary>
		/// <typeparam name="T">Тип объекта</typeparam>
		/// <param name="root">Используемый корневой элемент XmlReader-а</param>
		/// <param name="serializer">Сконфигурированный XmlSerializer</param>
		/// <param name="criteria">Метод фильтрации подходящих элементов bool Criteria( XmlReader element ),
		/// если не задан (null), все элементы первого уровня под корневым элементом</param>
		/// <param name="end">Метод вызываемый по окончании выборки</param>
		/// <returns>Типизированный итератор элементов XmlReader-а</returns>
		public static DeserializeEnumerator<T> ToSubtreeDeserializeEnumerator<T>( this XmlReader root,XmlSerializer serializer,Func<XmlReader,bool> criteria = null,Action end = null )
			where T : class, new()
		{
			var subtree = root.ReadSubtree();
			subtree.MoveToContent();
			subtree.Read();
			return new DeserializeEnumerator<T>(subtree,serializer,criteria,() => {
					subtree.Close();
					root.Skip();
					if(end != null) end();
				});
		}
		/// <summary>
		/// Создать типизированнный итератор по элементам XmlReader-а для заданного узла в качестве корневого,
		/// в котором будут элементы с указанным названием
		/// </summary>
		/// <typeparam name="T">Тип объекта</typeparam>
		/// <param name="root">Используемый корневой элемент XmlReader-а</param>
		/// <param name="name">Название элементов</param>
		/// <param name="end">Метод вызываемый по окончании выборки</param>
		/// <returns>Типизированный итератор элементов XmlReader-а</returns>
		public static DeserializeEnumerator<T> ToSubtreeDeserializeEnumerator<T>( this XmlReader root,string name,Action end = null )
			where T : class, new()
		{
			return root.ToSubtreeDeserializeEnumerator<T>( (item) => { return item != null && item.LocalName.Equals( name ); },end );
		}
		/// <summary>
		/// Создать типизированнный итератор по элементам XmlReader-а для заданного узла в качестве корневого,
		/// в котором будут элементы с указанным названием
		/// </summary>
		/// <typeparam name="T">Тип объекта</typeparam>
		/// <param name="root">Используемый корневой элемент XmlReader-а</param>
		/// <param name="serializer">Сконфигурированный XmlSerializer</param>
		/// <param name="name">Название элементов</param>
		/// <param name="end">Метод вызываемый по окончании выборки</param>
		/// <returns>Типизированный итератор элементов XmlReader-а</returns>
		public static DeserializeEnumerator<T> ToSubtreeDeserializeEnumerator<T>( this XmlReader root,XmlSerializer serializer,string name,Action end = null )
			where T : class, new()
		{
			return root.ToSubtreeDeserializeEnumerator<T>( serializer,(item) => { return item != null && item.LocalName.Equals( name ); },end );
		}
	}
}
