//-----------------------------------------------------------------------
// Cls.CryptHelper.cs - описание вспомогательных методов для
//						шифрования/дешифрования данных
//
// Created by *** 01.03.2012
//-----------------------------------------------------------------------
using System;
using System.Text;


namespace Lib.Base
{
	/// <summary>
	/// Вспомогательные методы для шифрования/дешифрования данных
	/// </summary>
	public static class CryptHelper
	{
		/// <summary>
		/// Зашифровать массив байтов указанным байтовым ключом и упаковать ключ в
		/// полученный результат для возможности реализовать последующую расшифровку
		/// </summary>
		/// <param name="data">Исходные данные</param>
		/// <param name="key">Ключ</param>
		/// <param name="extendData">Флаг необходимости расширения данных, если их длина оказалась меньше, чем у ключа</param>
		/// <param name="extendValue">Значение, которым раширяются данные</param>
		/// <returns>Зашифрованные данные с упакованным в них ключом</returns>
		public static byte[] CryptWithKey( this byte[] data,byte[] key,bool extendData = false,byte extendValue = 0 )
		{
			byte[] crypt = null;

			int dataLen = data == null ? 0 : data.Length;
			if(dataLen > 0)
			{
				int keyLen = key.Length;
				int cryptLen = (extendData && dataLen < keyLen ? keyLen : dataLen) * 2;
				crypt = new byte[cryptLen];
				for(int id = 0, ik = 0, icd = 0, ick = cryptLen - 1; icd < cryptLen; icd += 2, ick -= 2,id++)
				{
					byte keyVal = key[ik < keyLen ? ik++ : ik = 0];
					crypt[icd] = (byte)((id < dataLen ? data[id] : extendValue) ^ keyVal);
					crypt[ick] = (byte)(keyVal ^ 0xFF);
				}
			}

			return crypt;
		}
		/// <summary>
		/// Расшифровать массив данных, содержаших упакованный в них ключ
		/// Метод обратный byte[].CryptWithKey
		/// </summary>
		/// <param name="crypt">Зашифрованный массив с упакованным ключом</param>
		/// <returns>Расшифрованный массив байтов</returns>
		public static byte[] DecryptWithKey( this byte[] crypt )
		{
			byte[] data = null;

			int cryptLen = crypt == null ? 0 : crypt.Length;
			if(cryptLen > 0)
			{
				data = new byte[cryptLen / 2];
				for(int id = 0, icd = 0, ick = cryptLen - 1; icd < cryptLen; icd += 2, ick -= 2, id++)
					data[id] = (byte)(crypt[icd] ^ (crypt[ick] ^ 0xFF));
			}

			return data;
		}

		/// <summary>
		/// Зашифровать строку и упаковать в полученный результат ключ для последующей расшифровки
		/// </summary>
		/// <param name="src">Исходная строка</param>
		/// <returns>Зашифрованные данные в байтовом виде с упакованным в них ключом</returns>
		public static byte[] CryptWithKey( this string src )
		{
			return Encoding.Unicode.GetBytes( src ).CryptWithKey( Guid.NewGuid().ToByteArray(),true,0 );
		}
		/// <summary>
		/// Расшифровать массив данных, содержаших упакованный в них ключ, в строку
		/// Метод обратный string.CryptWithKey
		/// </summary>
		/// <param name="crypt">Зашифрованный массив с упакованным ключом</param>
		/// <returns>Расшифрованная строка</returns>
		public static string DecryptWithKeyString( this byte[] crypt )
		{
			return Encoding.Unicode.GetString( crypt.DecryptWithKey() ).TrimEnd( '\0' );
		}
	}
}
