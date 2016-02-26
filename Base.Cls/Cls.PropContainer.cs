//-----------------------------------------------------------------------
// Cls.PropContainer.cs - реализация контейнера именованных свойств
//
// Created by *** 07.10.2011
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;


namespace Lib.Base
{
	/// <summary>
	/// Реализация контейнера именованных свойств, основанная на использовании объекта Dictionary
	/// </summary>
	public sealed class PropContainer :
		ICloneable
	{
		/// <summary>
		/// Именованные свойства
		/// </summary>
		public Dictionary<string,object> Dictionary { get; private set; }

		/// <summary>
		/// Конструктор пустого объекта
		/// </summary>
		public PropContainer()
		{
			Dictionary = new Dictionary<string,object>();
		}
		/// <summary>
		/// Конструктор с инициализацией из массивов названий и значений
		/// </summary>
		/// <param name="names">Список названий</param>
		/// <param name="values">Список значений</param>
		public PropContainer( string[] names,object[] values )
		{
			if(names == null || names.Length == 0)
				Dictionary = new Dictionary<string,object>();
			else
			{
				Dictionary = new Dictionary<string,object>(names.Length);
				Assign( names,values );
			}
		}
		/// <summary>
		/// Конструктор с инициализацией из единичной пары названия и значения
		/// </summary>
		/// <param name="name">Название</param>
		/// <param name="value">Значение</param>
		public PropContainer( string name,object value )
		{
			if(name.IsNoValue())
				Dictionary = new Dictionary<string,object>();
			else
			{
				Dictionary = new Dictionary<string,object>(1);
				Dictionary[name] = value;
			}
		}
		/// <summary>
		/// Конструктор с инициализацией из объекта Dictionary
		/// </summary>
		/// <param name="props">Словарь именованных свойств</param>
		/// <param name="copy">Признак копирования, true - создать копию, false - поместить в контенер по ссылке</param>
		public PropContainer( Dictionary<string,object> props,bool copy = true )
		{
			if(props == null)
				Dictionary = new Dictionary<string,object>();
			else if(copy)
				Dictionary = new Dictionary<string,object>(props);
			else
				Dictionary = props;
		}
		/// <summary>
		/// Конструктор с инициализации из объекта KeyValuePair
		/// </summary>
		/// <param name="prop">Пара ключ/значение</param>
		public PropContainer( KeyValuePair<string,object> prop )
		{
			Dictionary = new Dictionary<string,object>(1);
			Dictionary[prop.Key] = prop.Value;
		}
		/// <summary>
		/// Конструктор с инициализацией из списка объектов именованных свойств
		/// </summary>
		/// <param name="props">Список объектов именованных свойств</param>
		public PropContainer( List<IProp> props )
		{
			if(props == null || props.Count == 0)
				Dictionary = new Dictionary<string,object>();
			else
			{
				Dictionary = new Dictionary<string,object>(props.Count);
				Assign( props );
			}
		}
		/// <summary>
		/// Конструктор с инициализацией из единичного именованного свойства
		/// </summary>
		/// <param name="prop">Интерфейс именованного свойства</param>
		public PropContainer( IProp prop )
		{
			if(prop == null || prop.Name.IsNoValue())
				Dictionary = new Dictionary<string,object>();
			else
			{
				Dictionary = new Dictionary<string,object>(1);
				Dictionary[prop.Name] = prop.Value;
			}
		}

		/// <summary>
		/// Получить значение по названию
		/// </summary>
		/// <param name="name">Название</param>
		/// <returns>Значение</returns>
		public object this[string name]
		{
			get
			{
				object value = null;
				return name.IsValue() && Dictionary.TryGetValue( name,out value ) ? value : null;
			}
			set
			{
				if(name.IsValue()) Dictionary[name] = value;
			}
		}
		
		/// <summary>
		/// Получить массив значение для указанного набора названий свойств
		/// </summary>
		/// <param name="names">Список названий свойств</param>
		/// <returns>Список значений</returns>
		public object[] GetValues( string[] names )
		{
			// 10М итераций на массивах по 2 элемента ~4c
			//values = Array.ConvertAll( names, ( name ) => { return GetValue( name ); } );
			// 10М итераций на массивах по 2 элемента ~1,4c
			//values = names.Select( ( name ) => { return GetValue( name ); } ).ToArray();
			// 10М итераций на массивах по 2 элемента ~1,2c
			int count = names == null ? 0 : names.Length;
			object[] values = count > 0 ? new object[count] : null;
			for(int i = 0; i < count; i++)
				values[i] = this[names[i]];
			return values;
		}

		/// <summary>
		/// Установить значения, если отстутствуют добавить в контейнер
		/// </summary>
		/// <param name="props">Устанавливаемые значения</param>
		public void Assign( PropContainer props )
		{
			if(props != null) Assign( props.Dictionary );
		}
		/// <summary>
		/// Установить значения, если отстутствуют добавить в контейнер
		/// </summary>
		/// <param name="props">Устанавливаемые значения</param>
		public void Assign( Dictionary<string,object> props )
		{
			if(props != null)
			{
				foreach(var prop in props)
					Dictionary[prop.Key] = prop.Value;
			}
		}
		/// <summary>
		/// Установить значения, если отстутствуют добавить в контейнер
		/// </summary>
		/// <param name="props">Устанавливаемые значения</param>
		public void Assign( List<IProp> props )
		{
			for(int i = 0, count = props == null ? 0 : props.Count; i < count; i++)
				Assign( props[i] );
		}
		/// <summary>
		/// Установить значения, если отстутствуют добавить в контейнер
		/// </summary>
		/// <param name="names">Список названий</param>
		/// <param name="values">Список значений</param>
		public void Assign( string[] names,object[] values )
		{
			if(names != null)
			{
				int countValues = values == null ? 0 : values.Length;
				for(int i = 0, count = names.Length; i < count; i++)
					Assign( names[i],i < countValues ? values[i] : null );
			}
		}
		/// <summary>
		/// Установить значение, если отстутствует добавить в контейнер
		/// </summary>
		/// <param name="name">Название</param>
		/// <param name="value">Значение</param>
		public void Assign( string name,object value )
		{
			if(name.IsValue()) Dictionary[name] = value;
		}
		/// <summary>
		/// Установить значение, если отстутствует добавить в контейнер
		/// </summary>
		/// <param name="prop">Устанавливаемое значение</param>
		public void Assign( KeyValuePair<string,object> prop )
		{
			Dictionary[prop.Key] = prop.Value;
		}
		/// <summary>
		/// Установить значение, если отстутствует добавить в контейнер
		/// </summary>
		/// <param name="prop">Устанавливаемое значение</param>
		public void Assign( IProp prop )
		{
			if(prop != null && prop.Name.IsValue())
				Dictionary[prop.Name] = prop.Value;
		}

		/// <summary>
		/// Установить соответсвующие в контейнере значения
		/// </summary>
		/// <param name="props">Устанавливаемые значения</param>
		/// <param name="replace">true - заменинть на переданные</param>
		public void Set( PropContainer props,bool replace = false )
		{
			if(props != null)
				Set( props.Dictionary,replace );
			else if(replace)
				Dictionary.Clear();
		}
		/// <summary>
		/// Установить соответсвующие в контейнере значения
		/// </summary>
		/// <param name="props">Устанавливаемые значения</param>
		/// <param name="replace">true - заменинть на переданные</param>
		public void Set( Dictionary<string,object> props,bool replace = false )
		{
			if(props != null && props.Count > 0)
			{
				if(replace)
					Dictionary = new Dictionary<string,object>(props);
				else
				{
					foreach(var prop in props)
						if(Dictionary.ContainsKey( prop.Key ))
							Dictionary[prop.Key] = prop.Value;
				}
			}
			else if(replace)
				Dictionary.Clear();
		}
		/// <summary>
		/// Установить соответсвующие в контейнере значения
		/// </summary>
		/// <param name="props">Устанавливаемые значения</param>
		/// <param name="replace">true - заменинть на переданные</param>
		public void Set( List<IProp> props,bool replace = false )
		{
			if(props != null && props.Count > 0)
			{
				if(replace)
				{
					Dictionary = new Dictionary<string,object>(props.Count);
					Assign( props );
				}
				else
				{
					for(int i = 0, count = props.Count; i < count; i++)
						Set( props[i] );
				}
			}
			else if(replace)
				Dictionary.Clear();
		}
		/// <summary>
		/// Установить соответсвующие в контейнере значения
		/// </summary>
		/// <param name="names">Список названий</param>
		/// <param name="values">Список значений</param>
		/// <param name="replace">true - заменинть на переданные</param>
		public void Set( string[] names,object[] values,bool replace = false )
		{
			if(replace)
			{
				if(names == null || names.Length == 0)
					Dictionary.Clear();
				else
				{
					Dictionary = new Dictionary<string,object>(names.Length);
					Assign( names,values );
				}
			}
			else if(names != null)
			{
				int countValues = values == null ? 0 : values.Length;
				for(int i = 0, count = names.Length; i < count; i++)
					Set( names[i],i < countValues ? values[i] : null );
			}
		}
		/// <summary>
		/// Установить соответсвующее в контейнере значения
		/// </summary>
		/// <param name="name">Название</param>
		/// <param name="value">Значение</param>
		public void Set( string name,object value )
		{
			if(name.IsValue() && Dictionary.ContainsKey( name )) Dictionary[name] = value;
		}
		/// <summary>
		/// Установить соответсвующее в контейнере значения
		/// </summary>
		/// <param name="prop">Устанавливаемое значение</param>
		public void Set( KeyValuePair<string,object> prop )
		{
			if(Dictionary.ContainsKey( prop.Key )) Dictionary[prop.Key] = prop.Value;
		}
		/// <summary>
		/// Установить соответсвующее в контейнере значения
		/// </summary>
		/// <param name="prop">Устанавливаемое значение</param>
		public void Set( IProp prop )
		{
			if(prop != null && Dictionary.ContainsKey( prop.Name )) Dictionary[prop.Name] = prop.Value;
		}

		/// <summary>
		/// Очистить содержимое контейнера
		/// </summary>
		public void Clear() { Dictionary.Clear(); }

		#region ICloneable members

		/// <summary>
		/// Создание копии объекта
		/// </summary>
		/// <returns>Копия объекта</returns>
		object ICloneable.Clone() { return new PropContainer(Dictionary); }

		#endregion ICloneable members
	}
}
