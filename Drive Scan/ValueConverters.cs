using System;
using System.Windows.Data;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Text;
using System.Windows;
using System.IO;
using System.Drawing;

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

    /// <summary>
    /// Calculates the Used Drive Space of a Drive
    /// </summary>
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

    /// <summary>
    /// Gets the icon of a file or folder by its path and returns it as a BitmapImageSource
    /// </summary>
    public class AssociatedIconConverter : IValueConverter
    {
        // Load the error icon as a static variable so we only had to load it once not every time it is needed
        public static Icon ErrorIcon = Icon.FromHandle((new Bitmap(Image.FromFile(@"Resources\Error.ico"), new System.Drawing.Size(128, 128))).GetHicon());
        public enum SourceType {File, Folder}
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //Use shell32.dll to extract the icon by the file/folder's path
            using (Icon icon = ExtractFromPath((string)value))
            {
                return System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                icon.Handle,
                new Int32Rect(0, 0, icon.Width, icon.Height),
                System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }

        #region SHGetFileInfo Usage

        private static Icon ExtractFromPath(string path)
        {
            SHFILEINFO shinfo = new SHFILEINFO();
            SHGetFileInfo(
                path,
                0, ref shinfo, (uint)Marshal.SizeOf(shinfo),
                SHGFI_ICON | SHGFI_LARGEICON);

            // If getting the icon fails then the file likely does not exist
            //  So we just use the default windows 10 error icon for the file
            try
            {
                return System.Drawing.Icon.FromHandle(shinfo.hIcon);
            }
            catch
            {
                return ErrorIcon;
            }
        }

        //Struct used by SHGetFileInfo function
        [StructLayout(LayoutKind.Sequential)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        };

        [DllImport("shell32.dll")]
        private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

        private const uint SHGFI_ICON = 0x100;
        private const uint SHGFI_LARGEICON = 0x0;
        private const uint SHGFI_SMALLICON = 0x000000001;

        #endregion
    }
}