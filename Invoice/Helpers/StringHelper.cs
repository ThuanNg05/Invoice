using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Controls;

namespace Invoice.Helpers;

public static class StringHelper
{
    public static string CleanStringSimple(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        
        var words = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        return string.Join(" ", words);
    }

    public static string GetNormalizedLastName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName)) 
            return "KhachLe"; // Default if name is empty

        // 1. Get the last part of the name (split by whitespace)
        var parts = fullName.Trim().Split(' ');
        var lastName = parts.Last();

        // 2. Remove Vietnamese diacritics
        return RemoveDiacritics(lastName);
    }

    private static string RemoveDiacritics(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return text;

        // Normalize to FormD (decomposes characters like 'á' to 'a' + '´')
        string normalizedString = text.Normalize(NormalizationForm.FormD);
        StringBuilder stringBuilder = new StringBuilder();

        foreach (char c in normalizedString)
        {
            // Check if character is a non-spacing mark (the accent)
            UnicodeCategory unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        // Normalize back to FormC and remove any remaining specific Vietnamese characters like 'đ'
        string result = stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        
        // Handle specific cases for 'đ' and 'Đ' which aren't handled by normalization
        result = result.Replace("đ", "d").Replace("Đ", "D");

        // Optional: Remove any non-alphanumeric characters to keep the code clean
        return Regex.Replace(result, "[^a-zA-Z0-9]", "");
    }

    private static readonly string[] Units = { "", "một", "hai", "ba", "bốn", "năm", "sáu", "bảy", "tám", "chín" };

    /// <summary>
    /// Converts a number to Vietnamese currency text (e.g., 10500 -> "Mười nghìn năm trăm đồng")
    /// </summary>
    public static double ParseDouble(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return 0;
        return double.TryParse(text.Trim().Replace(",", ""), NumberStyles.Any, CultureInfo.InvariantCulture, out double val) ? val : 0;
    }

    public static void ClearInputs(DependencyObject parent)
    {
        int count = VisualTreeHelper.GetChildrenCount(parent);

        for (int i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);

            if (child is TextBox textBox &&
                !string.IsNullOrEmpty(textBox.Name))
            {
                textBox.Text = string.Empty;
            }
            ClearInputs(child);
        }
    }

    public static string NumberToTextVN(double inputNumber)
    {
        long number = (long)inputNumber; // Convert to long to handle currency as integer
        if (number == 0) return "Không đồng";

        string sNumber = number.ToString();
        
        // Pad length to be divisible by 3 (e.g., 1000 -> 001000)
        int padding = 3 - sNumber.Length % 3;
        if (padding < 3)
        {
            sNumber = new string('0', padding) + sNumber;
        }

        int len = sNumber.Length;
        int groupCount = len / 3;
        
        StringBuilder result = new StringBuilder();
        
        // Loop through groups of 3 digits
        for (int i = 0; i < groupCount; i++)
        {
            string group = sNumber.Substring(i * 3, 3);
            int a = group[0] - '0'; // Hundreds
            int b = group[1] - '0'; // Tens
            int c = group[2] - '0'; // Units

            if (a == 0 && b == 0 && c == 0) continue; // Skip "000" groups unless special handling needed

            // Process Hundreds
            if (result.Length > 0 || a > 0) // Only read hundreds if we have handled previous groups or a > 0
            {
                result.Append(Units[a] + " trăm ");
            }
            
            // Process Tens & Units
            if (b == 0)
            {
                if (c == 0) { } // x00 -> skip
                else
                {
                    // x05 -> "linh năm" (if hundreds existed)
                    if (result.Length > 0) result.Append("linh ");
                    result.Append(Units[c] + " ");
                }
            }
            else if (b == 1) // 1x (10-19)
            {
                result.Append("mười ");
                if (c == 1) result.Append("một "); 
                else if (c == 5) result.Append("lăm ");
                else if (c > 0) result.Append(Units[c] + " ");
            }
            else // 2x, 3x... (20-99)
            {
                result.Append(Units[b] + " mươi ");
                if (c == 1) result.Append("mốt ");
                else if (c == 4) result.Append("bốn "); // "Hai mươi bốn" is standard
                else if (c == 5) result.Append("lăm ");
                else if (c > 0) result.Append(Units[c] + " ");
            }

            // Add Scale (Nghìn, Triệu, Tỷ)
            int remainingGroups = groupCount - i - 1;
            if (remainingGroups == 3) result.Append("tỷ ");
            else if (remainingGroups == 2) result.Append("triệu ");
            else if (remainingGroups == 1) result.Append("nghìn ");
        }

        // Final formatting
        string text = result.ToString().Trim();
        
        // Capitalize first letter
        if (text.Length > 0)
        {
            text = char.ToUpper(text[0]) + text.Substring(1);
        }

        return text + " đồng";
    }
}