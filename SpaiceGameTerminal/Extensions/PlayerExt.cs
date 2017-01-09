using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaiceGameTerminal.Models.Requests;

namespace SpaiceGameTerminal.Extensions
{
	public static class PlayerExt
	{
		public static void Log(this Player player)
		{
			Console.WriteLine(player.GetType().FullName);
			Console.WriteLine(player.Id);
			Console.WriteLine(player.Name);
			Console.WriteLine(player.Url);
			Console.WriteLine();
		}
	}
}
