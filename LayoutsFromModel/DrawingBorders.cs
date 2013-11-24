/*
 * User: aleksey.nakoryakov
 * Date: 06.03.12
 * Time: 18:44
 */
using System;
using Autodesk.AutoCAD.Geometry;

namespace LayoutsFromModel
{
	/// <summary>
	/// Класс, описывающий поведение границ выделенной
	/// области для отображения в пространстве листа
	/// </summary>
	public sealed class DrawingBorders
	{
		Point3d first;
		/// <summary>
		/// Первая точка рамки
		/// </summary>
		public Point3d First {
			get { return first; }
		}
		
		Point3d second;
		/// <summary>
		/// Вторая точка рамки, противоположная первой
		/// </summary>
		public Point3d Second {
			get { return second; }
		}

		/// <summary>
		/// Высота выделенной области
		/// </summary>
		public double Height {
			get { return Math.Abs(first.Y-second.Y); }
		}
		
		/// <summary>
		/// Ширина выделенной области
		/// </summary>
		public double Width {
			get { return Math.Abs(first.X-second.X); }
		}
		
		/// <summary>
		/// Центр выделенной области
		/// </summary>
		public Point3d Center {
			get { return (new LineSegment3d(first, second)).MidPoint;  }
		}
		
		string name;
		/// <summary>
		/// Имя области, в дальнейшем имя листа
		/// </summary>
		public string Name {
			get { return name; }
			set { name = value; }
		}
		
		/// <summary>
		/// Масштаб
		/// </summary>
		public double ScaleFactor { get; private set; }
		
		/// <summary>
		/// Информация о формате бумаги, в который вписываются данные границы
		/// </summary>
		public PlotSettingsInfo PSInfo { get; set; }
		
		private DrawingBorders() {}
		
		private DrawingBorders(Point3d first, Point3d second, string name, double scale)
		{
			this.first = first;
			this.second = second;
			this.name = name;
			this.ScaleFactor = scale;
		}
		
		public static DrawingBorders CreateDrawingBorders(Point3d first, Point3d second, string name, double scale)
		{
			DrawingBorders borders = new DrawingBorders(first, second, name, scale);
			PlotSettingsManager psm = PlotSettingsManager.Current;
			borders.PSInfo = psm.GetPlotSettings(borders, Configuration.AppConfig.Instance.Precision);
			return borders;
		}
		
		public override string ToString()
		{
			return string.Format("[LayoutBorders First={0}, Second={1}, Name={2}, Scale={3}]", first, second, name, ScaleFactor);
		}
		
		public void Accept(IBorderVisitor visitor)
		{
			visitor.DrawBorder(this);
		}
	}
}
