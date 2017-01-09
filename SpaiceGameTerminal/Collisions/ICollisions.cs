using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaiceGameTerminal.Models.Requests;

namespace SpaiceGameTerminal.Collisions
{
	public interface ICollisions
	{
		void HandleShipToShip(GameState gameState);

		void HandleLaserToShip(GameState gameState);
	}
}
