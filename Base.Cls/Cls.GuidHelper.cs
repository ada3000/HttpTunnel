//-----------------------------------------------------------------------
// Cls.GuidHelper.cs - описание вспомогательных методов
//						для работы с GUID-ами
//
// Created by *** 29.03.2012
//-----------------------------------------------------------------------
using System;
using System.Text;


namespace Lib.Base
{
	/// <summary>
	/// Вспомогательные методы для работы с GUID-ами
	/// </summary>
	public static class GuidHelper
	{
		/// <summary>
		/// Проверить отсутствие значения
		/// Считается, что у Guid? отсутствует значение, если:
		/// - объект = null
		/// - не установлен флаг HasValue
		/// </summary>
		/// <param name="val">Тестируемое значение</param>
		/// <returns>true - значение отсутствует, иначе - false</returns>
		public static bool IsNoValue( this Guid? val )
		{
			return val == null || !val.HasValue;
		}
		/// <summary>
		/// Проверить наличие значения
		/// Операция противоположна IsNoValue
		/// </summary>
		/// <param name="val">Тестируемое значение</param>
		/// <returns>true - значение имеется, иначе - false</returns>
		public static bool IsValue( this Guid? val )
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
		public static Guid? IfNull( this Guid? check,Guid? def )
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
		public static Guid? IfNull( this Guid? check,Guid? def,Func<Guid?,Guid?> notNullAction )
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
		public static Guid? ThrowIfNull( this Guid? check,string msg )
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
		public static Guid? ThrowIfNull( this Guid? check,Exception err = null )
		{
			if(check.IsNoValue())
				throw err == null ? new Exception() : err;
			return check;
		}

		private const string ObsoleteToADOGuidMessage = "Этот метод был создан для корректной работы с unmanaged ADO, теперь его нет... Не будь лохом! Подумай, нужен ли тебе в этом месте данный метод";

		/// <summary>
		/// Сформировать "правильный" GUID, который понимает unmanaged ADO (с фигурными скобками { и })
		/// </summary>
		/// <param name="guid">Оригинальная структура GUID</param>
		/// <returns>Сроковое представление GUID с фигурными скобками { и }</returns>
		[Obsolete(ObsoleteToADOGuidMessage)]
		public static string ToADOGuid( this Guid guid )
		{
			return guid == null ? null : guid.ToString( "B" ).ToUpper();
		}
		/// <summary>
		/// Сформировать "правильный" GUID, который понимает unmanaged ADO (с фигурными скобками { и })
		/// </summary>
		/// <param name="guid">Оригинальная структура GUID</param>
		/// <returns>Сроковое представление GUID с фигурными скобками { и }</returns>
		[Obsolete(ObsoleteToADOGuidMessage)]
		public static string ToADOGuid( this Guid? guid )
		{
			return guid == null || !guid.HasValue ? null : guid.Value.ToString( "B" ).ToUpper();
		}
		/// <summary>
		/// Сформировать "правильный" GUID, который понимает unmanaged ADO (с фигурными скобками { и })
		/// </summary>
		/// <param name="guid">Исходное строковое представление GUID</param>
		/// <returns>Сроковое представление GUID с фигурными скобками { и }</returns>
		[Obsolete(ObsoleteToADOGuidMessage)]
		public static string ToADOGuid( this string guid )
		{
			if(guid.IsNoValue()) return null;

			bool addFirst = guid[0] != '{';
			bool addLast = guid[guid.Length - 1] != '}';

			int addLength = (addFirst ? 1 : 0) + (addLast ? 1 : 0);
			if(addLength > 0)
			{
				StringBuilder result = new StringBuilder(guid.Length + addLength);
				if(addFirst) result.Append( '{' );
				result.Append( guid );
				if(addLast) result.Append( '}' );
				guid = result.ToString();
			}

			//if(addFirst && addLast)
			//	guid = '{' + guid + '}';
			//else if(addFirst)
			//	guid = '{' + guid;
			//else if(addLast)
			//	guid += '}';

			return guid.ToUpper();
		}
	}
}
