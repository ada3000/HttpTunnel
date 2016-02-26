//-----------------------------------------------------------------------
// Cls.ObjSimple.cs - общие описания и реализация "простейшего" объекта
//
// Created by *** 09.09.2010
//-----------------------------------------------------------------------
using System;


namespace Lib.Base
{
	/// <summary>
	/// Интерфейс "простейшего" объекта
	/// </summary>
	public interface IObjSimple
	{
		/// <summary>
		/// Идентификатор
		/// </summary>
		object ID { get; set; }
		/// <summary>
		/// Название
		/// </summary>
		string Name { get; set; }
	}

	/// <summary>
	/// Стандартная реализация интерфейса IObjSimple
	/// </summary>
	public abstract class IObjSimpleImpl :
		IObjSimple
	{
		/// <summary>
		/// Идентификатор
		/// </summary>
		public virtual object ID { get; set; }
		/// <summary>
		/// Название
		/// </summary>
		public virtual string Name { get; set; }

		/// <summary>
		/// Конструктор пустого объекта
		/// </summary>
		public IObjSimpleImpl() {}
		/// <summary>
		/// Конструктор с инициализацией параметров по интерфейсу
		/// </summary>
		/// <param name="obj">Интерфейс "простейшего" объекта</param>
		public IObjSimpleImpl( IObjSimple obj )
		{
			if(obj != null)
			{
				ID = obj.ID;
				Name = obj.Name;
			}
		}
		/// <summary>
		/// Конструктор с инициализацией параметров из переданных значений
		/// </summary>
		/// <param name="id">Идентификатор</param>
		/// <param name="name">Название</param>
		public IObjSimpleImpl( object id,string name )
		{
			ID = id;
			Name = name;
		}
	}

	/// <summary>
	/// Стандартная реализация "простейшего" объекта
	/// </summary>
	public sealed class ObjSimple : IObjSimpleImpl,
		ICloneable
	{
		/// <summary>
		/// Конструктор пустого объекта
		/// </summary>
		public ObjSimple() {}
		/// <summary>
		/// Конструктор с инициализацией параметров по интерфейсу
		/// </summary>
		/// <param name="obj">Интерфейс "простейшего" объекта</param>
		public ObjSimple( IObjSimple obj ) : base(obj) {}
		/// <summary>
		/// Конструктор с инициализацией параметров из переданных значений
		/// </summary>
		/// <param name="id">Идентификатор</param>
		/// <param name="name">Название</param>
		public ObjSimple( object id,string name ) : base(id,name) {}

		/// <summary>
		/// Переопределение преобразования к строке для возврата названия
		/// </summary>
		/// <returns>Название</returns>
		public override string ToString() { return Name; }

		/// <summary>
		/// Создание копии объекта
		/// </summary>
		/// <returns>Копия объекта</returns>
		object ICloneable.Clone() { return new ObjSimple(ID,Name); }
	}
}
