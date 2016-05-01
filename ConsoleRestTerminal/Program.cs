using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace ConsoleRestTerminal
{
	//string url = "http://localhost:8000/DEMOService/";
	class Program
	{
		static void Main(string[] args)
		{
			// begin dialog to get url of server
			string url = GetUrl();
			if (url.Equals("exit")) return;

			Console.WriteLine("sending gamestate");
			GameState gameState = MakeGameState();
			TurnResponse turnResponse = MakePostRequest<GameState, TurnResponse>(url + "Turn", gameState);
			ProcessTurnResponse(turnResponse);
			
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


		private static GameState MakeGameState()
		{
			var gameState = new GameState()
			{
				Round = 5,
				GridSize = new int[] {8,8},
				Players = new List<int[]>() { new int[] {0, 1}, new int[] {2, 3}}
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


	}
}
