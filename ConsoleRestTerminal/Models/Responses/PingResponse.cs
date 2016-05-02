using System.Runtime.Serialization;

namespace ConsoleRestTerminal.Models.Responses
{
	[DataContract]
	public class PingResponse
	{
		[DataMember(Name = "ping")]
		public string Ping { get; set; }
	}
}

