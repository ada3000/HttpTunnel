//-----------------------------------------------------------------------
// Cls.Prop.cs - общие описания и реализация именованного свойства
//
// Created by *** 07.10.2011
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;


namespace Lib.Base
{
	/// <summary>
	/// Интерфейс именованного свойства
	/// </summary>
	public interface IProp
	{
		/// <summary>
		/// Название
		/// </summary>
		string Name { get; set; }
		/// <summary>
		/// Значение
		/// </summary>
		object Value { get; set; }
	}

	/// <summary>
	/// Стандартная реализация интерфейса IProp
	/// </summary>
	public abstract class IPropImpl :
		IProp
	{
		/// <summary>
		/// Название
		/// </summary>
		public virtual string Name { get; set; }
		/// <summary>
		/// Значение
		/// </summary>
		public virtual object Value { get; set; }

		/// <summary>
		/// Конструктор пустого объекта
		/// </summary>
		public IPropImpl() {}
		/// <summary>
		/// Конструктор с инициализацией параметров по интерфейсу
		/// </summary>
		/// <param name="prop">Интерфейс именованного свойства</param>
		public IPropImpl( IProp prop )
		{
			if(prop != null)
			{
				Name = prop.Name;
				Value = prop.Value;
			}
		}
		/// <summary>
		/// Конструктор с инициализацией параметров из переданных значений
		/// </summary>
		/// <param name="name">Название</param>
		/// <param name="value">Значение</param>
		public IPropImpl( string name,object value )
		{
			Name = name;
			Value = value;
		}
		/// <summary>
		/// Конструктор с инициализацией параметров из KeyValuePair
		/// </summary>
		/// <param name="prop">Значение KeyValuePair</param>
		public IPropImpl( KeyValuePair<string,object> prop )
		{
			Name = prop.Key;
			Value = prop.Value;
		}
	}

	/// <summary>
	/// Стандартная реализация объекта именованного свойства
	/// </summary>
	public sealed class Prop : IPropImpl,
		ICloneable
	{
		/// <summary>
		/// Конструктор пустого объекта
		/// </summary>
		public Prop() {}
		/// <summary>
		/// Конструктор с инициализацией параметров по интерфейсу
		/// </summary>
		/// <param name="prop">Интерфейс именованного свойства</param>
		public Prop( IProp prop ) : base(prop) {}
		/// <summary>
		/// Конструктор с инициализацией параметров из переданных значений
		/// </summary>
		/// <param name="name">Название</param>
		/// <param name="value">Значение</param>
		public Prop( string name,object value ) : base(name,value) {}
		/// <summary>
		/// Конструктор с инициализацией параметров из KeyValuePair
		/// </summary>
		/// <param name="prop">Значение KeyValuePair</param>
		public Prop( KeyValuePair<string,object> prop ) : base(prop) {}

		/// <summary>
		/// Создание копии объекта
		/// </summary>
		/// <returns>Копия объекта</returns>
		object ICloneable.Clone() { return new Prop(Name,Value); }
	}
}
