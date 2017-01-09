using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaiceGameTerminal.Config
{
	public interface IConfigProvider
	{
		void ReadAllSettings();
		string ReadSetting(string key);
		int ReadNumberSetting(string key);
		void AddUpdateAppSettings(string key, string value);
	}
}
