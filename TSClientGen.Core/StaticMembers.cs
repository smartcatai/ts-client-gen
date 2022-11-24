using System;
using System.Collections.Generic;

namespace TSClientGen
{
	public class StaticMembers
	{
		private readonly List<Func<string>> _generators = new List<Func<string>>();

		public StaticMembers(IReadOnlyCollection<Type> enumImportTypes = null)
		{
			EnumImportTypes = enumImportTypes ?? Array.Empty<Type>();
		}

		public IReadOnlyCollection<Type> EnumImportTypes { get; }

		public IReadOnlyCollection<Func<string>> Generators => _generators;

		public void AddGenerator(Func<string> generator) => _generators.Add(generator);
	}
}