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
using SpaiceGameTerminal.Models;
using SpaiceGameTerminal.Models.Requests;
using SpaiceGameTerminal.Models.Responses;

namespace SpaiceGameTerminal
{
	public class Requests
	{
		public async Task<IList<TurnResponse>> RunGameRequest(GameState gameState)
		{
			// Declare an HttpClient object, and increase the buffer size. The
			// default buffer size is 65,536.
			HttpClient client = new HttpClient() { MaxResponseContentBufferSize = 1000000 };  // is this max buffer size needed?
			IList<TurnResponse> turnResponses = new List<TurnResponse>();

			List<Task<string>> responses = new List<Task<string>>();
			//int identifier = 0;
			foreach (var player in gameState.Players)
			{
				player.Me = "true";
				responses.Add(SendPostAsync<GameState>(client, player.Url + "Turn", gameState));
				player.Me = "false";
				Thread.Sleep(10); // hack to make sure target servers are generating seeds from different timestamps
			}

			List<string> jsonResponses = new List<string>();
			foreach (var response in responses)
			{
				jsonResponses.Add(await response);
			}

			foreach (var jsonResponse in jsonResponses)
			{
				turnResponses.Add(new JavaScriptSerializer().Deserialize<TurnResponse>(jsonResponse));
			}
			
			return turnResponses;
		}



		/// <summary>
		/// This is used primarily for the ping request
		/// It is a Get Request, and it is not Async
		/// </summary>
		/// <typeparam name="U"></typeparam>
		/// <param name="requestUrl"></param>
		/// <returns></returns>
		public static U MakeGetRequest<U>(string requestUrl) where U : class
		{
			HttpWebRequest request = WebRequest.Create(requestUrl) as HttpWebRequest;
			if (request == null)
			{
				throw new Exception("Unable to create a request");
			}
			using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
			{
				if (response == null)
				{
					throw new Exception("Unable to get a response");
				}

				if (response.StatusCode != HttpStatusCode.OK)
					throw new Exception($"Server error (HTTP {response.StatusCode}: {response.StatusDescription}).");
				DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(U));
				object objResponse = jsonSerializer.ReadObject(response.GetResponseStream());
				U jsonResponse = objResponse as U;

				return jsonResponse;
			}

			//try
			//{
			//	HttpWebRequest request = WebRequest.Create(requestUrl) as HttpWebRequest;
			//	if (request == null)
			//	{
			//		throw new Exception("Unable to create a request");
			//	}
			//	using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
			//	{
			//		if (response == null)
			//		{
			//			throw new Exception("Unable to get a response");
			//		}

			//		if (response.StatusCode != HttpStatusCode.OK)
			//			throw new Exception($"Server error (HTTP {response.StatusCode}: {response.StatusDescription}).");
			//		DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(U));
			//		object objResponse = jsonSerializer.ReadObject(response.GetResponseStream());
			//		U jsonResponse = objResponse as U;

			//		return jsonResponse;
			//	}
			//}
			//catch (Exception e)
			//{
			//	Console.ForegroundColor = ConsoleColor.Red;
			//	Console.WriteLine("ERROR: " + e.Message);
			//	return null;
			//}
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
