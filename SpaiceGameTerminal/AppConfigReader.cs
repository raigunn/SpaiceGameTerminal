using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using SpaiceGameTerminal.Config;

namespace SpaiceGameTerminal
{
	/// <summary>
	/// https://msdn.microsoft.com/en-us/library/system.configuration.configurationmanager.appsettings(v=vs.110).aspx
	/// todo: make a config provider so we could potentially swap out the config source
	/// </summary>
	public class AppConfigReader : IConfigProvider
	{

		public void ReadAllSettings()
		{
			try
			{
				var appSettings = ConfigurationManager.AppSettings;

				if (appSettings.Count == 0)
				{
					Console.WriteLine("AppSettings is empty.");
				}
				else
				{
					foreach (var key in appSettings.AllKeys)
					{
						Console.WriteLine("Key: {0} Value: {1}", key, appSettings[key]);
					}
				}
			}
			catch (ConfigurationErrorsException)
			{
				Console.WriteLine("Error reading app settings");
			}
		}


		public int ReadNumberSetting(string key)
		{
			string originalValue = ReadSetting(key);
			int value;
			if (!int.TryParse(originalValue, out value))
			{
				throw new ArgumentException($"Could not convert key: {key} to integer value");
			}
			return value;
		}

		public string ReadSetting(string key)
		{
			string result = string.Empty;
			try
			{
				var appSettings = ConfigurationManager.AppSettings;
				result = appSettings[key] ?? "Not Found";
				//Console.WriteLine(result);
			}
			catch (ConfigurationErrorsException)
			{
				Console.WriteLine("Error reading app settings");
			}
			return result;
		}

		public void AddUpdateAppSettings(string key, string value)
		{
			try
			{
				var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
				var settings = configFile.AppSettings.Settings;
				if (settings[key] == null)
				{
					settings.Add(key, value);
				}
				else
				{
					settings[key].Value = value;
				}
				configFile.Save(ConfigurationSaveMode.Modified);
				ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
			}
			catch (ConfigurationErrorsException)
			{
				Console.WriteLine("Error writing app settings");
			}
		}
	}
}
