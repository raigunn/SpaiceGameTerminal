using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using SpaiceGameTerminal.Config;

namespace SpaiceGameTerminal.Domain
{
	[DataContract]
	public class GridSize
	{
		private int _gridRowsDefault;
		private int _gridColumnsDefault;
		private int _gridRowsMin;
		private int _gridRowsMax;
		private int _gridColumnsMin;
		private int _gridColumnsMax;
		private readonly IConfigProvider _iConfigProvider;

		private int _cols;
		[DataMember]
		public int Cols
		{
			get { return _cols; }
			set
			{
				if (value < _gridColumnsMin || value > _gridColumnsMax)
				{
					throw new ArgumentException($"GridSize: columns must be >= {_gridColumnsMin} and <= {_gridColumnsMax}.  Value entered is {value}");
				}
				_cols = value;
			}
		}

		private int _rows;
		[DataMember]
		public int Rows
		{
			get { return _rows; }
			set
			{
				if (value < _gridRowsMin || value > _gridRowsMax)
				{
					throw new ArgumentException($"GridSize: rows must be >= {_gridRowsMin} and <= {_gridRowsMax}.  Value entered is {value}");
				}
				_rows = value;
			}
		}

		public GridSize(IConfigProvider iConfigProvider)
		{
			_iConfigProvider = iConfigProvider;
			LoadConfigs();
			Cols = _gridColumnsDefault;
			Rows = _gridRowsDefault;
		}

		public GridSize(int cols, int rows, IConfigProvider iConfigProvider)
		{
			_iConfigProvider = iConfigProvider;
			LoadConfigs();
			Cols = cols;
			Rows = rows;
		}

		public GridSize(string cols, string rows, IConfigProvider iConfigProvider)
		{
			_iConfigProvider = iConfigProvider;
			LoadConfigs();
			SetCols(cols);
			SetRows(rows);
		}

		private void LoadConfigs()
		{
			_gridRowsDefault = _iConfigProvider.ReadNumberSetting("Grid.Rows.Default");
			_gridRowsMin = _iConfigProvider.ReadNumberSetting("Grid.Rows.Min");
			_gridRowsMax = _iConfigProvider.ReadNumberSetting("Grid.Rows.Max");
			_gridColumnsDefault = _iConfigProvider.ReadNumberSetting("Grid.Columns.Default");
			_gridColumnsMin = _iConfigProvider.ReadNumberSetting("Grid.Columns.Min");
			_gridColumnsMax = _iConfigProvider.ReadNumberSetting("Grid.Columns.Max");
		}

		public void SetCols(string cols)
		{
			int c;
			if (!int.TryParse(cols, out c))
			{
				throw new ArgumentException($"GridSize: columns must be >= {_gridColumnsMin} and <= {_gridColumnsMax}.  Value entered is {cols}");
			}
			Cols = c;
		}

		public void SetRows(string rows)
		{
			int r;
			if (!int.TryParse(rows, out r))
			{
				throw new ArgumentException($"GridSize: rows must be >= {_gridRowsMin} and <= {_gridRowsMax}.  Value entered is {rows}");
			}
			Rows = r;
		}
	}
}
