//-----------------------------------------------------------------------
// Cls.Sequence.cs - классы целочисленных последовательностей
//
// Created by *** 12.02.2013
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;


namespace Lib.Base
{
	/// <summary>
	/// Класс методов последовательности положительных целочисленных значений
	/// </summary>
	public static class Sequence
	{
		/// <summary>
		/// Ограничение счётчика
		/// </summary>
		private const long CounterMax = 0x00007FFF;
		/// <summary>
		/// Счётчик последовательности
		/// </summary>
		private static long _counter = CounterMax + 1;
		/// <summary>
		/// Базовая часть последовательности
		/// </summary>
		public readonly static SeedParams Seed = new SeedParams();

		/// <summary>
		/// Получить новый идентификатор
		/// </summary>
		/// <returns>Целочисленное значение</returns>
		public static long NewValue()
		{
			long seed = 0;
			long counter = 0;
			lock(Seed)
			{
				if(_counter > CounterMax)
				{
					seed = Seed.Renew();
					_counter = 0;
				}
				else
					seed = Seed.Value;
				counter = _counter++;
			}
			return seed | (counter << 48);
		}

		/// <summary>
		/// Класс параметров базовой части последовательности
		/// Под значение отводятся младшие 48 бит - 0x0000FFFFFFFFFFFF
		/// </summary>
		public sealed class SeedParams
		{
			private long _value = 0;
			private long _generation = 0;
			private long _duplicates = 0;
			private readonly HashSet<long> _exists = new HashSet<long>();

			/// <summary>
			/// Значение базовой части последовательности
			/// </summary>
			public long Value { get { return _value; } }
			/// <summary>
			/// Текущее поколение базовой части последовательности
			/// </summary>
			public long Generation { get { return _generation; } }
			/// <summary>
			/// Кол-во дублей базовой части последовательности, полученных в данном объекте
			/// </summary>
			public long Duplicates { get { return _duplicates; } }
			/// <summary>
			/// Используемые ранее базовые части последовательности данного объекта
			/// </summary>
			public List<long> Exists { get { lock(this) return new List<long>(_exists); } }

			/// <summary>
			/// Сгенерировать уникальное значение базовой части последовательности,
			/// !!! метод не потоко-безопасен !!!
			/// </summary>
			/// <returns>Целочисленное значение</returns>
			private long _generate()
			{
				long seed = 0;
				while(seed == 0)
				{
					long rand = BitConverter.ToUInt32( Guid.NewGuid().ToByteArray(),0 );
					long time = DateTime.UtcNow.Ticks;
					if(_exists.Contains( seed = rand | ((time & 0x000000000000FFFF) << 32) ))
					{
						_duplicates++;
						seed = 0;
					}
				}
				_exists.Add( seed );
				return seed;
			}

			/// <summary>
			/// Получить новое значение базовой части последовательности
			/// </summary>
			/// <returns>Целочисленное значение</returns>
			public long Create()
			{
				lock(this) return _generate();
			}

			/// <summary>
			/// Обновить используемое значение базовой части последовательности
			/// </summary>
			/// <returns>Установленное целочисленное значение</returns>
			public long Renew()
			{
				lock(this)
				{
					_value = _generate();
					_generation++;
					return _value;
				}
			}
		}
	}
}
