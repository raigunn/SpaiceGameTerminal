using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaiceGameTerminal.Models.Requests;

namespace SpaiceGameTerminal.Collisions
{
	public class StandardCollisions : ICollisions
	{
		/// <summary>
		/// 
		/// Logic around kills/deaths gets a little complicated
		/// considering moving this out so all this method would do is check for collisions.
		/// </summary>
		/// <param name="gameState"></param>
		public void HandleShipToShip(GameState gameState)
		{
			// mechanic for keeping tracking if someone died during this round
			// so they can still get their own move if they didn't go first
			//Dictionary<string, bool> justdied = new Dictionary<string, bool>();
			//foreach (var player in gameState.Players)
			//{
			//	justdied.Add(player.Id, false);
			//}

			for (int i = 0; i < gameState.Players.Count; i++)
			{
				Player p = gameState.Players[i];
				if (p.Destroyed && p.RoundDestroyed != gameState.Round) continue;
				for (int j = 0; j < gameState.Players.Count; j++)
				{
					Player otherPlayer = gameState.Players[j];
					if (otherPlayer.Id == p.Id) continue;
					if (p.Position.Equals(otherPlayer.Position))
					{
						p.Destroyed = true;
						if (!otherPlayer.Destroyed || otherPlayer.RoundDestroyed == gameState.Round)
						{
							p.Kills++;
						}
						if (p.RoundDestroyed != gameState.Round)
						{
							p.Deaths++;
						}
						p.RoundDestroyed = gameState.Round;
					}
				}
			}
		}

		/// <summary>
		/// SRC - doing two things.  first half calculates the laser's projection
		/// second half checks for collisions
		/// Logic around kills/deaths gets a little complicated
		/// considering moving this out so all this method would do is check for collisions.
		/// </summary>
		/// <param name="gameState"></param>
		public void HandleLaserToShip(GameState gameState)
		{
			// mechanic for keeping tracking if someone died during this round
			// so they can still get their own move if they didn't go first
			//Dictionary<string, bool> justdied = new Dictionary<string, bool>();
			//foreach (var player in gameState.Players)
			//{
			//	justdied.Add(player.Id, false);
			//}

			for (int i = 0; i < gameState.Players.Count; i++)
			{
				// get a ship
				// project a laser
				// see if another ship is on the projection
				Player p = gameState.Players[i];
				if (p.Destroyed && p.RoundDestroyed != gameState.Round) continue;
				List<Position> laserProjection = new List<Position>();
				// todo adjust for current position of shooter
				if (p.ShootDirection == 0) // up
				{
					int y = p.Position.Y - 1;
					while (y >= 0)
					{
						laserProjection.Add(new Position(p.Position.X, y));
						y--;
					}
				}
				else if (p.ShootDirection == 1) // right
				{
					int x = p.Position.X + 1;
					while (x < gameState.GridSize.Cols)
					{
						laserProjection.Add(new Position(x, p.Position.Y));
						x++;
					}
				}
				else if (p.ShootDirection == 2) // down
				{
					int y = p.Position.Y + 1;
					while (y < gameState.GridSize.Rows)
					{
						laserProjection.Add(new Position(p.Position.X, y));
						y++;
					}
				}
				else if (p.ShootDirection == 3) // left
				{
					int x = p.Position.X - 1;
					while (x >= 0)
					{
						laserProjection.Add(new Position(x, p.Position.Y));
						x--;
					}
				}


				bool endCollisionCheck = false;
				foreach (var laserPosition in laserProjection)
				{
					if (endCollisionCheck) break;
					var otherPlayers = gameState.Players.Where(x => x.Id != p.Id);
					foreach (Player otherPlayer in otherPlayers)
					{
						if (otherPlayer.Destroyed && otherPlayer.RoundDestroyed != gameState.Round)
						{
							endCollisionCheck = true;
							break; // break to stop the laser from continuing
						}

						// true if we have a collision
						if (laserPosition.Equals(otherPlayer.Position))
						{
							// if the other player died in a prevous round, don't do anything
							// except stop the laser from continuing
							if (otherPlayer.RoundDestroyed != gameState.Round)
							{
								otherPlayer.Destroyed = true;
								otherPlayer.Deaths++;
								otherPlayer.RoundDestroyed = gameState.Round;
							}
							p.Kills++;
							endCollisionCheck = true;
							break; // break to stop the laser from continuing
						}
					}
				}
			}
		}
	}
}
