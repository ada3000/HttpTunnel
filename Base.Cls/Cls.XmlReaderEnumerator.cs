//-----------------------------------------------------------------------
// Cls.XmlReaderEnumerator.cs - описание класса итератора по элементам
//	XmlReader-а
//
// Created by *** 13.03.2013
//-----------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;


namespace Lib.Base
{
	/// <summary>
	/// Класс итератора по элементам XmlReader-а
	/// </summary>
	public sealed class XmlReaderEnumerator : IDisposableImpl,
		IEnumerable<XmlReader>,
		IEnumerator<XmlReader>
	{
		/// <summary>
		/// Используемый XmlReader
		/// </summary>
		private XmlReader _reader = null;
		/// <summary>
		/// Текущая позиция в XmlReader-е
		/// </summary>
		private XmlReader _current = null;
		/// <summary>
		/// Метод фильтрации элементов коллекции
		/// </summary>
		private Func<XmlReader,bool> _criteria;
		/// <summary>
		/// Метод вызываемый по окончании выборки итератора
		/// </summary>
		private Action _end;

		/// <summary>
		/// Создание итератора по элементам XmlReader соответствующих критерию
		/// </summary>
		/// <param name="reader">Используемый XmlReader</param>
		/// <param name="criteria">Метод фильтрации подходящих элементов bool Criteria( XmlReader element ),
		/// если не задан (null), все элементы первого уровня</param>
		/// <param name="end">Метод вызываемый по окончании выборки</param>
		public XmlReaderEnumerator( XmlReader reader,Func<XmlReader,bool> criteria = null,Action end = null )
		{
			if((_reader = reader) != null && _reader.ReadState == ReadState.Initial)
				_reader.MoveToContent();
			_criteria = criteria ?? ((item) => { return true; });
			_end = end ?? (() => {});
		}
		/// <summary>
		/// Создание итератора по элементам XmlReader с заданным названием
		/// </summary>
		/// <param name="reader">Используемый XmlReader</param>
		/// <param name="name">Название элементов</param>
		/// <param name="end">Метод вызываемый по окончании выборки</param>
		public XmlReaderEnumerator( XmlReader reader,string name,Action end = null ) :
			this(reader,(item) => { return item != null && item.LocalName.Equals( name ); },end)
		{
		}

		/// <summary>
		/// Закрыть используемый коллекцией XmlReader
		/// </summary>
		public void Close()
		{
			_reader = null;
			if(_current != null)
			{
				_current.Close();
				_current = null;
			}
		}

		#region IEnumerator members

		/// <summary>
		/// Текущий элемент в коллекции
		/// </summary>
		public XmlReader Current { get { return _current; } }
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
			bool moved = false;
			if(_reader != null)
			{
				if(_current != null)
				{
					_current.Close();
					_current = null;
				}
				bool read = _reader.ReadState == ReadState.Interactive;
				while((moved = _reader.ReadState == ReadState.Interactive) && _current == null)
				{
					if(_reader.NodeType == XmlNodeType.Element && _criteria( _reader ))
						_current = _reader.ReadSubtree();
					else
						_reader.Skip();
				}
				if(read && !moved) _end();
			}
			return moved;
		}
		/// <summary>
		/// Установить в начальное положение (до первого элемента в коллекции)
		/// </summary>
		public void Reset() { throw new NotImplementedException(); }

		#endregion IEnumerator members

		#region IEnumerable members

		/// <summary>
		/// Получить итератор коллекции
		/// </summary>
		/// <returns>Итератор коллекции</returns>
		public IEnumerator<XmlReader> GetEnumerator() { return this; }
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
			Close();
		}

		#endregion IDisposable members
	}

	/// <summary>
	/// Класс вспомогательных методов для работы с XmlReaderEnumerator-ом
	/// </summary>
	public static class XmlReaderEnumeratorHelper
	{
		/// <summary>
		/// Создать итератор элементов XmlReader-а для заданного узла в качестве корневого,
		/// в котором будут элементы соответствующие указанному критерию
		/// </summary>
		/// <param name="root">Используемый корневой элемент XmlReader-а</param>
		/// <param name="criteria">Метод фильтрации подходящих элементов bool Criteria( XmlReader element ),
		/// если не задан (null), все элементы первого уровня под корневым элементом</param>
		/// <param name="end">Метод вызываемый по окончании выборки</param>
		/// <returns>Итератор элементов XmlReader-а</returns>
		public static XmlReaderEnumerator ToSubtreeXmlReaderEnumerator( this XmlReader root,Func<XmlReader,bool> criteria = null,Action end = null )
		{
			var subtree = root.ReadSubtree();
			subtree.MoveToContent();
			subtree.Read();
			return new XmlReaderEnumerator(subtree,criteria,() => {
					subtree.Close();
					root.Skip();
					if(end != null) end();
				});
		}
		/// <summary>
		/// Создать итератор элементов XmlReader-а для заданного узла в качестве корневого,
		/// в котором будут элементы соответствующие указанному названию
		/// </summary>
		/// <param name="root">Используемый корневой элемент XmlReader-а</param>
		/// <param name="name">Название элементов</param>
		/// <param name="end">Метод вызываемый по окончании выборки</param>
		/// <returns>Итератор элементов XmlReader-а</returns>
		public static XmlReaderEnumerator ToSubtreeXmlReaderEnumerator( this XmlReader root,string name,Action end = null )
		{
			return root.ToSubtreeXmlReaderEnumerator( (item) => { return item != null && item.LocalName.Equals( name ); },end );
		}
	}
}
