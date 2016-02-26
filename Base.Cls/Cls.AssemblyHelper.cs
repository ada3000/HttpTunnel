//-----------------------------------------------------------------------
// Cls.AssemblyHelper.cs - описание вспомогательных методов для работы
//						со сборками и управления объектами
//
// Created by *** 01.03.2012
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography;


namespace Lib.Base
{
	/// <summary>
	/// Вспомогательные методы для работы с Assembly
	/// </summary>
	public static class AssemblyHelper
	{
		/// <summary>
		/// Получить доступ к интерфейсу сборки
		/// В случае необходимости сборка загружается
		/// </summary>
		/// <param name="assemblyName">Название сборки</param>
		/// <returns>Зкземпляр сборки</returns>
		public static Assembly GetAssembly( string assemblyName )
		{
			Assembly assembly = null;

			lock(AppDomain.CurrentDomain)
			{
				var assemblies = AppDomain.CurrentDomain.GetAssemblies();
				for(int i = 0, count = assemblies == null ? 0 : assemblies.Length; i < count && assembly == null; i++)
				{
					if(assemblies[i].GetName().Name.Equals( assemblyName,StringComparison.InvariantCultureIgnoreCase ))
						assembly = assemblies[i];
				}

				if(assembly == null)
				{
					assembly = AppDomain.CurrentDomain.Load( assemblyName );
					if(assembly == null)
						throw new Exception(string.Format( "Не удалось загрузить сборку '{0}'",assemblyName ));
				}
			}

			return assembly;
		}

		/// <summary>
		/// Получить тип объекта класса из указанной сборки
		/// В случае необходимости сборка загружается
		/// </summary>
		/// <param name="assemblyName">Название сборки</param>
		/// <param name="typeName">Название класса</param>
		/// <returns>Тип объекта</returns>
		public static Type GetType( string assemblyName,string typeName )
		{
			Type type = null;
			if(assemblyName.IsValue())
				type = GetAssembly( assemblyName ).GetType( typeName );
			else
			{
				var assemblies = AppDomain.CurrentDomain.GetAssemblies();
				for(int i = 0, count = assemblies == null ? 0 : assemblies.Length; i < count && type == null; i++)
					type = assemblies[i].GetType( typeName );
			}
			return type;
		}

		/// <summary>
		/// Динамически создать объект класса из указанной сборки
		/// В случае необходимости сборка загружается
		/// </summary>
		/// <param name="assemblyName">Название сборки</param>
		/// <param name="typeName">Название класса</param>
		/// <returns>Созданный объект</returns>
		public static object CreateInstance( string assemblyName,string typeName )
		{
			object obj = GetAssembly( assemblyName ).CreateInstance( typeName );

			if(obj == null)
				throw new Exception(string.Format( "Не удалось создать объект '{0}' из сборки '{1}'",typeName,assemblyName ));

			return obj;
		}

		/// <summary>
		/// Пул объектов для распределения
		/// </summary>
		private static List<object> _objects = new List<object>();

		/// <summary>
		/// Получить объект класса из указанной сборки
		/// Объект получается пула свободных для распределения, либо создаётся новый (если нет доступных в пуле)
		/// В случае необходимости сборка загружается
		/// </summary>
		/// <param name="assemblyName">Название сборки</param>
		/// <param name="typeName">Название класса</param>
		/// <returns>Полученный объект</returns>
		public static object GetObject( string assemblyName,string typeName )
		{
			object obj = null;

			lock(_objects)
			{
				int index = _objects.FindIndex( delegate( object item ) { return typeName == item.GetType().FullName; } );
				if(index != -1)
				{
					obj = _objects[index];
					_objects.RemoveAt( index );
				}
			}

			return obj ?? CreateInstance( assemblyName,typeName );
		}

		/// <summary>
		/// Вернуть объект в пул для дальнейшего распределения
		/// </summary>
		/// <param name="obj">Помещаемый объект</param>
		public static void PoolObject( object obj )
		{
			if(obj != null)
				lock(_objects) _objects.Add( obj );
		}

		/// <summary>
		/// Получить открытый ключ подписи сборки
		/// </summary>
		/// <param name="assembly">Сборка</param>
		/// <returns>Открытый ключ</returns>
		public static byte[] GetSignKey( this Assembly assembly )
		{
			byte[] key = null;
			if(assembly != null)
			{
				byte[] assemblyKey = assembly.GetName().GetPublicKey();
				if(assemblyKey.Length > 12)
				{
					key = new byte[assemblyKey.Length - 12];
					Array.Copy( assemblyKey,12,key,0,key.Length );
				}
			}
			return key;
		}
		/// <summary>
		/// Получить алгоритм шифрования с открытым ключом сборки
		/// </summary>
		/// <typeparam name="T">Тип алгоритма шифрования</typeparam>
		/// <param name="assembly">Сборка</param>
		/// <param name="alg">Алгоритм шифрования, которому необходимо установить ключ</param>
		/// <param name="errOnEmptyKey">Исключение, в случае отсутствия публичного ключа у сборки, если не задано, исключение не вызывается</param>
		/// <returns>Алгоритм шифрования с установленным открытым ключом сборки</returns>
		public static AsymmetricAlgorithm GetSignAlgorithm<T>( this Assembly assembly,T alg,Exception errOnEmptyKey = null )
			where T : AsymmetricAlgorithm, ICspAsymmetricAlgorithm
		{
			if(alg != null)
			{
				var key = assembly.GetSignKey();
				if(key != null && key.Length > 0)
					alg.ImportCspBlob( key );
				else if(errOnEmptyKey != null)
					throw errOnEmptyKey;
			}
			return alg;
		}
	}
}
