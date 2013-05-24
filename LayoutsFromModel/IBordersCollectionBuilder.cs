/*
 * User: aleksey.nakoryakov
 * Date: 07.05.13
 * Time: 14:22
 */
using System;

namespace LayoutsFromModel
{
	/// <summary>
	/// Description of IBordersCollectionBuilder.
	/// </summary>
	public interface IBordersCollectionBuilder
	{
		int InitialBorderIndex { get; set; }
		DrawingBorders[] GetDrawingBorders();
	}
}
