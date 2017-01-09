using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaiceGameTerminal.Enums;

namespace SpaiceGameTerminal.Extensions
{
	public static class DirectionExt
	{
		public static Position ConvertToNewPosition(this Directions d, Position p)
		{
			int newX = p.X, newY = p.Y;
			if (d == Directions.Right) newX++;// = p.X+1;
			if (d == Directions.Left) newX--;// = p.X-1;
			if (d == Directions.Up) newY--;//p.Y--;
			if (d == Directions.Down) newY++;// p.Y++;
			return new Position(newX, newY);
		}
	}
}
