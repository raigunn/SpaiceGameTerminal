using System.Runtime.Serialization;

namespace SpaiceGameTerminal.Models.Responses
{
	[DataContract]
	public class PingResponse
	{
		[DataMember(Name = "ping")]
		public string Ping { get; set; }
	}
}

