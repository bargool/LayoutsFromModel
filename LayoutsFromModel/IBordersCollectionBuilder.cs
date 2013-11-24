/*
 * User: aleksey.nakoryakov
 * Date: 07.05.13
 * Time: 14:22
 */
using System;

namespace LayoutsFromModel
{
	/// <summary>
	/// Интерфейс строителя коллекции границ чертежей
	/// </summary>
	public interface IBordersCollectionBuilder
	{
		/// <summary>
		/// Номер первого чертежа
		/// </summary>
		int InitialBorderIndex { get; set; }
		
		DrawingBorders[] GetDrawingBorders();
	}
}
