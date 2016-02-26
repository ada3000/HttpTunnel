//-----------------------------------------------------------------------
// Cls.CommonHelper.cs - описание общих вспомогательных методов
//
// Created by *** 19.06.2012
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;


namespace Lib.Base
{
	/// <summary>
	/// Общие вспомогательные методы
	/// </summary>
	public static class CommonHelper
	{
		/// <summary>
		/// Найти элемент массива по условию, если не найден возвращает null
		/// </summary>
		/// <typeparam name="T">Тип элемента массива</typeparam>
		/// <param name="array">Массив</param>
		/// <param name="match">Условие на элемент массива</param>
		/// <returns>Найденный элемент или null</returns>
		public static T Find<T>( this T[] array,Predicate<T> match ) where T : class
		{
			T item = null;
			for(int i = 0, count = array == null ? -1 : array.Length; item == null && i < count; i++)
			{
				if(match( array[i] ))
					item = array[i];
			}
			return item;
		}

		/// <summary>
		/// Добавить список элементов в набор уникальных значений
		/// </summary>
		/// <typeparam name="T">Тип объектов набора</typeparam>
		/// <param name="set">Набор уникальных значений</param>
		/// <param name="enumerator">Список элементов</param>
		/// <returns>Полученный набор уникальных значений</returns>
		public static HashSet<T> AddRange<T>( this HashSet<T> set,IEnumerable<T> enumerator )
		{
			if(set != null && enumerator != null)
			{
				foreach(var item in enumerator)
					set.Add( item );
			}
			return set;
		}
	}
}
