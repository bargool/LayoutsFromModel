/*
 * User: aleksey.nakoryakov
 * Date: 06.03.12
 * Time: 18:05
 */
//Microsoft
using System;

//Autodesk
using Autodesk.AutoCAD.Runtime;
using acad = Autodesk.AutoCAD.ApplicationServices.Application;

[assembly:CommandClass(typeof(LayoutsFromModel.CommandClass))]

namespace LayoutsFromModel
{
	/// <summary>
	/// Данный класс содержит методы для непосредственной работы с AutoCAD
	/// </summary>
	public class CommandClass
	{
		[CommandMethodAttribute("bargLFM", CommandFlags.Modal|CommandFlags.NoPaperSpace)]
		[CommandMethodAttribute("LFM", CommandFlags.Modal|CommandFlags.NoPaperSpace)]
		public void LayoutFromUserInput()
		{
			CreateLayouts(new UserInputBordersBuilder());
		}
		
		[CommandMethod("bargLFBL", CommandFlags.Modal|CommandFlags.NoPaperSpace|CommandFlags.UsePickSet)]
		public void LayoutFromBlocks()
		{
			CreateLayouts(new BlocksBordersBuilder());
		}
		
		private void CreateLayouts(IBordersCollectionBuilder bordersBuilder)
		{
			InitialUserInteraction initial = new InitialUserInteraction();
			initial.GetInitialData();
			if (initial.InitialDataStatus == PromptResultStatus.Cancelled)
				return;
			initial.FillPlotInfoManager();
			bordersBuilder.InitialBorderIndex = initial.Index;
			DrawingBorders[] borders = bordersBuilder.GetDrawingBorders();
			if (borders.Length == 0)
			{
				acad.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nНе выбран ни один чертёж");
				return;
			}
			LayoutCreator layoutCreator = new LayoutCreator();
			foreach (DrawingBorders border in borders)
			{
				layoutCreator.CreateLayout(border);
			}
			
			Configuration.AppConfig cfg = Configuration.AppConfig.Instance;
			// Если в конфигурации отмечено "возвращаться в модель" - то переходим в модель
			if (cfg.TilemodeOn)
				acad.SetSystemVariable("TILEMODE", 1);
			
			// Если в конфигурации отмечено "удалять неинициализированные листы" - удаляем их
			if (cfg.DeleteNonInitializedLayouts)
			{
				layoutCreator.DeleteNoninitializedLayouts();
				acad.DocumentManager.MdiActiveDocument.Editor.Regen();
			}
		}
	}
}