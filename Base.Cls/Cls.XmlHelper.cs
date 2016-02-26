//-----------------------------------------------------------------------
// Cls.XmlHelper.cs - описание вспомогательных методов для работы с XML
//
// Created by *** 01.03.2012
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;


namespace Lib.Base
{
	/// <summary>
	/// Логические операции
	/// </summary>
	public enum LogicalOperation
	{
		/// <summary>
		/// Операция логического "И"
		/// </summary>
		And = 0,
		/// <summary>
		/// Операция логического "ИЛИ"
		/// </summary>
		Or = 1,
		/// <summary>
		/// Операция логического "НЕ"
		/// </summary>
		Not = 2
	}

	/// <summary>
	/// Вспомогательные методы для работы с XML
	/// </summary>
	public static class XmlHelper
	{
		/// <summary>
		/// Значения параметров по умолчанию для операций сериализации объектов
		/// </summary>
		private static class SerializeDefault
		{
			/// <summary>
			/// Значения по умолчанию для используемых пространств имён
			/// </summary>
			public static readonly XmlSerializerNamespaces XmlSerializerNamespaces =
				new XmlSerializerNamespaces(new XmlQualifiedName[]{ new XmlQualifiedName() });

			/// <summary>
			/// Значения по умолчанию для настроек XmlWriter-а
			/// </summary>
			public static readonly XmlWriterSettings XmlWriterSettings =
				new XmlWriterSettings(){
					Indent = true,
					IndentChars = "\t",
					OmitXmlDeclaration = true, // убрать <?xml бла-бла-бла ?>
				};
		}

		/// <summary>
		/// Класс пула объектов сериализаторов
		/// </summary>
		private sealed class SerializerPool
		{
			private Dictionary<string,XmlSerializer> _pool = new Dictionary<string,XmlSerializer>();

			/// <summary>
			/// Получить объект сериализатора для указанного типа объектов
			/// </summary>
			/// <param name="type">Тип объекта</param>
			/// <returns>Сериализатор</returns>
			public XmlSerializer Query( Type type )
			{
				XmlSerializer serializer = null;
				string key = type.AssemblyQualifiedName;
				lock(_pool)
				{
					if(_pool.ContainsKey( key ))
						serializer = _pool[key];
					else
						_pool[key] = serializer = new XmlSerializer(type);
				}
				return serializer;
			}
			/// <summary>
			/// Получить объект сериализатора для указанного типа объектов c заданным коневым элементом
			/// </summary>
			/// <param name="type">Тип объекта</param>
			/// <param name="root">Название корневого элемента</param>
			/// <returns>Сериализатор</returns>
			public XmlSerializer Query( Type type,string root )
			{
				XmlSerializer serializer = null;
				string key = type.AssemblyQualifiedName + ":" + root;
				lock(_pool)
				{
					if(_pool.ContainsKey( key ))
						serializer = _pool[key];
					else
						_pool[key] = serializer = new XmlSerializer(type,new XmlRootAttribute(root));
				}
				return serializer;
			}
		}
		/// <summary>
		/// Пул объектов сериализаторов
		/// </summary>
		private static SerializerPool _serializers = new SerializerPool();

		#region XML десериализация на основе типа
		/*
		  Функции десериализации на основе шаблона не заменены на
		  функции на основе типа по причине быстродействия
		 */
		/// <summary>
		/// Получить объект из XML
		/// </summary>
		/// <param name="xml">Исходный XML</param>
		/// <param name="type">Тип для десериализации</param>
		/// <returns>Созданный объект</returns>
		public static object Deserialize(this XmlReader xml, Type type)
		{
			return xml.Deserialize(_serializers.Query(type));
		}
		/// <summary>
		/// Получить объект из XML используя указанный XmlSerializer
		/// </summary>
		/// <param name="xml">Исходный XML</param>
		/// <param name="serializer">Сконфигурированный XmlSerializer</param>
		/// <returns>Созданный объект</returns>
		public static object Deserialize(this XmlReader xml, XmlSerializer serializer)
		{
			var val = xml == null ? null : serializer.Deserialize(xml);
			return val;
		}
		/// <summary>
		/// Получить объект из XML с указаным корневым элементом
		/// </summary>
		/// <param name="xml">Исходный XML</param>
		/// <param name="root">Название корневого элемента</param>
		/// <param name="type">Тип для десериализации</param>
		/// <returns>Созданный объект</returns>
		public static object Deserialize(this XmlReader xml,Type type, string root)
		{
			return xml.Deserialize(_serializers.Query(type, root));
		}

		#endregion

		/// <summary>
		/// Получить объект из XML
		/// </summary>
		/// <param name="xml">Исходный XML</param>
		/// <returns>Созданный объект</returns>
		public static T Deserialize<T>( this XmlReader xml )
			where T : new()
		{
			return xml.Deserialize<T>( _serializers.Query( typeof(T) ) );
		}
		/// <summary>
		/// Получить объект из XML используя указанный XmlSerializer
		/// </summary>
		/// <typeparam name="T">Тип объекта</typeparam>
		/// <param name="xml">Исходный XML</param>
		/// <param name="serializer">Сконфигурированный XmlSerializer</param>
		/// <returns>Созданный объект</returns>
		public static T Deserialize<T>( this XmlReader xml,XmlSerializer serializer )
			where T : new()
		{
			var val = xml == null ? null : serializer.Deserialize( xml );
			return (T)(val ?? new T());
		}
		/// <summary>
		/// Получить объект из XML с указаным корневым элементом
		/// </summary>
		/// <typeparam name="T">Тип объекта</typeparam>
		/// <param name="xml">Исходный XML</param>
		/// <param name="root">Название корневого элемента</param>
		/// <returns>Созданный объект</returns>
		public static T Deserialize<T>( this XmlReader xml,string root )
			where T : new()
		{
			return xml.Deserialize<T>( _serializers.Query( typeof(T),root ) );
		}
		/// <summary>
		/// Получить массив объектов из XML используя указанный XmlSerializer
		/// </summary>
		/// <typeparam name="T">Тип объекта</typeparam>
		/// <param name="xml">Исходный XML</param>
		/// <param name="serializer">Сконфигурированный XmlSerializer</param>
		/// <returns>Созданный массив объектов</returns>
		public static T[] DeserializeArray<T>( this XmlReader xml,XmlSerializer serializer )
			where T : new()
		{
			var val = xml == null ? null : serializer.Deserialize( xml );
			return (T[])(val ?? new T[]{});
		}
		/// <summary>
		/// Получить массив объектов из XML с указаным корневым элементом
		/// </summary>
		/// <typeparam name="T">Тип объекта</typeparam>
		/// <param name="xml">Исходный XML</param>
		/// <param name="root">Название корневого элемента</param>
		/// <returns>Созданный массив объектов</returns>
		public static T[] DeserializeArray<T>( this XmlReader xml,string root )
			where T : new()
		{
			return xml.DeserializeArray<T>( _serializers.Query( typeof(T[]),root ) );
		}
		/// <summary>
		/// Получить массив объектов из XML с указаным корневым элементом
		/// </summary>
		/// <typeparam name="T">Тип объекта</typeparam>
		/// <param name="xml">Исходный XML</param>
		/// <returns>Созданный массив объектов</returns>
		public static T[] DeserializeArray<T>(this XmlReader xml)
			where T : new()
		{
			return xml.DeserializeArray<T>( _serializers.Query( typeof(T[]) ) );
		}

		/// <summary>
		/// Сериализовать объект в XML
		/// </summary>
		/// <typeparam name="T">Тип объекта</typeparam>
		/// <param name="writer">XmlWriter, в который записывать сериализацию объекта</param>
		/// <param name="obj">Сериализуемый объект</param>
		/// <param name="namespaces">Используемые пространства имён</param>
		/// <returns>XmlWriter, в который был записан объект</returns>
		public static XmlWriter Serialize<T>( this XmlWriter writer,T obj,XmlSerializerNamespaces namespaces = null )
		{
			return writer.Serialize( obj,_serializers.Query( typeof(T) ),namespaces );
		}
		/// <summary>
		/// Сериализовать объект в XML используя указанный XmlSerializer
		/// </summary>
		/// <typeparam name="T">Тип объекта</typeparam>
		/// <param name="writer">XmlWriter, в который записывать сериализацию объекта</param>
		/// <param name="obj">Сериализуемый объект</param>
		/// <param name="serializer">Сконфигурированный XmlSerializer</param>
		/// <param name="namespaces">Используемые пространства имён</param>
		/// <returns>XmlWriter, в который был записан объект</returns>
		public static XmlWriter Serialize<T>( this XmlWriter writer,T obj,XmlSerializer serializer,XmlSerializerNamespaces namespaces = null )
		{
			if(writer != null)
			{
				serializer.Serialize( writer,obj,namespaces ?? SerializeDefault.XmlSerializerNamespaces );
			}
			return writer;
		}
		/// <summary>
		/// Сериализовать объект в XML с указаным корневым элементом
		/// </summary>
		/// <typeparam name="T">Тип объекта</typeparam>
		/// <param name="writer">XmlWriter, в который записывать сериализацию объекта</param>
		/// <param name="obj">Сериализуемый объект</param>
		/// <param name="root">Название корневого элемента</param>
		/// <param name="namespaces">Используемые пространства имён</param>
		/// <returns>XmlWriter, в который был записан объект</returns>
		public static XmlWriter Serialize<T>( this XmlWriter writer,T obj,string root,XmlSerializerNamespaces namespaces = null )
		{
			return writer.Serialize( obj,_serializers.Query( typeof(T),root ),namespaces );
		}

		/// <summary>
		/// Сериализовать объект в XML-строку
		/// </summary>
		/// <typeparam name="T">Тип объекта</typeparam>
		/// <param name="obj">Сериализуемый объект</param>
		/// <param name="settings">Настройки XmlWriter-а</param>
		/// <param name="namespaces">Используемые пространства имён</param>
		/// <returns>XML-строка</returns>
		public static string ToXmlString<T>( this T obj,XmlWriterSettings settings = null,XmlSerializerNamespaces namespaces = null )
		{
			var builder = new StringBuilder();
			using(var writer = builder.ToXmlWriter( settings ?? SerializeDefault.XmlWriterSettings ))
			{
				writer.Serialize( obj,namespaces );
			}
			return builder.ToString();
		}
		/// <summary>
		/// Сериализовать объект в XML-строку используя указанный XmlSerializer
		/// </summary>
		/// <typeparam name="T">Тип объекта</typeparam>
		/// <param name="obj">Сериализуемый объект</param>
		/// <param name="serializer">Сконфигурированный XmlSerializer</param>
		/// <param name="settings">Настройки XmlWriter-а</param>
		/// <param name="namespaces">Используемые пространства имён</param>
		/// <returns>XML-строка</returns>
		public static string ToXmlString<T>( this T obj,XmlSerializer serializer,XmlWriterSettings settings = null,XmlSerializerNamespaces namespaces = null )
		{
			var builder = new StringBuilder();
			using(var writer = builder.ToXmlWriter( settings ?? SerializeDefault.XmlWriterSettings ))
			{
				writer.Serialize( obj,serializer,namespaces );
			}
			return builder.ToString();
		}
		/// <summary>
		/// Сериализовать объект в XML-строку с указаным корневым элементом
		/// </summary>
		/// <typeparam name="T">Тип объекта</typeparam>
		/// <param name="obj">Сериализуемый объект</param>
		/// <param name="root">Название корневого элемента</param>
		/// <param name="settings">Настройки XmlWriter-а</param>
		/// <param name="namespaces">Используемые пространства имён</param>
		/// <returns>XML-строка</returns>
		public static string ToXmlString<T>( this T obj,string root,XmlWriterSettings settings = null,XmlSerializerNamespaces namespaces = null )
		{
			return obj.ToXmlString( _serializers.Query( typeof(T),root ),settings,namespaces );
		}

		/// <summary>
		/// Переместить XmlReader на следующий элемент с указанным именем
		/// </summary>
		/// <param name="xml">Текущий XmlReader</param>
		/// <param name="elemName">Название элемента, на который необходимо переместиться</param>
		/// <returns>Новая позиция в XmlReader</returns>
		public static XmlReader MoveTo( this XmlReader xml,string elemName )
		{
			return xml != null && xml.ReadToFollowing( elemName ) ? xml : null;
		}

		/// <summary>
		/// Найти XML-элемент, соответствующий условиям
		/// </summary>
		/// <param name="xml">Исходный XML</param>
		/// <param name="elemName">Название элемента</param>
		/// <param name="attrName">Название атрибута</param>
		/// <param name="attrValue">Значение атрибута</param>
		/// <param name="ignoreCase">Флаг игнорирования регистра при сравнении</param>
		/// <returns>Найденный элемент или null</returns>
		public static XmlReader FindElement( this XmlReader xml,string elemName,string attrName,string attrValue,bool ignoreCase = true )
		{
			XmlReader node = null;
			if(xml != null)
			{
				while( (node = xml.MoveTo( elemName )) != null &&
					string.Compare( node.GetAttribute( attrName ) ?? string.Empty,attrValue,ignoreCase ) != 0 );
			}
			return node;
		}

		/// <summary>
		/// Получить строковое значение элемента
		/// </summary>
		/// <param name="xml">Исходный XML</param>
		/// <param name="elemName">Название элемента</param>
		/// <param name="def">Значение по умолчанию</param>
		/// <returns>Полученное значение</returns>
		public static string GetElementString( this XmlReader xml,string elemName,string def = null )
		{
			return (xml.MoveTo( elemName ) == null ? null : xml.ReadString()).IfNull( def );
		}
		/// <summary>
		/// Получить целочисленное значение типа int элемента
		/// </summary>
		/// <param name="xml">Исходный XML</param>
		/// <param name="elemName">Название элемента</param>
		/// <param name="def">Значение по умолчанию</param>
		/// <returns>Полученное значение</returns>
		public static int GetElementInt( this XmlReader xml,string elemName,int def = 0 )
		{
			string val = xml.GetElementString( elemName );
			return val.IsValue() ? Convert.ToInt32( val ) : def;
		}
		/// <summary>
		/// Получить целочисленное значение типа long элемента
		/// </summary>
		/// <param name="xml">Исходный XML</param>
		/// <param name="elemName">Название элемента</param>
		/// <param name="def">Значение по умолчанию</param>
		/// <returns>Полученное значение</returns>
		public static long GetElementLong( this XmlReader xml,string elemName,long def = 0 )
		{
			string val = xml.GetElementString( elemName );
			return val.IsValue() ? Convert.ToInt64( val ) : def;
		}
		/// <summary>
		/// Получить целочисленное значение типа bool элемента
		/// </summary>
		/// <param name="xml">Исходный XML</param>
		/// <param name="elemName">Название элемента</param>
		/// <param name="def">Значение по умолчанию</param>
		/// <returns>Полученное значение</returns>
		public static bool GetElementBool( this XmlReader xml,string elemName,bool def = false )
		{
			string val = xml.GetElementString( elemName );
			return val.IsValue() ? Convert.ToInt32( val ) != 0 : def;
		}

		/// <summary>
		/// Получить строковое значение элемента
		/// </summary>
		/// <param name="xml">Исходный XML</param>
		/// <param name="xPath">xPath путь элемента</param>
		/// <param name="def">Значение по умолчанию</param>
		/// <returns>Полученное значение</returns>
		public static string GetElementString( this XmlNode xml,string xPath,string def = null )
		{
			XmlNode node = xml.SelectSingleNode( xPath );
			return (node == null ? null : node.InnerText).IfNull( def );
		}

		/// <summary>
		/// Преобразовать значение в правильную xml-строку
		/// </summary>
		/// <param name="val">Исходное значение</param>
		/// <returns>Правильная xml-строка</returns>
		public static string ToXmlValue( this object val )
		{
			string res = null;

			if(val.IsValue())
			{
				if(Type.GetTypeCode( val.GetType() ) == TypeCode.DateTime)
					res = Convert.ToDateTime( val ).ToString( "s" );
				else
					res = Convert.ToString( val,CultureInfo.InvariantCulture );
			}
			return res;
		}

		/// <summary>
		/// Преобразовать в XmlReader
		/// </summary>
		/// <param name="xml">Исходный XML</param>
		/// <param name="settings">Настройки XmlReader-а</param>
		/// <returns>Созданный XmlReader</returns>
		public static XmlReader ToXmlReader( this TextReader xml,XmlReaderSettings settings = null )
		{
			if(settings == null)
			{
				settings = new XmlReaderSettings();
				settings.ConformanceLevel = ConformanceLevel.Fragment;
			}
			return XmlReader.Create( xml ?? string.Empty.ToReader(),settings );
		}
		/// <summary>
		/// Преобразовать в XmlReader
		/// </summary>
		/// <param name="xml">Исходный XML</param>
		/// <param name="lvl">Настройки ConformanceLevel</param>
		/// <returns>Созданный XmlReader</returns>
		public static XmlReader ToXmlReader( this TextReader xml,ConformanceLevel lvl )
		{
			XmlReaderSettings settings = new XmlReaderSettings();
			settings.ConformanceLevel = lvl;
			return xml.ToXmlReader( settings );
		}
		/// <summary>
		/// Преобразовать в XmlReader
		/// </summary>
		/// <param name="xml">Исходный XML</param>
		/// <param name="lvl">Настройки ConformanceLevel</param>
		/// <param name="resolver">Объект разрешающий внешние ссылки</param>
		/// <returns>Созданный XmlReader</returns>
		public static XmlReader ToXmlReader( this TextReader xml,ConformanceLevel lvl,XmlResolver resolver )
		{
			XmlReaderSettings settings = new XmlReaderSettings();
			settings.ConformanceLevel = lvl;
			settings.XmlResolver = resolver;
			return xml.ToXmlReader( settings );
		}

		/// <summary>
		/// Преобразовать в XmlReader
		/// </summary>
		/// <param name="xml">Исходный XML</param>
		/// <param name="settings">Настройки XmlReader-а</param>
		/// <returns>Созданный XmlReader</returns>
		public static XmlReader ToXmlReader( this string xml,XmlReaderSettings settings = null )
		{
			return xml.ToReader().ToXmlReader( settings );
		}
		/// <summary>
		/// Преобразовать в XmlReader
		/// </summary>
		/// <param name="xml">Исходный XML</param>
		/// <param name="lvl">Настройки ConformanceLevel</param>
		/// <returns>Созданный XmlReader</returns>
		public static XmlReader ToXmlReader( this string xml,ConformanceLevel lvl )
		{
			return xml.ToReader().ToXmlReader( lvl );
		}
		/// <summary>
		/// Преобразовать в XmlReader
		/// </summary>
		/// <param name="xml">Исходный XML</param>
		/// <param name="lvl">Настройки ConformanceLevel</param>
		/// <param name="resolver">Объект разрешающий внешние ссылки</param>
		/// <returns>Созданный XmlReader</returns>
		public static XmlReader ToXmlReader( this string xml,ConformanceLevel lvl,XmlResolver resolver )
		{
			return xml.ToReader().ToXmlReader( lvl,resolver );
		}

		/// <summary>
		/// Преобразовать в XmlReader
		/// </summary>
		/// <param name="input">Исходный поток, содержащий XML</param>
		/// <param name="settings">Настройки XmlReader-а</param>
		/// <returns>Созданный XmlReader</returns>
		public static XmlReader ToXmlReader( this Stream input,XmlReaderSettings settings = null )
		{
			return XmlReader.Create( input,settings ?? new XmlReaderSettings(){
				ConformanceLevel = ConformanceLevel.Fragment,
			} );
		}
		/// <summary>
		/// Преобразовать в XmlReader
		/// </summary>
		/// <param name="input">Исходный поток, содержащий XML</param>
		/// <param name="lvl">Настройки ConformanceLevel</param>
		/// <returns>Созданный XmlReader</returns>
		public static XmlReader ToXmlReader( this Stream input,ConformanceLevel lvl )
		{
			return input.ToXmlReader( new XmlReaderSettings(){
				ConformanceLevel = lvl,
			} );
		}
		/// <summary>
		/// Преобразовать в XmlReader
		/// </summary>
		/// <param name="input">Исходный поток, содержащий XML</param>
		/// <param name="lvl">Настройки ConformanceLevel</param>
		/// <param name="resolver">Объект разрешающий внешние ссылки</param>
		/// <returns>Созданный XmlReader</returns>
		public static XmlReader ToXmlReader( this Stream input,ConformanceLevel lvl,XmlResolver resolver )
		{
			return input.ToXmlReader( new XmlReaderSettings(){
				ConformanceLevel = lvl,
				XmlResolver = resolver,
			} );
		}

		/// <summary>
		/// Преобразовать в XmlDocument
		/// </summary>
		/// <param name="xml">Исходный XML</param>
		/// <returns>Созданный XmlDocument</returns>
		public static XmlDocument ToXmlDocument( this XmlReader xml )
		{
			XmlDocument xmlDoc = new XmlDocument();
			if(xml != null) xmlDoc.Load( xml );
			return xmlDoc;
		}

		/// <summary>
		/// Преобразовать в XmlDocument
		/// </summary>
		/// <param name="xml">Исходный XML</param>
		/// <param name="settings">Настройки XmlReader-а</param>
		/// <returns>Созданный XmlDocument</returns>
		public static XmlDocument ToXmlDocument( this TextReader xml,XmlReaderSettings settings = null )
		{
			return xml.ToXmlReader( settings ).ToXmlDocument();
		}
		/// <summary>
		/// Преобразовать в XmlDocument
		/// </summary>
		/// <param name="xml">Исходный XML</param>
		/// <param name="lvl">Настройки ConformanceLevel</param>
		/// <returns>Созданный XmlDocument</returns>
		public static XmlDocument ToXmlDocument( this TextReader xml,ConformanceLevel lvl )
		{
			return xml.ToXmlReader( lvl ).ToXmlDocument();
		}
		/// <summary>
		/// Преобразовать в XmlDocument
		/// </summary>
		/// <param name="xml">Исходный XML</param>
		/// <param name="lvl">Настройки ConformanceLevel</param>
		/// <param name="resolver">Объект разрешающий внешние ссылки</param>
		/// <returns>Созданный XmlDocument</returns>
		public static XmlDocument ToXmlDocument( this TextReader xml,ConformanceLevel lvl,XmlResolver resolver )
		{
			var xmlDoc = xml.ToXmlReader( lvl,resolver ).ToXmlDocument();
			xmlDoc.XmlResolver = resolver;
			return xmlDoc;
		}

		/// <summary>
		/// Преобразовать в XmlDocument
		/// </summary>
		/// <param name="xml">Исходный XML</param>
		/// <param name="settings">Настройки XmlReader-а</param>
		/// <returns>Созданный XmlDocument</returns>
		public static XmlDocument ToXmlDocument( this string xml,XmlReaderSettings settings = null )
		{
			return xml.ToXmlReader( settings ).ToXmlDocument();
		}
		/// <summary>
		/// Преобразовать в XmlDocument
		/// </summary>
		/// <param name="xml">Исходный XML</param>
		/// <param name="lvl">Настройки ConformanceLevel</param>
		/// <returns>Созданный XmlDocument</returns>
		public static XmlDocument ToXmlDocument( this string xml,ConformanceLevel lvl )
		{
			return xml.ToXmlReader( lvl ).ToXmlDocument();
		}
		/// <summary>
		/// Преобразовать в XmlDocument
		/// </summary>
		/// <param name="xml">Исходный XML</param>
		/// <param name="lvl">Настройки ConformanceLevel</param>
		/// <param name="resolver">Объект разрешающий внешние ссылки</param>
		/// <returns>Созданный XmlDocument</returns>
		public static XmlDocument ToXmlDocument( this string xml,ConformanceLevel lvl,XmlResolver resolver )
		{
			var xmlDoc = xml.ToXmlReader( lvl,resolver ).ToXmlDocument();
			xmlDoc.XmlResolver = resolver;
			return xmlDoc;
		}
		/// <summary>
		/// Преобразовать в XML-документ
		/// </summary>
		/// <param name="xml">XML-элемент</param>
		/// <param name="settings">Настройки XmlReader-а</param>
		/// <returns>XML-документ</returns>
		public static XmlDocument ToXmlDocument( this XmlElement xml,XmlReaderSettings settings = null )
		{
			return
				xml == null ? null :
				xml.OuterXml.IsNoValue() ? null :
				xml.OuterXml.ToXmlDocument( settings );
		}
		/// <summary>
		/// Преобразовать в XML-документ
		/// </summary>
		/// <param name="xml">XML-элемент</param>
		/// <param name="lvl">Настройки ConformanceLevel</param>
		/// <returns>XML-документ</returns>
		public static XmlDocument ToXmlDocument( this XmlElement xml,ConformanceLevel lvl )
		{
			return
				xml == null ? null :
				xml.OuterXml.IsNoValue() ? null :
				xml.OuterXml.ToXmlDocument( lvl );
		}
		/// <summary>
		/// Преобразовать в XML-документ
		/// </summary>
		/// <param name="xml">XML-элемент</param>
		/// <param name="lvl">Настройки ConformanceLevel</param>
		/// <param name="resolver">Объект разрешающий внешние ссылки</param>
		/// <returns>XML-документ</returns>
		public static XmlDocument ToXmlDocument( this XmlElement xml,ConformanceLevel lvl,XmlResolver resolver )
		{
			return
				xml == null ? null :
				xml.OuterXml.IsNoValue() ? null :
				xml.OuterXml.ToXmlDocument( lvl,resolver );
		}

		/// <summary>
		/// Преобразовать в XmlNode
		/// </summary>
		/// <param name="xml">Исходный XML</param>
		/// <returns>Созданный XmlNode</returns>
		public static XmlNode ToXmlNode( this XmlReader xml )
		{
			return (new XmlDocument()).ReadNode( xml ?? string.Empty.ToXmlReader() );
		}

		/// <summary>
		/// Преобразовать в XmlNode
		/// </summary>
		/// <param name="xml">Исходный XML</param>
		/// <param name="settings">Настройки XmlReader-а</param>
		/// <returns>Созданный XmlNode</returns>
		public static XmlNode ToXmlNode( this TextReader xml,XmlReaderSettings settings = null )
		{
			return xml.ToXmlReader( settings ).ToXmlNode();
		}
		/// <summary>
		/// Преобразовать в XmlNode
		/// </summary>
		/// <param name="xml">Исходный XML</param>
		/// <param name="lvl">Настройки ConformanceLevel</param>
		/// <returns>Созданный XmlNode</returns>
		public static XmlNode ToXmlNode( this TextReader xml,ConformanceLevel lvl )
		{
			return xml.ToXmlReader( lvl ).ToXmlNode();
		}
		/// <summary>
		/// Преобразовать в XmlNode
		/// </summary>
		/// <param name="xml">Исходный XML</param>
		/// <param name="lvl">Настройки ConformanceLevel</param>
		/// <param name="resolver">Объект разрешающий внешние ссылки</param>
		/// <returns>Созданный XmlNode</returns>
		public static XmlNode ToXmlNode( this TextReader xml,ConformanceLevel lvl,XmlResolver resolver )
		{
			return xml.ToXmlReader( lvl,resolver ).ToXmlNode();
		}

		/// <summary>
		/// Преобразовать в XmlNode
		/// </summary>
		/// <param name="xml">Исходный XML</param>
		/// <param name="settings">Настройки XmlReader-а</param>
		/// <returns>Созданный XmlNode</returns>
		public static XmlNode ToXmlNode( this string xml,XmlReaderSettings settings = null )
		{
			return xml.ToXmlReader( settings ).ToXmlNode();
		}
		/// <summary>
		/// Преобразовать в XmlNode
		/// </summary>
		/// <param name="xml">Исходный XML</param>
		/// <param name="lvl">Настройки ConformanceLevel</param>
		/// <returns>Созданный XmlNode</returns>
		public static XmlNode ToXmlNode( this string xml,ConformanceLevel lvl )
		{
			return xml.ToXmlReader( lvl ).ToXmlNode();
		}
		/// <summary>
		/// Преобразовать в XmlNode
		/// </summary>
		/// <param name="xml">Исходный XML</param>
		/// <param name="lvl">Настройки ConformanceLevel</param>
		/// <param name="resolver">Объект разрешающий внешние ссылки</param>
		/// <returns>Созданный XmlNode</returns>
		public static XmlNode ToXmlNode( this string xml,ConformanceLevel lvl,XmlResolver resolver )
		{
			return xml.ToXmlReader( lvl,resolver ).ToXmlNode();
		}

		/// <summary>
		/// Преобразовать в XmlReader
		/// </summary>
		/// <param name="xml">Исходный XML</param>
		/// <param name="settings">Настройки XmlReader-а</param>
		/// <returns>Созданный XmlReader</returns>
		public static XmlReader ToXmlReader( this XmlNode xml,XmlReaderSettings settings = null )
		{
			return (xml == null ? string.Empty : xml.OuterXml).ToXmlReader( settings );
		}
		/// <summary>
		/// Преобразовать в XmlReader
		/// </summary>
		/// <param name="xml">Исходный XML</param>
		/// <param name="lvl">Настройки ConformanceLevel</param>
		/// <returns>Созданный XmlReader</returns>
		public static XmlReader ToXmlReader( this XmlNode xml,ConformanceLevel lvl )
		{
			return (xml == null ? string.Empty : xml.OuterXml).ToXmlReader( lvl );
		}
		/// <summary>
		/// Преобразовать в XmlReader
		/// </summary>
		/// <param name="xml">Исходный XML</param>
		/// <param name="lvl">Настройки ConformanceLevel</param>
		/// <param name="resolver">Объект разрешающий внешние ссылки</param>
		/// <returns>Созданный XmlReader</returns>
		public static XmlReader ToXmlReader( this XmlNode xml,ConformanceLevel lvl,XmlResolver resolver )
		{
			return (xml == null ? string.Empty : xml.OuterXml).ToXmlReader( lvl,resolver );
		}

		/// <summary>
		/// Преобразовать в XmlWriter
		/// </summary>
		/// <param name="str">Буфер для построения строки</param>
		/// <param name="settings">Настройки XmlWrite-а</param>
		/// <returns>Созданный XmlWriter</returns>
		public static XmlWriter ToXmlWriter( this StringBuilder str,XmlWriterSettings settings = null )
		{
			return settings == null ? XmlWriter.Create( str ) : XmlWriter.Create( str,settings );
		}
		/// <summary>
		/// Преобразовать в XmlWriter
		/// </summary>
		/// <param name="str">Буфер для построения строки</param>
		/// <param name="lvl">Настройки ConformanceLevel</param>
		/// <returns>Созданный XmlWriter</returns>
		public static XmlWriter ToXmlWriter( this StringBuilder str,ConformanceLevel lvl )
		{
			XmlWriterSettings settings = new XmlWriterSettings();
			settings.ConformanceLevel = lvl;
			return str.ToXmlWriter( settings );
		}

		/// <summary>
		/// Проверить имеет ли атрибут необходимое значение
		/// </summary>
		/// <param name="attr">Проверяемый атрибут</param>
		/// <param name="value">Требуемое значение</param>
		/// <param name="comparisonType">Опции проверки</param>
		/// <returns>Результат проверки</returns>
		public static bool IsValue( this XmlAttribute attr,string value,StringComparison comparisonType = StringComparison.InvariantCultureIgnoreCase )
		{
			return attr != null && attr.Value != null && attr.Value.Equals( value,comparisonType );
		}
		/// <summary>
		/// Проверить имеет ли атрибут необходимое название
		/// </summary>
		/// <param name="attr">Проверяемый атрибут</param>
		/// <param name="name">Требуемое название</param>
		/// <param name="comparisonType">Опции проверки</param>
		/// <returns>Результат проверки</returns>
		public static bool IsAttr( this XmlAttribute attr,string name,StringComparison comparisonType = StringComparison.InvariantCultureIgnoreCase )
		{
			return attr != null && attr.Name.Equals( name,comparisonType );
		}

		/// <summary>
		/// Проверить наличие у элемента указанного атрибута
		/// </summary>
		/// <param name="node">Проверяемый элемент</param>
		/// <param name="name">Требуемое название</param>
		/// <param name="value">Требуемое значение</param>
		/// <param name="comparisonType">Опции проверки</param>
		/// <returns>Результат проверки</returns>
		public static bool ExistsAttr( this XmlNode node,string name,string value = null,StringComparison comparisonType = StringComparison.InvariantCultureIgnoreCase )
		{
			bool isEqual = false;

			if(node != null && name.IsValue())
			{
				if(value.IsValue())
					for(int i = 0, count = node.Attributes.Count; i < count && !isEqual; i++)
						isEqual = node.Attributes[i].IsAttr( name,comparisonType ) && node.Attributes[i].IsValue( value,comparisonType );
				else
					for(int i = 0, count = node.Attributes.Count; i < count && !isEqual; i++)
						isEqual = node.Attributes[i].IsAttr( name,comparisonType );
			}

			return isEqual;
		}
		/// <summary>
		/// Проверить наличие у элемента указанного атрибута
		/// </summary>
		/// <param name="node">Проверяемый элемент</param>
		/// <param name="attr">Требуемые название, значение атрибута</param>
		/// <param name="comparisonType">Опции проверки</param>
		/// <returns>Результат проверки</returns>
		public static bool ExistsAttr( this XmlNode node,Prop attr,StringComparison comparisonType = StringComparison.InvariantCultureIgnoreCase )
		{
			return attr != null && node.ExistsAttr( attr.Name,attr.Value.ToXmlValue(),comparisonType );
		}
		/// <summary>
		/// Проверить наличие у элемента указанных атрибутов
		/// </summary>
		/// <param name="node">Проверяемый элемент</param>
		/// <param name="attrs">Набор названий атрибутов</param>
		/// <param name="condition">Условия проверки: должны быть все, хотя бы один, ни один из указанных</param>
		/// <param name="comparisonType">Опции проверки</param>
		/// <returns>Результат проверки</returns>
		public static bool ExistsAttr( this XmlNode node,string[] attrs,LogicalOperation condition = LogicalOperation.And,StringComparison comparisonType = StringComparison.InvariantCultureIgnoreCase )
		{
			bool isEqual = condition == LogicalOperation.Not;

			if(attrs != null)
			{
				for(int i = 0, count = attrs.Length; i < count; i++)
				{
					if(node.ExistsAttr( attrs[i].Trim().ToProp(),comparisonType ))
					{
						if(condition == LogicalOperation.And) { isEqual = true; }
						else if(condition == LogicalOperation.Or) { isEqual = true; break; }
						else if(condition == LogicalOperation.Not) { isEqual = false; break; }
					}
					else
					{
						if(condition == LogicalOperation.And) { isEqual = false; break; }
						else if(condition == LogicalOperation.Not) { isEqual = true; }
					}
				}
			}

			return isEqual;
		}

		/// <summary>
		/// Проверить имеет ли узел необходимое название
		/// </summary>
		/// <param name="node">Проверяемый узел</param>
		/// <param name="name">Требуемое название</param>
		/// <param name="comparisonType">Опции проверки</param>
		/// <returns>Результат проверки</returns>
		public static bool IsNode( this XmlNode node,string name,StringComparison comparisonType = StringComparison.InvariantCultureIgnoreCase )
		{
			return node != null && node.Name.Equals( name,comparisonType );
		}
		/// <summary>
		/// Проверить имеет ли узел необходимое название и атрибут
		/// </summary>
		/// <param name="node">Проверяемый узел</param>
		/// <param name="name">Требуемое название узла</param>
		/// <param name="attr">Требуемое название атрибута</param>
		/// <param name="value">Требуемое значение атрибута</param>
		/// <param name="comparisonType">Опции проверки</param>
		/// <returns>Результат проверки</returns>
		public static bool IsNode( this XmlNode node,string name,string attr,string value = null,StringComparison comparisonType = StringComparison.InvariantCultureIgnoreCase )
		{
			return node.IsNode( name,comparisonType ) && (attr.IsNoValue() || node.ExistsAttr( attr,value,comparisonType ));
		}
		/// <summary>
		/// Проверить имеет ли узел необходимое название и набор атрибутов
		/// </summary>
		/// <param name="node">Проверяемый узел</param>
		/// <param name="name">Требуемое название узла</param>
		/// <param name="attrs">Набор названий атрибутов</param>
		/// <param name="condition">Условия проверки: должны быть все, хотя бы один, ни один из указанных</param>
		/// <param name="comparisonType">Опции проверки</param>
		/// <returns>Результат проверки</returns>
		public static bool IsNode( this XmlNode node,string name,string[] attrs,LogicalOperation condition = LogicalOperation.And,StringComparison comparisonType = StringComparison.InvariantCultureIgnoreCase )
		{
			return node.IsNode( name,comparisonType ) && (attrs == null || attrs.Length == 0 || node.ExistsAttr( attrs,condition,comparisonType ));
		}
		/// <summary>
		/// Проверить соответствие узла xPath выражению
		/// </summary>
		/// <param name="node">Проверяемый узел</param>
		/// <param name="xPath">Строка вида: elem[@attr="value" @attr="value"] или elem[-],
		/// выражение elem[-] означает, что узел не должен содержать ни одного атрибута</param>
		/// <param name="condition">Условия проверки: должны быть все, хотя бы один, ни один из указанных</param>
		/// <param name="comparisonType">Опции проверки</param>
		/// <returns>Результат проверки</returns>
		public static bool IsNode( this XmlNode node,string xPath,LogicalOperation condition,StringComparison comparisonType = StringComparison.InvariantCultureIgnoreCase )
		{
			bool isEqual = false;

			// elem[@attr="value" @attr="value"]
			// elem[-]
			if(xPath.IsValue())
			{
				int indAttrs = xPath.IndexOf( '[' );
				if(indAttrs >= 0)
				{
					char[] SeparatorAttrs = { '@' };
					string name = xPath.Substring( 0,indAttrs ).Trim();
					string[] attrs = xPath.Substring( "[","]" ).Trim().Split( SeparatorAttrs,StringSplitOptions.RemoveEmptyEntries );
					isEqual = attrs.Length == 1 && attrs[0].Trim()[0] == '-' ?
						node.IsNode( name,comparisonType ) && node.Attributes.Count == 0 :
						node.IsNode( name,attrs,condition,comparisonType );
				}
				else
					isEqual = node.IsNode( xPath,comparisonType );
			}

			return isEqual;
		}
		/// <summary>
		/// Проверить соответствие узла одному из xPath выражений
		/// </summary>
		/// <param name="node">Проверяемый узел</param>
		/// <param name="xPaths">Набор строк вида: elem[@attr="value" @attr="value"] или elem[-],
		/// выражение elem[-] означает, что узел не должен содержать ни одного атрибута</param>
		/// <param name="condition">Условия проверки: должны быть все, хотя бы один, ни один из указанных</param>
		/// <param name="comparisonType">Опции проверки</param>
		/// <returns>Результат проверки</returns>
		public static bool IsNodeAny( this XmlNode node,string[] xPaths,LogicalOperation condition = LogicalOperation.And,StringComparison comparisonType = StringComparison.InvariantCultureIgnoreCase )
		{
			bool isEqual = false;

			if(node != null && xPaths != null)
			{
				for(int i = 0, count = xPaths.Length; i < count && !isEqual; i++)
				{
					isEqual = node.IsNode( xPaths[i].IfNull( string.Empty ).Trim(),condition,comparisonType );
				}
			}

			return isEqual;
		}

		/// <summary>
		/// Получить первый узел имеющий соответствующее название
		/// </summary>
		/// <param name="list">Список проверяемых узлов</param>
		/// <param name="name">Требуемое название узла</param>
		/// <param name="comparisonType">Опции проверки</param>
		/// <returns>Найденный узел</returns>
		public static XmlNode GetFirst( this XmlNodeList list,string name,StringComparison comparisonType = StringComparison.InvariantCultureIgnoreCase )
		{
			XmlNode node = null;

			if(name.IsValue() && list != null)
			{
				foreach(XmlNode item in list)
				{
					if(item.IsNode( name,comparisonType ))
					{
						node = item;
						break;
					}
				}
			}

			return node;
		}

		/// <summary>
		/// Получить первый дочерний узел имеющий соответствующее название
		/// </summary>
		/// <param name="node">Родительский узел</param>
		/// <param name="name">Требуемое название узла</param>
		/// <param name="comparisonType">Опции проверки</param>
		/// <returns>Найденный узел</returns>
		public static XmlNode GetChild( this XmlNode node,string name,StringComparison comparisonType = StringComparison.InvariantCultureIgnoreCase )
		{
			return node == null ? null : node.ChildNodes.GetFirst( name,comparisonType );
		}
		/// <summary>
		/// Получить первый дочерний узел имеющий соответствующее название
		/// </summary>
		/// <param name="doc">XML-документ</param>
		/// <param name="name">Требуемое название узла</param>
		/// <param name="comparisonType">Опции проверки</param>
		/// <returns>Найденный узел</returns>
		public static XmlNode GetChild( this XmlDocument doc,string name,StringComparison comparisonType = StringComparison.InvariantCultureIgnoreCase )
		{
			return doc == null ? null : doc.ChildNodes.GetFirst( name,comparisonType );
		}
		/// <summary>
		/// Получить первый дочерний узел имеющий соответствующее название и атрибут
		/// </summary>
		/// <param name="node">Родительский узел</param>
		/// <param name="name">Требуемое название узла</param>
		/// <param name="attr">Требуемое название атрибута</param>
		/// <param name="value">Требуемое значение атрибута</param>
		/// <param name="comparisonType">Опции проверки</param>
		/// <returns>Найденный узел</returns>
		public static XmlNode GetChild( this XmlNode node,string name,string attr,string value = null,StringComparison comparisonType = StringComparison.InvariantCultureIgnoreCase )
		{
			XmlNode child = null;

			if(node != null)
			{
				for(int i = 0, count = node.ChildNodes.Count; i < count && child == null; i++)
				{
					var cur = node.ChildNodes[i];
					if(cur.IsNode( name,attr,value,comparisonType ))
						child = cur;
				}
			}

			return child;
		}
		/// <summary>
		/// Получить первый дочерний узел имеющий соответствующее название и набор атрибутов
		/// </summary>
		/// <param name="node">Родительский узел</param>
		/// <param name="name">Требуемое название узла</param>
		/// <param name="attrs">Набор названий атрибутов</param>
		/// <param name="condition">Условия проверки: должны быть все, хотя бы один, ни один из указанных</param>
		/// <param name="comparisonType">Опции проверки</param>
		/// <returns>Найденный узел</returns>
		public static XmlNode GetChild( this XmlNode node,string name,string[] attrs,LogicalOperation condition = LogicalOperation.And,StringComparison comparisonType = StringComparison.InvariantCultureIgnoreCase )
		{
			XmlNode child = null;

			if(node != null)
			{
				for(int i = 0, count = node.ChildNodes.Count; i < count && child == null; i++)
				{
					var cur = node.ChildNodes[i];
					if(cur.IsNode( name,attrs,condition,comparisonType ))
						child = cur;
				}
			}

			return child;
		}
		/// <summary>
		/// Получить первый дочерний узел соответствующий xPath выражению
		/// </summary>
		/// <param name="node">Родительский узел</param>
		/// <param name="xPath">Строка вида: elem[@attr="value" @attr="value"] или elem[-],
		/// выражение elem[-] означает, что узел не должен содержать ни одного атрибута</param>
		/// <param name="condition">Условия проверки: должны быть все, хотя бы один, ни один из указанных</param>
		/// <param name="comparisonType">Опции проверки</param>
		/// <returns>Найденный узел</returns>
		public static XmlNode GetChild( this XmlNode node,string xPath,LogicalOperation condition,StringComparison comparisonType = StringComparison.InvariantCultureIgnoreCase )
		{
			XmlNode child = null;

			if(node != null)
			{
				for(int i = 0, count = node.ChildNodes.Count; i < count && child == null; i++)
				{
					var cur = node.ChildNodes[i];
					if(cur.IsNode( xPath,condition,comparisonType ))
						child = cur;
				}
			}

			return child;
		}
		/// <summary>
		/// Получить первый дочерний узел соответствующий одному из xPath выражений
		/// </summary>
		/// <param name="node">Родительский узел</param>
		/// <param name="xPaths">Набор строк вида: elem[@attr="value" @attr="value"] или elem[-],
		/// выражение elem[-] означает, что узел не должен содержать ни одного атрибута</param>
		/// <param name="condition">Условия проверки: должны быть все, хотя бы один, ни один из указанных</param>
		/// <param name="comparisonType">Опции проверки</param>
		/// <returns>Найденный узел</returns>
		public static XmlNode GetChildAny( this XmlNode node,string[] xPaths,LogicalOperation condition = LogicalOperation.And,StringComparison comparisonType = StringComparison.InvariantCultureIgnoreCase )
		{
			XmlNode child = null;

			if(node != null && xPaths != null)
			{
				for(int i = 0, count = xPaths.Length; i < count && child == null; i++)
				{
					child = node.GetChild( xPaths[i].IfNull( string.Empty ).Trim(),condition,comparisonType );
				}
			}

			return child;
		}

		/// <summary>
		/// Получить первый дочерний узел соответствующий одному из иерархических xPath выражений,
		/// набор выражений применяется последовательно для каждого уровня
		/// </summary>
		/// <param name="node">Родительский узел</param>
		/// <param name="xTree">Набор строк вида: elem[@attr="value" @attr="value"] или elem[-],
		/// выражение elem[-] означает, что узел не должен содержать ни одного атрибута</param>
		/// <param name="condition">Условия проверки: должны быть все, хотя бы один, ни один из указанных</param>
		/// <param name="comparisonType">Опции проверки</param>
		/// <returns>Найденный узел</returns>
		public static XmlNode GetTreeChild( this XmlNode node,string[] xTree,LogicalOperation condition = LogicalOperation.And,StringComparison comparisonType = StringComparison.InvariantCultureIgnoreCase )
		{
			XmlNode child = null;

			if(xTree != null && xTree.Length > 0)
			{
				child = node;
				for(int i = 0, count = xTree.Length; i < count && child != null; i++)
				{
					string xPath = xTree[i].IfNull( string.Empty ).Trim();
					if(i == 0 && xPath.IsNoValue())
					{
						if(xTree.Length == 1) child = null;
					}
					else
					{
						child = child.GetChild( xPath,condition,comparisonType );
					}
				}
			}

			return child;
		}
		/// <summary>
		/// Получить первый дочерний узел соответствующий xPath выражению, содержащему полный путь до дочернего узла
		/// </summary>
		/// <param name="node">Родительский узел</param>
		/// <param name="xPath">Строка вида: elem[@attr="value" @attr="value"]|[-]/elem[@attr="value" @attr="value"]|[-]...,
		/// выражение elem[-] означает, что узел не должен содержать ни одного атрибута</param>
		/// <param name="condition">Условия проверки: должны быть все, хотя бы один, ни один из указанных</param>
		/// <param name="comparisonType">Опции проверки</param>
		/// <returns>Найденный узел</returns>
		public static XmlNode GetTreeChild( this XmlNode node,string xPath,LogicalOperation condition = LogicalOperation.And,StringComparison comparisonType = StringComparison.InvariantCultureIgnoreCase )
		{
			return xPath == null ? null : node.GetTreeChild( xPath.Trim().Split( '/' ),condition,comparisonType );
		}
		/// <summary>
		/// Получить первый дочерний узел соответствующий одному из xPath выражений, содержащих полный путь до дочернего узла
		/// </summary>
		/// <param name="node">Родительский узел</param>
		/// <param name="xPaths">Набор строк вида: elem[@attr="value" @attr="value"]|[-]/elem[@attr="value" @attr="value"]|[-]...,
		/// выражение elem[-] означает, что узел не должен содержать ни одного атрибута</param>
		/// <param name="condition">Условия проверки: должны быть все, хотя бы один, ни один из указанных</param>
		/// <param name="comparisonType">Опции проверки</param>
		/// <returns>Найденный узел</returns>
		public static XmlNode GetTreeChildAny( this XmlNode node,string[] xPaths,LogicalOperation condition = LogicalOperation.And,StringComparison comparisonType = StringComparison.InvariantCultureIgnoreCase )
		{
			XmlNode child = null;

			if(node != null && xPaths != null)
			{
				for(int i = 0, count = xPaths.Length; i < count && child == null; i++)
				{
					child = node.GetTreeChild( xPaths[i],condition,comparisonType );
				}
			}

			return child;
		}

		/// <summary>
		/// Получить значение одного из атрибутов списка
		/// Возвращает значение первого найденного атрибута из списка
		/// </summary>
		/// <param name="node">Узел XML</param>
		/// <param name="names">Список названий атрибутов</param>
		/// <param name="def">Значение по умолчанию (если не найден атрибут или у него нет значения)</param>
		/// <returns>Значение первого найденного атрибута из списка или значение по умолчанию</returns>
		public static string GetAttrAnyValue( this XmlNode node,string[] names,string def = null )
		{
			string val = def;

			if(node != null && names != null)
			{
				for(int i = 0, count = names.Length; i < count; i++)
				{
					string name = names[i];
					if(name == ".")
					{
						if(node.InnerText.IsValue())
						{
							val = node.InnerText;
							break;
						}
					}
					else
					{
						var attr = node.Attributes[name];
						if(attr != null)
						{
							val = attr.Value;
							break;
						}
					}
				}
			}

			return val;
		}

		/// <summary>
		/// Получить значение атрибута XML-узла
		/// </summary>
		/// <param name="xml">XML-узел</param>
		/// <param name="name">Название атрибута</param>
		/// <param name="def">Значение по умолчанию, если xml = null, нет указанного атрибута или атрибут = пустое значение</param>
		/// <returns>Значение атрибута или значение по умолчанию</returns>
		public static string GetAttrValue( this XmlNode xml,string name,string def = null )
		{
			var attr = xml == null ? null : xml.Attributes[name];
			return (attr == null ? null : attr.Value).IfNull( def );
		}
		/// <summary>
		/// Поиск значения
		/// </summary>
		/// <param name="node"></param>
		/// <param name="xPath">XPath выражение</param>
		/// <param name="defValue">Значение поумолчанию</param>
		/// <returns></returns>
		public static string GetValue(this XmlNode node, string xPath, string defValue = "")
		{
			XmlNodeList result = null;

			if (node.IsValue()
				&& xPath.IsValue()
				&& (result = node.SelectNodes(xPath)).Count > 0)
			{
				string resultData = defValue;
				foreach (XmlNode n in result)
				{
					switch (n.NodeType)
					{
						case XmlNodeType.Attribute:
							resultData += resultData == "" ? n.Value : "\r\n" + n.Value;
							break;
						case XmlNodeType.Text:
						case XmlNodeType.Element:
							resultData += resultData == "" ? n.InnerText : "\r\n" + n.InnerText;
							break;
					}
				}
				return resultData;
			}
			return defValue;
		}

		/// <summary>
		/// Вычислить хэш-код значения содержимого XML
		/// </summary>
		/// <param name="xml">XML-текст</param>
		/// <param name="def">Значение по умолчанию, если xml = null</param>
		/// <returns>Вычисленный хэш-код</returns>
		public static int GetXmlHashCode( this XmlNode xml,int def = 0 )
		{
			return xml == null ? def : xml.OuterXml.GetHashCode();
		}
		/// <summary>
		/// Вычислить хэш-код значения содержимого XML
		/// </summary>
		/// <param name="xml">XML-текст</param>
		/// <param name="def">Значение по умолчанию, если xml = null</param>
		/// <returns>Вычисленный хэш-код</returns>
		public static int GetXmlHashCode( this XmlDocument xml,int def = 0 )
		{
			return xml == null ? def : xml.OuterXml.GetHashCode();
		}

		/// <summary>
		/// Записать атрибут с указанными локальным именем и значением
		/// </summary>
		/// <typeparam name="T">Один из типов, используемых в значениях метода XmlWriter.WriteValue</typeparam>
		/// <param name="writer">Исходный XmlWriter</param>
		/// <param name="localName">Локальное имя атрибута</param>
		/// <param name="value">Значение атрибута</param>
		/// <returns>Исходный XmlWriter</returns>
		public static XmlWriter WriteAttributeValue<T>( this XmlWriter writer,string localName,T value )
		{
			writer.WriteStartAttribute( localName );
			writer.WriteValue( value );
			writer.WriteEndAttribute();
			return writer;
		}
		/// <summary>
		/// Записать атрибут с указанными локальным именем, URI пространства имён и значением
		/// </summary>
		/// <typeparam name="T">Один из типов, используемых в значениях метода XmlWriter.WriteValue</typeparam>
		/// <param name="writer">Исходный XmlWriter</param>
		/// <param name="localName">Локальное имя атрибута</param>
		/// <param name="ns">URI пространства имён атрибута</param>
		/// <param name="value">Значение атрибута</param>
		/// <returns>Исходный XmlWriter</returns>
		public static XmlWriter WriteAttributeValue<T>( this XmlWriter writer,string localName,string ns,T value )
		{
			writer.WriteStartAttribute( localName,ns );
			writer.WriteValue( value );
			writer.WriteEndAttribute();
			return writer;
		}
		/// <summary>
		/// Записать атрибут с указанными префиксом, локальным именем, URI пространства имён и значением
		/// </summary>
		/// <typeparam name="T">Один из типов, используемых в значениях метода XmlWriter.WriteValue</typeparam>
		/// <param name="writer">Исходный XmlWriter</param>
		/// <param name="prefix">Префикс пространства имён атрибута</param>
		/// <param name="localName">Локальное имя атрибута</param>
		/// <param name="ns">URI пространства имён атрибута</param>
		/// <param name="value">Значение атрибута</param>
		/// <returns>Исходный XmlWriter</returns>
		public static XmlWriter WriteAttributeValue<T>( this XmlWriter writer,string prefix,string localName,string ns,T value )
		{
			writer.WriteStartAttribute( prefix,localName,ns );
			writer.WriteValue( value );
			writer.WriteEndAttribute();
			return writer;
		}

		/// <summary>
		/// Записать атрибут с указанными локальным именем и значением, если он не null
		/// </summary>
		/// <param name="writer">Исходный XmlWriter</param>
		/// <param name="localName">Локальное имя атрибута</param>
		/// <param name="value">Значение атрибута</param>
		/// <returns>Исходный XmlWriter</returns>
		public static XmlWriter WriteAttributeStringIfNotNull( this XmlWriter writer,string localName,string value )
		{
			if(value != null)
				writer.WriteAttributeString( localName,value );
			return writer;
		}
		/// <summary>
		/// Записать атрибут с указанными локальным именем, URI пространства имён и значением, если он не null
		/// </summary>
		/// <param name="writer">Исходный XmlWriter</param>
		/// <param name="localName">Локальное имя атрибута</param>
		/// <param name="ns">URI пространства имён атрибута</param>
		/// <param name="value">Значение атрибута</param>
		/// <returns>Исходный XmlWriter</returns>
		public static XmlWriter WriteAttributeStringIfNotNull( this XmlWriter writer,string localName,string ns,string value )
		{
			if(value != null)
				writer.WriteAttributeString( localName,ns,value );
			return writer;
		}
		/// <summary>
		/// Записать атрибут с указанными префиксом, локальным именем, URI пространства имён и значением, если он не null
		/// </summary>
		/// <param name="writer">Исходный XmlWriter</param>
		/// <param name="prefix">Префикс пространства имён атрибута</param>
		/// <param name="localName">Локальное имя атрибута</param>
		/// <param name="ns">URI пространства имён атрибута</param>
		/// <param name="value">Значение атрибута</param>
		/// <returns>Исходный XmlWriter</returns>
		public static XmlWriter WriteAttributeStringIfNotNull( this XmlWriter writer,string prefix,string localName,string ns,string value )
		{
			if(value != null)
				writer.WriteAttributeString( prefix,localName,ns,value );
			return writer;
		}

		/// <summary>
		/// Значения по умолчанию для операций подписи XML
		/// </summary>
		private static class SignDefault
		{
			/// <summary>
			/// Название элемента подписи в XML
			/// </summary>
			public const string Element = "Signature";
			/// <summary>
			/// Представление по умолчанию элемента reference в подписи XML
			/// </summary>
			public static readonly Reference Reference = new Reference("");

			/// <summary>
			/// Инициализация статических переменных
			/// </summary>
			static SignDefault()
			{
				Reference.AddTransform( new XmlDsigEnvelopedSignatureTransform() );
			}
		}

		/// <summary>
		/// Подписать ключём элемент XML
		/// </summary>
		/// <param name="xml">Подписываемый элемент XML</param>
		/// <param name="alg">Алгоритм шифрования ключа</param>
		/// <param name="reference">Представление элемента reference в подписи XML</param>
		/// <returns>Исходный элемент XML с добавленной подписью</returns>
		public static XmlElement AddSign( this XmlElement xml,AsymmetricAlgorithm alg,Reference reference = null )
		{
			if(xml != null)
			{
				var exist = xml[SignDefault.Element];
				if(exist != null) xml.RemoveChild( exist );
				var signed = new SignedXml(xml);
				signed.SigningKey = alg;
				signed.AddReference( reference ?? SignDefault.Reference );
				signed.ComputeSignature();
				xml.AppendChild( signed.GetXml() );
			}
			return xml;
		}
		/// <summary>
		/// Подписать ключём документ XML
		/// </summary>
		/// <param name="xml">Подписываемый документ XML</param>
		/// <param name="alg">Алгоритм шифрования ключа</param>
		/// <param name="reference">Представление элемента reference в подписи XML</param>
		/// <returns>Исходный документ XML с добавленной подписью</returns>
		public static XmlDocument AddSign( this XmlDocument xml,AsymmetricAlgorithm alg,Reference reference = null )
		{
			if(xml != null) xml.DocumentElement.AddSign( alg,reference );
			return xml;
		}
		/// <summary>
		/// Проверить подпись элемента XML
		/// </summary>
		/// <param name="xml">Проверяемый элемент XML</param>
		/// <param name="alg">Алгоритм шифрования ключа</param>
		/// <returns>Результат проверки</returns>
		public static bool CheckSign( this XmlElement xml,AsymmetricAlgorithm alg )
		{
			bool check = false;
			if(xml != null && xml[SignDefault.Element] != null)
			{
				var signed = new SignedXml(xml);
				signed.LoadXml( xml[SignDefault.Element] );
				check = signed.CheckSignature( alg );
			}
			return check;
		}
		/// <summary>
		/// Проверить подпись документа XML
		/// </summary>
		/// <param name="xml">Проверяемый документ XML</param>
		/// <param name="alg">Алгоритм шифрования ключа</param>
		/// <returns>Результат проверки</returns>
		public static bool CheckSign( this XmlDocument xml,AsymmetricAlgorithm alg )
		{
			return xml != null && xml.DocumentElement.CheckSign( alg );
		}
	}
}
