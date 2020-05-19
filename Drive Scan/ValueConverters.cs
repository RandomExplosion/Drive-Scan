using System;
using System.Windows.Data;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Text;
using System.Windows;
using System.IO;

namespace Drive_Scan
{
    /// <summary>
    /// Converts Byte Values into their simplest unit
    /// Source: https://thomasfreudenberg.com/archive/2017/01/21/presenting-byte-size-values-in-wpf/
    /// </summary>
    public class FormatSizeConverter : IValueConverter
    {
        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        private static extern long StrFormatByteSizeW(long qdw, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszBuf,
            int cchBuf);

        public object Convert(
            object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            // if (targetType is null)
            // {
            //     throw new ArgumentNullException(nameof(targetType));
            // }

            // if (parameter is null)
            // {
            //     throw new ArgumentNullException(nameof(parameter));
            // }

            // if (culture is null)
            // {
            //     throw new ArgumentNullException(nameof(culture));
            // }

            long number = System.Convert.ToInt64(value);
            StringBuilder    sb = new StringBuilder(32);
            StrFormatByteSizeW(number, sb, sb.Capacity);
            return sb.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }

        public static object Convert(object value)
        {
            return new FormatSizeConverter().Convert(value, null, null, CultureInfo.CurrentCulture);
        }
    } 

    public class UsedDriveSpaceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DriveInfo driveInfo = value as DriveInfo;
            long usedBytes = driveInfo.TotalSize - driveInfo.TotalFreeSpace;
            return FormatSizeConverter.Convert(usedBytes);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}