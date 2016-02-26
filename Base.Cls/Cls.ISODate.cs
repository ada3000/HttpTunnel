using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Lib.DatesWorker
{
	/// <summary>
	/// Перечисление представлений даты в строковом виде
	/// </summary>
	public enum ISODateStyle
	{
		/// <summary>
		/// Дата представлена с максимальной точностью (включая дробные доли секунд)
		/// </summary>
		Full,
		/// <summary>
		/// Дата представлена с точностью до секунд
		/// </summary>
		Seconds
	}

	/// <summary>
	/// Контейнер методов по работе с датой в формате ISO 8601
	/// </summary>
	public static class ISODateHelper
	{
		private static readonly Dictionary<ISODateStyle, string> formats = new Dictionary<ISODateStyle, string>()
        {
			{ ISODateStyle.Full, "yyyy-MM-ddTHH:mm:ss.fffffffK" },
			{ ISODateStyle.Seconds, "yyyy-MM-ddTHH:mm:ssK" }
        };

		/// <summary>
		/// Возвращает формат преобразования даты к строке
		/// </summary>
		/// <param name="style">Представление даты</param>
		/// <returns>Формат преобразования даты к строке</returns>
		public static string GetFormat(ISODateStyle style)
		{
			return formats[style];
		}

		/// <summary>
		/// Преобразует дату к строке в соответствии со стандартом ISO 8601
		/// </summary>
		/// <param name="date">Дата</param>
		/// <param name="style">Представление даты</param>
		/// <returns>Строковое представление даты</returns>
		public static string ToISO(this DateTime date, ISODateStyle style = ISODateStyle.Seconds)
		{
			return date.ToString(GetFormat(style));
		}

		/// <summary>
		/// Преобразует строковое представление к дата в соответствии со стандартом ISO 8601
		/// </summary>
		/// <param name="date">Строковое представление даты</param>
		/// <returns>Дата</returns>
		public static DateTime ToISO(this string date)
		{
			return DateTime.Parse(date, null, System.Globalization.DateTimeStyles.RoundtripKind);
		}

		/// <summary>
		/// Выполняет попытку преобразования строкового представления к дате
		/// </summary>
		/// <param name="date">Строковое представление даты</param>
		/// <returns>Дата</returns>
		/// <remarks>
		/// При неудачной попытке преобразования возвращаемое значение будет null
		/// </remarks>
		public static DateTime? TryToISO(this string date)
		{
			DateTime result;
			if (DateTime.TryParse(date, null, System.Globalization.DateTimeStyles.RoundtripKind, out result))
				return result;

			return null;
		}
	}
}