using Microsoft.UI.Xaml.Data;

namespace Invoice.Helpers;

public class IntToStringConverter : IValueConverter
{    
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is int intValue)
        {
            return intValue == 0 ? "" : intValue.ToString(); // Hoặc trả về intValue.ToString() luôn nếu muốn hiện số 0
        }
        return "";
    }    
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        string strValue = value as string;
        if (string.IsNullOrEmpty(strValue))
        {
            return 0; // Nếu xóa hết chữ thì trả về 0 (để không bị lỗi)
        }

        if (int.TryParse(strValue, out int result))
        {
            return result;
        }

        return 0; // Nhập linh tinh cũng trả về 0
    }
}