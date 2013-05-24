/*
 * User: aleksey.nakoryakov
 * Date: 07.05.13
 * Time: 14:26
 */
using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Bargool.Acad.Library;

using CO = LayoutsFromModel.Properties.CmdOptions;
using CP = LayoutsFromModel.Properties.CmdPrompts;

namespace LayoutsFromModel
{
	/// <summary>
	/// Класс, создающий коллекцию границ чертежей с помощью
	/// ввода этих самых границ пользователем
	/// </summary>
	public class UserInputBordersBuilder : IBordersCollectionBuilder
	{
		Editor ed;
		public int InitialBorderIndex { get; set; }
		
		public UserInputBordersBuilder()
		{
			ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
		}
		
		public UserInputBordersBuilder(int firstPageIndex)
			: this()
		{
			this.InitialBorderIndex = firstPageIndex;
		}
		
		public DrawingBorders[] GetDrawingBorders()
		{
			List<DrawingBorders> borders = new List<DrawingBorders>();
			
			// Выставляем osmode в 1 - ENDpoint
			using (AcadSystemVariableSwitcher varSW = new AcadSystemVariableSwitcher("OSMODE", 1))
			{
				Configuration.AppConfig cfg = Configuration.AppConfig.Instance;
				// Крутимся, пока нужны новые масштабы (а один раз он точно нужен)
				bool needNewScale = true;
				
				BorderDrawer drawer = new BorderDrawer();
				
				while (needNewScale)
				{
					// Получаем масштаб
					double scale = GetScale(cfg.ReferenceDimension);
					if (scale == 0)
						return borders.ToArray();
					
					needNewScale = false;
					
					// Крутимся, пока нужны новые рамки
					bool needNewBorder = true;
					while (needNewBorder)
					{
						BorderPromptResult borderRes = GetBorderPoints();
						switch (borderRes.QueryStatus)
						{
							case PromptResultStatus.Cancelled:
								// Пользователь нажал escape, запускаем процесс создания листов
								needNewBorder = false;
								break;
								
							case PromptResultStatus.Keyword:
								// Использованы параметры ком. строки
								
								// Нужен новый масштаб
								if (borderRes.StringResult.Equals(CO.NewScale, StringComparison.InvariantCulture))
								{
									needNewBorder = false;
									needNewScale = true;
								}
								
								// Запускаем процесс создания листов
								if (borderRes.StringResult.Equals(CO.Process, StringComparison.InvariantCulture))
								{
									needNewBorder = false;
								}
								
								// Отменяем последний введённый чертёж
								if (borderRes.StringResult.Equals(CO.Undo, StringComparison.InvariantCulture))
								{
									if (borders.Count>0)
									{
										borders.RemoveAt(borders.Count-1);
										InitialBorderIndex--;
										
										drawer.ClearData();
										foreach (DrawingBorders b in borders)
										{
											b.Accept(drawer);
										}
									}
									else
									{
										ed.WriteMessage("\nНечего возвращать");
									}
								}
								
								// Выходим из команды
								if (borderRes.StringResult.Equals(CO.Cancel, StringComparison.InvariantCulture))
								{
									ed.WriteMessage("\nОтмена!");
									borders.Clear();
									return borders.ToArray();
								}
								
								break;
								
							case PromptResultStatus.OK:
								// Введены точки
								string bordername = string.Format("{0}{1}{2}", cfg.Prefix, InitialBorderIndex++, cfg.Suffix);
								DrawingBorders border =
									DrawingBorders.CreateDrawingBorders(borderRes.FirstPoint,
									                                    borderRes.SecondPoint,
									                                    bordername,
									                                    scale);
								ed.WriteMessage("\nДобавляем лист {0}. Формат листа: {1}", bordername, border.PSInfo.Name);
								borders.Add(border);
								border.Accept(drawer);
								
								break;
							default:
								throw new Exception("Invalid value for BorderQueryResultStatus");
						}
					}
				}
				drawer.ClearData();
				// Возвращаем osmode в исходное состояние
			}
			return borders.ToArray();
		}
		
		double GetScale(double baseReferenceDimension)
		{
			string prompt = string.Format("\nЗадайте или укажите длину основной надписи (то, что должно быть {0} мм):", baseReferenceDimension);
			PromptDistanceOptions pdo =
				new PromptDistanceOptions(prompt);
			pdo.AllowZero = false;
			pdo.AllowNegative = false;
			pdo.Only2d = true;
			pdo.UseDashedLine = true;
			pdo.DefaultValue = baseReferenceDimension;
			PromptDoubleResult res = ed.GetDistance(pdo);
			if (res.Status!= PromptStatus.OK)
				return 0.0;
			double scale = res.Value / baseReferenceDimension;
			ed.WriteMessage("\nМасштабный коэффициент: {0}", scale);
			return scale;
		}
		
		BorderPromptResult GetBorderPoints()
		{
			PromptPointOptions ppo = new PromptPointOptions("\n" + CP.FrameFirstPointQuery);
			ppo.Keywords.Add(CO.Process);
			ppo.Keywords.Add(CO.NewScale);
			ppo.Keywords.Add(CO.Undo);
			ppo.Keywords.Add(CO.Cancel);
			// Запрашиваем первую точку
			PromptPointResult res1 = ed.GetPoint(ppo);
			if (res1.Status== PromptStatus.OK)
			{
				Point3d p1 = res1.Value;
				// Запрашиваем вторую точку
				PromptCornerOptions pco = new PromptCornerOptions(CP.FrameOppositePointQuery, p1);
				pco.UseDashedLine = true;
				PromptPointResult res2 = ed.GetCorner(pco);
				if (res2.Status != PromptStatus.OK)
					return new BorderPromptResult(PromptResultStatus.Cancelled);
				
				p1 = p1.TransformBy(ed.CurrentUserCoordinateSystem);
				Point3d p2 = res2.Value.TransformBy(ed.CurrentUserCoordinateSystem);
				return new BorderPromptResult(p1, p2);
			}
			else if (res1.Status == PromptStatus.Keyword)
			{
				return new BorderPromptResult(res1.StringResult);
			}
			
			return new BorderPromptResult();
		}
	}
}
