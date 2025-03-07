﻿using System;
using System.Reflection;
using ProtoBuf.Meta;

namespace Havit.Blazor.Grpc.Core
{
	/// <summary>
	/// Replaces need for [DataContract] and [DataMember] attributes for gRPC stack.
	/// Known limitation: Currently does not support generic types, which have to be decorated manually.
	/// </summary>
	public static class ProtoBufModelConfiguration
	{
		public static RuntimeTypeModel RegisterApplicationContracts(this RuntimeTypeModel model, Assembly assemblyToScan)
		{
			var types = assemblyToScan.GetTypes();

			foreach (var type in types)
			{
				if (type.IsInterface || (!type.IsPublic && !type.IsNestedPublic) || type.IsGenericType || type.IsAbstract)
				{
					continue;
				}

				var modelType = model.Add(type);

				var fieldNumber = 1;
				foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
				{
					modelType.AddField(fieldNumber, property.Name);
					fieldNumber++;
				}
			}

			return model;
		}
	}
}
