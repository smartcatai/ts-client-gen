using System;
using System.Collections.Generic;

namespace TSClientGen
{
	public interface IImportEnumModuleGenerator
	{
		string Generate(IReadOnlyCollection<Type> enumTypes, string enumModuleName);
	}
}