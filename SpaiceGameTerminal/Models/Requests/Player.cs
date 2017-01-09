using System;
using System.Runtime.Serialization;
using SpaiceGameTerminal.Enums;

namespace SpaiceGameTerminal.Models.Requests
{
	[DataContract]
	public class Player
	{
		[DataMember]
		public string Id { get; set; }

		[DataMember]
		public string Name { get; set; }

		[DataMember]
		public string Url { get; set; }

		[DataMember]
		public Position Position { get; set; }

		[DataMember]
		public int ShootDirection { get; set; }

		[DataMember]
		public string Me { get; set; }

		public bool Destroyed { get; set; }
		public int Kills { get; set; }
		public int Deaths { get; set; }
		public int Draws { get; set; }
		public int Wins { get; set; }
		public int RoundDestroyed { get; set; }

		public Player(string id, string name, string url, Position position, int shootDirection, string me)
		{
			Id = id;
			Name = name;
			Url = url;
			Position = position;
			ShootDirection = shootDirection;
			Me = me;
			Destroyed = false;
			Wins = 0;
			Deaths = 0;
			Draws = 0;
			Kills = 0;
			RoundDestroyed = 0;
			ConsoleColor = ConsoleColors.GetRandomConsoleColor();
		}

		public static Player CreateDefault(string playerId, string playerName, string url)
		{
			Player player = new Player(playerId, playerName, url, new Position(0, 0), 0, "false");
			return player;
		}

		public void WriteStats()
		{
			Console.Write($"Wins [{Wins}] Kills [{Kills}] Draws [{Draws}] Deaths [{Deaths}]");
    }

		public ConsoleColor ConsoleColor { get; set; }
	}
}
