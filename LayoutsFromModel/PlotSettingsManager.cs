/*
 * User: aleksey.nakoryakov
 * Date: 27.02.13
 * Time: 17:47
 */
using System;
using System.Collections.Generic;
using System.Linq;

namespace LayoutsFromModel
{
	/// <summary>
	/// Класс, инкапсулирующий логику работы с коллекцией PlotSettings.
	/// В коллекции не допускаются PlotSettings с одинаковым размером листа
	/// </summary>
	public class PlotSettingsManager : ICollection<PlotSettingsInfo>
	{
		private List<PlotSettingsInfo> plotSettingsInfos = new List<PlotSettingsInfo>();
		
		private static PlotSettingsManager current = new PlotSettingsManager();
		
		public static PlotSettingsManager Current {
			get { return current; }
		}
		
		private PlotSettingsManager()
		{}
		
		
//		public PlotSettingsManager(IEnumerable<PlotSettingsInfo> plotSettingsInfos)
//		{
//			if (plotSettingsInfos == null || plotSettingsInfos.Count() == 0)
//				throw new ArgumentNullException("plotSettingsInfos");
//			this.plotSettingsInfos = new List<PlotSettingsInfo>(plotSettingsInfos);
//		}
		
		// Формат бумаги с максимальной высотой
		private PlotSettingsInfo maximumHeighted{
			get
			{
				return plotSettingsInfos
					.OrderBy(n => n.Height)
					.ThenBy(n => n.Width)
					.Last();
			}
		}
		
		/// <summary>
		/// Метод ищет наименьший формат бумаги, в который помещается указанная область
		/// </summary>
		/// <param name="borders">Границы области, для которой надо делать Layout</param>
		/// <param name="precision">Точность определения формата</param>
		/// <returns>Формат бумаги</returns>
		public PlotSettingsInfo GetPlotSettings(DrawingBorders borders, int precision)
		{
			double scale = 1 / borders.ScaleFactor;
			return plotSettingsInfos
				.Where(n => n.Height >= borders.Height*scale - precision)
				.Where(n => n.Width >= borders.Width*scale - precision)
				.Aggregate(maximumHeighted, (work, next) => {
				           	double DiffHeightNext = Math.Round(next.Height) - borders.Height*scale;
				           	double DiffHeightWork = Math.Round(work.Height) - borders.Height*scale;
				           	double DiffWidthNext = Math.Round(next.Width) - borders.Width*scale;
				           	double DiffWidthWork = Math.Round(work.Width) - borders.Width*scale;
				           	if (DiffHeightNext<DiffHeightWork)
				           		return next;
				           	if (DiffHeightNext==DiffHeightWork)
				           	{
				           		if (DiffWidthWork<DiffWidthNext)
				           			return work;
				           		return next;
				           	}
				           	return work;
				           });
			
		}
		
		#region ICollection members
		public int Count {
			get {
				return plotSettingsInfos.Count();
			}
		}
		
		public bool IsReadOnly {
			get {
				return false;
			}
		}
		
		public void Add(PlotSettingsInfo item)
		{
			if (!plotSettingsInfos.Contains(item))
			{
				plotSettingsInfos.Add(item);
			}
			else
			{
				throw new ArgumentException(string.Format("Collection have PlotSetting same size like {0}x{1}", item.Width, item.Height));
			}
		}
		
		public void Clear()
		{
			plotSettingsInfos.Clear();
		}
		
		public bool Contains(PlotSettingsInfo item)
		{
			return plotSettingsInfos.Contains(item);
		}
		
		public void CopyTo(PlotSettingsInfo[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}
		
		public bool Remove(PlotSettingsInfo item)
		{
			return plotSettingsInfos.Remove(item);
		}
		
		public IEnumerator<PlotSettingsInfo> GetEnumerator()
		{
			return plotSettingsInfos.GetEnumerator();
		}
		
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return plotSettingsInfos.GetEnumerator();
		}
		#endregion
	}
}
