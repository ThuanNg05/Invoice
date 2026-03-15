using Microsoft.UI.Xaml.Data;

namespace Invoice.Helpers;

public class BoolToNegationConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {    
        if (value is bool booleanValue)
            return !booleanValue;
        
        return true;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is bool booleanValue)
            return !booleanValue;
            
        return true;
    }
}