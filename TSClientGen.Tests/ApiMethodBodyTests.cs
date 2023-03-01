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
			generator.WriteBody(false, false);
			
			TextAssert.ContainsLine("const queryStringParams = { startDate: startDate.toISOString() };", sb.ToString());	
		}
		
		[Test]
		public void Date_parameters_are_converted_to_ISO_string_in_route_sections()
		{
			var method = createMethodDescriptor("/func/{startDate}", ("startDate", typeof(DateTime)));
			var sb = new IndentedStringBuilder();
			var generator = createGenerator(method, sb);
			generator.WriteBody(false, false);
			
			TextAssert.ContainsLine("const url = `/func/${startDate.toISOString()}`;", sb.ToString());
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
			generator.WriteBody(false, false);
			
			TextAssert.ContainsLine($"const method = '{httpMethod}';", sb.ToString());
		}
		
		[Test]
		public void Url_section_parameters_are_inserted_into_placeholders()
		{
			var method = createMethodDescriptor("/func/{id}", ("id", typeof(int)));
			var sb = new IndentedStringBuilder();
			var generator = createGenerator(method, sb);
			generator.WriteBody(false, false);
			
			TextAssert.ContainsLine("const url = `/func/${id}`;", sb.ToString());
		}

		[Test]
		public void Query_parameters_are_passed_to_request()
		{			
			var method = createMethodDescriptor("/func", ("id", typeof(int)));
			var sb = new IndentedStringBuilder();
			var generator = createGenerator(method, sb);
			generator.WriteBody(false, false);
			
			TextAssert.ContainsLine("const queryStringParams = { id };", sb.ToString());	
		}
		
		[Test]
		public void Multiple_query_parameters_are_passed_to_request()
		{			
			var method = createMethodDescriptor("/func", ("id", typeof(int)), ("reason", typeof(string)));
			var sb = new IndentedStringBuilder();
			var generator = createGenerator(method, sb);
			generator.WriteBody(false, false);
			
			TextAssert.ContainsLine("const queryStringParams = { id, reason };", sb.ToString());	
		}
		
		[Test]
		public void Multiple_query_parameters_with_nullables_are_passed_to_request()
		{			
			var method = createMethodDescriptor("/func", ("id", typeof(int?)), ("longId", typeof(long?)));
			var sb = new IndentedStringBuilder();
			var generator = createGenerator(method, sb);
			generator.WriteBody(false, false);
			
			TextAssert.ContainsLine("const queryStringParams = { id, longId };", sb.ToString());	
		}
		
		[Test]
		public void Aborting_request_is_supported()
		{
			var method = createMethodDescriptor("/func");
			var sb = new IndentedStringBuilder();
			var generator = createGenerator(method, sb);
			generator.WriteBody(false, false);
			
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
			generator.WriteBody(false, false);
			
			TextAssert.ContainsLine("return request<void>({ url, requestBody, getAbortFunc, onUploadProgress, timeout, method, jsonResponseExpected });", sb.ToString());
		}

		[Test]
		public void Get_url_method_includes_query_parameters()
		{
			var method = createMethodDescriptor("/get", ("id", typeof(int)));
			var sb = new IndentedStringBuilder();
			var generator = createGenerator(method, sb);
			generator.WriteBody(true, false);
			
			TextAssert.ContainsLine("const queryStringParams = { id };", sb.ToString());	
		}
		
		[Test]
		public void Custom_types_parameter_are_converted_to_query_params()
		{
			var method = createMethodDescriptor("/func", ("requestParams", typeof(RequestParametersFirst)));
			var sb = new IndentedStringBuilder();
			var generator = createGenerator(method, sb);
			generator.WriteBody(false, false);
			
			TextAssert.ContainsLine("const queryStringParams = { skip: requestParams.skip, reason: requestParams.reason };", sb.ToString());	
		}
		
		[Test]
		public void Custom_types_parameters_are_converted_to_query_params()
		{
			var method = createMethodDescriptor("/func", ("requestParams", typeof(RequestParametersFirst)), ("count", typeof(int)));
			var sb = new IndentedStringBuilder();
			var generator = createGenerator(method, sb);
			generator.WriteBody(false, false);
			
			TextAssert.ContainsLine("const queryStringParams = { skip: requestParams.skip, reason: requestParams.reason, count };", sb.ToString());	
		}
		
		[Test]
		public void Custom_types_multiple_parameters_are_converted_to_query_params()
		{
			var method = createMethodDescriptor("/func", ("firstParams", typeof(RequestParametersFirst)), ("secondParams", typeof(RequestParametersSecond)));
			var sb = new IndentedStringBuilder();
			var generator = createGenerator(method, sb);
			generator.WriteBody(false, false);
			
			TextAssert.ContainsLine("const queryStringParams = { skip: firstParams.skip, reason: firstParams.reason, check: secondParams.check.toISOString() };", sb.ToString());	
		}

		private class RequestParametersFirst
		{
			public int Skip { get; set; }
			public string Reason { get; set; }
		}
		
		private class RequestParametersSecond
		{
			public DateTime Check { get; set; }
		}


		private static ApiMethodGenerator createGenerator(ApiMethod apiMethod, IndentedStringBuilder sb)
		{
			var generator = new ApiMethodGenerator(apiMethod, sb, new TypeMapping());
			generator.GetTypescriptParams().ToArray();
			return generator;
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