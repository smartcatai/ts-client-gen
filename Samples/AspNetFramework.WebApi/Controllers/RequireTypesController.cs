using System.Web.Http;
using TSClientGen.Samples.SharedModels;

namespace TSClientGen.Samples.AspNetFramework.WebApi.Controllers
{
	[RoutePrefix("requireTypes")]
	public class RequireTypesController : ApiController
	{
		[HttpGet, Route("get")]
		public BaseResponse Get()
		{
			return new SomeSpecificResponse();
		}		
	}
}