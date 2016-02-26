//-----------------------------------------------------------------------
// Cls.StringHelper.cs - описание вспомогательных методов
//						для работы со строками
//
// Created by *** 01.03.2012
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;


namespace Lib.Base
{
	/// <summary>
	/// Вспомогательные методы для работы со строками
	/// </summary>
	public static class StringHelper
	{
		/// <summary>
		/// Проверить отсутствие значения у строки
		/// Проверка эквивалента вызову метода string.IsNullOrEmpty,
		/// расширяющий метод введён для единообразия использования с типом object
		/// </summary>
		/// <param name="val">Тестируемое значение</param>
		/// <returns>true - значение отсутствует, иначе - false</returns>
		public static bool IsNoValue( this string val )
		{
			return string.IsNullOrEmpty( val );
		}
		/// <summary>
		/// Проверить наличие значения у строки
		/// Проверка эквивалента вызову метода string.IsNullOrEmpty,
		/// расширяющий метод введён для единообразия использования с типом object
		/// </summary>
		/// <param name="val">Тестируемое значение</param>
		/// <returns>true - значение имеется, иначе - false</returns>
		public static bool IsValue( this string val )
		{
			return !string.IsNullOrEmpty( val );
		}

		/// <summary>
		/// В случае, если у строки отсутствует значение, вернуть значение по умолчанию, иначе - исходное
		/// Проверка значения осуществляется по тем же правилам, что и для IsNoValue
		/// </summary>
		/// <param name="check">Тестируемое значение</param>
		/// <param name="def">Возвращаемое значение по умолчанию</param>
		/// <returns>Значение</returns>
		public static string IfNull( this string check,string def )
		{
			return string.IsNullOrEmpty( check ) ? def : check;
		}
		/// <summary>
		/// В случае, если у строки отсутствует значение, вернуть значение по умолчанию,
		/// иначе исходное, к которому применяется операция notNullAction,
		/// если операция не задана, возвращается исходное значение
		/// Проверка значения осуществляется по тем же правилам, что и для IsNoValue
		/// </summary>
		/// <param name="check">Тестируемое значение</param>
		/// <param name="def">Возвращаемое значение по умолчанию</param>
		/// <param name="notNullAction">Дополнительная операция над значением, если оно не Null</param>
		/// <returns>Значение</returns>
		public static object IfNull( this string check,string def,Func<string,string> notNullAction )
		{
			return string.IsNullOrEmpty( check ) ? def : notNullAction == null ? check : notNullAction( check );
		}

		/// <summary>
		/// В случае, если у строки отсутствует значение, создать исключение с указанным сообщением
		/// Проверка значения осуществляется по тем же правилам, что и для IsNoValue
		/// </summary>
		/// <param name="check">Тестируемое значение</param>
		/// <param name="msg">Сообщение исключения, если строка Null</param>
		/// <returns>Значение</returns>
		public static string ThrowIfNull( this string check,string msg )
		{
			if(string.IsNullOrEmpty( check ))
				throw new Exception(msg);
			return check;
		}
		/// <summary>
		/// В случае, если у строки отсутствует значение, выдать указанное исключение
		/// Проверка значения осуществляется по тем же правилам, что и для IsNoValue
		/// </summary>
		/// <param name="check">Тестируемое значение</param>
		/// <param name="err">Исключение, если строка Null</param>
		/// <returns>Значение</returns>
		public static string ThrowIfNull( this string check,Exception err = null )
		{
			if(string.IsNullOrEmpty( check ))
				throw err == null ? new Exception() : err;
			return check;
		}

		/// <summary>
		/// Получить объект StringReader для строки
		/// </summary>
		/// <param name="val">Исходная строка</param>
		/// <returns>Зкземпляр StringReader</returns>
		public static StringReader ToReader( this string val )
		{
			return new StringReader(val ?? string.Empty);
		}

		/// <summary>
		/// Получить подстроку между указанными tag-ами
		/// </summary>
		/// <param name="src">Исходная строка</param>
		/// <param name="tagBegin">Начальный tag</param>
		/// <param name="tagEnd">Конечный tag</param>
		/// <param name="checkValue">Значение для проверки, которое должно встречаться между tag-ами</param>
		/// <param name="outerValue">Флаг выдачи подстроки с tag-ами</param>
		/// <param name="compType">Опции сравнения строк</param>
		/// <returns>Выделенная подстрока или null</returns>
		/// <remarks>
		/// Если не указан конечный tag, возвращается значение от начального и до конца строки
		/// Когда указано checkValue, искаться будет первая подходящая подстрока ограниченная начальным и конечным tag-ами, содержащая это значение.
		/// Если вначале будут встречаться подстроки с нужными tag-ами, но не содержащие контрольное значение, они будут пропускаться.
		/// </remarks>
		public static string Substring( this string src,string tagBegin,string tagEnd,string checkValue = null,bool outerValue = false,StringComparison compType = StringComparison.InvariantCultureIgnoreCase )
		{
			string res = null;

			if(src.IsValue() && tagBegin.IsValue())
			{
				if(tagEnd == null) tagEnd = string.Empty;
				for(int indBegin = 0; indBegin >= 0 && res == null; )
				{
					indBegin = src.IndexOf( tagBegin,indBegin,compType );
					if(indBegin >= 0)
					{
						int indEnd = tagEnd.IsNoValue() ? src.Length : src.IndexOf( tagEnd,indBegin + tagBegin.Length,compType );
						if(indEnd >= 0)
						{
							if(outerValue)
								indEnd += tagEnd.Length;
							else
								indBegin += tagBegin.Length;
							int indCheck = checkValue.IsNoValue() ? indBegin : src.IndexOf( checkValue,indBegin,compType );
							if(indCheck >= indBegin && indCheck <= indEnd)
								res = src.Substring( indBegin,indEnd - indBegin );
							else if(indCheck > indEnd)
								indBegin = indEnd + (outerValue ? 0 : tagEnd.Length);
							else
								indBegin = -1;
						}
						else
							indBegin = -1;
					}
				}
			}

			return res;
		}

		/// <summary>
		/// Проверить наличие подстроки в указанном месте исходной строки
		/// </summary>
		/// <param name="src">Исходная строка</param>
		/// <param name="value">Искомая подстрока</param>
		/// <param name="startIndex">Начальный индекс в исходной строке</param>
		/// <param name="comparisonType">Параметры сравнения</param>
		/// <returns>Подстрока найдена - true, иначе - flase</returns>
		public static bool SubstringEquals( this string src,string value,int startIndex,StringComparison comparisonType = StringComparison.InvariantCultureIgnoreCase )
		{
			int lenSrc = src == null ? 0 : src.Length;
			int lenVal = value == null ? 0 : value.Length;
			return startIndex >= 0 && lenSrc > 0 && lenVal > 0 && startIndex + lenVal <= lenSrc && src.IndexOf( value,startIndex,lenVal,comparisonType ) >= 0;
		}
		/// <summary>
		/// Проверить наличие одной из подстрок в указанном месте исходной строки
		/// </summary>
		/// <param name="src">Исходная строка</param>
		/// <param name="values">Список искомых подстрок</param>
		/// <param name="startIndex">Начальный индекс в исходной строке</param>
		/// <param name="comparisonType">Параметры сравнения</param>
		/// <returns>Индекс найденной подстроки в списке или -1</returns>
		public static int SubstringEquals( this string src,string[] values,int startIndex,StringComparison comparisonType = StringComparison.InvariantCultureIgnoreCase )
		{
			int i = 0;
			int count = values == null ? 0 : values.Length;
			for(; i < count && !src.SubstringEquals( values[i],startIndex,comparisonType ); i++);
			return i < count ? i : -1;
		}

		/// <summary>
		/// Разбить строку на массив строк используя указанный разделитель,
		/// перегруженый метод введён для использования единичного разделителя
		/// </summary>
		/// <param name="src">Исходная строка</param>
		/// <param name="separator">Разделитель</param>
		/// <param name="options">Опции операции разделения строк StringSplitOptions</param>
		/// <returns>Массив разделённых строк</returns>
		public static string[] Split( this string src,string separator,StringSplitOptions options = StringSplitOptions.RemoveEmptyEntries )
		{
			return src.IsNoValue() || separator.IsNoValue() ? new string[]{} : src.Split( new string[] { separator },options );
		}

		/// <summary>
		/// Собрать строку из списка строк используя опции разделения строк
		/// </summary>
		/// <param name="src">Исходный список строк</param>
		/// <param name="separator">Разделитель</param>
		/// <param name="options">Опции операции разделения строк StringSplitOptions</param>
		/// <returns>Результирующая строка</returns>
		public static string Join( this IEnumerable<string> src,string separator = ",",StringSplitOptions options = StringSplitOptions.RemoveEmptyEntries )
		{
			StringBuilder result = null;
			if(src != null)
			{
				bool next = false;
				foreach(var str in src)
				{
					if(options == StringSplitOptions.RemoveEmptyEntries && str.IsNoValue())
						continue;
					if(next)
						result.Append( separator );
					else
						next = true;
					result.Append( str ?? string.Empty );
				}
			}
			return result != null && result.Length > 0 ? result.ToString() : null;
		}

		/// <summary>
		/// Преобразовать к целочисленному значению типа short
		/// </summary>
		/// <param name="val">Преобразуемое значение</param>
		/// <param name="def">Значение по умолчанию, если отсутствует значение или невозможно преобразовать</param>
		/// <param name="throwOnError">Флаг создания исключения, в случае невозможности выполнить преобразование</param>
		/// <returns>Целочисленное значение типа short</returns>
		public static short ToShort( this string val,short def = 0,bool throwOnError = true )
		{
			short result = 0;

			if(val.IsNoValue())
				result = def;
			else if(throwOnError)
				result = short.Parse( val );
			else if(!short.TryParse( val,out result ))
				result = def;

			return result;
		}

		/// <summary>
		/// Преобразовать к целочисленному значению типа int
		/// </summary>
		/// <param name="val">Преобразуемое значение</param>
		/// <param name="def">Значение по умолчанию, если отсутствует значение или невозможно преобразовать</param>
		/// <param name="throwOnError">Флаг создания исключения, в случае невозможности выполнить преобразование</param>
		/// <returns>Целочисленное значение типа int</returns>
		public static int ToInt( this string val,int def = 0,bool throwOnError = true )
		{
			int result = 0;

			if(val.IsNoValue())
				result = def;
			else if(throwOnError)
				result = int.Parse( val );
			else if(!int.TryParse( val,out result ))
				result = def;

			return result;
		}

		/// <summary>
		/// Преобразовать к целочисленному значению типа long
		/// </summary>
		/// <param name="val">Преобразуемое значение</param>
		/// <param name="def">Значение по умолчанию, если отсутствует значение или невозможно преобразовать</param>
		/// <param name="throwOnError">Флаг создания исключения, в случае невозможности выполнить преобразование</param>
		/// <returns>Целочисленное значение типа long</returns>
		public static long ToLong( this string val,long def = 0,bool throwOnError = true )
		{
			long result = 0;

			if(val.IsNoValue())
				result = def;
			else if(throwOnError)
				result = long.Parse( val );
			else if(!long.TryParse( val,out result ))
				result = def;

			return result;
		}

		/// <summary>
		/// Преобразовать к численному значению типа double
		/// </summary>
		/// <param name="val">Преобразуемое значение</param>
		/// <param name="def">Значение по умолчанию, если отсутствует значение или невозможно преобразовать</param>
		/// <param name="throwOnError">Флаг создания исключения, в случае невозможности выполнить преобразование</param>
		/// <returns>Численное значение типа double</returns>
		public static double ToDouble( this string val,double def = 0,bool throwOnError = true )
		{
			double result = 0;

			if(val.IsNoValue())
				result = def;
			else if(throwOnError)
				result = double.Parse( val.Replace( ',','.' ) );
			else if(!double.TryParse( val.Replace( ',','.' ),out result ))
				result = def;

			return result;
		}

		/// <summary>
		/// Преобразовать к значению типа bool
		/// </summary>
		/// <param name="val">Преобразуемое значение</param>
		/// <param name="def">Значение по умолчанию, если отсутствует значение или невозможно преобразовать</param>
		/// <param name="throwOnError">Флаг создания исключения, в случае невозможности выполнить преобразование</param>
		/// <returns>Значение типа bool</returns>
		public static bool ToBool( this string val,bool def = false,bool throwOnError = true )
		{
			bool result = false;

			if(val.IsNoValue())
				result = def;
			else if(throwOnError)
				result = bool.Parse( val );
			else if(!bool.TryParse( val,out result ))
				result = def;

			return result;
		}

		/// <summary>
		/// Преобразовать строковое значение в целочисленное с Null
		/// </summary>
		/// <param name="val">Строковое значение</param>
		/// <param name="def">Значение по умолчанию, если отсутствует исходное</param>
		/// <param name="throwOnError">Флаг создания исключения, в случае невозможности выполнить преобразование</param>
		/// <returns>Целочисленное значение</returns>
		public static int? ToIntNullable( this string val,int? def = null,bool throwOnError = true )
		{
			int? result = null;

			if(string.IsNullOrEmpty( val ) || string.IsNullOrWhiteSpace( val ))
				result = def;
			else
			{
				int parse;
				if(throwOnError)
					result = int.Parse( val );
				else if(int.TryParse( val,out parse ))
					result = parse;
				else
					result = def;
			}

			return result;
		}
		/// <summary>
		/// Преобразовать строковое значение в целочисленное с Null
		/// </summary>
		/// <param name="val">Строковое значение</param>
		/// <param name="def">Значение по умолчанию, если отсутствует исходное</param>
		/// <param name="throwOnError">Флаг создания исключения, в случае невозможности выполнить преобразование</param>
		/// <returns>Целочисленное значение</returns>
		public static short? ToShortNullable( this string val,short? def = null,bool throwOnError = true )
		{
			short? result = null;

			if(string.IsNullOrEmpty( val ) || string.IsNullOrWhiteSpace( val ))
				result = def;
			else
			{
				short parse;
				if(throwOnError)
					result = short.Parse( val );
				else if(short.TryParse( val,out parse ))
					result = parse;
				else
					result = def;
			}

			return result;
		}
		/// <summary>
		/// Преобразовать строковое значение в целочисленное с Null
		/// </summary>
		/// <param name="val">Строковое значение</param>
		/// <param name="def">Значение по умолчанию, если отсутствует исходное</param>
		/// <param name="throwOnError">Флаг создания исключения, в случае невозможности выполнить преобразование</param>
		/// <returns>Целочисленное значение</returns>
		public static long? ToLongNullable( this string val,long? def = null,bool throwOnError = true )
		{
			long? result = null;

			if(string.IsNullOrEmpty( val ) || string.IsNullOrWhiteSpace( val ))
				result = def;
			else
			{
				long parse;
				if(throwOnError)
					result = long.Parse( val );
				else if(long.TryParse( val,out parse ))
					result = parse;
				else
					result = def;
			}

			return result;
		}

		/// <summary>
		/// Преобразовать к значению типа DateTime?
		/// </summary>
		/// <param name="val">Исходное значение</param>
		/// <param name="def">Значение по умолчанию, если отсутствует исходное</param>
		/// <param name="throwOnError">Флаг создания исключения, в случае невозможности выполнить преобразование</param>
		/// <returns>Значение типа DateTime?</returns>
		public static DateTime? ToDateTime( this string val,DateTime? def = null,bool throwOnError = true )
		{
			DateTime? result = null;

			if(val.IsNoValue())
				result = def;
			else
			{
				DateTime parse;
				if(throwOnError)
					result = DateTime.Parse( val );
				else if(DateTime.TryParse( val,out parse ))
					result = parse;
				else
					result = def;
			}

			return result;
		}
		/// <summary>
		/// Преобразовать к значению типа DateTime
		/// </summary>
		/// <param name="val">Исходное значение</param>
		/// <param name="def">Значение по умолчанию, если отсутствует исходное</param>
		/// <param name="throwOnError">Флаг создания исключения, в случае невозможности выполнить преобразование</param>
		/// <returns>Значение типа DateTime</returns>
		public static DateTime ToDateTime( this string val,DateTime def,bool throwOnError = true )
		{
			DateTime result;

			if(val.IsNoValue())
				result = def;
			else if(throwOnError)
				result = DateTime.Parse( val );
			else if(!DateTime.TryParse( val,out result ))
				result = def;

			return result;
		}

		/// <summary>
		/// Преобразовать к значению типа DateTimeOffset?
		/// </summary>
		/// <param name="val">Исходное значение</param>
		/// <param name="def">Значение по умолчанию, если отсутствует исходное</param>
		/// <param name="throwOnError">Флаг создания исключения, в случае невозможности выполнить преобразование</param>
		/// <returns>Значение типа DateTimeOffset?</returns>
		public static DateTimeOffset? ToDateTimeOffset( this string val,DateTimeOffset? def = null,bool throwOnError = true )
		{
			DateTimeOffset? result = null;

			if(val.IsNoValue())
				result = def;
			else
			{
				DateTimeOffset parse;
				if(throwOnError)
					result = DateTimeOffset.Parse( val,null,DateTimeStyles.AssumeUniversal );
				else if(DateTimeOffset.TryParse( val,null,DateTimeStyles.AssumeUniversal,out parse ))
					result = parse;
				else
					result = def;
			}

			return result;
		}
		/// <summary>
		/// Преобразовать к значению типа DateTimeOffset
		/// </summary>
		/// <param name="val">Исходное значение</param>
		/// <param name="def">Значение по умолчанию, если отсутствует исходное</param>
		/// <param name="throwOnError">Флаг создания исключения, в случае невозможности выполнить преобразование</param>
		/// <returns>Значение типа DateTimeOffset</returns>
		public static DateTimeOffset ToDateTimeOffset( this string val,DateTimeOffset def,bool throwOnError = true )
		{
			DateTimeOffset result;

			if(val.IsNoValue())
				result = def;
			else if(throwOnError)
				result = DateTimeOffset.Parse( val,null,DateTimeStyles.AssumeUniversal );
			else if(!DateTimeOffset.TryParse( val,null,DateTimeStyles.AssumeUniversal,out result ))
				result = def;

			return result;
		}

		/// <summary>
		/// Преобразовать к значению типа Guid
		/// </summary>
		/// <param name="val">Преобразуемое значение</param>
		/// <param name="def">Значение по умолчанию, если отсутствует значение или невозможно преобразовать</param>
		/// <param name="throwOnError">Флаг создания исключения, в случае невозможности выполнить преобразование</param>
		/// <returns>Значение типа Guid</returns>
		public static Guid ToGuid( this string val,Guid def,bool throwOnError = true )
		{
			Guid result;

			if(val.IsNoValue())
				result = def;
			else if(throwOnError)
				result = Guid.Parse( val );
			else if(!Guid.TryParse( val,out result ))
				result = def;

			return result;
		}
		/// <summary>
		/// Преобразовать к значению типа Guid?
		/// </summary>
		/// <param name="val">Преобразуемое значение</param>
		/// <param name="throwOnError">Флаг создания исключения, в случае невозможности выполнить преобразование</param>
		/// <returns>Значение типа Guid?</returns>
		public static Guid? ToGuid( this string val,bool throwOnError = true )
		{
			Guid? result = null;

			if(val.IsValue())
			{
				Guid parse = Guid.Empty;
				if(throwOnError)
					result = Guid.Parse( val );
				else if(Guid.TryParse( val,out parse ))
					result = parse;
			}

			return result;
		}
	}
}
