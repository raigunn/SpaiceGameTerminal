using System.Collections.Generic;

namespace ConsoleRestTerminal.Models.Requests
{
	public class GameState
	{
		public int Round { get; set; }
		public int[] GridSize { get; set; }
		public List<int[]> Players { get; set; }
	}
}
