using System.Linq;
using System.Net.Http;
using NUnit.Framework;
using TSClientGen.Extensibility.ApiDescriptors;

namespace TSClientGen.Tests
{
	[TestFixture]
	public class ApiMethodSignatureTests
	{
		[Test]
		public void Optional_parameters_follow_required_parameters()
		{
			var method = new ApiMethod(
				"func", "/func", HttpMethod.Get, 
				new[]
				{
					new ApiMethodParam("param1", typeof(Model), true, false),
					new ApiMethodParam("param2", typeof(string), false, false),
					new ApiMethodParam("param3", typeof(int), true, false)
				},
				typeof(void),
				false,
				false);
			
			var generator = createGenerator(method);
			CollectionAssert.AreEqual(
				new[] { "param2: string", "param1?: Model", "param3?: number" },
				generator.GetTypescriptParams().Take(3));
			CollectionAssert.AreEqual(
				new[] { "param2: string", "param1?: Model", "param3?: number" },
				generator.GetTypescriptParamsForUrl().Take(method.AllParams.Count));
		}
		
		[Test]
		public void Request_options_parameter_is_the_last()
		{
			var method = new ApiMethod(
				"func", "/func", HttpMethod.Get, 
				new[]
				{
					new ApiMethodParam("param1", typeof(Model), true, false),
					new ApiMethodParam("param2", typeof(string), false, false),
					new ApiMethodParam("param3", typeof(int), true, false),
				},
				typeof(void),
				false,
				false);
			
			var generator = createGenerator(method);
			Assert.AreEqual(
				"{ getAbortFunc, headers }: HttpRequestOptions = {}",
				generator.GetTypescriptParams().Last());
		}

		[Test]
		public void Conflicting_param_names_are_modified()
		{
			var method = new ApiMethod(
				"func", "/func", HttpMethod.Get, 
				new[]
				{
					new ApiMethodParam("someImport", typeof(string), false, false),
					new ApiMethodParam("method", typeof(string), false, false),
					new ApiMethodParam("queryStringParams", typeof(Model), false, false),
				},
				typeof(void),
				false,
				false);
			
			var generator = createGenerator(method);
			generator.ResolveConflictingParamNames(new[] { "someImport" });
			CollectionAssert.AreEqual(
				new[] { "someImportParam: string", "methodParam: string", "queryStringParamsParam: Model" },
				generator.GetTypescriptParams().Take(method.AllParams.Count));
			CollectionAssert.AreEqual(
				new[] { "someImportParam: string", "methodParam: string", "queryStringParamsParam: Model" },
				generator.GetTypescriptParamsForUrl().Take(method.AllParams.Count));
		}
		
		[Test]
		public void On_upload_progress_callback_can_be_provided_when_uploading_files()
		{
			var method = new ApiMethod(
				"func", "/func", HttpMethod.Post, 
				new[]
				{
					new ApiMethodParam("param1", typeof(Model), true, false)
				},
				typeof(void), true, false);
			
			var generator = createGenerator(method);
			Assert.AreEqual(
				"{ getAbortFunc, headers, onUploadProgress, timeout }: UploadFileHttpRequestOptions = {}",
				generator.GetTypescriptParams().Last());
		}

		[Test]
		public void Files_parameter_is_generated_when_uploading_files()
		{
			var method = new ApiMethod(
				"func", "/func", HttpMethod.Post,
				new[]
				{
					new ApiMethodParam("param1", typeof(Model), true, false),
					new ApiMethodParam("param2", typeof(Model), false, false)
				},
				typeof(void), true, false);
			
			var generator = createGenerator(method);
			Assert.AreEqual(				
				new[] { "param2: Model", "files: Array<NamedBlob | File>", "param1?: Model" },
				generator.GetTypescriptParams().Take(method.AllParams.Count + 1));
		}
		
		
		private static ApiMethodGenerator createGenerator(ApiMethod apiMethod)
		{
			return new ApiMethodGenerator(apiMethod, new IndentedStringBuilder(), new TypeMapping());
		}

		class Model { }
	}
}