using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SpaiceGameTerminal;
using SpaiceGameTerminal.Collisions;
using SpaiceGameTerminal.Config;
using SpaiceGameTerminal.Domain;
using SpaiceGameTerminal.EndGame;
using SpaiceGameTerminal.Models.Requests;

namespace SpaceGameTerminalTests
{
	[TestFixture]
	public class EndGameTests
	{
		private Player _p1;
		private Player _p2;
		private Player _p3;
		private Player _p4;
		private List<Player> _players;
		private GameState _gameState;
		//configs
		private static int _gridRowsDefault;
		private static int _gridRowsMin;
		private static int _gridRowsMax;
		private static int _gridColumnsDefault;
		private static int _gridColumnsMin;
		private static int _gridColumnsMax;
		private static int _turnCountDefault;
		private static int _turnCountMin;
		private static int _turnCountMax;
		private static int _playerCountMin;
		private static int _playerCountMax;
		private IConfigProvider _iConfigProvider;

		[OneTimeSetUp]
		public void GetConfigs()
		{
			 _iConfigProvider = new AppConfigReader();
			_gridRowsDefault = _iConfigProvider.ReadNumberSetting("Grid.Rows.Default");
			_gridRowsMin = _iConfigProvider.ReadNumberSetting("Grid.Rows.Min");
			_gridRowsMax = _iConfigProvider.ReadNumberSetting("Grid.Rows.Max");
			_gridColumnsDefault = _iConfigProvider.ReadNumberSetting("Grid.Columns.Default");
			_gridColumnsMin = _iConfigProvider.ReadNumberSetting("Grid.Columns.Min");
			_gridColumnsMax = _iConfigProvider.ReadNumberSetting("Grid.Columns.Max");
			_turnCountDefault = _iConfigProvider.ReadNumberSetting("TurnCount.Default");
			_turnCountMin = _iConfigProvider.ReadNumberSetting("TurnCount.Min");
			_turnCountMax = _iConfigProvider.ReadNumberSetting("TurnCount.Max");
			_playerCountMin = _iConfigProvider.ReadNumberSetting("Players.Min");
			_playerCountMax = _iConfigProvider.ReadNumberSetting("Players.Max");
		}

		[SetUp]
		public void Setup()
		{
			ConsoleColors.Reset();
			_p1 = new Player("1", "one", "http://test/one", new Position(1, 1), 0, "true");
			_p2 = new Player("2", "two", "http://test/two", new Position(1, 1), 0, "true");
			_p3 = new Player("3", "three", "http://test/three", new Position(1, 1), 0, "true"); // not explicitly added here
			_p4 = new Player("4", "four", "http://test/four", new Position(1, 1), 0, "true"); // not explicitly added here

			_players = new List<Player>();
			_players.Add(_p1);
			_players.Add(_p2);
			
			_gameState = GameState.MakeInitialGameState(
					_players,
					new TurnCount(_turnCountDefault, _iConfigProvider),
					new GridSize(_gridColumnsDefault, _gridRowsDefault, _iConfigProvider),
					new StandardCollisions(),
					new StandardEndGame());
		}

		[Test]
		public void EndGame_NoneDestroyed_NotEndOfGame()
		{
			// first Assert they are not yet destroyed
			Assert.IsFalse(_p1.Destroyed);
			Assert.IsFalse(_p2.Destroyed);

			int p1OriginalWinCount = _p1.Wins;
			int p1OriginalDrawCount = _p1.Draws;
			int p2OriginalWinCount = _p2.Wins;
			int p2OriginalDrawCount = _p2.Draws;

			_gameState.EndGame.CheckForEndGame(_gameState);

			// win counts should be unchnaged
			Assert.AreEqual(_p1.Wins, p1OriginalWinCount);
			Assert.AreEqual(_p2.Wins, p2OriginalWinCount);

			// draw counts should be unchanged
			Assert.AreEqual(_p1.Draws, p1OriginalDrawCount);
			Assert.AreEqual(_p2.Draws, p2OriginalDrawCount);

			// should not be any winners and game isn't over
			Assert.IsNull(_gameState.Winners);
			Assert.IsFalse(_gameState.GameOver);
		}


		[Test]
		public void EndGame_NoneDestroyedOnLastRound_Draws()
		{
			// make sure we are on last round
			_gameState.Round = _gameState.TotalRounds.Value;

			int p1OriginalWinCount = _p1.Wins;
			int p1OriginalDrawCount = _p1.Draws;
			int p2OriginalWinCount = _p2.Wins;
			int p2OriginalDrawCount = _p2.Draws;

			_gameState.EndGame.CheckForEndGame(_gameState);

			// win counts should be unchnaged
			Assert.AreEqual(_p1.Wins, p1OriginalWinCount);
			Assert.AreEqual(_p2.Wins, p2OriginalWinCount);

			// draw counts should increment
			Assert.AreEqual(_p1.Draws, p1OriginalDrawCount + 1);
			Assert.AreEqual(_p2.Draws, p2OriginalDrawCount + 1);

			// both draws are considered winners
			Assert.AreEqual(_gameState.Winners.Count, 2);
			Assert.IsTrue(_gameState.GameOver);
		}


		[Test]
		public void EndGame_OneOfTwoRemaining_EndOfGame()
		{
			_p2.Destroyed = true;

			int p1OriginalWinCount = _p1.Wins;
			int p1OriginalDrawCount = _p1.Draws;
			int p2OriginalWinCount = _p2.Wins;
			int p2OriginalDrawCount = _p2.Draws;

			_gameState.EndGame.CheckForEndGame(_gameState);

			// _p1 should get a win
			Assert.AreEqual(_p1.Wins, p1OriginalWinCount + 1);
			Assert.AreEqual(_p2.Wins, p2OriginalWinCount);

			// draw counts should be unchanged
			Assert.AreEqual(_p1.Draws, p1OriginalDrawCount);
			Assert.AreEqual(_p2.Draws, p2OriginalDrawCount);

			Assert.AreEqual(_gameState.Winners.Count, 1);
			Assert.IsTrue(_gameState.GameOver);
		}

		[Test]
		public void EndGame_NoneOfTwoRemaining_EndOfGame()
		{
			_p1.Destroyed = true;
			_p2.Destroyed = true;

			int p1OriginalWinCount = _p1.Wins;
			int p1OriginalDrawCount = _p1.Draws;
			int p2OriginalWinCount = _p2.Wins;
			int p2OriginalDrawCount = _p2.Draws;

			_gameState.EndGame.CheckForEndGame(_gameState);

			// wins should remain unchanged
			Assert.AreEqual(_p1.Wins, p1OriginalWinCount);
			Assert.AreEqual(_p2.Wins, p2OriginalWinCount);

			// no players should get a draw
			Assert.AreEqual(_p1.Draws, p1OriginalDrawCount);
			Assert.AreEqual(_p2.Draws, p2OriginalDrawCount);

			Assert.IsNull(_gameState.Winners);
			Assert.IsTrue(_gameState.GameOver);
		}

		[Test]
		public void EndGame_OneOfThreeRemaining_EndOfGame()
		{
			_gameState.Players.Add(_p3);
			
			_p2.Destroyed = true;
			_p3.Destroyed = true;

			int p1OriginalWinCount = _p1.Wins;
			int p1OriginalDrawCount = _p1.Draws;
			int p2OriginalWinCount = _p2.Wins;
			int p2OriginalDrawCount = _p2.Draws;
			int p3OriginalWinCount = _p3.Wins;
			int p3OriginalDrawCount = _p3.Draws;

			_gameState.EndGame.CheckForEndGame(_gameState);

			// wins should remain unchanged
			Assert.AreEqual(_p1.Wins, p1OriginalWinCount + 1);
			Assert.AreEqual(_p2.Wins, p2OriginalWinCount);
			Assert.AreEqual(_p3.Wins, p3OriginalWinCount);

			// both players should get a draw
			Assert.AreEqual(_p1.Draws, p1OriginalDrawCount);
			Assert.AreEqual(_p2.Draws, p2OriginalDrawCount);
			Assert.AreEqual(_p3.Draws, p3OriginalDrawCount);
			
			Assert.AreEqual(_gameState.Winners[0].Id, _p1.Id);
			Assert.AreEqual(_gameState.Winners.Count, 1);
			Assert.IsTrue(_gameState.GameOver);
		}

		[Test]
		public void EndGame_TwoOfThreeRemaining_GameNotOver()
		{
			_gameState.Players.Add(_p3);
			_p3.Destroyed = true;

			int p1OriginalWinCount = _p1.Wins;
			int p1OriginalDrawCount = _p1.Draws;
			int p2OriginalWinCount = _p2.Wins;
			int p2OriginalDrawCount = _p2.Draws;
			int p3OriginalWinCount = _p3.Wins;
			int p3OriginalDrawCount = _p3.Draws;

			_gameState.EndGame.CheckForEndGame(_gameState);

			// wins should remain unchanged
			Assert.AreEqual(_p1.Wins, p1OriginalWinCount);
			Assert.AreEqual(_p2.Wins, p2OriginalWinCount);
			Assert.AreEqual(_p3.Wins, p3OriginalWinCount);

			// both players should get a draw
			Assert.AreEqual(_p1.Draws, p1OriginalDrawCount);
			Assert.AreEqual(_p2.Draws, p2OriginalDrawCount);
			Assert.AreEqual(_p3.Draws, p3OriginalDrawCount);

			Assert.IsNull(_gameState.Winners);
			Assert.IsFalse(_gameState.GameOver);
		}


		[Test]
		public void EndGame_NoneOfThreeRemaining_GameOver()
		{
			_gameState.Players.Add(_p3);
			_p1.Destroyed = true;
			_p2.Destroyed = true;
			_p3.Destroyed = true;

			int p1OriginalWinCount = _p1.Wins;
			int p1OriginalDrawCount = _p1.Draws;
			int p2OriginalWinCount = _p2.Wins;
			int p2OriginalDrawCount = _p2.Draws;
			int p3OriginalWinCount = _p3.Wins;
			int p3OriginalDrawCount = _p3.Draws;

			_gameState.EndGame.CheckForEndGame(_gameState);

			// wins should remain unchanged
			Assert.AreEqual(_p1.Wins, p1OriginalWinCount);
			Assert.AreEqual(_p2.Wins, p2OriginalWinCount);
			Assert.AreEqual(_p3.Wins, p3OriginalWinCount);

			// both players should get a draw
			Assert.AreEqual(_p1.Draws, p1OriginalDrawCount);
			Assert.AreEqual(_p2.Draws, p2OriginalDrawCount);
			Assert.AreEqual(_p3.Draws, p3OriginalDrawCount);

			Assert.IsNull(_gameState.Winners);
			Assert.IsTrue(_gameState.GameOver);
		}

		[Test]
		public void EndGame_TwoOfThreeRemainingLastRound_TwoDraws()
		{
			// make sure we are on last round
			_gameState.Round = _gameState.TotalRounds.Value;

			_gameState.Players.Add(_p3);
			_p3.Destroyed = true;

			int p1OriginalWinCount = _p1.Wins;
			int p1OriginalDrawCount = _p1.Draws;
			int p2OriginalWinCount = _p2.Wins;
			int p2OriginalDrawCount = _p2.Draws;
			int p3OriginalWinCount = _p3.Wins;
			int p3OriginalDrawCount = _p3.Draws;

			_gameState.EndGame.CheckForEndGame(_gameState);

			// wins should remain unchanged
			Assert.AreEqual(_p1.Wins, p1OriginalWinCount);
			Assert.AreEqual(_p2.Wins, p2OriginalWinCount);
			Assert.AreEqual(_p3.Wins, p3OriginalWinCount);

			// two surviving players should get a draw
			Assert.AreEqual(_p1.Draws, p1OriginalDrawCount + 1);
			Assert.AreEqual(_p2.Draws, p2OriginalDrawCount + 1);
			Assert.AreEqual(_p3.Draws, p3OriginalDrawCount);

			Assert.AreEqual(_gameState.Winners.Count, 2);
			Assert.IsTrue(_gameState.GameOver);
		}
	}
}
