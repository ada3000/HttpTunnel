//-----------------------------------------------------------------------
// Cls.ByteHelper.cs - описание вспомогательных методов для работы с
//					байтовыми типами byte и byte[]
//
// Created by *** 01.03.2012
//-----------------------------------------------------------------------
using System;
using System.Text;


namespace Lib.Base
{
	/// <summary>
	/// Вспомогательные методы для работы с объектами типа byte и byte[]
	/// </summary>
	public static class ByteHelper
	{
		/// <summary>
		/// Преобразовать символьное шестнадцатиричное значение в байтовое
		/// </summary>
		/// <param name="c">Исходное шестнадцетиричное значение (напр: 1F)</param>
		/// <returns>Полученное значение байта</returns>
		public static byte FromHex( this char c )
		{
			return (byte)(
				c >= '0' && c <= '9' ? c - '0' :
				c >= 'A' && c <= 'F' ? c - 'A' + 10 :
				c >= 'a' && c <= 'f' ? c - 'a' + 10 :
				0);
		}

		/// <summary>
		/// Выполнить битовую операцию xor над элементами байтового массива
		/// </summary>
		/// <param name="arr">Исходный массив</param>
		/// <param name="mask">Применяемая маска</param>
		/// <returns>Исходный массив после применения xor</returns>
		public static byte[] Xor( this byte[] arr,byte mask )
		{
			for(int i = 0, count = arr == null ? 0 : arr.Length; i < count; i++)
				arr[i] ^= mask;
			return arr;
		}
		/// <summary>
		/// Выполнить битовую операцию or над элементами байтового массива
		/// </summary>
		/// <param name="arr">Исходный массив</param>
		/// <param name="mask">Применяемая маска</param>
		/// <returns>Исходный массив после применения or</returns>
		public static byte[] Or( this byte[] arr,byte mask )
		{
			for(int i = 0, count = arr == null ? 0 : arr.Length; i < count; i++)
				arr[i] |= mask;
			return arr;
		}
		/// <summary>
		/// Выполнить битовую операцию and над элементами байтового массива
		/// </summary>
		/// <param name="arr">Исходный массив</param>
		/// <param name="mask">Применяемая маска</param>
		/// <returns>Исходный массив после применения and</returns>
		public static byte[] And( this byte[] arr,byte mask )
		{
			for(int i = 0, count = arr == null ? 0 : arr.Length; i < count; i++)
				arr[i] &= mask;
			return arr;
		}

		/// <summary>
		/// Выполнить битовую операцию xor над элементами байтового массива,
		/// используя маску на несколько байтов
		/// </summary>
		/// <param name="arr">Исходный массив</param>
		/// <param name="mask">Применяемая маска</param>
		/// <returns>Исходный массив после применения xor</returns>
		public static byte[] Xor( this byte[] arr,byte[] mask )
		{
			int maskLen = mask.Length;
			for(int i = 0, m = 0, count = arr == null ? 0 : arr.Length; i < count; i++)
				arr[i] ^= mask[m < maskLen ? m++ : m = 0];
			return arr;
		}
		/// <summary>
		/// Выполнить битовую операцию or над элементами байтового массива,
		/// используя маску на несколько байтов
		/// </summary>
		/// <param name="arr">Исходный массив</param>
		/// <param name="mask">Применяемая маска</param>
		/// <returns>Исходный массив после применения or</returns>
		public static byte[] Or( this byte[] arr,byte[] mask )
		{
			int maskLen = mask.Length;
			for(int i = 0, m = 0, count = arr == null ? 0 : arr.Length; i < count; i++)
				arr[i] |= mask[m < maskLen ? m++ : m = 0];
			return arr;
		}
		/// <summary>
		/// Выполнить битовую операцию and над элементами байтового массива,
		/// используя маску на несколько байтов
		/// </summary>
		/// <param name="arr">Исходный массив</param>
		/// <param name="mask">Применяемая маска</param>
		/// <returns>Исходный массив после применения and</returns>
		public static byte[] And( this byte[] arr,byte[] mask )
		{
			int maskLen = mask.Length;
			for(int i = 0, m = 0, count = arr == null ? 0 : arr.Length; i < count; i++)
				arr[i] &= mask[m < maskLen ? m++ : m = 0];
			return arr;
		}

		/// <summary>
		/// Поменять порядок следования байтов в массиве на противоположный
		/// </summary>
		/// <param name="arr">Исходный массив</param>
		/// <returns>Исходный массив с изменнённым порядком байтов</returns>
		public static byte[] Reverse( this byte[] arr )
		{
			for(int start = 0, end = arr == null ? 0 : arr.Length - 1; start < end; start++, end--)
			{
				byte b = arr[start];
				arr[start] = arr[end];
				arr[end] = b;
			}
			return arr;
		}

		/// <summary>
		/// Преобразовать строковое шестнадцетиричное значение в байтовый массив
		/// </summary>
		/// <param name="src">Строковое значение (напр: 1F22EA5B)</param>
		/// <param name="start">Начальный индекс в строке, если не указан (0), с начала</param>
		/// <param name="len">Длинна преобразуемой подстроки, если не указана (-1), до конца</param>
		/// <returns>Полученный байтовый массив</returns>
		public static byte[] TranslateToByteArray( this string src,int start = 0,int len = -1 )
		{
			int srcLen = src == null ? 0 : src.Length;
			if(start < 0) start = 0;
			else if(start > srcLen) start = srcLen;
			if(len < 0 || len + start > srcLen) len = srcLen - start;
			int end = start + len;
			len = (len >> 1) + (len & 1);
			byte[] bytes = len == 0 ? null : new byte[len];
			for(int i = start, n = 0; i < end; n++)
			{
				int b = src[i++].FromHex() << 4;
				if(i < end) b |= src[i++].FromHex();
				bytes[n] = (byte)(b & 0x00FF);
			}
			return bytes;
		}
		/// <summary>
		/// Преобразовать байтовый массив в шестнадцетиричное строковое значение
		/// </summary>
		/// <param name="src">Исходный байтовый массив</param>
		/// <param name="start">Начальный индекс в массиве, если не указан (0), с начала</param>
		/// <param name="len">Кол-во преобразуемых байтов, если не указана (-1), до конца</param>
		/// <returns>Строковое шестнадцетиричное значение</returns>
		public static string TranslateToString( this byte[] src,int start = 0,int len = -1 )
		{
			int srcLen = src == null ? 0 : src.Length;
			if(start < 0) start = 0;
			else if(start > srcLen) start = srcLen;
			if(len < 0 || len + start > srcLen) len = srcLen - start;
			int end = start + len;
			StringBuilder str = len == 0 ? null : new StringBuilder(len << 1);
			for(int i = start; i < end; i++)
				str.AppendFormat( "{0:X2}",src[i] );
			return str == null ? null : str.ToString();
		}

		/// <summary>
		/// Проверить отсутствие значения
		/// Считается, что отсутствует значение, если:
		/// - объект = null
		/// - длина массива == 0
		/// </summary>
		/// <param name="val">Тестируемое значение</param>
		/// <returns>true - значение отсутствует, иначе - false</returns>
		public static bool IsNoValue( this byte[] val )
		{
			return val == null || val.Length <= 0;
		}
		/// <summary>
		/// Проверить наличие значения
		/// Операция противоположна IsNoValue
		/// </summary>
		/// <param name="val">Тестируемое значение</param>
		/// <returns>true - значение имеется, иначе - false</returns>
		public static bool IsValue( this byte[] val )
		{
			return val != null && val.Length > 0;
		}

		/// <summary>
		/// В случае, если отсутствует значение, вернуть значение по умолчанию, иначе - исходное
		/// Проверка значения осуществляется по тем же правилам, что и для IsNoValue
		/// </summary>
		/// <param name="check">Тестируемое значение</param>
		/// <param name="def">Возвращаемое значение по умолчанию</param>
		/// <returns>Значение</returns>
		public static object IfNull( this byte[] check,byte[] def )
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
		public static object IfNull( this byte[] check,byte[] def,Func<object,object> notNullAction )
		{
			return check.IsNoValue() ? def : notNullAction == null ? check : notNullAction( check );
		}

		/// <summary>
		/// В случае, если отсутствует значение, создать исключение с указанным сообщением
		/// Проверка значения осуществляется по тем же правилам, что и для IsNoValue
		/// </summary>
		/// <param name="check">Тестируемое значение</param>
		/// <param name="msg">Сообщение исключения, если объект Null</param>
		/// <returns>Значение</returns>
		public static object ThrowIfNull( this byte[] check,string msg )
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
		/// <param name="err">Исключение, если объект Null</param>
		/// <returns>Значение</returns>
		public static object ThrowIfNull( this byte[] check,Exception err = null )
		{
			if(check.IsNoValue())
				throw err == null ? new Exception() : err;
			return check;
		}

		/// <summary>
		/// Преобразовать к значению типа Guid
		/// </summary>
		/// <param name="val">Преобразуемое значение</param>
		/// <param name="def">Значение по умолчанию, если отсутствует значение или невозможно преобразовать</param>
		/// <param name="throwOnError">Флаг создания исключения, в случае невозможности выполнить преобразование</param>
		/// <returns>Значение типа Guid</returns>
		public static Guid ToGuid( this byte[] val,Guid def,bool throwOnError = true )
		{
			Guid result;

			if(val.IsNoValue())
				result = def;
			else if(throwOnError)
				result = new Guid(val);
			else try
			{
				result = new Guid(val);
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
		public static Guid? ToGuid( this byte[] val,bool throwOnError = true )
		{
			Guid? result = null;

			if(val.IsValue())
			{
				if(throwOnError)
					result = new Guid(val);
				else try
				{
					result = new Guid(val);
				}
				catch
				{
				}
			}

			return result;
		}

		/// <summary>
		/// Преобразовать буфер построения строки в байтовый массив значений Unicode символов.
		/// Если необходимо, по завершении преобразования в массив, буфер очищается от содержимого
		/// </summary>
		/// <param name="builder">Буфер построения строки</param>
		/// <param name="clear">Флаг очистки буфера после преобразования в массив байт</param>
		/// <param name="encoding">Используемая кодировка строки, по умолчанию Unicode</param>
		/// <returns>Байтовый массив</returns>
		public static byte[] ToByteArray( this StringBuilder builder,bool clear,Encoding encoding = null )
		{
			byte[] array = builder.Length > 0 ? (encoding ?? Encoding.Unicode).GetBytes( builder.ToString() ) : null;
			if(clear) builder.Clear();
			return array;
		}
	}
}
