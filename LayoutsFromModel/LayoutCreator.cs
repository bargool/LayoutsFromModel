/*
 * User: aleksey
 * Date: 02.05.2013
 * Time: 15:35
 */
using System;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Bargool.Acad.Library;

namespace LayoutsFromModel
{
	/// <summary>
	/// Класс для создания листов
	/// </summary>
	public class LayoutCreator
	{
		Database wdb;
		
		public LayoutCreator()
		{
			this.wdb = HostApplicationServices.WorkingDatabase;
		}
		
		/// <summary>
		/// Метод создаёт Layout по заданным параметрам
		/// </summary>
		/// <param name="borders">Границы выделенной области в пространстве модели</param>
		public void CreateLayout(DrawingBorders borders)
		{
			using (Transaction tr = this.wdb.TransactionManager.StartTransaction())
			{
				string layoutName = CheckLayoutName(borders.Name);
				
				PlotSettings ps = ImportPlotSettings(borders, tr);
				LayoutManager lm = LayoutManager.Current;
				Layout layout;
				try
				{
					layout = (Layout)tr.GetObject(lm.CreateLayout(layoutName), OpenMode.ForWrite);
				}
				catch (System.Exception ex)
				{
					throw new System.Exception(String.Format("Ошибка создания Layout {0}\n{1}", layoutName, ex.Message));
				}
				
				layout.CopyFrom(ps);
				layout.AnnoAllVisible = true;
				lm.CurrentLayout = layout.LayoutName;
				View.Zoom(new Point3d(0,0,0), new Point3d(layout.PlotPaperSize.X, layout.PlotPaperSize.Y, 0), new Point3d(), 1);
				CreateViewport(layout, borders, tr);
				tr.Commit();
			}
		}
		
		/// <summary>
		/// Метод удаляет неинициализированные листы
		/// </summary>
		public void DeleteNoninitializedLayouts()
		{
			using (Transaction tr = wdb.TransactionManager.StartTransaction())
			{
				DBDictionary dic = (DBDictionary)tr.GetObject(wdb.LayoutDictionaryId, OpenMode.ForRead);
				foreach (DBDictionaryEntry entry in dic)
				{
					Layout layout = (Layout)tr.GetObject(entry.Value, OpenMode.ForRead);
					if (!layout.ModelType && layout.GetViewports().Count == 0)
					{
						if (dic.Count>1)
						{
							if (!dic.IsWriteEnabled)
								dic.UpgradeOpen();
							dic.Remove(entry.Value);
							layout.UpgradeOpen();
							layout.Erase(true);
							Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager
								.MdiActiveDocument.Editor.WriteMessage("\nУдаляю лист "+layout.LayoutName+Environment.NewLine);
						}
					}
				}
				tr.Commit();
			}
		}
		
		/// <summary>
		/// Проверка корректности желаемого имени листа
		/// Если лист с желаемым именем уже существует, к данному имени добавится "(1)"
		/// Если и это имя уже есть - цифра в скобках будет увеличиваться, пока не будет найден
		/// уникальный вариант
		/// </summary>
		/// <param name="expectedName">Желаемое имя листа</param>
		/// <returns>Корректное имя листа</returns>
		string CheckLayoutName(string expectedName)
		{
			string layoutName = expectedName;
			using (Transaction tr = this.wdb.TransactionManager.StartTransaction())
			{
				// Проверяем на наличие листа с указанным именем
				DBDictionary layoutsDic = (DBDictionary)tr.GetObject(wdb.LayoutDictionaryId, OpenMode.ForRead);
				if (layoutsDic.Contains(layoutName))
				{
					// Если есть - добавляем номер в скобках, итерируем номер, пока имя не станет уникальным
					int dublicateLayoutIndex = 1;
					while (layoutsDic.Contains(string.Format("{0}({1})", layoutName, dublicateLayoutIndex)))
					{
						dublicateLayoutIndex++;
					}
					layoutName = string.Format("{0}({1})", layoutName, dublicateLayoutIndex);
				}
				tr.Commit();
			}
			return layoutName;
		}
		
		/// <summary>
		/// Метод добавляет из объекта границ чертежа новые
		/// именованые настройки печати в файл, если таковых там нет
		/// </summary>
		/// <param name="borders">Объект границ чертежа</param>
		/// <param name="tr">Текущая транзакция</param>
		/// <returns>Настройки печати, соответствующие границам чертежа</returns>
		PlotSettings ImportPlotSettings(DrawingBorders borders, Transaction tr)
		{
			PlotSettings ps = new PlotSettings(false);
			ps.CopyFrom(borders.PSInfo.PSettings);
			
			DBDictionary psDict = (DBDictionary)tr.GetObject(this.wdb.PlotSettingsDictionaryId,
			                                                 OpenMode.ForRead);
			if (!psDict.Contains(ps.PlotSettingsName))
			{
				psDict.UpgradeOpen();
				ps.AddToPlotSettingsDictionary(this.wdb);
				tr.AddNewlyCreatedDBObject(ps, true);
				psDict.DowngradeOpen();
			}
			return ps;
		}
		
		/// <summary>
		/// Метод создаёт Viewport на заданном Layout, в размер листа
		/// </summary>
		/// <param name="layout">Layout, на котором создаётся viewport</param>
		/// <param name="borders">Границы выделенной области в модели</param>
		void CreateViewport(Layout layout, DrawingBorders borders, Transaction tr)
		{
			int vpCount = layout.GetViewports().Count;
			if (vpCount == 0)
			{
				throw new System.Exception(String.Format("Layout {0} не инициализирован", layout.LayoutName));
			}
			Viewport vp;
			if (vpCount == 1)
			{
				BlockTableRecord lbtr =
					(BlockTableRecord)tr.GetObject(layout.BlockTableRecordId, OpenMode.ForWrite);
				vp = new Viewport();
				vp.SetDatabaseDefaults();
				lbtr.AppendEntity(vp);
				tr.AddNewlyCreatedDBObject(vp, true);
				vp.On = true;
			}
			else
			{
				ObjectId vpId = layout.GetViewports()[vpCount-1];
				if (vpId.IsNull)
					throw new System.Exception("Не удалось получить вьюпорт!");
				
				vp = (Viewport)tr.GetObject(vpId, OpenMode.ForWrite);
				if (vp == null)
					throw new System.Exception("Не удалось получить вьюпорт!");
			}
			// Высоту и ширину вьюпорта выставляем в размер выделенной области
			vp.Height = borders.Height / borders.ScaleFactor;
			vp.Width = borders.Width / borders.ScaleFactor;
			vp.CenterPoint = new Point3d(vp.Width/2 + layout.PlotOrigin.X,
			                             vp.Height/2 + layout.PlotOrigin.Y,
			                             0);
			vp.ViewTarget = new Point3d(0,0,0);
			vp.ViewHeight = borders.Height;
			vp.ViewCenter = new Point2d(borders.Center.X, borders.Center.Y);
			vp.Locked = LayoutsFromModel.Configuration.AppConfig.Instance.LockViewPorts;
		}
	}
}
