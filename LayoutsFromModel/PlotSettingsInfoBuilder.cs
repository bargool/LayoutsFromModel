/*
 * User: aleksey.nakoryakov
 * Date: 28.02.13
 * Time: 13:27
 */
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;

namespace LayoutsFromModel
{
	/// <summary>
	/// Класс служит для создания коллекции PlotSettingsInfo
	/// </summary>
	public static class PlotSettingsInfoBuilder
	{
		/// <summary>
		/// Чтение именованных настроек печати из указанного файла
		/// </summary>
		/// <param name="templatePath">Путь к файлу с именованными настройками печати</param>
		/// <returns>Коллекция PlotSettingsInfo</returns>
		public static IEnumerable<PlotSettingsInfo> CreatePlotSettingsInfos(string templatePath)
		{
			Database db = null;
			bool isTemplateOpened = false;
			
			DocumentCollection docMan = Application.DocumentManager;
			Document doc = docMan.Cast<Document>().FirstOrDefault(d => d.Name.Equals(templatePath, StringComparison.InvariantCulture));
			if (doc != null)
			{
				db = doc.Database;
				isTemplateOpened = true;
			}
			else
			{
				db = new Database(false, true);
				db.ReadDwgFile(templatePath, System.IO.FileShare.Read, true, null);
			}
			
			using (Transaction tr = db.TransactionManager.StartTransaction())
			{
				DBDictionary psDict = tr.GetObject(db.PlotSettingsDictionaryId, OpenMode.ForRead) as DBDictionary;
				if (psDict != null)
				{
					foreach (DBDictionaryEntry entry in psDict)
					{
						ObjectId psId = entry.Value;
						PlotSettings ps = tr.GetObject(psId, OpenMode.ForRead) as PlotSettings;
						// Настройки печати для модели и настройки листов самих по себе
						// нам не нужны
						// только именованные настройки печати для листов
						if (!ps.ModelType && !ps.PlotSettingsName.Contains("*"))
						{
							PlotSettings newPS = new PlotSettings(false);
							newPS.CopyFrom(ps);
							yield return new PlotSettingsInfo(newPS);
						}
					}
				}
				tr.Commit();
			}
			if (!isTemplateOpened)
				db.Dispose();
		}
		
		/// <summary>
		/// Создание коллекции PlotSettingsInfo из пользовательских форматов
		/// в файле DWG to PDF.pc3
		/// Также в коллекцию будут включены форматы, начинающиеся с "ISO_A" - это поведение
		/// подлежит изменению
		/// </summary>
		/// <returns>Коллекция PlotSettingsInfo</returns>
		public static IEnumerable<PlotSettingsInfo> CreatePlotSettingsInfos()
		{
			string PLOTTER_NAME = "DWG To PDF.pc3";
			Database db = HostApplicationServices.WorkingDatabase;
			using (Transaction tr = db.TransactionManager.StartTransaction())
			{
				PlotSettingsValidator psv = PlotSettingsValidator.Current;
				PlotSettings ps = new PlotSettings(false);
				psv.RefreshLists(ps);
				psv.SetPlotConfigurationName(ps, PLOTTER_NAME, null);
				// Получаем список CanonicalMediaNames плоттера
				StringCollection canonicalMediaNames = psv.GetCanonicalMediaNameList(ps);
				
				string plotStyle = "acad.ctb";
				System.Text.RegularExpressions.Regex re = new System.Text.RegularExpressions.Regex(@"[\<>/?"":;*|,=`]");
				
				for (int nameIndex = 0; nameIndex < canonicalMediaNames.Count; nameIndex++)
				{
					// Работаем только с пользовательскими форматами
					if (canonicalMediaNames[nameIndex].Contains("UserDefinedMetric") ||
					    canonicalMediaNames[nameIndex].StartsWith("ISO_A"))
					{
						psv.SetPlotConfigurationName(ps, PLOTTER_NAME, canonicalMediaNames[nameIndex]);

						psv.SetPlotType(ps, Autodesk.AutoCAD.DatabaseServices.PlotType.Layout);
						psv.SetPlotPaperUnits(ps, PlotPaperUnit.Millimeters);

						psv.SetStdScaleType(ps, StdScaleType.StdScale1To1);
						psv.SetUseStandardScale(ps, true);
						
						psv.SetCurrentStyleSheet(ps, plotStyle);
						
						if (canonicalMediaNames[nameIndex].StartsWith("ISO_A0"))
							psv.SetPlotRotation(ps, PlotRotation.Degrees090);
						else
							psv.SetPlotRotation(ps, PlotRotation.Degrees000);
						
						string plotSettingsName = re.Replace(psv.GetLocaleMediaName(ps, nameIndex), "");
						if (string.IsNullOrEmpty(plotSettingsName))
						{
							plotSettingsName = canonicalMediaNames[nameIndex];
						}
						ps.PlotSettingsName = plotSettingsName;
						
						PlotSettings newPS = new PlotSettings(false);
						newPS.CopyFrom(ps);
						yield return new PlotSettingsInfo(newPS);
					}
				}
			}
		}
	}
}
