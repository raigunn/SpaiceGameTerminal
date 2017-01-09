using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using SpaiceGameTerminal.Models.Responses;

namespace SpaiceGameTerminal.Extensions
{
	public static class RegisterResponseExt
	{
		public static void Log(this RegisterResponse registerResponse)
		{
			Console.WriteLine(registerResponse.GetType());
			Console.WriteLine(registerResponse.ShipName);
			Console.WriteLine();
		}
	}
}
