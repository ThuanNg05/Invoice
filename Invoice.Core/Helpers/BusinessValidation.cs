using System.Text.RegularExpressions;

namespace Invoice.Core.Helpers;

public static class BusinessValidation
{
    /// <summary>
    /// Validates a Vietnamese local phone number.
    /// Rules: Digits only, starts with '0', length is 10 digits.
    /// </summary>
    public static bool IsValidVietnamesePhoneNumber(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return false;
        
        // Pattern for Vietnamese mobile numbers: starts with 0, then 3, 5, 7, 8, or 9, followed by 8 digits.
        // This ensures it's 10 digits total and follows local conventions.
        string pattern = @"^0(3|5|7|8|9)[0-9]{8}$";
        return Regex.IsMatch(phone, pattern);
    }

    /// <summary>
    /// Validates if a price (Odd or Even) is within the allowed length range relative to the original price.
    /// Range: [originalLength - 1, originalLength] after removing formatting dots.
    /// </summary>
    public static bool IsValidPriceLength(string originalPrice, string targetPrice)
    {
        if (string.IsNullOrWhiteSpace(originalPrice) || string.IsNullOrWhiteSpace(targetPrice)) 
            return false;

        string normalizedOriginal = originalPrice.Replace(".", "");
        string normalizedTarget = targetPrice.Replace(".", "");

        int originLength = normalizedOriginal.Length;
        int targetLength = normalizedTarget.Length;

        int minAllowedLength = originLength - 1;
        int maxAllowedLength = originLength;

        return targetLength >= minAllowedLength && targetLength <= maxAllowedLength;
    }
}
