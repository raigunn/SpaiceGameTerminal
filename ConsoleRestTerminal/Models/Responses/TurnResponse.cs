using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace ConsoleRestTerminal.Models.Responses
{
	[DataContract]
	public class TurnResponse
	{
		[DataMember]
		public int MoveDirection { get; set; }
		[DataMember]
		public int ShootDirection { get; set; }

		public static implicit operator Task<object>(TurnResponse v)
		{
			throw new NotImplementedException();
		}
	}
}
