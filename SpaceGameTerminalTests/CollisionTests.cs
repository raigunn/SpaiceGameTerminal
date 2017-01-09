using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
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
  public class CollisionTests
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
		public void Collision_TwoPlayersInSameSpace_BothDestroyed()
		{
			// first Assert they are not yet destroyed
			Assert.IsFalse(_p1.Destroyed);
			Assert.IsFalse(_p2.Destroyed);
			int p1OriginalKillCount = _p1.Kills;
			int p1OriginalDeathCount = _p1.Deaths;
			int p2OriginalKillCount = _p2.Kills;
			int p2OriginalDeathCount = _p2.Deaths;
			_p1.Position = new Position(1, 1);
			_p2.Position = new Position(1, 1);

			_gameState.Collisions.HandleShipToShip(_gameState);

			// after running the method, they should now be destroyed.
			Assert.IsTrue(_p1.Destroyed);
			Assert.IsTrue(_p2.Destroyed);

			// both players death count incremented
			Assert.AreEqual(_p1.Deaths, p1OriginalDeathCount + 1);
			Assert.AreEqual(_p2.Deaths, p2OriginalDeathCount + 1);

			// both players kill count incremented
			Assert.AreEqual(_p1.Kills, p1OriginalKillCount + 1);
			Assert.AreEqual(_p2.Kills, p2OriginalKillCount + 1);
		}

		[Test]
		public void Collision_ThreePlayersInSameSpace_AllDestroyed()
		{
			_gameState.Players.Add(_p3);

			int p1OriginalKillCount = _p1.Kills;
			int p1OriginalDeathCount = _p1.Deaths;
			int p2OriginalKillCount = _p2.Kills;
			int p2OriginalDeathCount = _p2.Deaths;
			int p3OriginalKillCount = _p3.Kills;
			int p3OriginalDeathCount = _p3.Deaths;
			_p1.Position = new Position(1, 1);
			_p2.Position = new Position(1, 1);
			_p3.Position = new Position(1, 1);

			_gameState.Collisions.HandleShipToShip(_gameState);

			// after running the method, they should now be destroyed.
			Assert.IsTrue(_p1.Destroyed);
			Assert.IsTrue(_p2.Destroyed);
			Assert.IsTrue(_p3.Destroyed);

			// all players death count incremented
			Assert.AreEqual(_p1.Deaths, p1OriginalDeathCount + 1);
			Assert.AreEqual(_p2.Deaths, p2OriginalDeathCount + 1);
			Assert.AreEqual(_p3.Deaths, p3OriginalDeathCount + 1);

			// they should each get credit for killing both other players
			Assert.AreEqual(_p1.Kills, p1OriginalKillCount + 2);
			Assert.AreEqual(_p2.Kills, p2OriginalKillCount + 2);
			Assert.AreEqual(_p3.Kills, p3OriginalKillCount + 2);
		}

		[Test]
		public void Collision_PlayerInSameSpaceAsDebris_PlayerDestroyed()
		{
			// assemble
			_p1.Position = new Position(1, 1);
			_p2.Position = new Position(1, 1);
			_p2.Destroyed = true;
			_p2.Deaths = 1;

			int p1OriginalKillCount = _p1.Kills;
			int p1OriginalDeathCount = _p1.Deaths;
			int p2OriginalKillCount = _p2.Kills;
			int p2OriginalDeathCount = _p2.Deaths;

			_gameState.Collisions.HandleShipToShip(_gameState);

			// player destroyed
			Assert.IsTrue(_p1.Destroyed);
			Assert.IsTrue(_p2.Destroyed); // remains destroyed

			// player kill count stays the same
			// player death count incremented
			Assert.AreEqual(_p1.Deaths, p1OriginalDeathCount + 1);
			Assert.AreEqual(_p1.Kills, p1OriginalKillCount);

			// debris-player all stats stay the same
			Assert.AreEqual(_p2.Deaths, p2OriginalDeathCount);
			Assert.AreEqual(_p2.Kills, p2OriginalKillCount);
		}

		[Test]
		public void Collision_PlayerShootsDebris_KillsAndDeathsDoNotAdvance()
		{
			// assemble
			_p1.Position = new Position(1, 1);
			_p2.Position = new Position(1, 1);
			_p2.Destroyed = true;
			_p2.Deaths = 1;

			int p1OriginalKillCount = _p1.Kills;
			int p2OriginalDeathCount = _p2.Deaths;

			_gameState.Collisions.HandleLaserToShip(_gameState);

			// p1 kill count should stay the same
			Assert.AreEqual(_p1.Kills, p1OriginalKillCount);

			// p2 death count should stay the same
			Assert.AreEqual(_p2.Deaths, p2OriginalDeathCount);
		}

		[Test]
		public void Collision_PlayersShootEachOtherHorizontally_BothKillsAndDeathsAdvance()
		{
			// assemble
			_p1.Position = new Position(1, 1);
			_p2.Position = new Position(6, 1);
			_p1.ShootDirection = 1; // shoot right
			_p2.ShootDirection = 3; // shoot left

			int p1OriginalKillCount = _p1.Kills;
			int p1OriginalDeathCount = _p1.Deaths;
			int p2OriginalKillCount = _p2.Kills;
			int p2OriginalDeathCount = _p2.Deaths;

			_gameState.Collisions.HandleLaserToShip(_gameState);

			// both kill counts should increment
			Assert.AreEqual(_p1.Kills, p1OriginalKillCount + 1);
			Assert.AreEqual(_p2.Kills, p2OriginalKillCount + 1);

			// both death counts should increment
			Assert.AreEqual(_p1.Deaths, p1OriginalDeathCount + 1);
			Assert.AreEqual(_p2.Deaths, p2OriginalDeathCount + 1);
		}

		[Test]
		public void Collision_PlayersShootEachOtherVertically_BothKillsAndDeathsAdvance()
		{
			// assemble
			_p1.Position = new Position(6, 1);
			_p2.Position = new Position(6, 6);
			_p1.ShootDirection = 2; // shoot down
			_p2.ShootDirection = 0; // shoot up

			int p1OriginalKillCount = _p1.Kills;
			int p1OriginalDeathCount = _p1.Deaths;
			int p2OriginalKillCount = _p2.Kills;
			int p2OriginalDeathCount = _p2.Deaths;

			_gameState.Collisions.HandleLaserToShip(_gameState);

			// both kill counts should increment
			Assert.AreEqual(_p1.Kills, p1OriginalKillCount + 1);
			Assert.AreEqual(_p2.Kills, p2OriginalKillCount + 1);

			// both death counts should increment
			Assert.AreEqual(_p1.Deaths, p1OriginalDeathCount + 1);
			Assert.AreEqual(_p2.Deaths, p2OriginalDeathCount + 1);
		}

		[Test]
		public void Collision_ThreeWayShooting_TwoKillsAndDeathsAdvance()
		{
			_gameState.Players.Add(_p3);

			// assemble
			_p1.Position = new Position(1, 1);
			_p1.ShootDirection = 1; // shoot right
			_p2.Position = new Position(2, 1);
			_p2.ShootDirection = 2; // shoot down
			_p3.Position = new Position(2, 2);
			_p3.ShootDirection = 0; // shoot Up

			int p1OriginalKillCount = _p1.Kills;
			int p1OriginalDeathCount = _p1.Deaths;
			int p2OriginalKillCount = _p2.Kills;
			int p2OriginalDeathCount = _p2.Deaths;
			int p3OriginalKillCount = _p3.Kills;
			int p3OriginalDeathCount = _p3.Deaths;

			_gameState.Collisions.HandleLaserToShip(_gameState);

			// after running the method, two should now be destroyed.
			Assert.IsFalse(_p1.Destroyed);
			Assert.IsTrue(_p2.Destroyed);
			Assert.IsTrue(_p3.Destroyed);

			// all kill counts should increment
			Assert.AreEqual(_p1.Kills, p1OriginalKillCount + 1);
			Assert.AreEqual(_p2.Kills, p2OriginalKillCount + 1);
			Assert.AreEqual(_p3.Kills, p3OriginalKillCount + 1);

			// two death counts should increment
			Assert.AreEqual(_p1.Deaths, p1OriginalDeathCount);
			Assert.AreEqual(_p2.Deaths, p2OriginalDeathCount + 1);
			Assert.AreEqual(_p3.Deaths, p3OriginalDeathCount + 1);
		}

		[Test]
		public void Collision_FourWayShooting_AllKillsAndDeathsAdvance()
		{
			_gameState.Players.Add(_p3);
			_gameState.Players.Add(_p4);

			// assemble
			_p1.Position = new Position(1, 1);
			_p1.ShootDirection = 1; // shoot right
			_p2.Position = new Position(6, 1);
			_p2.ShootDirection = 2; // shoot down
			_p3.Position = new Position(6, 6);
			_p3.ShootDirection = 3; // shoot left
			_p4.Position = new Position(1, 6);
			_p4.ShootDirection = 0; // shoot Up

			int p1OriginalKillCount = _p1.Kills;
			int p1OriginalDeathCount = _p1.Deaths;
			int p2OriginalKillCount = _p2.Kills;
			int p2OriginalDeathCount = _p2.Deaths;
			int p3OriginalKillCount = _p3.Kills;
			int p3OriginalDeathCount = _p3.Deaths;
			int p4OriginalKillCount = _p4.Deaths;
			int p4OriginalDeathCount = _p4.Deaths;

			_gameState.Collisions.HandleLaserToShip(_gameState);

			// after running the method, they should now be destroyed.
			Assert.IsTrue(_p1.Destroyed);
			Assert.IsTrue(_p2.Destroyed);
			Assert.IsTrue(_p3.Destroyed);
			Assert.IsTrue(_p4.Destroyed);

			// all kill counts should increment
			Assert.AreEqual(_p1.Kills, p1OriginalKillCount + 1);
			Assert.AreEqual(_p2.Kills, p2OriginalKillCount + 1);
			Assert.AreEqual(_p3.Kills, p3OriginalKillCount + 1);
			Assert.AreEqual(_p4.Kills, p4OriginalKillCount + 1);

			// all death counts should increment
			Assert.AreEqual(_p1.Deaths, p1OriginalDeathCount + 1);
			Assert.AreEqual(_p2.Deaths, p2OriginalDeathCount + 1);
			Assert.AreEqual(_p3.Deaths, p3OriginalDeathCount + 1);
			Assert.AreEqual(_p4.Deaths, p4OriginalDeathCount + 1);
		}


		// ship a runs into debris of ship b
		// while ship c shoots into same spot.
		// expected outcome:
		// ship a should be destroyed from the colision with the debris
		// and still gets to shoot before it dies
		// and the shooting from ship c counts too
		[Test]
		public void Collision_ThreeWayCombo_DebrisCausesDeath()
		{
			_gameState.Players.Add(_p3);

			// assemble
			_p1.Position = new Position(6, 1);
			_p1.ShootDirection = 2; // down
			_p2.Position = new Position(6, 1);
			_p2.ShootDirection = 2; // down
			_p2.Destroyed = true;
			_p3.Position = new Position(6, 6);
			_p3.ShootDirection = 3; // shoot up

			int p1OriginalKillCount = _p1.Kills;
			int p1OriginalDeathCount = _p1.Deaths;
			int p2OriginalKillCount = _p2.Kills;
			int p2OriginalDeathCount = _p2.Deaths;
			int p3OriginalKillCount = _p3.Kills;
			int p3OriginalDeathCount = _p3.Deaths;

			_gameState.Collisions.HandleShipToShip(_gameState);
			_gameState.Collisions.HandleLaserToShip(_gameState);

			// p1 should now be destroyed, p2 was already destroyed
			Assert.IsTrue(_p1.Destroyed);
			Assert.IsTrue(_p2.Destroyed);
			Assert.IsFalse(_p3.Destroyed);

			// no one gets any kills
			Assert.AreEqual(_p1.Kills, p1OriginalKillCount);
			Assert.AreEqual(_p2.Kills, p2OriginalKillCount);
			Assert.AreEqual(_p3.Kills, p3OriginalKillCount);

			// only p1 died
			Assert.AreEqual(_p1.Deaths, p1OriginalDeathCount + 1);
			Assert.AreEqual(_p2.Deaths, p2OriginalDeathCount);
			Assert.AreEqual(_p3.Deaths, p3OriginalDeathCount);
		}




		[Test]
		public void Collision_BehindAnotherShip_NotDestroyed()
		{
			_gameState.Players.Add(_p3);
			Assert.IsFalse(_p1.Destroyed);
			Assert.IsFalse(_p2.Destroyed);
			Assert.IsFalse(_p3.Destroyed);

			// assemble
			_p1.Position = new Position(1, 1);
			_p1.ShootDirection = 1; // right
			_p2.Position = new Position(3, 1);
			_p2.ShootDirection = 2; // down
			_p3.Position = new Position(6, 1);
			_p3.ShootDirection = 2; // down

			int p1OriginalKillCount = _p1.Kills;
			int p1OriginalDeathCount = _p1.Deaths;
			int p2OriginalKillCount = _p2.Kills;
			int p2OriginalDeathCount = _p2.Deaths;
			int p3OriginalKillCount = _p3.Kills;
			int p3OriginalDeathCount = _p3.Deaths;

			_gameState.Collisions.HandleShipToShip(_gameState);
			_gameState.Collisions.HandleLaserToShip(_gameState);

			// p1 should now be destroyed, p2 was already destroyed
			Assert.IsFalse(_p1.Destroyed);
			Assert.IsTrue(_p2.Destroyed);
			Assert.IsFalse(_p3.Destroyed);

			// no one gets any kills
			Assert.AreEqual(_p1.Kills, p1OriginalKillCount + 1);
			Assert.AreEqual(_p2.Kills, p2OriginalKillCount);
			Assert.AreEqual(_p3.Kills, p3OriginalKillCount);

			// only p1 died
			Assert.AreEqual(_p1.Deaths, p1OriginalDeathCount);
			Assert.AreEqual(_p2.Deaths, p2OriginalDeathCount + 1);
			Assert.AreEqual(_p3.Deaths, p3OriginalDeathCount);
		}


		[Test]
		public void Collision_BehindDebris_NotDestroyed()
		{
			_gameState.Players.Add(_p3);
			_p2.Destroyed = true;
			Assert.IsFalse(_p1.Destroyed);
			Assert.IsTrue(_p2.Destroyed);
			Assert.IsFalse(_p3.Destroyed);

			// assemble
			_p1.Position = new Position(1, 1);
			_p1.ShootDirection = 1; // right
			_p2.Position = new Position(2, 1);
			_p2.ShootDirection = 2; // down
			_p3.Position = new Position(3, 1);
			_p3.ShootDirection = 3; // left

			int p1OriginalKillCount = _p1.Kills;
			int p1OriginalDeathCount = _p1.Deaths;
			int p2OriginalKillCount = _p2.Kills;
			int p2OriginalDeathCount = _p2.Deaths;
			int p3OriginalKillCount = _p3.Kills;
			int p3OriginalDeathCount = _p3.Deaths;

			_gameState.Collisions.HandleShipToShip(_gameState);
			_gameState.Collisions.HandleLaserToShip(_gameState);

			// p2 is already destroyed, and blocks the others from being destroyed
			Assert.IsFalse(_p1.Destroyed);
			Assert.IsTrue(_p2.Destroyed);
			Assert.IsFalse(_p3.Destroyed);

			// no one gets any kills
			Assert.AreEqual(_p1.Kills, p1OriginalKillCount);
			Assert.AreEqual(_p2.Kills, p2OriginalKillCount);
			Assert.AreEqual(_p3.Kills, p3OriginalKillCount);

			// no one died this round
			Assert.AreEqual(_p1.Deaths, p1OriginalDeathCount);
			Assert.AreEqual(_p2.Deaths, p2OriginalDeathCount);
			Assert.AreEqual(_p3.Deaths, p3OriginalDeathCount);
		}
	}
}
