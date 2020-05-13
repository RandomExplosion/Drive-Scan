using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using System.IO;
using System.Runtime.InteropServices;
using System.Globalization;

namespace Drive_Scan
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    
    public partial class DriveScanWindow : MetroWindow
    {

        //Singleton Pattern
        static DriveScanWindow currentWindow;

        //Runs on window open
        public DriveScanWindow()
        {
            InitializeComponent();

            //Populate Drive List
            DriveList.ItemsSource = DriveInfo.GetDrives();
        }

        public void DriveList_Row_DoubleClick(object Sender, MouseButtonEventArgs e)
        {
            Console.WriteLine("DoubleClicked on Row");
        }
    }

    #region XAML Display Converters
    /// <summary>
    /// Converts Byte Values into their simplest unit
    /// Source: https://thomasfreudenberg.com/archive/2017/01/21/presenting-byte-size-values-in-wpf/
    /// </summary>
    public class FormatSizeConverter : IValueConverter
    {
        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        private static extern long StrFormatByteSizeW(long qdw, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszBuf,
            int cchBuf);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
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
    #endregion

}
