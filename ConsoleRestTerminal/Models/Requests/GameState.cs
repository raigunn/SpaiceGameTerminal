using System.Collections.Generic;
using System.Runtime.Serialization;
using ConsoleRestTerminal.Enums;

namespace ConsoleRestTerminal.Models.Requests
{
	[DataContract]
	public class GameState
	{
		[DataMember]
		public int Round { get; set; }
		[DataMember]
		public int[] GridSize { get; set; }
		[DataMember]
		public List<Player> Players { get; set; }

		[DataMember]
		public int Identifier { get; set; }

		public static GameState MakeInitialGameState()
		{
			var gameState = new GameState()
			{
				Round = 1,
				GridSize = new int[] { 8, 8 },
				Players = new List<Player>() { new Player(0, "player1", new Position(3, 3, 0, 0), (int)Directions.Up), new Player(1, "player2", new Position(4, 4, 0, 0), (int)Directions.Left ) }
			};
			return gameState;
		}
	}

}
