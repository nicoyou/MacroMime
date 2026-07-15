using System.Globalization;
using System.Windows.Data;

namespace MacroMime.App.Views;

/// <summary>真偽値を反転するコンバーター</summary>
public sealed class InverseBooleanConverter : IValueConverter {
	/// <summary>値を反転する</summary>
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		=> value is false;

	/// <summary>値を反転して戻す</summary>
	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		=> value is false;
}
