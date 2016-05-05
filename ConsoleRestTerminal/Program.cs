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
using ConsoleRestTerminal.Models.Requests;
using ConsoleRestTerminal.Models.Responses;

namespace ConsoleRestTerminal
{
	//string url = "http://localhost:8000/DEMOService/";

	class Program
	{
		private static string _url1 = "";
		private static string _url2 = "";
		static void Main(string[] args)
		{
			// begin dialog to get url of server
			_url1 = GetUrl1();
			_url2 = GetUrl2();
			int turnCount = GetTurnCount();
			DisplayServers();

			Console.WriteLine(@"Press 'enter' to start game");
			Console.ReadKey();
			ConsoleDraw.InitializeGrid();

			GameState initialGameState = GameState.MakeInitialGameState();
			for (int i = turnCount; i > 0; i--)
			{
				ConsoleDraw.DrawGrid(initialGameState.Players);

				Requests bs = new Requests();
				Task<IList<TurnResponse>> turnResponses = bs.RunGameRequests(_url1, _url2, initialGameState);

				Task.WaitAll(turnResponses); // block while the task completes

				int[] player1position = ConvertDirectionToCoordinate(turnResponses.Result[0].MoveDirection, initialGameState.Players[0][0], initialGameState.Players[0][1]);
				int[] player2position = ConvertDirectionToCoordinate(turnResponses.Result[1].MoveDirection, initialGameState.Players[1][0], initialGameState.Players[1][1]);

				initialGameState = MakeGameState(1, player1position[0], player1position[1], player2position[0], player2position[1]);
			}



			Console.WriteLine("done calculating, press 'enter' to watch");
			Console.ReadKey();
		//	DrawGame();

			// exit on keystroke
			Console.ReadKey();
		}


		private static int[] ConvertDirectionToCoordinate(int direction, int x, int y)
		{
			if (direction == 1) x = x + 1;
			if (direction == 3) x = x - 1;
			if (direction == 0) y = y - 1;
			if (direction == 2) y = y + 1;
			return new int[] { x, y };
		}


		private static string GetUrl1()
		{
			Console.WriteLine("Enter in server info: (http://localhost:8000/DEMOService/");

			string url1 = "";
			bool validServer = false;
			do
			{
				url1 = Console.ReadLine();
				if (url1 == "exit") return "exit";
				if (url1 == "default1") url1 = "http://localhost:8000/DEMOService/";
				else if (url1 == "default2") url1 = "http://localhost:8001/RestService/";

				Console.WriteLine("");
				PingResponse pingResponse = Requests.MakeGetRequest<PingResponse>(url1 + "Ping");
				if (pingResponse == null)
				{
					Console.WriteLine("Please try again or type 'exit'");
				}
				else
				{
					ProcessResponse(pingResponse);
					validServer = true;
				}
			} while (!validServer);
			return url1;
		}

		private static string GetUrl2()
		{
			Console.WriteLine("Enter in server info: (http://localhost:8000/DEMOService/");

			string url2 = "";
			bool validServer = false;
			do
			{
				url2 = Console.ReadLine();
				if (url2 == "exit") return "exit";
				if (url2 == "default1") url2 = "http://localhost:8000/DEMOService/";
				else if (url2 == "default2") url2 = "http://localhost:8001/RestService/";

				Console.WriteLine("");
				PingResponse pingResponse = Requests.MakeGetRequest<PingResponse>(url2 + "Ping");
				if (pingResponse == null)
				{
					Console.WriteLine("Please try again or type 'exit'");
				}
				else
				{
					ProcessResponse(pingResponse);
					validServer = true;
				}
			} while (!validServer);

			return url2;
		}

		private static void DisplayServers()
		{
			Console.WriteLine("Thank you, your two servers are:");
			Console.WriteLine("URL 1: " + _url1);
			Console.WriteLine("URL 2: " + _url2);
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


		private static GameState MakeInitialGameState()
		{
			var gameState = new GameState()
			{
				Round = 1,
				GridSize = new int[] {8,8},
				Players = new List<int[]>() { new int[] {0, 1}, new int[] {4, 5}}
			};
			return gameState;
		}

		private static GameState MakeGameState(int round, int x, int y, int xx, int yy)
		{
			var gameState = new GameState()
			{
				Round = round,
				GridSize = new int[] { 8, 8 },
				Players = new List<int[]>() { new int[] { x, y }, new int[] { xx, yy } }
			};
			return gameState;
		}



		private static void ProcessTurnResponse(TurnResponse turnResponse)
		{
			var json = new JavaScriptSerializer().Serialize(turnResponse);
			Console.WriteLine(json);
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
		private static void DrawGame()
		{
			Console.CursorVisible = false;

			string shipTop =		@"/''\";
			string shipBottom = @"\__/";

			var arr = new[]
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


			Console.WindowWidth = 100;
			Console.WindowHeight = 40;
			Console.Clear();
			foreach (string line in arr)
			{
				Console.WriteLine(line);
			}
			Thread.Sleep(500);



			List<int[]> testPositions = new List<int[]>();
			testPositions.Add(new[] { 0, 0 });
			testPositions.Add(new[] { 1, 1 });
			testPositions.Add(new[] { 2, 2 });
			testPositions.Add(new[] { 3, 3 });
			testPositions.Add(new[] { 4, 4 });

			foreach (var testPosition in testPositions)
			{
				int rowIndexA = 1 + (testPosition[1]*3);
				int rowIndexB = rowIndexA + 1;
				string rowA = arr[rowIndexA];
				string rowB = arr[rowIndexB];
				int colIndexA = 1 + (testPosition[0]*5);
				int colIndexB = colIndexA + 4;
				StringBuilder sbA = new StringBuilder(rowA);
				sbA.Replace("    ", shipTop, colIndexA, 4);
				StringBuilder sbB = new StringBuilder(rowB);
				sbB.Replace("    ", shipBottom, colIndexA, 4);
				arr[rowIndexA] = sbA.ToString();
				arr[rowIndexB] = sbB.ToString();


				Console.Clear();
				foreach (string line in arr)
				{
					Console.WriteLine(line);
				}
				Thread.Sleep(500);
			}



			Console.ReadKey();
		}

	}
}
