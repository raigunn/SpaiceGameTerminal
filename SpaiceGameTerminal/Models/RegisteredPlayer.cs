using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaiceGameTerminal.Models
{
	public class RegisteredPlayer
	{
		public string Name { get; }

		public string Id { get; }

		public string Url { get; }

		public RegisteredPlayer(string name, string url, string id)
		{
			Name = name;
			Id = id;
			Url = url;
		}
	}
}
