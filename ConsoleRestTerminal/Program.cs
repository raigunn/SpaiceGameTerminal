using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using ConsoleRestTerminal.Enums;
using ConsoleRestTerminal.Models;
using ConsoleRestTerminal.Models.Requests;
using ConsoleRestTerminal.Models.Responses;

namespace ConsoleRestTerminal
{
	//string url = "http://localhost:8000/DEMOService/";

	class Program
	{
		private static readonly List<string> _urls = new List<string>();
		private static string _gameId = "";
		static void Main(string[] args)
		{
			// generate game id
			_gameId = RandomWrapper.RandomNumber(1, 10000).ToString();  // I will need this, but don't need it yet



			// begin dialog to get url of server
			GetUrls();

			// dialog to get turn count
			int turnCount = GetTurnCount();

			// output that we are ready
			DisplayServers();

			Console.WriteLine(@"Press 'enter' to start game");
			Console.ReadKey();
			ConsoleDraw.InitializeGrid();

			GameState initialGameState = GameState.MakeInitialGameState();
			for (int i = turnCount; i > 0; i--)
			{
				ConsoleDraw.DrawGrid(initialGameState.Players);

				Requests bs = new Requests();
				Task<IList<TurnResponse>> turnResponses = bs.RunGameRequests2(_urls, initialGameState);

				Task.WaitAll(turnResponses); // block while the task completes

				// todo convert this to take N players
				// note: I think it will require correlating player responses
				Position position1 = ConvertDirectionToPosition((Directions)turnResponses.Result[0].MoveDirection, initialGameState.Players[0].Position);
				Position position2 = ConvertDirectionToPosition((Directions)turnResponses.Result[1].MoveDirection, initialGameState.Players[1].Position);
				Player player1 = new Player(0, "player 1", position1, turnResponses.Result[0].ShootDirection);
				Player player2 = new Player(0, "player 2", position2, turnResponses.Result[1].ShootDirection);

				initialGameState = MakeGameState(1, player1.Position, player2.Position, player1.ShootDirection, player2.ShootDirection);
			}


			Console.WriteLine("done calculating, press 'enter' to watch");


			// exit on keystroke
			Console.ReadKey();
		}


		private static Position ConvertDirectionToPosition(Directions direction, Position p)
		{
			if (direction == Directions.Right) p.X++;
			if (direction == Directions.Left) p.X--;
			if (direction == Directions.Up) p.Y--;
			if (direction == Directions.Down) p.Y++;
			return p;
		}




		private static void GetUrls()
		{
			do
			{
				Console.WriteLine("");
				Console.WriteLine("Enter in server info: (http://localhost:8000/DEMOService/");
				Console.WriteLine("'exit' to close application");
				Console.WriteLine("'done' when finished adding servers");

				string input = Console.ReadLine();
				//if (input == "exit") return "exit"; // todo
				if (input == "done")
				{
					if (_urls.Count >= 2)
					{
						break;
					}
					else
					{
						Console.WriteLine("There should be at least 2 servers to continue, please enter another.");
						continue;
					}
				}

				var inputUrl = input == "default1" ? "http://localhost:8000/DEMOService/" : input;

				PingResponse pingResponse = Requests.MakeGetRequest<PingResponse>(inputUrl + "Ping");
				if (pingResponse == null)
				{
					Console.WriteLine("Please try again or type 'exit' or 'done'");
					continue;
				}
					
				ProcessResponse(pingResponse);
				_urls.Add(inputUrl);
			} while (true);
		}

		//private static void GetUrls()
		//{

		//	// http://stackoverflow.com/questions/20365214/a-simple-menu-in-a-console-application

		//}




		private static void DisplayServers()
		{
			Console.WriteLine("Thank you, your two servers are:");
			foreach (var url in _urls)
			{
				Console.WriteLine("URL: " + url);
			}
		}


		private static int GetTurnCount()
		{
			Console.WriteLine("Enter number of rounds");

			string turnCount = "";
			int intTurnCount;
			bool isValid = false;
			do
			{
				turnCount = Console.ReadLine();
				if (!int.TryParse(turnCount, out intTurnCount) || intTurnCount < 1 || intTurnCount > 100)
				{
					Console.WriteLine("Please enter a valid number");
				}
				else isValid = true;
			} while (!isValid);
			return intTurnCount;
		}



		



		static public void ProcessResponse(PingResponse pingResponse)
		{
			Console.WriteLine(pingResponse.Ping);
			Console.WriteLine();
		}


		//private static GameState MakeInitialGameState()
		//{
		//	var gameState = new GameState()
		//	{
		//		Round = 1,
		//		GridSize = new int[] {8,8},
		//		Players = new List<Player>() { new Player(0, "player1", new Position(0, 7, 0, 0), (int)Directions.Down), new Player(1, "player2", new Position(4, 5, 0, 0), (int)Directions.Up) }
		//	};
		//	return gameState;
		//}

		//private static GameState MakeGameState(int round, Position player1, Position player2)
		//{
		//	var gameState = new GameState()
		//	{
		//		Round = round,
		//		GridSize = new int[] { 8, 8 },
		//		Players = new List<Player>() { new Player(0, "player1", player1, (int)Directions.Down), new Player(1, "player2", player2, (int)Directions.Up) }
		//	};
		//	return gameState;
		//}


		private static GameState MakeGameState(int round, Position player1, Position player2, int player1ShootDirection, int player2ShootDirection)
		{
			var gameState = new GameState()
			{
				Round = round,
				GridSize = new int[] { 8, 8 },
				Players = new List<Player>() { new Player(0, "player1", player1, player1ShootDirection), new Player(1, "player2", player2, player2ShootDirection) }
			};
			return gameState;
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
		//private static void DrawGame()
		//{
		//	Console.CursorVisible = false;

		//	string shipTop =		@"/''\";
		//	string shipBottom = @"\__/";

		//	var arr = new[]
		//	{
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


		//	Console.WindowWidth = 100;
		//	Console.WindowHeight = 40;
		//	Console.Clear();
		//	foreach (string line in arr)
		//	{
		//		Console.WriteLine(line);
		//	}
		//	Thread.Sleep(500);



		//	List<int[]> testPositions = new List<int[]>();
		//	testPositions.Add(new[] { 0, 0 });
		//	testPositions.Add(new[] { 1, 1 });
		//	testPositions.Add(new[] { 2, 2 });
		//	testPositions.Add(new[] { 3, 3 });
		//	testPositions.Add(new[] { 4, 4 });

		//	foreach (var testPosition in testPositions)
		//	{
		//		int rowIndexA = 1 + (testPosition[1]*3);
		//		int rowIndexB = rowIndexA + 1;
		//		string rowA = arr[rowIndexA];
		//		string rowB = arr[rowIndexB];
		//		int colIndexA = 1 + (testPosition[0]*5);
		//		int colIndexB = colIndexA + 4;
		//		StringBuilder sbA = new StringBuilder(rowA);
		//		sbA.Replace("    ", shipTop, colIndexA, 4);
		//		StringBuilder sbB = new StringBuilder(rowB);
		//		sbB.Replace("    ", shipBottom, colIndexA, 4);
		//		arr[rowIndexA] = sbA.ToString();
		//		arr[rowIndexB] = sbB.ToString();


		//		Console.Clear();
		//		foreach (string line in arr)
		//		{
		//			Console.WriteLine(line);
		//		}
		//		Thread.Sleep(500);
		//	}



		//	Console.ReadKey();
		//}

	}
}
