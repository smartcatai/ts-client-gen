using System;
using System.Linq;
using System.Reflection;
using System.Web.Http;

namespace TSClientGen
{
	public class ActionParamDescriptor
	{
		public ActionParamDescriptor(ParameterInfo param, TypeMapper mapper)
		{
			Name = param.Name;
			TypescriptAlias = param.Name;
			DotNetType = param.ParameterType;
			TypescriptType = mapper.GetTSType(param.ParameterType);
			IsOptional = param.IsOptional;
			IsBodyContent = param.GetCustomAttributes<FromBodyAttribute>().Any();
		}
		
		public string Name { get; }
		
		public string TypescriptAlias { get; set; }
		
		public string TypescriptType { get; }
		
		public Type DotNetType { get; }
		
		public bool IsOptional { get; }
		
		public bool IsBodyContent { get; }
	}
}