/*
 * User: aleksey.nakoryakov
 * Date: 27.08.12
 * Time: 15:19
 */
using System;
using System.Windows.Controls;

namespace LayoutsFromModel.Configuration
{
	/// <summary>
	/// Заготовка для PrecisionValidationRule.
	/// </summary>
	public class PrecisionValidationRule : ValidationRule
	{
		int minValue = 0;
		
		public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
		{
			int precision;
			if (!int.TryParse((string)value, out precision))
				return new ValidationResult(false, "Должно быть число");
			if (precision<minValue)
				return new ValidationResult(false, "Точность должна быть положительной или нулём");
			return new ValidationResult(true, value);
		}
	}
}
