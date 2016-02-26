//-----------------------------------------------------------------------
// Cls.PropHepler.cs - вспомогательные методы для классов именованных свойств
//
// Created by *** 10.10.2011
//-----------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;


namespace Lib.Base
{
	/// <summary>
	/// Класс вспомогательных методов для классов и интерфейсов Prop*
	/// </summary>
	public static class PropHelper
	{
		/// <summary>
		/// Исключить из словаря ключ-значение указаное подмножество ключей
		/// </summary>
		/// <typeparam name="TKey">Тип ключа</typeparam>
		/// <typeparam name="TValue">Тип значения</typeparam>
		/// <param name="props">Исходный словарь ключ-значение</param>
		/// <param name="names">Подмножество исключаемых ключей</param>
		/// <returns>Полученный словарь</returns>
		public static Dictionary<TKey,TValue> Exclude<TKey,TValue>( Dictionary<TKey,TValue> props,TKey[] names )
		{
			Dictionary<TKey,TValue> res = null;

			int count = props == null ? 0 : props.Count;
			int countNames = names == null ? 0 : names.Length;
			if(count > 0 && countNames > 0)
			{
				res = new Dictionary<TKey,TValue>(count);
				foreach(var prop in props)
					if(!names.Contains( prop.Key ))
						res.Add( prop.Key,prop.Value );
			}
			else if(props != null)
				res = new Dictionary<TKey,TValue>(props);
			else
				res = new Dictionary<TKey,TValue>();

			return res;
		}
		/// <summary>
		/// Исключить из списка именованный свойств указанное подмножество по именам
		/// </summary>
		/// <param name="props">Исходный список именованных свойств</param>
		/// <param name="names">Подмножество исключаемых имён</param>
		/// <returns>Полученный список</returns>
		public static List<IProp> Exclude( List<IProp> props,string[] names )
		{
			List<IProp> res = null;

			int count = props == null ? 0 : props.Count;
			int countNames = names == null ? 0 : names.Length;
			if(count > 0 && countNames > 0)
				res = props.Where( ( prop ) => { return prop != null && !names.Contains( prop.Name ); } ).ToList();
			else if(props != null)
				res = new List<IProp>(props);
			else
				res = new List<IProp>();

			return res;
		}

		/// <summary>
		/// Установить в словаре ключ-значение подмножество значений
		/// </summary>
		/// <typeparam name="TKey">Тип ключа</typeparam>
		/// <typeparam name="TValue">Тип значения</typeparam>
		/// <param name="dictionary">Исходный словарь ключ-значение</param>
		/// <param name="keys">Набор ключей</param>
		/// <param name="values">Набор значений</param>
		/// <returns>Полученный словарь ключ-значение</returns>
		public static Dictionary<TKey,TValue> Set<TKey,TValue>( this Dictionary<TKey,TValue> dictionary,TKey[] keys,TValue[] values )
		{
			if(dictionary != null)
			{
				dictionary.Clear();
				if(keys != null && values != null)
				{
					for(int i = 0,count = keys.Length; i < count; i++)
						dictionary[keys[i]] = values[i];
				}
			}

			return dictionary;
		}

		/// <summary>
		/// Получить словарь, в котором ключи и значения поменяны местами
		/// </summary>
		/// <typeparam name="T">Тип ключей и значений</typeparam>
		/// <param name="source">Исходный массив</param>
		/// <returns>Полученный словарь ключ-значение</returns>
		public static Dictionary<T,T> ReverseKeyValue<T>( this Dictionary<T,T> source )
		{
			if(source == null) return null;
			Dictionary<T,T> reverse = new Dictionary<T,T>(source.Count);
			foreach(var item in source)
				reverse[item.Value] = item.Key;
			return reverse;
		}

		/// <summary>
		/// Переименовать элементы в списке именованых свойств на основе набора соответсвий имён
		/// </summary>
		/// <param name="props">Исходный список именованных свойств</param>
		/// <param name="map">Набор соответствия имён</param>
		/// <param name="alwaysCopy">Флаг обязательности создания копии списка, даже в случае отсутствия набора соответствий имён</param>
		/// <returns>Полученный список именованных свойств</returns>
		public static PropContainer Remap( this PropContainer props,Dictionary<string,string> map,bool alwaysCopy = false )
		{
			PropContainer remap;
			if(props != null && props.Dictionary.Count > 0 && map != null && map.Count > 0)
			{
				remap = new PropContainer();
				foreach(var prop in props.Dictionary)
					remap[map.ContainsKey(prop.Key) ? map[prop.Key] : prop.Key] = prop.Value;
			}
			else if(alwaysCopy && props != null)
				remap = new PropContainer(props.Dictionary);
			else
				remap = props;

			return remap;
		}

		/// <summary>
		/// Преобразовать XML в список именованных свойств
		/// </summary>
		/// <param name="xml">Исходный XML</param>
		/// <param name="asElements">Способ преобразования: значения дочерних элементов - true, значения атрибутов - false</param>
		/// <returns>Полученый список именованных свойств</returns>
		public static Dictionary<string,object> ToDictionary( this XmlReader xml,bool asElements = false )
		{
			var res = new Dictionary<string,object>();

			if(xml != null)
			{
				xml.MoveToContent();
				if(asElements)
				{
					if(xml.NodeType == XmlNodeType.Element && xml.Read())
					{
						while(xml.NodeType == XmlNodeType.Element)// && xml.ReadState == ReadState.Interactive)
							//res[xml.Name] = xml.ReadInnerXml();
							res[xml.Name] = xml.ReadElementString();
					}
				}
				else
				{
					for(bool next = xml.MoveToFirstAttribute(); next; next = xml.MoveToNextAttribute())
						res[xml.Name] = xml.Value;
				}
			}

			return res;
		}

		/// <summary>
		/// Записать список именованных свойств в XML как атрибуты текущего элемента
		/// </summary>
		/// <param name="xml">Создаваемый XML</param>
		/// <param name="props">Список именованых свойств</param>
		/// <returns>Созданный XML</returns>
		public static XmlWriter WriteAttributes( this XmlWriter xml,Dictionary<string,object> props )
		{
			if(xml != null && props != null)
				foreach(var prop in props)
					xml.WriteAttributeString( prop.Key,prop.Value.ToXmlValue() );
			return xml;
		}
		/// <summary>
		/// Записать список именованных свойств в XML как атрибуты нового элемента
		/// </summary>
		/// <param name="xml">Создаваемый XML</param>
		/// <param name="elemName">Название создаваемого элемента</param>
		/// <param name="props">Список именованых свойств</param>
		/// <returns>Созданный XML</returns>
		public static XmlWriter WriteAttributes( this XmlWriter xml,string elemName,Dictionary<string,object> props )
		{
			if(xml != null)
			{
				xml.WriteStartElement( elemName );
				xml.WriteAttributes( props ).WriteEndElement();
			}
			return xml;
		}
		/// <summary>
		/// Записать список именованных свойств в XML как атрибуты текущего элемента
		/// </summary>
		/// <param name="xml">Создаваемый XML</param>
		/// <param name="props">Список именованых свойств</param>
		/// <returns>Созданный XML</returns>
		public static XmlWriter WriteAttributes( this XmlWriter xml,List<IProp> props )
		{
			if(xml != null && props != null)
				for(int i = 0,count = props.Count; i < count; i++)
				{
					IProp prop = props[i];
					if(prop != null)
						xml.WriteAttributeString( prop.Name,prop.Value.ToXmlValue() );
				}
			return xml;
		}
		/// <summary>
		/// Записать список именованных свойств в XML как атрибуты нового элемента
		/// </summary>
		/// <param name="xml">Создаваемый XML</param>
		/// <param name="elemName">Название создаваемого элемента</param>
		/// <param name="props">Список именованых свойств</param>
		/// <returns>Созданный XML</returns>
		public static XmlWriter WriteAttributes( this XmlWriter xml,string elemName,List<IProp> props )
		{
			if(xml != null)
			{
				xml.WriteStartElement( elemName );
				xml.WriteAttributes( props ).WriteEndElement();
			}
			return xml;
		}
		/// <summary>
		/// Записать список именованных свойств в XML как дочерние элементы текущего элемента
		/// </summary>
		/// <param name="xml">Создаваемый XML</param>
		/// <param name="props">Список именованых свойств</param>
		/// <returns>Созданный XML</returns>
		public static XmlWriter WriteElements( this XmlWriter xml,Dictionary<string,object> props )
		{
			if(xml != null && props != null)
				foreach(var prop in props)
					xml.WriteElementString( prop.Key,prop.Value.ToXmlValue() );
			return xml;
		}
		/// <summary>
		/// Записать список именованных свойств в XML как дочерние элементы нового элемента
		/// </summary>
		/// <param name="xml">Создаваемый XML</param>
		/// <param name="elemRoot">Название создаваемого родительского элемента</param>
		/// <param name="props">Список именованых свойств</param>
		/// <returns>Созданный XML</returns>
		public static XmlWriter WriteElements( this XmlWriter xml,string elemRoot,Dictionary<string,object> props )
		{
			if(xml != null)
			{
				xml.WriteStartElement( elemRoot );
				xml.WriteElements( props ).WriteEndElement();
			}
			return xml;
		}
		/// <summary>
		/// Записать список именованных свойств в XML как дочерние элементы текущего элемента
		/// </summary>
		/// <param name="xml">Создаваемый XML</param>
		/// <param name="props">Список именованых свойств</param>
		/// <returns>Созданный XML</returns>
		public static XmlWriter WriteElements( this XmlWriter xml,List<IProp> props )
		{
			if(xml != null && props != null)
				for(int i = 0,count = props.Count; i < count; i++)
				{
					IProp prop = props[i];
					if(prop != null)
						xml.WriteElementString( prop.Name,prop.Value.ToXmlValue() );
				}
			return xml;
		}
		/// <summary>
		/// Записать список именованных свойств в XML как дочерние элементы нового элемента
		/// </summary>
		/// <param name="xml">Создаваемый XML</param>
		/// <param name="elemRoot">Название создаваемого родительского элемента</param>
		/// <param name="props">Список именованых свойств</param>
		/// <returns>Созданный XML</returns>
		public static XmlWriter WriteElements( this XmlWriter xml,string elemRoot,List<IProp> props )
		{
			if(xml != null)
			{
				xml.WriteStartElement( elemRoot );
				xml.WriteElements( props ).WriteEndElement();
			}
			return xml;
		}
		/// <summary>
		/// Преобразовать XML в контейнер именованных свойств
		/// </summary>
		/// <param name="xml">Исходный XML</param>
		/// <param name="asElements">Способ разбора, true - список элементов, false - список атрибутов</param>
		/// <returns>Созданный контейнер именованных свойств</returns>
		public static PropContainer ToPropContainer( this XmlReader xml,bool asElements = false )
		{
			return new PropContainer(xml.ToDictionary( asElements ),false);
		}

		/// <summary>
		/// Template метода вывода в консоль словаря
		/// </summary>
		/// <typeparam name="TKey">Тип ключа</typeparam>
		/// <typeparam name="TValue">Тип значения</typeparam>
		/// <param name="props">Словарь</param>
		/// <param name="nameDic">Название словаря</param>
		/// <param name="nameItem">Название элемента</param>
		public static void ToConsole<TKey,TValue>( this Dictionary<TKey,TValue> props,string nameDic,string nameItem )
		{
			if(props == null)
				Console.WriteLine( "{0} => null",nameDic );
			else
			{
				Console.WriteLine( "{0}.Count = {1}",nameDic,props.Count );
				foreach(var prop in props)
					Console.WriteLine( "{0}[{1}]='{2}'",nameItem,prop.Key,prop.Value );
			}
		}
		/// <summary>
		/// Вывести контейнер именованных свойств в консоль
		/// </summary>
		/// <param name="props">Контейнер именованных свойств</param>
		/// <param name="nameContainer">Название контейнера</param>
		/// <param name="nameItem">Название элемента</param>
		public static void ToConsole( this PropContainer props,string nameContainer,string nameItem )
		{
			props.Dictionary.ToConsole( nameContainer,nameItem );
		}

		/// <summary>
		/// Создать объект именованного свойства из строки
		/// </summary>
		/// <param name="src">строка в формате: name[sepSet][sepValueBegin]value[sepValueEnd]</param>
		/// <param name="sepSet">Разделитель названия и значения</param>
		/// <param name="tagValueBegin">Начальный tag значения</param>
		/// <param name="tagValueEnd">Конечный tag значения</param>
		/// <param name="outerValue">Флаг выдачи подстроки с tag-ами</param>
		/// <param name="compType">Опции сравнения строк</param>
		/// <returns>Разобранный объект или null для пустой строки</returns>
		public static Prop ToProp( this string src,string sepSet = "=",string tagValueBegin = "\"",string tagValueEnd = "\"",bool outerValue = false,StringComparison compType = StringComparison.InvariantCultureIgnoreCase )
		{
			Prop prop = null;

			if(src.IsValue())
			{
				int indSet = src.IndexOf( sepSet,compType );
				prop = indSet < 0 ? new Prop(src,null) :
					new Prop(
						src.Substring( 0,indSet ).Trim(),
						src.Substring( tagValueBegin.IfNull( sepSet ),tagValueEnd,null,outerValue,compType ).IfNull( string.Empty ).Trim());
			}

			return prop;
		}

		#region расширение методов PropContainer

		/// <summary>
		/// Получить значение из контейнера именованных свойств
		/// </summary>
		/// <param name="props">Контейнер именованных свойств</param>
		/// <param name="name">Название свойства</param>
		/// <returns>Значение свойства</returns>
		public static object GetValue( this PropContainer props,string name )
		{
			return props == null ? null : props[name];
		}
		/// <summary>
		/// Получить значение из контейнера именованных свойств в виде строки
		/// </summary>
		/// <param name="props">Контейнер именованных свойств</param>
		/// <param name="name">Название свойства</param>
		/// <returns>Значение свойства</returns>
		public static string GetString( this PropContainer props,string name )
		{
			object val = props.GetValue( name );
			return val.IsNoValue() ? null : val.ToString();
		}

		/// <summary>
		/// Исключить из контейнера именованный свойств указанное подмножество по именам
		/// </summary>
		/// <param name="props">Исходный контейнер именованных свойств</param>
		/// <param name="names">Подмножество исключаемых имён</param>
		/// <returns>Полученный контейнер</returns>
		public static PropContainer Exclude( this PropContainer props,string[] names )
		{
			return props == null ? new PropContainer() : new PropContainer(Exclude( props.Dictionary,names ),false);
		}

		/// <summary>
		/// Записать контейнер именованных свойств в XML
		/// </summary>
		/// <param name="props">Контейнер именованных свойств</param>
		/// <param name="xml">Создаваемый XML</param>
		/// <param name="asElements">Способ записи в XML, true - список элементов, false - список атрибутов</param>
		/// <returns>Созданный XML</returns>
		public static XmlWriter WriteToXml( this PropContainer props,XmlWriter xml,bool asElements = false )
		{
			return	props == null ? xml :
					asElements ? xml.WriteElements( props.Dictionary ) :
					xml.WriteAttributes( props.Dictionary );
		}
		/// <summary>
		/// Записать контейнер именованных свойств в XML в новый родительский элемент
		/// </summary>
		/// <param name="props">Контейнер именованных свойств</param>
		/// <param name="xml">Создаваемый XML</param>
		/// <param name="elemName">Название создаваемого родительского элемента</param>
		/// <param name="asElements">Способ записи в XML, true - список элементов, false - список атрибутов</param>
		/// <returns>Созданный XML</returns>
		public static XmlWriter WriteToXml( this PropContainer props,XmlWriter xml,string elemName,bool asElements = false )
		{
			return	props == null ? xml :
					asElements ? xml.WriteElements( elemName,props.Dictionary ) :
					xml.WriteAttributes( elemName,props.Dictionary );
		}
		/// <summary>
		/// Преобразовать контейнер именованных свойств в XML-строку
		/// </summary>
		/// <param name="props">Контейнер именованных свойств</param>
		/// <param name="rootName">Название корневого элемента</param>
		/// <param name="asElements">Способ записи в XML, true - список элементов, false - список атрибутов</param>
		/// <returns>Созданная XML-строка</returns>
		public static string ToXmlString( this PropContainer props,string rootName,bool asElements = false )
		{
			StringBuilder str = new StringBuilder();
			props.WriteToXml( str.ToXmlWriter( ConformanceLevel.Fragment ),rootName,asElements ).Close();
			return str.ToString();
		}

		#endregion расширение методов PropContainer

		#region вспомогательные методы для IProp

		/// <summary>
		/// Получить название именованного свойства
		/// </summary>
		/// <param name="prop">Именованное свойство</param>
		/// <returns>Название свойства</returns>
		public static string GetName( this IProp prop )
		{
			return prop == null ? null : prop.Name;
		}
		/// <summary>
		/// Получить значение именованного свойства
		/// </summary>
		/// <param name="prop">Именованное свойство</param>
		/// <returns>Значение свойства</returns>
		public static object GetValue( this IProp prop )
		{
			return prop == null ? null : prop.Value;
		}
		/// <summary>
		/// Получить значение именованного свойства в виде строки
		/// </summary>
		/// <param name="prop">Именованное свойство</param>
		/// <returns>Значение свойства</returns>
		public static object GetString( this IProp prop )
		{
			object val = prop.GetValue();
			return val.IsNoValue() ? null : val.ToString();
		}

		#endregion вспомогательные методы для IProp
	}
}
