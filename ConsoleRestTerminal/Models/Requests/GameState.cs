using System.Collections.Generic;

namespace ConsoleRestTerminal.Models.Requests
{
	public class GameState
	{
		public int Round { get; set; }
		public int[] GridSize { get; set; }
		public List<int[]> Players { get; set; }

		public static GameState MakeInitialGameState()
		{
			var gameState = new GameState()
			{
				Round = 1,
				GridSize = new int[] { 8, 8 },
				Players = new List<int[]>() { new int[] { 0, 1 }, new int[] { 2, 3 } }
			};
			return gameState;
		}
	}

}
