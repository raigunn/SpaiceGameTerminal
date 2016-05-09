using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ConsoleRestTerminal.Enums;
using ConsoleRestTerminal.Models;
using ConsoleRestTerminal.Models.Requests;

namespace ConsoleRestTerminal
{
	/// <summary>
	///  see for some coloring ideas
	/// http://colorfulconsole.com/
	/// </summary>
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
		public static void DrawGrid(List<Player> players)
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

			AddShipsToGrid(ref gridTemplate, players);

			// draw the updated grid
			DrawGrid(ref gridTemplate);

			AddShootingToGrid(ref gridTemplate, players);
			
			// draw the updated grid
			Thread.Sleep(500);
			DrawGrid(ref gridTemplate);
		}

		private static void DrawGrid(ref string[] gridTemplate)
		{
			Console.Clear();
			foreach (string line in gridTemplate)
			{
				Console.WriteLine(line);
			}
		}

		private static void AddShipsToGrid(ref string[] gridTemplate, List<Player> players)
		{

			// edit grid to include new position of each player.
			foreach (var player in players)
			{
				int rowIndexA = 1 + (player.Position.Y * 3);
				int rowIndexB = rowIndexA + 1;
				string rowA = gridTemplate[rowIndexA];
				string rowB = gridTemplate[rowIndexB];
				int colIndexA = 1 + (player.Position.X * 5);
				int shipWidth = 4;
				StringBuilder sbA = new StringBuilder(rowA);
				sbA.Replace("    ", ShipTop, colIndexA, shipWidth);
				StringBuilder sbB = new StringBuilder(rowB);
				sbB.Replace("    ", ShipBottom, colIndexA, shipWidth);
				gridTemplate[rowIndexA] = sbA.ToString();
				gridTemplate[rowIndexB] = sbB.ToString();
			}
		}

		private static void AddShootingToGrid(ref string[] gridTemplate, List<Player> players)
		{    
			// draw the laser fire from each ship
			foreach (var player in players)
			{
				int topRowOfShip = 1 + (player.Position.Y * 3);
				int botRowOfShip = 2 + (player.Position.Y * 3);

				switch ((Directions)player.ShootDirection)
				{
					case Directions.Right:
						AddShootingRight(ref gridTemplate, topRowOfShip, player.Position.X);
						break;
					case Directions.Left:
						AddShootingLeft(ref gridTemplate, topRowOfShip, player.Position.X);
						break;
					case Directions.Up:
						AddShootingUp(ref gridTemplate, topRowOfShip, player.Position.X);
						break;
					case Directions.Down:
						AddShootingDown(ref gridTemplate, botRowOfShip, player.Position.X);
						break;
					default:
						break;
				}
			}
		}

		private static void AddShootingRight(ref string[] gridTemplate, int topRowOfShip, int playerXPosition)
		{
			StringBuilder rowToEdit = new StringBuilder(gridTemplate[topRowOfShip]);
			int xPosition = (playerXPosition * 5) + 3;
			rowToEdit.Replace(" ", "-", xPosition, rowToEdit.Length - xPosition);
			gridTemplate[topRowOfShip] = rowToEdit.ToString();
		}

		private static void AddShootingLeft(ref string[] gridTemplate, int topRowOfShip, int playerXPosition)
		{
			StringBuilder rowToEdit = new StringBuilder(gridTemplate[topRowOfShip + 1]);
			rowToEdit.Replace(" ", "-", 0, playerXPosition * 5);
			gridTemplate[topRowOfShip + 1] = rowToEdit.ToString();
		}

		private static void AddShootingUp(ref string[] gridTemplate, int topRowOfShip, int playerXPosition)
		{
			for (int i = topRowOfShip; i > 0; i--)
			{
				StringBuilder rowToEdit = new StringBuilder(gridTemplate[i]);
				rowToEdit.Replace(" ", ":", (playerXPosition * 5) + 3, 1);
				gridTemplate[i] = rowToEdit.ToString();
			}
		}

		private static void AddShootingDown(ref string[] gridTemplate, int botRowOfShip, int playerXPosition)
		{
			for (int i = botRowOfShip; i < gridTemplate.Length - 1; i++)
			{
				StringBuilder rowToEdit = new StringBuilder(gridTemplate[i]);
				rowToEdit.Replace(" ", ":", (playerXPosition * 5) + 2, 1);
				gridTemplate[i] = rowToEdit.ToString();
			}
		}
	}
}
