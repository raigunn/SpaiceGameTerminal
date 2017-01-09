using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using SpaiceGameTerminal.Config;

namespace SpaiceGameTerminal.Domain
{

	[DataContract]
	public class TurnCount
	{
		private int _turnCountMin;
		private int _turnCountMax;
		private int _turnCountDefault;
		private readonly IConfigProvider _iConfigProvider;

		public TurnCount(IConfigProvider iConfigProvider)
		{
			_iConfigProvider = iConfigProvider;
			LoadConfigs();
			Value = _turnCountDefault;
		}

		public TurnCount(int value, IConfigProvider iConfigProvider)
		{
			_iConfigProvider = iConfigProvider;
			LoadConfigs();
			Value = value;
		}
		private void LoadConfigs()
		{
			_turnCountMin = _iConfigProvider.ReadNumberSetting("TurnCount.Min");
			_turnCountMax = _iConfigProvider.ReadNumberSetting("TurnCount.Max");
			_turnCountDefault = _iConfigProvider.ReadNumberSetting("TurnCount.Default");
		}

		private int _value;
		[DataMember]
		public int Value
		{
			get { return _value; }
			set
			{
				if (value < _turnCountMin || value > _turnCountMax)
				{
					throw new ArgumentException($"TurnCount: count must be >= {_turnCountMin} and <= {_turnCountMax}.  Value entered is {value}");
				}
				_value = value;
			}
		}


		public void SetValue(string rows)
		{
			int v;
			if (!int.TryParse(rows, out v))
			{
				throw new ArgumentException($"TurnCount: count must be >= {_turnCountMin} and <= {_turnCountMax}.  Value entered is {rows}");
			}
			Value = v;
		}
	}


}
