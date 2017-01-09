using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace SpaiceGameTerminal.Models.Responses
{
	[DataContract]
	public class TurnResponse
	{
		[DataMember]
		public int MoveDirection { get; set; }
		[DataMember]
		public int ShootDirection { get; set; }
		[DataMember]
		public string PlayerId { get; set; }

		public static implicit operator Task<object>(TurnResponse v)
		{
			throw new NotImplementedException();
		}
	}
}
