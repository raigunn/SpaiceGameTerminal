using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleRestTerminal
{
	public class GameState
	{
		public int Round { get; set; }
		public int[] GridSize { get; set; }
		public List<int[]> Players { get; set; }
	}
}
