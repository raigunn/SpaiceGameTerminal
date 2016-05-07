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
	public class AynchRequest
	{

		public async Task GetTurnCount3(string url1, string url2)
		{
			Console.WriteLine("Enter number of rounds to begin game, or type 'exit'");

			string turnCount = "";
			int intTurnCount;
			bool isValid = false;
			do
			{
				turnCount = Console.ReadLine();
				//if (turnCount == "exit") return "exit";
				if (!int.TryParse(turnCount, out intTurnCount) || intTurnCount < 1 || intTurnCount > 100)
				{
					Console.WriteLine("Please enter a valid number");
				}
				else isValid = true;
			} while (!isValid);


			int x = 0;
			int y = 1;
			Console.WriteLine("Starting Game...");

			// first turn
			GameState gameState = MakeInitialGameState();


			TurnResponse turnResponse1 = await GetURLContentsAsync(url1 + "Turn", gameState);
			TurnResponse turnResponse2 = await GetURLContentsAsync(url2 + "Turn", gameState);

			Console.Write("1: ");
			ProcessTurnResponse(turnResponse1);
			Console.Write("2: ");
			ProcessTurnResponse(turnResponse2);

			// all turns past first turn
			for (int i = 1; i < intTurnCount; i++)
			{
				int[] newPosition1 = ConvertDirectionToCoordinate(turnResponse1.MoveDirection, x, y);
				int[] newPosition2 = ConvertDirectionToCoordinate(turnResponse2.MoveDirection, x, y);
				gameState = MakeGameState(i + 1, newPosition1[0], newPosition1[1]);

				turnResponse1 = await GetURLContentsAsync(url1 + "Turn", gameState);
				turnResponse2 = await GetURLContentsAsync(url2 + "Turn", gameState);

				ProcessTurnResponse(turnResponse1);
				ProcessTurnResponse(turnResponse2);
			}
		}




		private async Task<TurnResponse> GetURLContentsAsync(string url, GameState gameState)
		{
			// The downloaded resource ends up in the variable named content.
			var content = new MemoryStream();

			// Initialize an HttpWebRequest for the current URL.
			var webReq = (HttpWebRequest)WebRequest.Create(url);
			string json = new JavaScriptSerializer().Serialize(gameState);
			byte[] byteArray = new UTF8Encoding().GetBytes(json);
			webReq.ContentLength = byteArray.Length;
			webReq.ContentType = @"application/json";
			webReq.Method = "POST";

			// send the request
			using (Stream dataStream = webReq.GetRequestStream())
			{
				dataStream.Write(byteArray, 0, byteArray.Length);
			}

			// Send the request to the Internet resource and wait for
			// the response.                
			using (WebResponse response = await webReq.GetResponseAsync())
			{
				// Get the data stream that is associated with the specified url.
				using (Stream responseStream = response.GetResponseStream())
				{
					// Read the bytes in responseStream and copy them to content. 
					await responseStream.CopyToAsync(content);

				}
			}

			// convert json string to type U
			DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(TurnResponse));
			object objResponse = jsonSerializer.ReadObject(content);
			return (TurnResponse)objResponse;
		}




		private static GameState MakeInitialGameState()
		{
			var gameState = new GameState();
			//{
			//	Round = 1,
			//	GridSize = new int[] { 8, 8 },
			//	Players = new List<int[]>() { new int[] { 0, 1 }, new int[] { 2, 3 } }
			//};
			return gameState;
		}

		private static GameState MakeGameState(int round, int x, int y)
		{
			var gameState = new GameState();
			//{
			//	Round = round,
			//	GridSize = new int[] { 8, 8 },
			//	Players = new List<int[]>() { new int[] { x, y }, new int[] { 2, 3 } }
			//};
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
