using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using ConsoleRestTerminal.Models.Requests;
using ConsoleRestTerminal.Models.Responses;

namespace ConsoleRestTerminal
{
	public class AsyncRequest2
	{



		public async Task GetTurnCount2(string url1, string url2)
		{
			Console.WriteLine("Enter number of rounds to begin game, or type 'exit'");

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
				else
				{
					//Task<TurnResponse> turnResponse;;// = new TurnResponse();
					int x = 0;
					int y = 1;
					Console.WriteLine("Starting Game...");

					// first turn
					GameState gameState = MakeInitialGameState();

					Task<TurnResponse> turnResponse1 = MakePostRequest2<GameState>(url1 + "Turn", gameState);
					Task<TurnResponse> turnResponse2 = MakePostRequest2<GameState>(url2 + "Turn", gameState);

					Task.WaitAll(turnResponse1, turnResponse2);

					Console.Write("1: ");
					ProcessTurnResponse(turnResponse1.Result);
					Console.Write("2: ");
					ProcessTurnResponse(turnResponse2.Result);

					// all turns past first turn
					for (int i = 1; i < intTurnCount; i++)
					{
						int[] newPosition1 = ConvertDirectionToCoordinate(turnResponse1.Result.MoveDirection, x, y);
						int[] newPosition2 = ConvertDirectionToCoordinate(turnResponse2.Result.MoveDirection, x, y);
						gameState = MakeGameState(i + 1, newPosition1[0], newPosition1[1]);

						turnResponse1 = MakePostRequest2<GameState>(url1 + "Turn", gameState);
						turnResponse2 = MakePostRequest2<GameState>(url2 + "Turn", gameState);
						Task.WaitAll(turnResponse1, turnResponse2);

						ProcessTurnResponse(turnResponse1.Result);
						ProcessTurnResponse(turnResponse2.Result);
					}
					isValid = true;
				}
			} while (!isValid);
		}



		// see: http://stackoverflow.com/questions/202481/how-to-use-httpwebrequest-net-asynchronously
		private async Task<TurnResponse> MakePostRequest2<T>(string requestUrl, T gameState) //where U : class
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
				using (HttpWebResponse response = (HttpWebResponse)await Task.Factory.FromAsync<WebResponse>(request.BeginGetResponse, request.EndGetResponse, null))
				{
					if (response.StatusCode != HttpStatusCode.OK)
						throw new Exception($"Server error (HTTP {response.StatusCode}: {response.StatusDescription}).");

					// convert json string to type U
					DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(TurnResponse));
					object objResponse = jsonSerializer.ReadObject(response.GetResponseStream());
					TurnResponse jsonResponse = (TurnResponse)objResponse;

					return jsonResponse;
				}

			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				return null;
			}
		}


		private static GameState MakeInitialGameState()
		{
			var gameState = new GameState()
			{
				Round = 1,
				GridSize = new int[] { 8, 8 },
				Players = new List<int[]>() { new int[] { 0, 1 }, new int[] { 2, 3 } }
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

		private static int[] ConvertDirectionToCoordinate(int direction, int x, int y)
		{
			if (direction == 1) x = x + 1;
			if (direction == 3) x = x - 1;
			if (direction == 0) y = y - 1;
			if (direction == 2) y = y + 1;
			return new int[] { x, y };
		}



		private static void ProcessTurnResponse(TurnResponse turnResponse)
		{
			var json = new JavaScriptSerializer().Serialize(turnResponse);
			Console.WriteLine(json);
		}



	}
}
