using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleRestTerminal
{
	[DataContract]
	public class Position
	{
		[DataMember]
		public int X { get; set; }
		[DataMember]
		public int Y { get; set; }
		[DataMember]
		public int MaxX { get; set; }
		[DataMember]
		public int MaxY { get; set; }

		public Position(int x, int y, int maxX, int maxY)
		{
			X = x;
			Y = y;
			MaxX = maxX;
			MaxY = maxY;
		}
	}
}
