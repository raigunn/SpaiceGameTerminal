using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SpaiceGameTerminal
{
	public static class Intro
	{
		public static void Play()
		{
			Console.CursorVisible = false;
			Console.ForegroundColor = ConsoleColor.Red;

			//var arr = new[]
			//{
			//			@"              _    ___             ",
			//			@"  ___ _ __   / \  |_ _|___ ___     ",
			//			@" / __| '_ \ / _ \  | |/ __/ _ \    ",
			//			@" \__ \ |_) / ___ \ | | (_|  __/    ",
			//			@" |___/ .__/_/  _\_\___\___\___|___ ",
			//			@"     |_|/ _` |/ _` | '_ ` _ \ / _ \",
			//			@"       | (_| | (_| | | | | | |  __/",
			//			@"        \__, |\__,_|_| |_| |_|\___|",
			//			@"        |___/                      "
			//	};



			var arr = new[]
			{
						@"                  _____  .___              ",
						@"  ____________   /  _  \ |   | ____  ____  ",
						@" /  ___/\____ \ /  /_\  \|   |/ ___\/ __ \ ",
						@" \___ \ |  |_> >    |    \   \  \__\  ___/ ",
						@"/____  >|   __/\____|__  /___|\___  >___  >",
						@"     \/ |__|           \/         \/    \/ ",
						@"                                           ",
						@"         _________    _____   ____         ",
						@"        / ___\__  \  /     \_/ __ \        ",
						@"       / /_/  > __ \|  Y Y  \  ___/        ",
						@"       \___  (____  /__|_|  /\___  >       ",
						@"      /_____/     \/      \/     \/        "
				};

			var maxLength = arr.Aggregate(0, (max, line) => Math.Max(max, line.Length));
			var x = Console.BufferWidth / 2 - maxLength / 2;
			for (int y = -arr.Length; y < Console.WindowHeight / 2 - arr.Length / 2; y++)
			{
				ConsoleDraw(arr, x, y);
				Thread.Sleep(100);
			}
			
			Console.SetCursorPosition(0, Console.WindowHeight - 1);
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.Write("press [any key] to continue...");
			Console.ReadKey();
		}

		static void ConsoleDraw(IEnumerable<string> lines, int x, int y)
		{
			if (x > Console.WindowWidth) return;
			if (y > Console.WindowHeight) return;

			var trimLeft = x < 0 ? -x : 0;
			int index = y;

			x = x < 0 ? 0 : x;
			y = y < 0 ? 0 : y;

			var linesToPrint =
					from line in lines
					let currentIndex = index++
					where currentIndex > 0 && currentIndex < Console.WindowHeight
					select new
					{
						Text = new String(line.Skip(trimLeft).Take(Math.Min(Console.WindowWidth - x, line.Length - trimLeft)).ToArray()),
						X = x,
						Y = y++
					};

			Console.Clear();
			foreach (var line in linesToPrint)
			{
				Console.SetCursorPosition(line.X, line.Y);
				Console.Write(line.Text);
			}
		}
	}
}
