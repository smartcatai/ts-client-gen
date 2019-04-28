using System;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using TSClientGen.Extensibility;
using TSClientGen.Extensibility.ApiDescriptors;

namespace TSClientGen.AspNetWebApi
{
	public class TypeDescriptorProvider : ITypeDescriptorProvider
	{
		public TypeDescriptorProvider(ITypeDescriptorProvider next)
		{
			_next = next;
		}

		public TypeDescriptor DescribeType(Type modelType, TypeDescriptor descriptor)
		{
			var namesToModify = modelType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
				.Select(p =>
				{
					var jsonProperty = p.GetCustomAttributes<JsonPropertyAttribute>().FirstOrDefault();
					return jsonProperty != null
						? new {Property = p, Name = jsonProperty.PropertyName}
						: null;
				})
				.Where(p => p != null)
				.ToDictionary(p => p.Property.Name, p => p.Name);

			if (namesToModify.Any())
			{
				var properties = descriptor.Properties
					.Select(property => namesToModify.TryGetValue(property.Name, out var newName)
						? new TypePropertyDescriptor(newName, property.Type, property.InlineTypeDefinition)
						: property)
					.ToList();

				descriptor = new TypeDescriptor(modelType, properties);
			}
			
			return _next?.DescribeType(modelType, descriptor) ?? descriptor;			
		}

		private readonly ITypeDescriptorProvider _next;		
	}
}