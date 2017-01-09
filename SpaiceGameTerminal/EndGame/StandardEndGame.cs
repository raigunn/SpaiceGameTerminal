using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaiceGameTerminal.Models.Requests;

namespace SpaiceGameTerminal.EndGame
{
	public class StandardEndGame : IEndGame
	{
		public void CheckForEndGame(GameState gameState)
		{
			int destroyedCount = gameState.Players.Count(player => player.Destroyed);
			if (destroyedCount == gameState.Players.Count) // no survivors - no winners or draws
			{
				gameState.Winners = null;
				gameState.GameOver = true;
			}
			else if (destroyedCount == gameState.Players.Count - 1)  // one survivor - the winner
			{
				gameState.Winners = gameState.Players.Where(player => !player.Destroyed).ToList();
				gameState.GameOver = true;
				foreach (var player in gameState.Winners)
				{
					player.Wins++;
				}
			}
			else if (gameState.Round == gameState.TotalRounds.Value) // multiple survivors - draw
			{
				gameState.Winners = gameState.Players.Where(player => !player.Destroyed).ToList();
				gameState.GameOver = true;
				foreach (var player in gameState.Winners)
				{
					player.Draws++;
				}
			}
		}

	}
}
