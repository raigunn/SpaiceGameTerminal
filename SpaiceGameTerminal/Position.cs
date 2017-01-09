using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SpaiceGameTerminal
{
	/// <summary>
	/// Want to be able to throw an error if a position is outside the grid.
	/// Will need a game cache to track that, current no access to game state.
	/// </summary>
	[DataContract]
	public class Position
	{
		[DataMember]
		public int X { get; set; }
		[DataMember]
		public int Y { get; set; }

		public Position(int x, int y)
		{
			X = x;
			Y = y;
		}

		public bool Equals(Position anotherPosition)
		{
			if (this.X == anotherPosition.X && this.Y == anotherPosition.Y) return true;
			return false;
		}
	}
}
