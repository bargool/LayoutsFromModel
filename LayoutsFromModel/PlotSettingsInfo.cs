/*
 * User: aleksey.nakoryakov
 * Date: 27.02.13
 * Time: 17:10
 */
using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace LayoutsFromModel
{
	/// <summary>
	/// Класс, содержащий информацию о настройках печати
	/// </summary>
	public class PlotSettingsInfo : IEqualityComparer<PlotSettingsInfo>
	{
		/// <summary>
		/// Имя формата бумаги
		/// </summary>
		public string Name {
			get { return PSettings.PlotSettingsName; }
		}
		/// <summary>
		/// Ширина листа
		/// </summary>
		public double Width {
			get { return PSettings.PlotRotation == PlotRotation.Degrees000 ? PSettings.PlotPaperSize.X : PSettings.PlotPaperSize.Y; }
		}
		/// <summary>
		/// Высота листа
		/// </summary>
		public double Height {
			get { return PSettings.PlotRotation == PlotRotation.Degrees000 ? PSettings.PlotPaperSize.Y : PSettings.PlotPaperSize.X; }
		}
		/// <summary>
		/// PlotSettings
		/// </summary>
		public PlotSettings PSettings { get; private set; }
		
		public PlotSettingsInfo(PlotSettings pSettings)
		{
			if (pSettings == null)
				throw new ArgumentNullException("PlotSettings");
			this.PSettings = pSettings;
		}
		
		public bool Equals(PlotSettingsInfo x, PlotSettingsInfo y)
		{
			return Math.Abs(x.Height - y.Height) < 1E-9 && Math.Abs(x.Width - y.Width) < 1E-9;
		}
		
		public int GetHashCode(PlotSettingsInfo obj)
		{
			return (obj.Height*obj.Width).GetHashCode();
		}
		
		public override string ToString()
		{
			return Name;
		}

	}
}
