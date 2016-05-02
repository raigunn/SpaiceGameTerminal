using System;
using System.Collections.Generic;
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
		private static string _url = "";
		static void Main(string[] args)
		{
			// begin dialog to get url of server
			_url = GetUrl();
			if (_url.Equals("exit")) return;

			string result = GetTurnCount();
			if (result.Equals("exit")) return;


			Thread.Sleep(1500);
			DrawGame();

			// exit on keystroke
			Console.ReadKey();
		}

		private static string GetUrl()
		{
			Console.WriteLine("Enter in server info: (http://localhost:8000/DEMOService/");

			string url = "";
			bool validServer = false;
			do
			{
				url = Console.ReadLine();
				if (url == "exit") return "exit";
				if (url == "default1") url = "http://localhost:8000/DEMOService/";

				Console.WriteLine("");
				PingResponse pingResponse = MakeGetRequest<PingResponse>(url + "Json");
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
			return url;
		}


		private static string GetTurnCount()
		{
			Console.WriteLine("Enter number of rounds to begin game, or type 'exit'");

			string turnCount = "";
			int intTurnCount;
			bool isValid = false;
			do
			{
				turnCount = Console.ReadLine();
				if (turnCount == "exit"  ) return "exit";
				if (!int.TryParse(turnCount, out intTurnCount) || intTurnCount < 1 || intTurnCount > 100)
				{
					Console.WriteLine("Please enter a valid number");
				}
				else
				{
					TurnResponse turnResponse = new TurnResponse();
					int x = 0;
					int y = 1;
					Console.WriteLine("Starting Game...");
					for (int i = 0; i < intTurnCount; i++)
					{
						GameState gameState;
						if (i == 0)
						{
							gameState = MakeInitialGameState();
						}
						else
						{
							if (turnResponse.MoveDirection == 1) x = x + 1;
							if (turnResponse.MoveDirection == 3) x = x - 1;
							if (turnResponse.MoveDirection == 0) y = y - 1;
							if (turnResponse.MoveDirection == 2) y = y + 1;
							gameState = MakeGameState(i + 1, x, y);
						}

						turnResponse = MakePostRequest<GameState, TurnResponse>(_url + "Turn", gameState);
						ProcessTurnResponse(turnResponse);
					}
					isValid = true;
				}
			} while (!isValid);
			return "done";
		}



		public static U MakeGetRequest<U>(string requestUrl) where U : class
		{
			try
			{
				HttpWebRequest request = WebRequest.Create(requestUrl) as HttpWebRequest;
				using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
				{
					if (response.StatusCode != HttpStatusCode.OK)
						throw new Exception(String.Format("Server error (HTTP {0}: {1}).", response.StatusCode, response.StatusDescription));
					DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(U));
					object objResponse = jsonSerializer.ReadObject(response.GetResponseStream());
					U jsonResponse = objResponse as U;
					
					return jsonResponse;
				}
			}
			catch (Exception e)
			{
				Console.WriteLine("ERROR: " + e.Message);
				return null;
			}
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
				Players = new List<int[]>() { new int[] {0, 1}, new int[] {2, 3}}
			};
			return gameState;
		}

		private static GameState MakeGameState(int round, int x, int y)
		{
			var gameState = new GameState()
			{
				Round = round,
				GridSize = new int[] { 8, 8 },
				Players = new List<int[]>() { new int[] { x, y }, new int[] { 2, 3 } }
			};
			return gameState;
		}



		private static void ProcessTurnResponse(TurnResponse turnResponse)
		{
			var json = new JavaScriptSerializer().Serialize(turnResponse);
			Console.WriteLine(json);
		}


		private static U MakePostRequest<T, U>(string requestUrl, T gameState) where U : class
		{
			try
			{
				// prepare the request
				HttpWebRequest request = WebRequest.Create(requestUrl) as HttpWebRequest;
				string json = new JavaScriptSerializer().Serialize(gameState);
				byte[] byteArray = new UTF8Encoding().GetBytes(json);
				request.ContentLength = byteArray.Length;
				request.ContentType = @"application/json";
				request.Method = "POST";

				// send the request
				using (Stream dataStream = request.GetRequestStream())
				{
					dataStream.Write(byteArray, 0, byteArray.Length);
				}

				// get the response
				using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
				{
					if (response.StatusCode != HttpStatusCode.OK)
						throw new Exception($"Server error (HTTP {response.StatusCode}: {response.StatusDescription}).");

					// convert json string to type U
					DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(U));
					object objResponse = jsonSerializer.ReadObject(response.GetResponseStream());
					U jsonResponse = (U)objResponse;

					return jsonResponse;
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				return null;
			}
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
