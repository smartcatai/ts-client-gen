using System;
using System.Linq;
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
			
			TextAssert.ContainsLine("const params = { startDate: startDate.toISOString() };", sb.ToString());	
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
		public void Http_verb_is_respected(string httpVerb)
		{
			var method = new ApiMethod(
				"func", "/func", httpVerb, new ApiMethodParam[0], 
				typeof(void), false, false);
			var sb = new IndentedStringBuilder();
			var generator = createGenerator(method, sb);
			generator.WriteBody(false, false);
			
			TextAssert.ContainsLine($"const method = '{httpVerb}';", sb.ToString());
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
			
			TextAssert.ContainsLine("const params = { id };", sb.ToString());	
		}
		
		[Test]
		public void Request_cancellation_is_supported()
		{
			var method = createMethodDescriptor("/func");
			var sb = new IndentedStringBuilder();
			var generator = createGenerator(method, sb);
			generator.WriteBody(false, false);
			
			TextAssert.ContainsLine("return request<void>({ url, method, cancelToken });", sb.ToString());	
		}
		
		[Test]
		public void Upload_progress_callback_is_supported_when_uploading_files()
		{
			var method = new ApiMethod(
				"func", "/upload", "POST", 
				new ApiMethodParam[0], 
				typeof(void), true, false);
			var sb = new IndentedStringBuilder();
			var generator = createGenerator(method, sb);
			generator.WriteBody(false, false);
			
			TextAssert.ContainsLine("return request<void>({ url, method, data, cancelToken, onUploadProgress });", sb.ToString());	
		}

		[Test]
		public void Get_url_method_includes_query_parameters()
		{
			var method = createMethodDescriptor("/get", ("id", typeof(int)));
			var sb = new IndentedStringBuilder();
			var generator = createGenerator(method, sb);
			generator.WriteBody(true, false);
			
			TextAssert.ContainsLine("const params = { id };", sb.ToString());	
		}


		private static ApiMethodGenerator createGenerator(ApiMethod apiMethod, IndentedStringBuilder sb)
		{
			return new ApiMethodGenerator(apiMethod, sb, new TypeMapping());
		}
		
		private static ApiMethod createMethodDescriptor(string url, params (string name, Type type)[] parameters)
		{
			return new ApiMethod(
				"func",url, "GET",
				parameters.Select(p => new ApiMethodParam(p.name, p.type, false, false)).ToArray(),
				typeof(void), false, false);
		}		
	}
}