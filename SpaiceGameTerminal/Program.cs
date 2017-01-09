using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using SpaiceGameTerminal.Collisions;
using SpaiceGameTerminal.Config;
using SpaiceGameTerminal.Domain;
using SpaiceGameTerminal.EndGame;
using SpaiceGameTerminal.Extensions;
using SpaiceGameTerminal.Models;
using SpaiceGameTerminal.Enums;
using SpaiceGameTerminal.Models.Requests;
using SpaiceGameTerminal.Models.Responses;

namespace SpaiceGameTerminal
{
	class Program
	{
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


		private static void LoadConfigs(IConfigProvider iConfigProvider)
		{
			_gridRowsDefault = iConfigProvider.ReadNumberSetting("Grid.Rows.Default");
			_gridRowsMin = iConfigProvider.ReadNumberSetting("Grid.Rows.Min");
			_gridRowsMax = iConfigProvider.ReadNumberSetting("Grid.Rows.Max");
			_gridColumnsDefault = iConfigProvider.ReadNumberSetting("Grid.Columns.Default");
			_gridColumnsMin = iConfigProvider.ReadNumberSetting("Grid.Columns.Min");
			_gridColumnsMax = iConfigProvider.ReadNumberSetting("Grid.Columns.Max");
			_turnCountDefault = iConfigProvider.ReadNumberSetting("TurnCount.Default");
			_turnCountMin = iConfigProvider.ReadNumberSetting("TurnCount.Min");
			_turnCountMax = iConfigProvider.ReadNumberSetting("TurnCount.Max");
			_playerCountMin = iConfigProvider.ReadNumberSetting("Players.Min");
			_playerCountMax = iConfigProvider.ReadNumberSetting("Players.Max");
		}

		private static readonly List<Player> _players = new List<Player>();
		private static string _gameId = "";
		private static TurnCount _turnCount;
		private static GridSize _gridSize;
		static void Main(string[] args)
		{
			IConfigProvider iConfigProvider = new AppConfigReader();
			LoadConfigs(iConfigProvider);
			_gridSize = new GridSize(iConfigProvider);
			_turnCount = new TurnCount(iConfigProvider);
			Intro.Play();

			// generate game id
			_gameId = RandomWrapper.RandomNumber(1, 10000).ToString();  // I will need this, but don't need it yet
			
			Console.CursorVisible = true;

			string userInput;
			do
			{
				userInput = DisplayMenu();

				switch (userInput)
				{
					case "1":
						AddPlayer();
						break;
					case "2":
						DisplayShips();
						break;
					case "3":
						GridSize();
						break;
					case "4":
						TurnCount();
						break;
					case "5":
						//DeletePlayer();
						break;
					case "6":
						RunGame();
						break;
				}
			} while (userInput != "7");
		}

		private static void RunGame()
		{
			GameState gameState = GameState.MakeInitialGameState(_players, _turnCount, _gridSize, new StandardCollisions(), new StandardEndGame());
			ConsoleDraw.BuildInitialGrid(_players, _gridSize);

			for (int i = _turnCount.Value; i > 0; i--)
			{
				Requests bs = new Requests();
				Task<IList<TurnResponse>> turnResponses = bs.RunGameRequest(gameState);

				Task.WaitAll(turnResponses); // block while the task completes

				// create players
				List<Player> players = new List<Player>();
				foreach (var turnResponse in turnResponses.Result)
				{
					Player player = gameState.Players.SingleOrDefault(x => x.Id == turnResponse.PlayerId); // todo: error handling
					if (player == null) continue;

					if (!player.Destroyed)
					{
						// convert direction to new position
						Directions direction = (Directions) turnResponse.MoveDirection;
						Position initialPosition = player.Position;
						Position newPosition = direction.ConvertToNewPosition(initialPosition);
						player.Position = newPosition;

						// new shoot direction
						player.ShootDirection = turnResponse.ShootDirection;
					}
					// update the new list of players
					player.Me = "false";
					players.Add(player);
				}

				gameState = GameState.UpdateGameState(gameState);
				ConsoleDraw.RebuildGrid(gameState);

				if (gameState.GameOver)
				{
					break;
				}
			}

			if (gameState.Winners == null)
			{
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.SetCursorPosition(0, Console.WindowHeight - 2);
				Console.Write("The results are a draw.  The are 0 remaining ships. \n");
			}
			else if (gameState.Winners.Count == 1)
			{
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.SetCursorPosition(0, Console.WindowHeight - 3);
				Console.Write("The Winner is...\n");
				Console.Write(gameState.Winners[0].ToString() + "\n");
			}
			else
			{
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.SetCursorPosition(0, Console.WindowHeight - gameState.Winners.Count - 2);
				Console.Write("The results are a draw.  The remaining ships are: \n");
				foreach (var player in gameState.Winners)
				{
					Console.Write(player.Name + "\n");
				}
			}
			
			Console.SetCursorPosition(0, Console.WindowHeight - 1);
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.Write("press [any key] to continue...");
			Console.ReadKey();
		}


		private static string DisplayMenu()
		{
			Console.Clear();
			Console.SetCursorPosition(0, Console.WindowHeight - 9);
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine("spAIce game manager");
			Console.WriteLine();

			// Add Ship
			Console.ForegroundColor = _players.Count >= _playerCountMax ? ConsoleColor.Gray : ConsoleColor.Cyan;
			Console.WriteLine("1. add a space ship");

			// List Ships
			if (_players.Count == 0)
			{
				Console.ForegroundColor = ConsoleColor.Cyan;
				Console.Write("2. list space ships [0]");
			}
			else
			{
				Console.ForegroundColor = ConsoleColor.Cyan;
				Console.Write("2. list space ships");
				Console.ForegroundColor = _players.Count >= _playerCountMin ? ConsoleColor.Yellow : ConsoleColor.Gray;
				Console.WriteLine($" [{_players.Count}]");
			}

			// Arena Size
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.Write("3. set arena size");
			if (_gridSize.Cols == _gridColumnsDefault && _gridSize.Rows == _gridRowsDefault)
			{
				Console.ForegroundColor = ConsoleColor.Gray;
				Console.Write($" [{_gridColumnsDefault}x{_gridRowsDefault}]\n");
      }
			else
			{
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.Write($" [{_gridSize.Cols}x{_gridSize.Rows}]\n");
      }

			// Turn Count
			if (_turnCount.Value != _turnCountDefault)
			{
				Console.ForegroundColor = ConsoleColor.Cyan;
				Console.Write("4. set number of rounds");
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.Write($" [{_turnCount.Value}]\n");
      }
			else
			{
				Console.ForegroundColor = ConsoleColor.Cyan;
				Console.Write($"4. set number of rounds");
				Console.ForegroundColor = ConsoleColor.Gray;
				Console.Write($" [{_turnCountDefault}]\n");
      }

			// delete ship - disabled
			Console.ForegroundColor = ConsoleColor.Gray;
			Console.WriteLine("5. delete a space ship");

			// start game.  disabled if not minumum number of players
			Console.ForegroundColor = _players.Count >= _playerCountMin ? ConsoleColor.Cyan : ConsoleColor.Gray;
			Console.WriteLine("6. start game");

			// exit game
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine("7. exit");

			// user input
			Console.ForegroundColor = ConsoleColor.White;
			var key = Console.ReadKey().KeyChar.ToString();
			return key;
		}

		

		private static void AddPlayer()
		{
			if (_players.Count >= _playerCountMax)
			{
				return;
			}

			Console.Clear();
			Console.SetCursorPosition(0, Console.WindowHeight - 1); ;
			RegisterResponse registerResponse;
			string playerId;
			string inputUrl;
			
			// request input
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine("enter in server info or [enter] to return to menu");
			Console.WriteLine("example: (http://localhost:8000/RandomSpaiceShip/");
			
			do
			{
				// collect user input
				Console.ForegroundColor = ConsoleColor.White;
				var input = Console.ReadLine();

				try
				{
					if (String.IsNullOrEmpty(input)) return;

					inputUrl = (input == "default1") ? "http://localhost:8000/RandomSpaiceShip/" : input;
					playerId = Guid.NewGuid().ToString().Substring(24);
					registerResponse = Requests.MakeGetRequest<RegisterResponse>(inputUrl + "Register/" + playerId);
					break;
				}
				catch (Exception ex)
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine(ex.Message);
				}
			} while (true);
			
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine("player successfully added");

			Console.ForegroundColor = ConsoleColor.Yellow;
			registerResponse.Log();
			_players.Add(Player.CreateDefault(playerId, registerResponse.ShipName, inputUrl));

			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine("press [any key] to continue...");
			Console.ReadKey();
		}




		private static void DisplayShips()
		{
			if (_players.Count == 0)
			{
				return;
			}

			Console.Clear();
			Console.SetCursorPosition(0, Console.WindowHeight - 1);
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine("Listing ships");

			// output all ship info
			Console.ForegroundColor = ConsoleColor.Yellow;
			foreach (var player in _players)
			{
				player.Log();
			}

			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine("press [any key] to continue...");
			Console.ReadKey();
		}


		private static void GridSize()
		{
			Console.Clear();
			Console.SetCursorPosition(0, Console.WindowHeight - 1);
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine($"Enter number of columns [min: {_gridColumnsMin}, max: {_gridColumnsMax}]");
			
			do
			{
				// collect user input
				Console.ForegroundColor = ConsoleColor.White;
				var input = Console.ReadLine();

				try
				{
					if (String.IsNullOrEmpty(input)) return;
					_gridSize.SetCols(input);
					break;
				}
				catch (ArgumentException)
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine($"Please enter a valid number [min: {_gridColumnsMin}, max: {_gridColumnsMax}]");
        }
			} while (true);

			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine($"Enter number of rows [min: {_gridRowsMin}, max: {_gridRowsMax}]");
			do
			{
				// collect user input
				Console.ForegroundColor = ConsoleColor.White;
				var input = Console.ReadLine();
				
				try
				{
					_gridSize.SetRows(input);
					break;
				}
				catch (ArgumentException)
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine($"Please enter a valid number [min: {_gridRowsMin}, max: {_gridRowsMax}]");
        }
				
			} while (true);
		}



		private static void TurnCount()
		{
			Console.Clear();
			Console.SetCursorPosition(0, Console.WindowHeight - 1);
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine("Enter number of rounds");
			
			do
			{
				// collect user input
				Console.ForegroundColor = ConsoleColor.White;
				var input = Console.ReadLine();
				
				try
				{
					if (String.IsNullOrEmpty(input)) return;
					_turnCount.SetValue(input);
					break;
				}
				catch (ArgumentException)
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine($"Please enter a valid number [min: {_turnCountMin}, max: {_turnCountMax}]");
        }
			} while (true);
		}
	}
}
