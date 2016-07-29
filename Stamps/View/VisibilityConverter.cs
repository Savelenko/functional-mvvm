using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Stamps.View
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    internal class VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var visible = (bool)value;

            return visible ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            Binding.DoNothing;
    }
}
