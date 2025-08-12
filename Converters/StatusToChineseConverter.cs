using System;
using System.Globalization;
using System.Windows.Data;

namespace AliDnsManager.Converters
{
    public class StatusToChineseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var raw = value?.ToString()?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(raw)) return string.Empty;

            var s = raw.ToLowerInvariant();
            // Normalize common variants from API/UI: end/enabled/enable, dis/disabled/disable
            if (s == "end" || s == "enable" || s == "enabled" || s == "enabledd") // defensive
                return "启用";
            if (s == "dis" || s == "disable" || s == "disabled")
                return "禁用";

            // Also handle Alibaba Cloud possible uppercase values
            if (s == "enable" || s == "on" || s == "ok") return "启用";
            if (s == "disable" || s == "off") return "禁用";

            // Fallback: return original text
            return raw;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // One-way: UI display only
            return Binding.DoNothing;
        }
    }
}

