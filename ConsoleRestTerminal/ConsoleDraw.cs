using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleRestTerminal
{
	public class ConsoleDraw
	{
		static readonly string ShipTop =			@"/''\";
		static readonly string ShipBottom =		@"\__/";

		public static void InitializeGrid()
		{
			Console.CursorVisible = false;
			Console.WindowWidth = 100;
			Console.WindowHeight = 40;
			Console.Clear();
		}


		// if x == 0; replace [1-4]
		// if x == 1; replace [6-9]
		// if x == 2; replace [11-14]
		// if y == 0; replace [1-2]
		// if y == 1; replace [4-5]
		// if y == 2; replace [7-8]
		// initial x offset == 1
		//	then add x * 5
		// initial y offset == 1
		// then add y * 3
		public static void DrawGrid(List<int[]> playerPositions )
		{
			var gridTemplate = new[]
			{
				@"+----+----+----+----+----+----+----+----+",
				@"|    |    |    |    |    |    |    |    |",
				@"|    |    |    |    |    |    |    |    |",
				@"+----+----+----+----+----+----+----+----+",
				@"|    |    |    |    |    |    |    |    |",
				@"|    |    |    |    |    |    |    |    |",
				@"+----+----+----+----+----+----+----+----+",
				@"|    |    |    |    |    |    |    |    |",
				@"|    |    |    |    |    |    |    |    |",
				@"+----+----+----+----+----+----+----+----+",
				@"|    |    |    |    |    |    |    |    |",
				@"|    |    |    |    |    |    |    |    |",
				@"+----+----+----+----+----+----+----+----+",
				@"|    |    |    |    |    |    |    |    |",
				@"|    |    |    |    |    |    |    |    |",
				@"+----+----+----+----+----+----+----+----+",
				@"|    |    |    |    |    |    |    |    |",
				@"|    |    |    |    |    |    |    |    |",
				@"+----+----+----+----+----+----+----+----+",
				@"|    |    |    |    |    |    |    |    |",
				@"|    |    |    |    |    |    |    |    |",
				@"+----+----+----+----+----+----+----+----+",
				@"|    |    |    |    |    |    |    |    |",
				@"|    |    |    |    |    |    |    |    |",
				@"+----+----+----+----+----+----+----+----+"
			};

			foreach (var playerPosition in playerPositions)
			{
				int rowIndexA = 1 + (playerPosition[1] * 3);
				int rowIndexB = rowIndexA + 1;
				string rowA = gridTemplate[rowIndexA];
				string rowB = gridTemplate[rowIndexB];
				int colIndexA = 1 + (playerPosition[0] * 5);
				int shipWidth = 4;
				StringBuilder sbA = new StringBuilder(rowA);
				sbA.Replace("    ", ShipTop, colIndexA, shipWidth);
				StringBuilder sbB = new StringBuilder(rowB);
				sbB.Replace("    ", ShipBottom, colIndexA, shipWidth);
				gridTemplate[rowIndexA] = sbA.ToString();
				gridTemplate[rowIndexB] = sbB.ToString();
			}
			Console.Clear();
			foreach (string line in gridTemplate)
			{
				Console.WriteLine(line);
			}
		}


	}
}
