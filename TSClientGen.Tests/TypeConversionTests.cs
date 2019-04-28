using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace TSClientGen.Tests
{
	[TestFixture]
	public class TypeConversionTests
	{
		[TestCase(typeof(bool), "boolean")]
		[TestCase(typeof(DateTime),	"Date")]
		[TestCase(typeof(object), "any")]
		[TestCase(typeof(void),	"void")]
		[TestCase(typeof(Task),	"void")]
		[TestCase(typeof(string), "string")]
		[TestCase(typeof(byte),	"number")]
		[TestCase(typeof(short), "number")]
		[TestCase(typeof(int),	"number")]
		[TestCase(typeof(long),	"number")]
		[TestCase(typeof(ushort), "number")]
		[TestCase(typeof(uint),	"number")]
		[TestCase(typeof(ulong), "number")]
		[TestCase(typeof(float), "number")]
		[TestCase(typeof(double), "number")]
		[TestCase(typeof(decimal), "number")]
		[TestCase(typeof(Guid),	 "string")]
		public void Primitive_types_are_converted_properly(Type type, string output)
		{
			Assert.AreEqual(
				output, 
				new TypeMapping().GetTSType(type));
		}
		
		[Test]
		public void Custom_complex_types_are_generated_as_interfaces()
		{
			var mapping = new TypeMapping();

			Assert.AreEqual("SimpleModel", mapping.GetTSType(typeof(SimpleModel)));
			TextAssert.ContainsLine("export interface SimpleModel {", mapping.GetGeneratedTypes()[typeof(SimpleModel)]);
		}

		[Test]
		public void Unknown_generic_types_are_prohibited()
		{
			Assert.Throws<InvalidOperationException>(() =>
				new TypeMapping().GetTSType(typeof(GenericModel<SimpleModel>)));
		}

		[Test]
		public void Dictionary_with_non_numeric_or_string_key_is_prohibited()
		{
			Assert.Throws<InvalidOperationException>(() =>
				new TypeMapping().GetTSType(typeof(Dictionary<SimpleModel, string>)));
		} 
		
		[Test]
		public void Dictionary_with_enum_key_is_allowed()
		{
			Assert.AreEqual(
				"{ [id: number]: string; }",
				new TypeMapping().GetTSType(typeof(Dictionary<Enum, string>)));
		}

		[Test]
		public void Enumerable_type_is_matched_to_array()
		{
			Assert.AreEqual(
				"SimpleModel[]",
				new TypeMapping().GetTSType(typeof(Collection<SimpleModel>)));
		}

		[Test]
		public void Nullable_type_is_handled_like_base_type()
		{
			Assert.AreEqual(
				"number", 
				new TypeMapping().GetTSType(typeof(int?)));
		}
		
		[Test]
		public void Generic_task_is_handled_as_task_result_type()
		{
			Assert.AreEqual(
				"SimpleModel", 
				new TypeMapping().GetTSType(typeof(Task<SimpleModel>)));
		}


		// ReSharper disable once ClassNeverInstantiated.Local
		class SimpleModel
		{
		}

		class GenericModel<T>
		{
		}

		class Collection<T> : IEnumerable<T>
		{
			public IEnumerator<T> GetEnumerator()
			{
				return Enumerable.Empty<T>().GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}

		enum Enum
		{
		}
	}
}