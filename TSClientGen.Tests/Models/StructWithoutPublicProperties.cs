using System;

namespace TSClientGen.Tests.Models
{
	public struct StructWithoutPublicProperties 
	{
		public StructWithoutPublicProperties(Guid id)
		{
			_id = id;
		}

		private readonly Guid _id;
	}
}
