using System;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using TSClientGen.Extensibility;
using TSClientGen.Extensibility.ApiDescriptors;

namespace TSClientGen
{
	public class TypeDescriptorProvider : ITypeDescriptorProvider
	{
		public TypeDescriptorProvider(ITypeDescriptorProvider next)
		{
			_next = next;
		}

		public TypeDescriptor DescribeType(
			Type modelType,
			TypeDescriptor descriptor,
			Func<TypePropertyDescriptor, PropertyInfo> getPropertyInfo)
		{
			var properties = descriptor.Properties
				.Select(desc =>
				{
					var property = getPropertyInfo(desc);
					var jsonProperty = property.GetCustomAttributes<JsonPropertyAttribute>().FirstOrDefault();
					return jsonProperty != null
						? new TypePropertyDescriptor(jsonProperty.PropertyName, desc.Type, desc.InlineTypeDefinition)
						: desc;
				})
				.ToList();
			descriptor = new TypeDescriptor(modelType, properties);
			return _next?.DescribeType(modelType, descriptor, getPropertyInfo) ?? descriptor;			
		}

		private readonly ITypeDescriptorProvider _next;		
	}
}