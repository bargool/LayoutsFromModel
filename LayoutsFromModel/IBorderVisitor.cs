/*
 * User: aleksey
 * Date: 01.05.2013
 * Time: 15:07
 */
using System;

namespace LayoutsFromModel
{
	/// <summary>
	/// Description of IBorderVisitor.
	/// </summary>
	public interface IBorderVisitor
	{
		void DrawBorder(DrawingBorders border);
		void ClearData();
	}
}
