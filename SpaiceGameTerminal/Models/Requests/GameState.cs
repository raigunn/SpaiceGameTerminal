using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using SpaiceGameTerminal.Collisions;
using SpaiceGameTerminal.Domain;
using SpaiceGameTerminal.EndGame;
using SpaiceGameTerminal.Enums;

namespace SpaiceGameTerminal.Models.Requests
{
	[DataContract]
	public class GameState
	{
		[DataMember]
		public int Round { get; set; }
		public TurnCount TotalRounds { get; set; }
		[DataMember]
		public GridSize GridSize { get; set; }
		[DataMember]
		public List<Player> Players { get; set; }
		[DataMember]
		public int Identifier { get; set; } // temp:  used to indicate if the first or second player is active

		// non-service properties
		public List<Player> Winners { get; set; }
		public bool GameOver { get; set; }
		public ICollisions Collisions;
		public IEndGame EndGame;

		private GameState()
		{
		}

		public static GameState MakeInitialGameState(List<Player> players, TurnCount totalRounds, GridSize gridSize, ICollisions iCollisions, IEndGame iEndGame)
		{
			var gameState = new GameState()
			{
				Round = 1,
				TotalRounds = totalRounds,
				GameOver = false,
				GridSize = gridSize,
				Players = new List<Player>(),
				Collisions = iCollisions,
				EndGame = iEndGame
			};

			foreach (var player in players)
			{
				int x = RandomWrapper.RandomNumber(0, gameState.GridSize.Cols);
				int y = RandomWrapper.RandomNumber(0, gameState.GridSize.Rows);
				player.Destroyed = false;
				player.Position = new Position(x, y);
				gameState.Players.Add(player);
			}
			return gameState;
		}

		public static GameState UpdateGameState(GameState gameState)
		{
			gameState.Round++;
			gameState.Players = gameState.Players;
			gameState.Collisions.HandleShipToShip(gameState);
			gameState.Collisions.HandleLaserToShip(gameState);
			gameState.EndGame.CheckForEndGame(gameState);
			return gameState;
		}
		
		//private static void CheckForEndGame(GameState gameState)
		//{
		//	int destroyedCount = gameState.Players.Count(player => player.Destroyed);
		//	if (destroyedCount == gameState.Players.Count) // no survivors - no winners or draws
		//	{
		//		gameState.Winners = null;
		//		gameState.GameOver = true;
		//	}
		//	else if (destroyedCount == gameState.Players.Count - 1)  // one survivor - the sinner
		//	{
		//		gameState.Winners = gameState.Players.Where(player => !player.Destroyed).ToList();
		//		gameState.GameOver = true;
		//		foreach (var player in gameState.Winners)
		//		{
		//			player.Wins++;
		//		}
		//	}
		//	else if(gameState.Round == gameState.TotalRounds.Value) // multiple survivors - draw
		//	{
		//		gameState.Winners = gameState.Players.Where(player => !player.Destroyed).ToList();
		//		gameState.GameOver = true;
		//		foreach (var player in gameState.Winners)
		//		{
		//			player.Draws++;
		//		}
		//	}
		//}
		
	}
}
