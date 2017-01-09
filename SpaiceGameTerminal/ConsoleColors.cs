using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaiceGameTerminal
{
	public static class ConsoleColors
	{


		// 0, 3, 4, 12, 13, 14, 15, 16
		private static List<int> _validShipColors = new List<int>() { 7, 9, 10, 11, 12, 13, 14, 15 };

		public static ConsoleColor GetRandomConsoleColor()
		{
			var consoleColors = Enum.GetValues(typeof(ConsoleColor));
			int colorIndex = RandomWrapper.RandomNumber(0, _validShipColors.Count);
			ConsoleColor color = (ConsoleColor)consoleColors.GetValue(_validShipColors[colorIndex]);
			_validShipColors.RemoveAt(colorIndex);
			return color;
		}

		/// <summary>
		/// thi is probably not good because it requires someone to remember to reset it
		/// needs to be called before creating players for a new game.
		/// </summary>
		public static void Reset()
		{
			_validShipColors = new List<int>() { 7, 9, 10, 11, 12, 13, 14, 15 };
		}
	}
}
