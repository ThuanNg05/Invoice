using Microsoft.UI.Xaml.Data;

namespace Invoice.Helpers
{
    public class DateTimeFormatter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is DateTime dt)
            {
                // Use the parameter as the format string (e.g., "dd/MM/yyyy"), or default to "dd/MM/yyyy"
                string format = parameter as string ?? "dd/MM/yyyy";
                return dt.ToString(format);
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
