//-----------------------------------------------------------------------
// Cls.ObjectHelper.cs - описание вспомогательных методов для работы с
//						типом object
//
// Created by *** 01.03.2012
//-----------------------------------------------------------------------
using System;


namespace Lib.Base
{
	/// <summary>
	/// Вспомогательные методы для работы с объектами типа object
	/// </summary>
	public static class ObjectHelper
	{
		/// <summary>
		/// Проверить отсутствие значения у объекта
		/// Считается, что у object отсутствует значение, если:
		/// - объект = null
		/// - значение объекта = System.DBNull
		/// </summary>
		/// <param name="val">Тестируемое значение</param>
		/// <returns>true - значение отсутствует, иначе - false</returns>
		public static bool IsNoValue( this object val )
		{
			//return val == null || Convert.IsDBNull( val ); // !!! так тормознее раз в 10 !!!
			return val == null || val is DBNull;
		}
		/// <summary>
		/// Проверить наличие значения у объекта
		/// Операция противоположна IsNoValue
		/// </summary>
		/// <param name="val">Тестируемое значение</param>
		/// <returns>true - значение имеется, иначе - false</returns>
		public static bool IsValue( this object val )
		{
			//return val != null && !Convert.IsDBNull( val ); // !!! так тормознее раз в 10 !!!
			return val != null && !(val is DBNull);
		}

		/// <summary>
		/// В случае, если у объекта отсутствует значение, вернуть значение по умолчанию, иначе - исходное
		/// Проверка значения осуществляется по тем же правилам, что и для IsNoValue
		/// </summary>
		/// <param name="check">Тестируемое значение</param>
		/// <param name="def">Возвращаемое значение по умолчанию</param>
		/// <returns>Значение</returns>
		public static object IfNull( this object check,object def )
		{
			return check.IsNoValue() ? def : check;
		}
		/// <summary>
		/// В случае, если у объекта отсутствует значение, вернуть значение по умолчанию,
		/// иначе исходное, к которому применяется операция notNullAction,
		/// если операция не задана, возвращается исходное значение
		/// Проверка значения осуществляется по тем же правилам, что и для IsNoValue
		/// </summary>
		/// <param name="check">Тестируемое значение</param>
		/// <param name="def">Возвращаемое значение по умолчанию</param>
		/// <param name="notNullAction">Дополнительная операция над значением, если оно не Null</param>
		/// <returns>Значение</returns>
		public static object IfNull( this object check,object def,Func<object,object> notNullAction )
		{
			return check.IsNoValue() ? def : notNullAction == null ? check : notNullAction( check );
		}

		/// <summary>
		/// В случае, если у объекта отсутствует значение, создать исключение с указанным сообщением
		/// Проверка значения осуществляется по тем же правилам, что и для IsNoValue
		/// </summary>
		/// <param name="check">Тестируемое значение</param>
		/// <param name="msg">Сообщение исключения, если объект Null</param>
		/// <returns>Значение</returns>
		public static object ThrowIfNull( this object check,string msg )
		{
			if(check.IsNoValue())
				throw new Exception(msg);
			return check;
		}
		/// <summary>
		/// В случае, если у объекта отсутствует значение, выдать указанное исключение
		/// Проверка значения осуществляется по тем же правилам, что и для IsNoValue
		/// </summary>
		/// <param name="check">Тестируемое значение</param>
		/// <param name="err">Исключение, если объект Null</param>
		/// <returns>Значение</returns>
		public static object ThrowIfNull( this object check,Exception err = null )
		{
			if(check.IsNoValue())
				throw err == null ? new Exception() : err;
			return check;
		}

		/// <summary>
		/// Преобразовать к значению типа short
		/// </summary>
		/// <param name="val">Преобразуемое значение</param>
		/// <param name="def">Значение по умолчанию, если отсутствует значение или невозможно преобразовать</param>
		/// <param name="throwOnError">Флаг создания исключения, в случае невозможности выполнить преобразование</param>
		/// <returns>Значение типа short</returns>
		public static short ToShort( this object val,short def = 0,bool throwOnError = true )
		{
			short result = 0;

			if(val.IsNoValue())
				result = def;
			else if(throwOnError)
				result = (short)Convert.ChangeType( val,typeof(short) );
			else try
			{
				result = (short)Convert.ChangeType( val,typeof(short) );
			}
			catch
			{
				result = def;
			}

			return result;
		}

		/// <summary>
		/// Преобразовать к значению типа int
		/// </summary>
		/// <param name="val">Преобразуемое значение</param>
		/// <param name="def">Значение по умолчанию, если отсутствует значение или невозможно преобразовать</param>
		/// <param name="throwOnError">Флаг создания исключения, в случае невозможности выполнить преобразование</param>
		/// <returns>Значение типа int</returns>
		public static int ToInt( this object val,int def = 0,bool throwOnError = true )
		{
			int result = 0;

			if(val.IsNoValue())
				result = def;
			else if(throwOnError)
				result = (int)Convert.ChangeType( val,typeof(int) );
			else try
			{
				result = (int)Convert.ChangeType( val,typeof(int) );
			}
			catch
			{
				result = def;
			}

			return result;
		}

		/// <summary>
		/// Преобразовать к значению типа long
		/// </summary>
		/// <param name="val">Преобразуемое значение</param>
		/// <param name="def">Значение по умолчанию, если отсутствует значение или невозможно преобразовать</param>
		/// <param name="throwOnError">Флаг создания исключения, в случае невозможности выполнить преобразование</param>
		/// <returns>Значение типа long</returns>
		public static long ToLong( this object val,long def = 0,bool throwOnError = true )
		{
			long result = 0;

			if(val.IsNoValue())
				result = def;
			else if(throwOnError)
				result = (long)Convert.ChangeType( val,typeof(long) );
			else try
			{
				result = (long)Convert.ChangeType( val,typeof(long) );
			}
			catch
			{
				result = def;
			}

			return result;
		}

		/// <summary>
		/// Преобразовать к значению типа double
		/// </summary>
		/// <param name="val">Преобразуемое значение</param>
		/// <param name="def">Значение по умолчанию, если отсутствует значение или невозможно преобразовать</param>
		/// <param name="throwOnError">Флаг создания исключения, в случае невозможности выполнить преобразование</param>
		/// <returns>Значение типа double</returns>
		public static double ToDouble( this object val,double def = 0,bool throwOnError = true )
		{
			double result = 0;

			if(val.IsNoValue())
				result = def;
			else if(throwOnError)
				result = (double)Convert.ChangeType( val,typeof(double) );
			else try
			{
				result = (double)Convert.ChangeType( val,typeof(double) );
			}
			catch
			{
				result = def;
			}

			return result;
		}

		/// <summary>
		/// Преобразовать к значению типа bool
		/// </summary>
		/// <param name="val">Преобразуемое значение</param>
		/// <param name="def">Значение по умолчанию, если отсутствует значение или невозможно преобразовать</param>
		/// <param name="throwOnError">Флаг создания исключения, в случае невозможности выполнить преобразование</param>
		/// <returns>Значение типа bool</returns>
		public static bool ToBool( this object val,bool def = false,bool throwOnError = true )
		{
			bool result = false;

			if(val.IsNoValue())
				result = def;
			else if(throwOnError)
				result = (bool)Convert.ChangeType( val,typeof(bool) );
			else try
			{
				result = (bool)Convert.ChangeType( val,typeof(bool) );
			}
			catch
			{
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
		public static DateTime? ToDateTime( this object val,DateTime? def = null,bool throwOnError = true )
		{
			DateTime? result = null;

			if(val.IsNoValue())
				result = def;
			else if(throwOnError)
				result = (DateTime?)Convert.ChangeType( val,typeof(DateTime) );
			else try
			{
				result = (DateTime?)Convert.ChangeType( val,typeof(DateTime) );
			}
			catch
			{
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
		public static DateTime ToDateTime( this object val,DateTime def,bool throwOnError = true )
		{
			DateTime result;

			if(val.IsNoValue())
				result = def;
			else if(throwOnError)
				result = (DateTime)Convert.ChangeType( val,typeof(DateTime) );
			else try
			{
				result = (DateTime)Convert.ChangeType( val,typeof(DateTime) );
			}
			catch
			{
				result = def;
			}

			return result;
		}

		/// <summary>
		/// Преобразовать к значению типа DateTimeOffset?
		/// </summary>
		/// <param name="val">Исходное значение</param>
		/// <param name="def">Значение по умолчанию, если отсутствует исходное</param>
		/// <param name="throwOnError">Флаг создания исключения, в случае невозможности выполнить преобразование</param>
		/// <returns>Значение типа DateTimeOffset?</returns>
		public static DateTimeOffset? ToDateTimeOffset( this object val,DateTimeOffset? def = null,bool throwOnError = true )
		{
			DateTimeOffset? result = null;

			if(val.IsNoValue())
				result = def;
			else if(val is DateTimeOffset)
				result = (DateTimeOffset?)val;
			else
				result = val.ToString().ToDateTimeOffset( def,throwOnError );

			return result;
		}
		/// <summary>
		/// Преобразовать к значению типа DateTimeOffset
		/// </summary>
		/// <param name="val">Исходное значение</param>
		/// <param name="def">Значение по умолчанию, если отсутствует исходное</param>
		/// <param name="throwOnError">Флаг создания исключения, в случае невозможности выполнить преобразование</param>
		/// <returns>Значение типа DateTimeOffset</returns>
		public static DateTimeOffset ToDateTimeOffset( this object val,DateTimeOffset def,bool throwOnError = true )
		{
			DateTimeOffset result;

			if(val.IsNoValue())
				result = def;
			else if(val is DateTimeOffset)
				result = (DateTimeOffset)val;
			else
				result = val.ToString().ToDateTimeOffset( def,throwOnError );

			return result;
		}

		/// <summary>
		/// Преобразовать к значению типа Guid
		/// </summary>
		/// <param name="val">Преобразуемое значение</param>
		/// <param name="def">Значение по умолчанию, если отсутствует значение или невозможно преобразовать</param>
		/// <param name="throwOnError">Флаг создания исключения, в случае невозможности выполнить преобразование</param>
		/// <returns>Значение типа Guid</returns>
		public static Guid ToGuid( this object val,Guid def,bool throwOnError = true )
		{		
			Guid result;

			if(val.IsNoValue())
				result = def;
			else if(val is Guid)
				result = (Guid)val;
			else if(val is string)
				result = ((string)val).ToGuid( def,throwOnError );
			else if(val is byte[])
				result = ((byte[])val).ToGuid( def,throwOnError );
			else if(throwOnError)
				result = (Guid)Convert.ChangeType( val,typeof(Guid) );
			else try
			{
				result = (Guid)Convert.ChangeType( val,typeof(Guid) );
			}
			catch
			{
				result = def;
			}

			return result;
		}
		/// <summary>
		/// Преобразовать к значению типа Guid?
		/// </summary>
		/// <param name="val">Преобразуемое значение</param>
		/// <param name="throwOnError">Флаг создания исключения, в случае невозможности выполнить преобразование</param>
		/// <returns>Значение типа Guid?</returns>
		public static Guid? ToGuid( this object val,bool throwOnError = true )
		{		
			Guid? result = null;

			if(val.IsValue())
			{
				if(val is Guid)
					result = (Guid)val;
				else if(val is string)
					result = ((string)val).ToGuid( throwOnError );
				else if(val is byte[])
					result = ((byte[])val).ToGuid( throwOnError );
				else if(throwOnError)
					result = (Guid)Convert.ChangeType( val,typeof(Guid) );
				else try
				{
					result = (Guid)Convert.ChangeType( val,typeof(Guid) );
				}
				catch
				{
				}
			}

			return result;
		}
	}
}
