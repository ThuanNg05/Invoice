using System.Globalization;
using Microsoft.UI.Xaml.Data;

namespace Invoice.Helpers;

public class DoubleToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is double d)
        {           
            string format = parameter as string ?? "N0";            
            return d.ToString(format, CultureInfo.CurrentCulture);
        }
        if (value is int i)
        {
            return i.ToString("N0", new CultureInfo("vi-VN"));
        }

        if (value is long l)
        {
            return l.ToString("N0", new CultureInfo("vi-VN"));
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}