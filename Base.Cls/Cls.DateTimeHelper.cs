//-----------------------------------------------------------------------
// Cls.DateTimeHelper.cs - описание вспомогательных методов
//						для работы со значениями дата/время
//
// Created by *** 01.03.2012
//-----------------------------------------------------------------------
using System;
using System.Globalization;


namespace Lib.Base
{
	/// <summary>
	/// Вспомогательные методы для работы со значениями дата/время
	/// </summary>
	public static class DateTimeHelper
	{
		/// <summary>
		/// Формат по умолчанию преобразования даты/времени в строку ISO8601 with timezone
		/// </summary>
		public const string DefDateTimeFormat = DateTimeFormatISO8601zone;
		/// <summary>
		/// Формат преобразования даты/времени в строку ISO8601 with timezone
		/// </summary>
		public const string DateTimeFormatISO8601zone = "yyyy-MM-ddTHH:mm:ss.fffzzz";
		/// <summary>
		/// Формат преобразования даты/времени в строку ISO8601
		/// </summary>
		public const string DateTimeFormatISO8601 = "yyyy-MM-ddTHH:mm:ss.fff";

		/// <summary>
		/// Проверить отсутствие значения
		/// Считается, что у DateTime? отсутствует значение, если:
		/// - объект = null
		/// - не установлен флаг HasValue
		/// </summary>
		/// <param name="val">Тестируемое значение</param>
		/// <returns>true - значение отсутствует, иначе - false</returns>
		public static bool IsNoValue( this DateTime? val )
		{
			return val == null || !val.HasValue;
		}
		/// <summary>
		/// Проверить наличие значения
		/// Операция противоположна IsNoValue
		/// </summary>
		/// <param name="val">Тестируемое значение</param>
		/// <returns>true - значение имеется, иначе - false</returns>
		public static bool IsValue( this DateTime? val )
		{
			return val != null && val.HasValue;
		}

		/// <summary>
		/// Проверить отсутствие значения
		/// Считается, что у DateTimeOffset? отсутствует значение, если:
		/// - объект = null
		/// - не установлен флаг HasValue
		/// </summary>
		/// <param name="val">Тестируемое значение</param>
		/// <returns>true - значение отсутствует, иначе - false</returns>
		public static bool IsNoValue( this DateTimeOffset? val )
		{
			return val == null || !val.HasValue;
		}
		/// <summary>
		/// Проверить наличие значения
		/// Операция противоположна IsNoValue
		/// </summary>
		/// <param name="val">Тестируемое значение</param>
		/// <returns>true - значение имеется, иначе - false</returns>
		public static bool IsValue( this DateTimeOffset? val )
		{
			return val != null && val.HasValue;
		}

		/// <summary>
		/// В случае, если отсутствует значение, вернуть значение по умолчанию, иначе - исходное
		/// Проверка значения осуществляется по тем же правилам, что и для IsNoValue
		/// </summary>
		/// <param name="check">Тестируемое значение</param>
		/// <param name="def">Возвращаемое значение по умолчанию</param>
		/// <returns>Значение</returns>
		public static DateTime? IfNull( this DateTime? check,DateTime? def )
		{
			return check.IsNoValue() ? def : check;
		}
		/// <summary>
		/// В случае, если отсутствует значение, вернуть значение по умолчанию,
		/// иначе исходное, к которому применяется операция notNullAction,
		/// если операция не задана, возвращается исходное значение
		/// Проверка значения осуществляется по тем же правилам, что и для IsNoValue
		/// </summary>
		/// <param name="check">Тестируемое значение</param>
		/// <param name="def">Возвращаемое значение по умолчанию</param>
		/// <param name="notNullAction">Дополнительная операция над значением, если оно не Null</param>
		/// <returns>Значение</returns>
		public static DateTime? IfNull( this DateTime? check,DateTime? def,Func<DateTime?,DateTime?> notNullAction )
		{
			return check.IsNoValue() ? def : notNullAction == null ? check : notNullAction( check );
		}

		/// <summary>
		/// В случае, если отсутствует значение, создать исключение с указанным сообщением
		/// Проверка значения осуществляется по тем же правилам, что и для IsNoValue
		/// </summary>
		/// <param name="check">Тестируемое значение</param>
		/// <param name="msg">Сообщение исключения, если Null</param>
		/// <returns>Значение</returns>
		public static DateTime? ThrowIfNull( this DateTime? check,string msg )
		{
			if(check.IsNoValue())
				throw new Exception(msg);
			return check;
		}
		/// <summary>
		/// В случае, если отсутствует значение, выдать указанное исключение
		/// Проверка значения осуществляется по тем же правилам, что и для IsNoValue
		/// </summary>
		/// <param name="check">Тестируемое значение</param>
		/// <param name="err">Исключение, если Null</param>
		/// <returns>Значение</returns>
		public static DateTime? ThrowIfNull( this DateTime? check,Exception err = null )
		{
			if(check.IsNoValue())
				throw err == null ? new Exception() : err;
			return check;
		}

		/// <summary>
		/// Преобразовать локальную дату/время в Universal дату/время
		/// </summary>
		/// <param name="val">Исходная дата/время</param>
		/// <returns>Universal дата/время</returns>
		public static DateTime? ToUniversalTime( this DateTime? val )
		{
			if(val.IsValue())
				return val.Value.ToUniversalTime();
			return val;
		}

		/// <summary>
		/// Преобразовать значение DateTime? в строку
		/// </summary>
		/// <param name="val">Значение DateTime?</param>
		/// <param name="format">Формат перобразования</param>
		/// <param name="formatProvider">Culture-specific formatting provider</param>
		/// <returns>Строковое значение</returns>
		public static string ToString( this DateTime? val,string format,IFormatProvider formatProvider = null )
		{
			return
				val == null ? null :
				!val.HasValue ? null :
				val.Value.ToString( format,formatProvider ?? CultureInfo.InvariantCulture );
		}
		/// <summary>
		/// Преобразовать значение DateTime? в строку с параметрами по умолчанию:
		///  формат даты ISO8601 - yyyy-MM-ddTHH:mm:ss.fff,
		///  InvariantCulture
		/// </summary>
		/// <param name="val">Значение DateTime?</param>
		/// <returns>Строковое значение</returns>
		public static string ToStringDefault( this DateTime? val )
		{
			return val.ToString( DateTimeFormatISO8601 );
		}

		/// <summary>
		/// Преобразовать значение DateTimeOffset? в строку
		/// </summary>
		/// <param name="val">Значение DateTimeOffset?</param>
		/// <param name="format">Формат перобразования</param>
		/// <param name="formatProvider">Culture-specific formatting provider</param>
		/// <returns>Строковое значение</returns>
		public static string ToString( this DateTimeOffset? val,string format,IFormatProvider formatProvider = null )
		{
			return
				val == null ? null :
				!val.HasValue ? null :
				val.Value.ToString( format,formatProvider ?? CultureInfo.InvariantCulture );
		}
		/// <summary>
		/// Преобразовать значение DateTimeOffset? в строку с параметрами по умолчанию:
		///  формат даты ISO8601 with timezone - yyyy-MM-ddTHH:mm:ss.fffzzz,
		///  InvariantCulture
		/// </summary>
		/// <param name="val">Значение DateTimeOffset?</param>
		/// <returns>Строковое значение</returns>
		public static string ToStringDefault( this DateTimeOffset? val )
		{
			return val.ToString( DateTimeFormatISO8601zone );
		}
	}
}
