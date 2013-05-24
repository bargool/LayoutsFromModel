/*
 * User: aleksey.nakoryakov
 * Date: 03.08.12
 * Time: 12:32
 */
using System;
using System.Reflection;
using Autodesk.AutoCAD.ApplicationServices;

namespace LayoutsFromModel
{
	/// <summary>
	/// Класс для работы с настройками AutoCAD
	/// </summary>
	public static class AcadPreferencesHelper
	{
		/// <summary>
		/// Метод задаёт значение для настройки LayoutCreateViewport
		/// </summary>
		/// <param name="newValue">Новое значение настройки</param>
		/// <returns>Старое значение настройки</returns>
		public static bool SetLayoutCreateViewportProperty(bool newValue)
		{
			object acadObject = Application.AcadApplication;
			object preferences = acadObject.GetType().InvokeMember("Preferences",
			                                                       BindingFlags.GetProperty,
			                                                       null, acadObject, null);
			object display =
				preferences.GetType().InvokeMember("Display",
				                                   BindingFlags.GetProperty,
				                                   null, preferences, null);
			object layoutProperty = display
				.GetType().InvokeMember("LayoutCreateViewport",
				                        BindingFlags.GetProperty,
				                        null, display, null);
			bool layoutCreateViewportProperty = Convert.ToBoolean(layoutProperty);
			object[] dataArray = new object[]{newValue};
			display.GetType()
				.InvokeMember("LayoutCreateViewport",
				              BindingFlags.SetProperty,
				              null, display, dataArray);
			return layoutCreateViewportProperty;
		}
	}
}
