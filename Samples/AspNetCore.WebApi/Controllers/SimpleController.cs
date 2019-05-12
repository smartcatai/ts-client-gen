using System.Linq;
using Microsoft.AspNetCore.Mvc;
using TSClientGen.Samples.SharedModels;

namespace TSClientGen.Samples.AspNetCore.WebApi.Controllers
{
	[ApiController]
	public class SimpleController : ControllerBase
	{
		[HttpGet, Route("name")]
		public Response Get(Request request)
		{
			return new Response
			{
				Items = Enumerable.Repeat("Item", request.ItemsCount).ToArray()
			};
		}
	}
}