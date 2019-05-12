using System.Linq;
using System.Web.Http;
using TSClientGen.Samples.SharedModels;

namespace TSClientGen.Samples.AspNetFramework.WebApi.Controllers
{
	[RoutePrefix("simple")]
	public class SimpleController : ApiController
	{
		[HttpGet, Route("name")]
		public Response Get(Request request)
		{
			return new Response
			{
				Items = Enumerable.Repeat("Item", request.ItemsCount).ToArray()
			};
		}

		[HttpPost, Route("enumInRequest")]
		public void PostWithEnum(RequestType type)
		{
		}
	}
}