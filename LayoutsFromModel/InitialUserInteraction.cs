/*
 * User: aleksey.nakoryakov
 * Date: 06.05.13
 * Time: 14:14
 */
using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.EditorInput;
using CO = LayoutsFromModel.Properties.CmdOptions;
using CP = LayoutsFromModel.Properties.CmdPrompts;

namespace LayoutsFromModel
{
	/// <summary>
	/// Обработка первоначального взаимодействия с пользователем. Получение номера первого листа. Работа с конфигурацией.
	/// Заполнение коллекции настроек печати
	/// </summary>
	public class InitialUserInteraction
	{
		bool useTemplate;
		
		Editor ed;
		
		public int Index { get; set; }
		
		public PromptResultStatus InitialDataStatus { get; private set; }
		
		public InitialUserInteraction()
		{
			this.Index = 1;
			this.useTemplate = false;
			this.ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
		}
		
		/// <summary>
		/// Метод служит для первоначального опроса пользователя
		/// </summary>
		public void GetInitialData()
		{
			PromptIntegerResult piRes = null; // Номер первого Layout
			bool exitLoop = false; // Условие продолжения команды
			do
			{
				// Получаем начальный номер для Layout
				PromptIntegerOptions pio = new PromptIntegerOptions("\n"+CP.FirstLayoutNumber);
				pio.Keywords.Add(CO.Configuration);
				if (!this.useTemplate)
				{
					pio.Keywords.Add(CO.UseTemplate);
					pio.Keywords.Add(CO.TemplateSelect);
				}
				pio.AllowNegative = false;
				pio.AllowNone = false;
				pio.DefaultValue = this.Index;
				piRes = ed.GetInteger(pio);
				// TODO Обрабатывать выход по escape
				switch (piRes.Status)
				{
						// Введён номер - продолжаем
					case PromptStatus.OK:
						this.Index = piRes.Value;
						exitLoop = true;
						break;
						// Отрабатываем ключевые слова
					case PromptStatus.Keyword:
						if (piRes.StringResult.Equals(CO.Configuration, StringComparison.InvariantCulture))
							Configuration.AppConfig.Instance.ShowDialog();
						else
							TemplateProcessing(piRes.StringResult);
						break;
					default:
						this.InitialDataStatus = PromptResultStatus.Cancelled;
						return;
				}
			} while (!exitLoop);
			this.InitialDataStatus = PromptResultStatus.OK;
		}
		
		/// <summary>
		/// Заполнение коллекции настроек печати
		/// </summary>
		public void FillPlotInfoManager()
		{
			Configuration.AppConfig cfg = Configuration.AppConfig.Instance;
			IEnumerable<PlotSettingsInfo> psinfos = null;
			if (this.useTemplate)
			{
				psinfos = PlotSettingsInfoBuilder.CreatePlotSettingsInfos(cfg.TemplatePath);
				ed.WriteMessage("\n" + CP.UsingTemplate + cfg.TemplatePath);
			}
			else
				psinfos = PlotSettingsInfoBuilder.CreatePlotSettingsInfos();
			
			PlotSettingsManager psm = PlotSettingsManager.Current;
			psm.Clear();
			
			foreach (PlotSettingsInfo psi in psinfos)
				psm.Add(psi);
		}
		
		/// <summary>
		/// Обработка параметров ком. строки, связанный с использованием шаблона
		/// </summary>
		/// <param name="keyword">Выбранный параметр ком. строки</param>
		private void TemplateProcessing(string keyword)
		{
			if (keyword.Equals(CO.UseTemplate, StringComparison.InvariantCulture))
			{
				// Если файл шаблона задан и существует, присваиваем true, иначе выкидываем ошибку
				if (Configuration.AppConfig.Instance.TemplateExists())
					this.useTemplate = true;
				else
					throw new System.Exception("Не задан, или неверно задан файл шаблона");
			}
			
			if (keyword.Equals(CO.TemplateSelect, StringComparison.InvariantCulture))
			{
				this.useTemplate = SelectTemplate();
			}
		}
		
		/// <summary>
		/// Запрос пользователя для выбора файла шаблона
		/// </summary>
		/// <returns>True, если был выбран корректный файл шаблона, иначе false</returns>
		private bool SelectTemplate()
		{
			Configuration.AppConfig cfg = Configuration.AppConfig.Instance;
			PromptOpenFileOptions pofo = new PromptOpenFileOptions(CP.TemplateFileQuery);
			pofo.Filter = CP.TemplateFileQueryFilter;
			pofo.InitialFileName = cfg.TemplatePath;
			
			PromptFileNameResult templateName = ed.GetFileNameForOpen(pofo);
			
			if (templateName.Status == PromptStatus.OK)
			{
				if (!System.IO.File.Exists(templateName.StringResult))
					throw new System.IO.FileNotFoundException(templateName.StringResult);
				cfg.TemplatePath = templateName.StringResult;
				cfg.Save();
				return true;
			}
			
			return false;
		}
	}
}
