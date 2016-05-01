using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleRestTerminal
{
	[DataContract]
	public class PingResponse
	{
		[DataMember(Name = "ping")]
		public string Ping { get; set; }
	}
}

