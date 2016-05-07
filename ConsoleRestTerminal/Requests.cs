using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using ConsoleRestTerminal.Models.Requests;
using ConsoleRestTerminal.Models.Responses;

namespace ConsoleRestTerminal
{
	public class Requests
	{
		public async Task<IList<TurnResponse>>  RunGameRequests(string url1, string url2, GameState gameState)
		{
			// Declare an HttpClient object, and increase the buffer size. The
			// default buffer size is 65,536.
			HttpClient client = new HttpClient() { MaxResponseContentBufferSize = 1000000 };  // is this max buffer size needed?

			Task<string> response2 = SendPostAsync<GameState>(client, url2 + "Turn", gameState);
			Thread.Sleep(10);  // hack to make sure target servers are generating seeds from different timestamps
			Task<string> response1 = SendPostAsync<GameState>(client, url1 + "Turn", gameState);
			
			// Await each task.
			string jsonResponse1 = await response1;
			string jsonResponse2 = await response2;


			var turnResponse1 = new JavaScriptSerializer().Deserialize<TurnResponse>(jsonResponse1);
			var turnResponse2 = new JavaScriptSerializer().Deserialize<TurnResponse>(jsonResponse2);

			IList<TurnResponse> turnResponses = new List<TurnResponse>();
			turnResponses.Add(turnResponse1);
			turnResponses.Add(turnResponse2);
			
			return turnResponses;
		}


		private static int[] ConvertDirectionToCoordinate(int direction, int x, int y)
		{
			if (direction == 1) x = x + 1;
			if (direction == 3) x = x - 1;
			if (direction == 0) y = y - 1;
			if (direction == 2) y = y + 1;
			return new int[] { x, y };
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


		async Task<string> SendPostAsync<T>(HttpClient client, string url, T postObject) //where U : String
		{
			// turn object to string
			string jsonPostBody = new JavaScriptSerializer().Serialize(postObject);

			// configure request
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));  // necessary?
			
			// post request and get response 
			HttpResponseMessage response = await client.PostAsync(url, new StringContent(jsonPostBody, Encoding.UTF8, "application/json"));

			// get response data
			string responseData = await response.Content.ReadAsStringAsync();
			return responseData;
		}



	}
}
