using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Invoice.Core.Models;

namespace Invoice.Helpers;

public class InventoryToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is Materials material)
        {
            if (material.Inventory <= material.MinAmount)
            {
                return new SolidColorBrush(Colors.Red);
            }
        }
        return new SolidColorBrush(Colors.Transparent);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
