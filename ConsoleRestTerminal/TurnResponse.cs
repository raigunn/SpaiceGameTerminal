using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleRestTerminal
{
	[DataContract]
	public class TurnResponse
	{
		[DataMember(Name = "move")]
		public string MoveDirection { get; set; }
		[DataMember(Name = "shoot")]
		public string ShootDirection { get; set; }
	}
}
