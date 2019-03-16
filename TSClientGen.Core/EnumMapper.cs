using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TSClientGen
{
	public class EnumMapper
	{
		public EnumMapper(Dictionary<string, string> moduleNamesByAssemblyPath)
		{
			_descriptors = new List<EnumDescriptor>();
			_resolvedDescriptors = new Lazy<List<EnumDescriptor>>(matchEnumsByModules);
			_moduleNamesByAssemblyPath = moduleNamesByAssemblyPath;
		}

		public void SaveEnum(Assembly asm, string moduleName, Type enumType)
		{
			_descriptors.Add(new EnumDescriptor(asm, getEnumModuleName(asm), moduleName, enumType));
		}

		public void SaveEnum(Assembly asm, Type enumType)
		{
			_descriptors.Add(new EnumDescriptor(asm, getEnumModuleName(asm), Enumerable.Empty<string>(), enumType));
		}

		public ILookup<string, EnumDescriptor> GetReferencedEnumsByControllerModules()
		{
			return (from d in _resolvedDescriptors.Value
					from m in d.UsedInApiClientModules
					select new {m, d})
				.ToLookup(g => g.m, g => g.d);
		}

		public ILookup<string, Type> GetEnumsByModules()
		{
			return _resolvedDescriptors.Value
				.ToLookup(d => d.EnumModuleName, d => d.EnumType);
		}

		private List<EnumDescriptor> matchEnumsByModules()
		{
			return _descriptors
				.GroupBy(d => d.EnumType)
				.Select(
					group =>
					{
						var values = group.ToList();
						if (values.Count == 1)
							return values[0];

						// Возможно все использования enum'а встретились в пределах одной сборки
						var singleAsm = values.Select(v => v.Assembly).Distinct().ToList();
						if (singleAsm.Count == 1)
							return new EnumDescriptor(singleAsm[0], getEnumModuleName(singleAsm[0]), values.SelectMany(v => v.UsedInApiClientModules), group.Key);

						// Возможно одна из сборок помечена атрибутом локализации
						var extendEnumAsm = (
							from v in values
							from attr in v.Assembly.GetCustomAttributes().OfType<TSExtendEnumAttribute>()
							where attr.EnumType == v.EnumType
							select v.Assembly).Distinct().ToList();

						if (extendEnumAsm.Count == 1)
							return new EnumDescriptor(extendEnumAsm[0], getEnumModuleName(extendEnumAsm[0]), values.SelectMany(v => v.UsedInApiClientModules), group.Key);

						// Возможно одна из сборок явно помечена как основная для enum'а
						var enumResolveAsm = (
							from v in values
							from attr in v.Assembly.GetCustomAttributes<TSEnumModuleAttribute>()
							where attr.EnumType == v.EnumType
							select v.Assembly).Distinct().ToList();

						if (enumResolveAsm.Count == 1)
							return new EnumDescriptor(enumResolveAsm[0], getEnumModuleName(enumResolveAsm[0]), values.SelectMany(v => v.UsedInApiClientModules), group.Key);

						throw new Exception("Ambigous enum: " + group.Key);
					})
				.ToList();
		}

		private string getEnumModuleName(Assembly asm)
		{
			return _moduleNamesByAssemblyPath[asm.Location];
		}


		private readonly IReadOnlyDictionary<string, string> _moduleNamesByAssemblyPath;
		private readonly List<EnumDescriptor> _descriptors;
		private readonly Lazy<List<EnumDescriptor>> _resolvedDescriptors;

		public struct EnumDescriptor
		{
			public EnumDescriptor(Assembly assembly, string enumModuleName, string moduleName, Type enumType)
			{
				if (moduleName == null) throw new ArgumentNullException(nameof(moduleName));

				Assembly = assembly;
				UsedInApiClientModules = new List<string> { moduleName };
				EnumType = enumType;
				EnumModuleName = enumModuleName;
			}

			public EnumDescriptor(Assembly assembly, string enumModuleName, IEnumerable<string> moduleNames, Type enumType)
			{
				Assembly = assembly;
				EnumType = enumType;
				EnumModuleName = enumModuleName;
				UsedInApiClientModules = moduleNames.Distinct().ToList();
			}

			public string EnumModuleName { get; }

			public Assembly Assembly { get; }

			public List<string> UsedInApiClientModules { get; }

			public Type EnumType { get; }
		}
	}
}