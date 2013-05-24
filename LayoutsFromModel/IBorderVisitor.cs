/*
 * User: aleksey
 * Date: 01.05.2013
 * Time: 15:07
 */
using System;

namespace LayoutsFromModel
{
	/// <summary>
	/// Интерфейс посетителя, отрисовывающего границы чертежей
	/// </summary>
	public interface IBorderVisitor
	{
		void DrawBorder(DrawingBorders border);
		void ClearData();
	}
}
