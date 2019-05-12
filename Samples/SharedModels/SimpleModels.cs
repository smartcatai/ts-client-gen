namespace TSClientGen.Samples.SharedModels
{
	public class Request
	{
		public int ItemsCount { get; set; }
	}

	public class Response
	{
		public string[] Items { get; set; }
	}

	public enum RequestType
	{
		Value1,
		Value2
	}
}