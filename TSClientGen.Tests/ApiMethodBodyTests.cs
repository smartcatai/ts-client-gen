using System;
using System.Linq;
using System.Net.Http;
using NUnit.Framework;
using TSClientGen.Extensibility.ApiDescriptors;

namespace TSClientGen.Tests
{
	[TestFixture]
	public class ApiMethodBodyTests
	{
		[Test]
		public void Date_parameters_are_converted_to_ISO_string_in_query_params()
		{
			var method = createMethodDescriptor("/func", ("startDate", typeof(DateTime)));
			var sb = new IndentedStringBuilder();
			var generator = createGenerator(method, sb);
			generator.WriteBody(false);
			
			TextAssert.ContainsLine("const queryStringParams = { startDate: startDate.toISOString() };", sb.ToString());	
		}
		
		[Test]
		public void Date_parameters_are_converted_to_ISO_string_in_route_sections()
		{
			var method = createMethodDescriptor("/func/{startDate}", ("startDate", typeof(DateTime)));
			var sb = new IndentedStringBuilder();
			var generator = createGenerator(method, sb);
			generator.WriteBody(false);
			
			TextAssert.ContainsLine("const url = (this.options ? this.options.hostname : '') + `/func/${startDate.toISOString()}`;", sb.ToString());
		}
		
		[TestCase("get")]
		[TestCase("post")]
		public void Http_method_is_respected(string httpMethod)
		{
			var method = new ApiMethod(
				"func", "/func", new HttpMethod(httpMethod), new ApiMethodParam[0], 
				typeof(void), false, false);
			var sb = new IndentedStringBuilder();
			var generator = createGenerator(method, sb);
			generator.WriteBody(false);
			
			TextAssert.ContainsLine($"const method = '{httpMethod}';", sb.ToString());
		}
		
		[Test]
		public void Url_section_parameters_are_inserted_into_placeholders()
		{
			var method = createMethodDescriptor("/func/{id}", ("id", typeof(int)));
			var sb = new IndentedStringBuilder();
			var generator = createGenerator(method, sb);
			generator.WriteBody(false);
			
			TextAssert.ContainsLine("const url = (this.options ? this.options.hostname : '') + `/func/${id}`;", sb.ToString());
		}

		[Test]
		public void Query_parameters_are_passed_to_request()
		{			
			var method = createMethodDescriptor("/func", ("id", typeof(int)));
			var sb = new IndentedStringBuilder();
			var generator = createGenerator(method, sb);
			generator.WriteBody(false);
			
			TextAssert.ContainsLine("const queryStringParams = { id };", sb.ToString());	
		}
		
		[Test]
		public void Aborting_request_is_supported()
		{
			var method = createMethodDescriptor("/func");
			var sb = new IndentedStringBuilder();
			var generator = createGenerator(method, sb);
			generator.WriteBody(false);
			
			TextAssert.ContainsLine("return request<void>({ url, getAbortFunc, method, jsonResponseExpected });", sb.ToString());	
		}
		
		[Test]
		public void Upload_progress_callback_is_supported_when_uploading_files()
		{
			var method = new ApiMethod(
				"func", "/upload", HttpMethod.Post, 
				new ApiMethodParam[0], 
				typeof(void), true, false);
			var sb = new IndentedStringBuilder();
			var generator = createGenerator(method, sb);
			generator.WriteBody(false);
			
			TextAssert.ContainsLine("return request<void>({ url, requestBody, getAbortFunc, onUploadProgress, method, jsonResponseExpected });", sb.ToString());	
		}

		[Test]
		public void Get_url_method_includes_query_parameters()
		{
			var method = createMethodDescriptor("/get", ("id", typeof(int)));
			var sb = new IndentedStringBuilder();
			var generator = createGenerator(method, sb);
			generator.WriteBody(true);
			
			TextAssert.ContainsLine("const queryStringParams = { id };", sb.ToString());	
		}

		[Test]
		public void Api_client_options_are_passed_into_transport_level_method_call()
		{
			var method = createMethodDescriptor("/func", ("id", typeof(int)));
			var sb = new IndentedStringBuilder();
			var generator = createGenerator(method, sb, true);
			generator.WriteBody(false);

			TextAssert.ContainsLine(
				"return request<void>({ ...(this.options || {}), url, queryStringParams, getAbortFunc, method, jsonResponseExpected });",
				sb.ToString());
		}


		private static ApiMethodGenerator createGenerator(ApiMethod apiMethod, IndentedStringBuilder sb,
			bool useApiClientOptions = false)
		{
			return new ApiMethodGenerator(apiMethod, sb, new TypeMapping(), useApiClientOptions);
		}

		private static ApiMethod createMethodDescriptor(string url, params (string name, Type type)[] parameters)
		{
			return new ApiMethod(
				"func",url, HttpMethod.Get, 
				parameters.Select(p => new ApiMethodParam(p.name, p.type, false, false)).ToArray(),
				typeof(void), false, false);
		}		
	}
}