using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaiceGameTerminal.Models;

namespace SpaiceGameTerminal.Extensions
{
	public static class RegisteredPlayerExt
	{
		public static void Log(this RegisteredPlayer registeredPlayer)
		{
			Console.WriteLine(registeredPlayer.GetType().FullName);
			Console.WriteLine(registeredPlayer.Id);
			Console.WriteLine(registeredPlayer.Name);
			Console.WriteLine(registeredPlayer.Url);
			Console.WriteLine();
		}
	}
}
