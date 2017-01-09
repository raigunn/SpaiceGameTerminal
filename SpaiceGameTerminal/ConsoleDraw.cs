using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SpaiceGameTerminal.Domain;
using SpaiceGameTerminal.Models;
using SpaiceGameTerminal.Enums;
using SpaiceGameTerminal.Models.Requests;

namespace SpaiceGameTerminal
{

	public class ConsoleDraw
	{
		private static readonly string ShipTop =			@"/''\";
		private static readonly string ShipBottom =		@"\__/";
		private static readonly string ShipExplodeTop			= @"\VV/";
		private static readonly string ShipExplodeBottom	= @"//\\";
		private static int GridWidth; // cols * 5 + 1
		private static int GridHeight; // rows * 3 + 1
		private static string wall = "";
		private static string grid = "";
		private static string[] gridTemplate;
		private static GridSize _gridSize;

		private static void BuildGridRows(int cols)
		{
			wall = "+"; // left edge
			grid = "|"; // left edge
			for (int i = 0; i < cols; i++)
			{
				wall += "----+"; // remainder of grid cell
				grid += "    |"; // remainder of grid cell
			}
		}

		private static void BuildGridTemplate(int rows)
		{
			gridTemplate = new string[(rows*3)+1];
			gridTemplate[0] = wall;
			for (int i = 1; i <= rows; i++)
			{
				var index = i*3 - 2;
				gridTemplate[index] = grid;
				gridTemplate[index + 1] = grid;
				gridTemplate[index + 2] = wall;
			}
		}

		//private static readonly string[] GridTemplate = new[]
		//{
		//		@"+----+----+----+----+----+----+----+----+",
		//		@"|    |    |    |    |    |    |    |    |",
		//		@"|    |    |    |    |    |    |    |    |",
		//		@"+----+----+----+----+----+----+----+----+",
		//		@"|    |    |    |    |    |    |    |    |",
		//		@"|    |    |    |    |    |    |    |    |",
		//		@"+----+----+----+----+----+----+----+----+",
		//		@"|    |    |    |    |    |    |    |    |",
		//		@"|    |    |    |    |    |    |    |    |",
		//		@"+----+----+----+----+----+----+----+----+",
		//		@"|    |    |    |    |    |    |    |    |",
		//		@"|    |    |    |    |    |    |    |    |",
		//		@"+----+----+----+----+----+----+----+----+",
		//		@"|    |    |    |    |    |    |    |    |",
		//		@"|    |    |    |    |    |    |    |    |",
		//		@"+----+----+----+----+----+----+----+----+",
		//		@"|    |    |    |    |    |    |    |    |",
		//		@"|    |    |    |    |    |    |    |    |",
		//		@"+----+----+----+----+----+----+----+----+",
		//		@"|    |    |    |    |    |    |    |    |",
		//		@"|    |    |    |    |    |    |    |    |",
		//		@"+----+----+----+----+----+----+----+----+",
		//		@"|    |    |    |    |    |    |    |    |",
		//		@"|    |    |    |    |    |    |    |    |",
		//		@"+----+----+----+----+----+----+----+----+"
		//	};


		public static void BuildInitialGrid(List<Player> players, GridSize gridSize)
		{
			BuildGridRows(gridSize.Cols);
			BuildGridTemplate(gridSize.Rows);
			GridWidth = gridSize.Cols*5 + 1;
			GridHeight = gridSize.Rows*3 + 1;
			_gridSize = gridSize;

			Console.WindowWidth = GridWidth + 100;
			Console.WindowHeight = GridHeight + 10;

			Console.CursorVisible = false;
			Console.Clear();
			DrawGrid();
			DrawShips(players);
			Console.SetCursorPosition(0, 0);
			DrawStatistics(players);
		}

		public static void RebuildGrid(GameState gameState)
		{
			Console.Clear();
			DrawGrid();
			DrawShips(gameState.Players);
			Console.SetCursorPosition(0, 0);
			DrawLasers(gameState.Players, gameState.Round);
			DrawStatistics(gameState.Players);
		}

		public static void DrawStatistics(List<Player> players)
		{
			int left = GridWidth + 5;
			int top = 1;
			foreach (var player in players)
			{
				if (player.Destroyed)
				{
					DrawDestroyedShip(left, top, player.ConsoleColor);
				}
				else
				{
					DrawShip(left, top, player.ConsoleColor);
				}
				Console.Write(" " + player.Name + " ");
				player.WriteStats();
				top = top + 3;
			}
		}

		private static void DrawGrid()
		{
			Console.ForegroundColor = ConsoleColor.DarkYellow;
			foreach (string row in gridTemplate)
			{
				Console.WriteLine(row);
			}
		}

		private static void DrawShips(List<Player> players)
		{
			// edit grid to include new position of each player.
			foreach (var player in players)
			{
				int x = 1 + (player.Position.X * 5);
				int y = 1 + (player.Position.Y * 3);
				if (player.Destroyed)
				{
					DrawDestroyedShip(x, y, ConsoleColor.DarkRed);
				}
				else
				{
					DrawShip(x, y, player.ConsoleColor);
				}
			}
		}

		private static void DrawShip(int x, int y, ConsoleColor color)
		{
			Console.ForegroundColor = color;
			Console.SetCursorPosition(x, y);
			Console.Write(ShipTop);
			Console.SetCursorPosition(x, y + 1);
			Console.Write(ShipBottom);
		}
		private static void DrawDestroyedShip(int x, int y, ConsoleColor color)
		{
			Console.ForegroundColor = color;
			Console.SetCursorPosition(x, y);
			Console.Write(ShipExplodeTop);
			Console.SetCursorPosition(x, y + 1);
			Console.Write(ShipExplodeBottom);
		}

		private static void DrawLasers(List<Player> players, int currentRound)
		{
			// draw the laser fire from each ship
			foreach (var player in players)
			{
				if (player.Destroyed && player.RoundDestroyed != currentRound) continue;

				var otherPositions = players.Where(x => x != player).Select(x => x.Position).ToList();

				switch ((Directions)player.ShootDirection)
				{
					case Directions.Right:
						DrawLaserRight(player.Position, otherPositions);
						break;
					case Directions.Left:
						DrawLaserLeft(player.Position, otherPositions);
						break;
					case Directions.Up:
						DrawLaserUp(player.Position, otherPositions);
						break;
					case Directions.Down:
						DrawLaserDown(player.Position, otherPositions);
						break;
					default:
						break;
				}
			}
		}

		private static void DrawLaserRight(Position playerPosition, List<Position> otherPositions)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			var cellToDraw = ConvertPositionToCursorPosition(playerPosition);
			Console.SetCursorPosition(cellToDraw[0] + 3, cellToDraw[1]);
			Console.Write("=");

			for (int x = playerPosition.X + 1; x < _gridSize.Cols; x++)
			{
				Position positionToDraw = new Position(x, playerPosition.Y);
				var laserStopped = DrawLaserCellRight(positionToDraw, otherPositions);
				if (laserStopped) break;
			}
		}

		private static void DrawLaserLeft(Position playerPosition, List<Position> otherPositions)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			var cellToDraw = ConvertPositionToCursorPosition(playerPosition);
			Console.SetCursorPosition(cellToDraw[0], cellToDraw[1]+1);
			Console.Write("=");

			for (int x = playerPosition.X - 1; x >= 0; x--)
			{
				Position positionToDraw = new Position(x, playerPosition.Y);
				var laserStopped = DrawLaserCellLeft(positionToDraw, otherPositions);
				if (laserStopped) break;
			}
		}
		
		private static void DrawLaserUp(Position playerPosition, List<Position> otherPositions)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			var cellToDraw = ConvertPositionToCursorPosition(playerPosition);
			Console.SetCursorPosition(cellToDraw[0] + 2, cellToDraw[1]);
			Console.Write("^");

			for (int y = playerPosition.Y - 1; y >= 0; y--)
			{
				Position positionToDraw = new Position(playerPosition.X, y);
				var laserStopped = DrawLaserCellUp(positionToDraw, otherPositions);
				if (laserStopped) break;
			}
		}

		private static void DrawLaserDown(Position playerPosition, List<Position> otherPositions)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			var cellToDraw = ConvertPositionToCursorPosition(playerPosition);
			Console.SetCursorPosition(cellToDraw[0] + 1, cellToDraw[1] + 1);
			Console.Write(".");

			for (int y = playerPosition.Y + 1; y < _gridSize.Rows; y++)
			{
				Position positionToDraw = new Position(playerPosition.X, y);
				var laserStopped = DrawLaserCellDown(positionToDraw, otherPositions);
				if (laserStopped) break;
			}
		}

		private static bool DrawLaserCellRight(Position positionToDraw, List<Position> otherPositions)
		{
			var laserStopped = otherPositions.Any(p => p.X == positionToDraw.X && p.Y == positionToDraw.Y);
			if (laserStopped) return true;
			var cellToDraw = ConvertPositionToCursorPosition(positionToDraw);
			Console.SetCursorPosition(cellToDraw[0], cellToDraw[1]);
			Console.Write("----");
			return false;
		}

		private static bool DrawLaserCellLeft(Position positionToDraw, List<Position> otherPositions)
		{
			var laserStopped = otherPositions.Any(p => p.X == positionToDraw.X && p.Y == positionToDraw.Y);
			if (laserStopped) return true;
			var cellToDraw = ConvertPositionToCursorPosition(positionToDraw);
			Console.SetCursorPosition(cellToDraw[0], cellToDraw[1] + 1);
			Console.Write("----");
			return false;
		}
		
		private static bool DrawLaserCellUp(Position positionToDraw, List<Position> otherPositions)
		{
			var laserStopped = otherPositions.Any(p => p.X == positionToDraw.X && p.Y == positionToDraw.Y);
			if (laserStopped) return true;
			var cellToDraw = ConvertPositionToCursorPosition(positionToDraw);
			Console.SetCursorPosition(cellToDraw[0] + 2, cellToDraw[1]);
			Console.Write(":");
			Console.SetCursorPosition(cellToDraw[0] + 2, cellToDraw[1] + 1);
			Console.Write(":");
			return false;
		}

		private static bool DrawLaserCellDown(Position positionToDraw, List<Position> otherPositions)
		{
			var laserStopped = otherPositions.Any(p => p.X == positionToDraw.X && p.Y == positionToDraw.Y);
			if (laserStopped) return true;
			var cellToDraw = ConvertPositionToCursorPosition(positionToDraw);
			Console.SetCursorPosition(cellToDraw[0] + 1, cellToDraw[1]);
			Console.Write(":");
			Console.SetCursorPosition(cellToDraw[0] + 1, cellToDraw[1] + 1);
			Console.Write(":");
			return false;
		}

		

		public static int[] ConvertPositionToCursorPosition(Position p)
		{
			int xCursorPosition = (p.X * 5) + 1;
			int yCursorPosition = (p.Y * 3) + 1;
			return new int[2] { xCursorPosition, yCursorPosition };
		}
	}
}
