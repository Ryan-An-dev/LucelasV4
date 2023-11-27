using System;
using System.Globalization;
using System.Windows.Controls;

namespace CommonModule.Validations
{
	public class BindingClassValidation : ValidationRule
	{
		public override ValidationResult Validate(object value, CultureInfo cultureInfo)
		{
            return string.IsNullOrWhiteSpace((value ?? "").ToString())
                ? new ValidationResult(false, "필수입력항목입니다.")
                : ValidationResult.ValidResult;
        }
	}
}
