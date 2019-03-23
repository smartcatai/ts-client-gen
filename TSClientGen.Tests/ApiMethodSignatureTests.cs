using System.Linq;
using NUnit.Framework;
using TSClientGen.ApiDescriptors;
using TSClientGen.Extensibility.ApiDescriptors;

namespace TSClientGen.Tests
{
	[TestFixture]
	public class ApiMethodSignatureTests
	{
		[Test]
		public void Optional_parameters_follow_required_parameters()
		{
			var method = new MethodDescriptor(
				"func", "/func", "get",
				new[]
				{
					new MethodParamDescriptor("param1", typeof(Model), true, false),
					new MethodParamDescriptor("param2", typeof(string), false, false),
					new MethodParamDescriptor("param3", typeof(int), true, false)
				},
				typeof(void),
				false,
				false);
			
			var generator = createGenerator(method);
			CollectionAssert.AreEqual(
				new[] { "param2: string", "param1?: IModel", "param3?: number" },
				generator.GetTypescriptParams().Take(3));
			CollectionAssert.AreEqual(
				new[] { "param2: string", "param1?: IModel", "param3?: number" },
				generator.GetTypescriptParamsForUrl().Take(method.AllParams.Count));
		}
		
		[Test]
		public void Request_options_parameter_is_the_last()
		{
			var method = new MethodDescriptor(
				"func", "/func", "get",
				new[]
				{
					new MethodParamDescriptor("param1", typeof(Model), true, false),
					new MethodParamDescriptor("param2", typeof(string), false, false),
					new MethodParamDescriptor("param3", typeof(int), true, false),
				},
				typeof(void),
				false,
				false);
			
			var generator = createGenerator(method);
			Assert.AreEqual(
				"{ cancelToken }: HttpRequestOptions = {}",
				generator.GetTypescriptParams().Last());
		}

		[Test]
		public void Conflicting_param_names_are_modified()
		{
			var method = new MethodDescriptor(
				"func", "/func", "get",
				new[]
				{
					new MethodParamDescriptor("someImport", typeof(string), false, false),
					new MethodParamDescriptor("method", typeof(string), false, false),
					new MethodParamDescriptor("params", typeof(Model), false, false),
				},
				typeof(void),
				false,
				false);
			
			var generator = createGenerator(method);
			generator.ResolveConflictingParamNames(new[] { "someImport" });
			CollectionAssert.AreEqual(
				new[] { "someImportParam: string", "methodParam: string", "paramsParam: IModel" },
				generator.GetTypescriptParams().Take(method.AllParams.Count));
			CollectionAssert.AreEqual(
				new[] { "someImportParam: string", "methodParam: string", "paramsParam: IModel" },
				generator.GetTypescriptParamsForUrl().Take(method.AllParams.Count));
		}
		
		[Test]
		public void On_upload_progress_callback_can_be_provided_when_uploading_files()
		{
			var method = new MethodDescriptor(
				"func", "/func", "post",
				new[]
				{
					new MethodParamDescriptor("param1", typeof(Model), true, false)
				},
				typeof(void), true, false);
			
			var generator = createGenerator(method);
			Assert.AreEqual(
				"{ cancelToken, onUploadProgress }: UploadFileHttpRequestOptions = {}",
				generator.GetTypescriptParams().Last());
		}

		[Test]
		public void Files_parameter_is_generated_when_uploading_files()
		{
			var method = new MethodDescriptor(
				"func", "/func", "post",
				new[]
				{
					new MethodParamDescriptor("param1", typeof(Model), true, false),
					new MethodParamDescriptor("param2", typeof(Model), false, false)
				},
				typeof(void), true, false);
			
			var generator = createGenerator(method);
			Assert.AreEqual(				
				new[] { "param2: IModel", "files: Array<NamedBlob | File>", "param1?: IModel" },
				generator.GetTypescriptParams().Take(method.AllParams.Count + 1));
		}
		
		
		private static ApiMethodGenerator createGenerator(MethodDescriptor method)
		{
			return new ApiMethodGenerator(method, new IndentedStringBuilder(), new TypeMapping(null));
		}

		class Model { }
	}
}