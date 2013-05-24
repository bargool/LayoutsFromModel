/*
 * User: aleksey
 * Date: 01.05.2013
 * Time: 15:09
 */
using System;
using System.Collections.Generic;

using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using ADS = Autodesk.AutoCAD.DatabaseServices;

namespace LayoutsFromModel
{
	/// <summary>
	/// Посетитель, отрисовывающий временной графикой выбранные пользователем рамки.
	/// </summary>
	public class BorderDrawer : IBorderVisitor
	{
		List<Drawable> objects = new List<Drawable>();
		Color graphicsColor = Color.FromColorIndex(ColorMethod.ByLayer, 20);
		
		public void DrawBorder(DrawingBorders border)
		{
			// Обводим рамку прямоугольником
			Drawable rec = CreateRectangle(border.First, border.Second);
			this.objects.Add(rec);
			TransientManager tm = TransientManager.CurrentTransientManager;
			tm.AddTransient(rec, TransientDrawingMode.Highlight, 128, new IntegerCollection());

			// Вписываем в рамку название будущего листа и его формат
			Drawable txt = CreateLayoutNameMText(border.Center, border.Name, border.PSInfo.Name, border.ScaleFactor);
			this.objects.Add(txt);
			tm.AddTransient(txt, TransientDrawingMode.DirectShortTerm, 256, new IntegerCollection());
		}
		
		private ADS.DBObject CreateRectangle(Point3d first, Point3d second)
		{
			ADS.Polyline pl = new ADS.Polyline(4);
			pl.Color = this.graphicsColor;
			double firstX = first.X;
			double secondX = second.X;
			double firstY = first.Y;
			double secondY = second.Y;
			pl.AddVertexAt(0, new Point2d(firstX, firstY), 0, 0, 0);
			pl.AddVertexAt(1, new Point2d(firstX, secondY), 0, 0, 0);
			pl.AddVertexAt(2, new Point2d(secondX, secondY), 0, 0, 0);
			pl.AddVertexAt(3, new Point2d(secondX, firstY), 0, 0, 0);
			pl.Closed = true;
			return pl;
		}
		
		private ADS.DBObject CreateLayoutNameMText(Point3d center, string name, string format, double scaleFactor)
		{
			ADS.MText mt = new ADS.MText();
			mt.SetDatabaseDefaults(ADS.HostApplicationServices.WorkingDatabase);
			mt.BackgroundFillColor = this.graphicsColor;
			mt.BackgroundFill = true;
			mt.Contents = name + "\\P" + format;
			mt.TextHeight = 12 * scaleFactor;
			mt.Location = center;
			mt.Attachment = ADS.AttachmentPoint.MiddleCenter;
			return mt;
		}
		
		public void ClearData()
		{
			TransientManager tm = TransientManager.CurrentTransientManager;
			if (this.objects != null || this.objects.Count != 0)
			{
				tm.EraseTransients(TransientDrawingMode.Highlight, 128, new IntegerCollection());
				tm.EraseTransients(TransientDrawingMode.DirectShortTerm, 256, new IntegerCollection());
				foreach (Drawable obj in this.objects)
					obj.Dispose();
				this.objects.Clear();
			}
		}
	}
}
