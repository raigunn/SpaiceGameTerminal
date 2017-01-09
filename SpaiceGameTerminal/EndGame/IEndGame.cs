using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaiceGameTerminal.Models.Requests;

namespace SpaiceGameTerminal.EndGame
{
	public interface IEndGame
	{
		void CheckForEndGame(GameState gameState);
	}
}
