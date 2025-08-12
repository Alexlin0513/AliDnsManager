using System;
using System.Globalization;
using System.Windows.Data;

namespace AliDnsManager.Converters
{
    public class LineToChineseConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var line = value?.ToString() ?? string.Empty;
            return line switch
            {
                "default" => "默认",
                "telecom" => "电信",
                "unicom" => "联通",
                "mobile" => "移动",
                "oversea" => "海外",
                "edu" => "教育网",
                _ => line
            };
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var text = value?.ToString() ?? string.Empty;
            return text switch
            {
                "默认" => "default",
                "电信" => "telecom",
                "联通" => "unicom",
                "移动" => "mobile",
                "海外" => "oversea",
                "教育网" => "edu",
                _ => text
            };
        }
    }
}

