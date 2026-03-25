using Microsoft.UI.Xaml.Data;

namespace Invoice.Helpers;

public class ActionTypeToVietnameseConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string actionType)
        {
            return actionType == "Import" ? "Nhập kho" : actionType == "Export" ? "Xuất kho" : actionType;
        }

        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is string vietnamese)
        {
            return vietnamese == "Nhập kho" ? "Import" : vietnamese == "Xuất kho" ? "Export" : vietnamese;
        }

        return value;
    }
}
