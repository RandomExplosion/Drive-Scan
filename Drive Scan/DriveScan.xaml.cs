using Microsoft.Win32;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Linq;
using System;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls;
using System.IO;
using ControlzEx.Theming;
using System.Diagnostics;
using Drive_Scan.Config;
using System.Collections.Generic;
using MahApps.Metro.Controls.Dialogs;
using LiveCharts.Wpf;
using LiveCharts;
using ControlzEx.Theming;

namespace Drive_Scan
{
    /// <summary>
    /// Code Behind DriveScan.xaml
    /// </summary>
    public partial class DriveScanWindow : MetroWindow
    {

        //Singleton Pattern
        static DriveScanWindow currentWindow;

        // Create an instance of the scan loader so we can keep track of the files that we have found
        //  So that when the user goes to save/load a scan it has all the files ready to go
        static ScanLoader currentScan = new ScanLoader();

        //Viewmodel for scanned folders and paths
        public ObservableCollection<FolderInfo> scannedDrives;

        //The folder in the DirectoryTree that the user has selected (Contents are shown in top right)
        //Can also be changed from top right
        public FolderInfo selectedFolder { get; set; }

        static AsyncLocal<FolderInfo> _workingTree = new AsyncLocal<FolderInfo>();

        public Func<ChartPoint, string> PointLabel { get; set; }
        System.Windows.Data.Binding pointLabelBinding = new System.Windows.Data.Binding("LabelPoint");

        //Runs on window open
        public DriveScanWindow()
        {
            InitializeComponent();

            //Get selected theme from config handler
            ThemeSwitch(ConfigHandler.readValue("theme"));
            HiddenFiles.IsChecked = Convert.ToBoolean(Convert.ToInt16(ConfigHandler.readValue("hidden")));

            //Singleton Init
            currentWindow = this;

            //Populate Drive List
            RefreshDrives(this, null);

            //Define Label Formatting
            PointLabel = chartPoint =>
                FormatSizeConverter.Convert((long)chartPoint.Y).ToString();

            //Bind binding to labelpoint
            pointLabelBinding.Source = PointLabel;

            //Init scanned Drives list
            scannedDrives = new ObservableCollection<FolderInfo>();

            //Populate Dir Tree
            DirectoryTree.ItemsSource = scannedDrives;
        }

#region Scan File Command
        /// <summary>
        /// Start scanning the selected drive
        /// </summary>
        /// <param name="target"></param>
        /// <param name="e"></param>
        private void ScanDrive_CommandBinding_Executed(object target, ExecutedRoutedEventArgs e)
        {
            //DriveInfo info = e.Parameter as DriveInfo;
            DriveInfo info = DriveList.SelectedItem as DriveInfo;
            ScanDrive(info.Name);
        }

        /// <summary>
        /// Checks if the conditions are right to start a drive scan (has the user selected a drive)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScanDrive_CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (DriveList != null)
            {
                if (DriveList.SelectedItem != null)
                {
                    e.CanExecute = true;
                }
            }
        }
#endregion

#region Mouse Events
        /// <summary>
        /// Updates selectedFolder when the user clicks on a new folder in the DirectoryTree
        /// (So that the Folder Contents View and Pie Chart can update)
        /// This is different to the behaviour in FolderContentsView where the user must double click!
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnDirectoryTreeItemClick(object sender, RoutedEventArgs e)
        {
            if (DirectoryTree.SelectedItem != null)
            {
                //Get selected item from DirectoryTree
                FileInfo item = DirectoryTree.SelectedItem as FileInfo;

                // Make sure the file actually exists
                if (File.Exists(item.path) || Directory.Exists(item.path))
                {
                    //Get attributes for the selected item
                    FileAttributes attr = File.GetAttributes(item.path);

                    //If the selected item is a folder
                    if((attr & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        //Set the selected Folder to the selected item
                        selectedFolder = DirectoryTree.SelectedItem as FolderInfo;

                        //Update the FolderContentsView
                        FolderContentsView.ItemsSource = selectedFolder.children;

                        //Refresh Pie
                        UpdateSelectedFolderPie();
                    }
                }
            }
        }

        /// <summary>
        /// Opens FILE if the user double clicks on it in the DirectoryTree
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnDirectoryTreeItemDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DirectoryTree.SelectedItem != null)
            {
                //Get selected item from DirectoryTree
                FileInfo item = DirectoryTree.SelectedItem as FileInfo;

                // Make sure the file actually exists
                if (File.Exists(item.path) || Directory.Exists(item.path))
                {
                    //Get attributes for the selected item
                    FileAttributes attr = File.GetAttributes(item.path);

                    //If the selected item is a file
                    if((attr & FileAttributes.Directory) != FileAttributes.Directory)
                    {
                        try
                        {
                            //Open it in its associated application
                            var p = new Process();
                            p.StartInfo = new ProcessStartInfo(item.path)
                            {
                                UseShellExecute = true
                            };
                            p.Start();
                        }
                        catch (Exception)
                        {
                            //User most likely attempted to open a file without an assigned application
                            //So open it with the 'how do you want to open this file' dialog
                            var p = new Process();
                            p.StartInfo = new ProcessStartInfo(item.path)
                            {
                                WindowStyle = ProcessWindowStyle.Normal,
                                Verb = "openas",
                                UseShellExecute = true,
                                ErrorDialog = true
                            };
                            p.Start();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Updates selectedFolder when the user double clicks on a new folder in the FolderContentsView
        /// (So that the Folder Contents View can update)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnFolderContentsViewItemDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (FolderContentsView.SelectedItem != null)
            {
                //Get selected item from FolderContentsView
                FileInfo item = FolderContentsView.SelectedItem as FileInfo;

                // Make sure the file actually exists
                if (File.Exists(item.path) || Directory.Exists(item.path))
                {
                    //Get attributes for the selected item
                    FileAttributes attr = File.GetAttributes(item.path);

                    //If the selected item is a folder
                    if((attr & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        //Set the selected Folder to the selected item
                        selectedFolder = FolderContentsView.SelectedItem as FolderInfo;

                        //Update the FolderContentsView
                        FolderContentsView.ItemsSource = selectedFolder.children;

                        //Refresh Pie
                        UpdateSelectedFolderPie();
                    }

                    //This is a file
                    else
                    {
                        try
                        {
                            //Open it in its associated application
                            var p = new Process();
                            p.StartInfo = new ProcessStartInfo(item.path)
                            {
                                UseShellExecute = true
                            };
                            p.Start();
                        }
                        catch (Exception)
                        {
                            //User most likely attempted to open a file without an assigned application
                            //So open it with the 'how do you want to open this file' dialog
                            var p = new Process();
                            p.StartInfo = new ProcessStartInfo(item.path)
                            {
                                WindowStyle = ProcessWindowStyle.Normal,
                                Verb = "openas",
                                UseShellExecute = true,
                                ErrorDialog = true
                            };
                            p.Start();
                        }
                    }
                }
            }
        }

#endregion

        /// <summary>
        /// Update the drive list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void RefreshDrives(object sender, RoutedEventArgs e)
        {
            DriveList.ItemsSource = DriveInfo.GetDrives();
        }

#region Pie Chart
        /// <summary>
        /// Updates Selected Folder Pie
        /// </summary>
        public void UpdateSelectedFolderPie()
        {
            //Clear the Pie
            SelectedFolderPie.Series = new SeriesCollection();

            //Sort children and convert to list
            List<FileInfo> sortedItemInfo = selectedFolder.children.OrderBy(x => x.size).ToList();
            if (sortedItemInfo.Count > 0)
            {
                long sizeOfSlices = 0;
                //Add top 5
                for (int i = 0; i < sortedItemInfo.Count && i < 5; i++)
                {
                    SelectedFolderPie.Series.Add( new PieSeries
                    {
                        Title = sortedItemInfo[i].name,
                        Values = new ChartValues<long> {sortedItemInfo[i].size},
                        Stroke = TLBLSplitter.Background,
                        LabelPoint = PointLabel
                    });

                    //Add to size of displayed slices
                    sizeOfSlices += sortedItemInfo[i].size;
                }
                //If there are more items
                if (sortedItemInfo.Count > 5)
                {
                    //Represent them all with one slice
                    SelectedFolderPie.Series.Add( new PieSeries
                    {
                        Title = "Others",
                        Values = new ChartValues<long> {selectedFolder.size - sizeOfSlices},
                        Stroke = TLBLSplitter.Background,
                        LabelPoint = PointLabel
                    });
                }
            }
        }

        /// <summary>
        /// Updates selectedFolder when the user clicks on a slice in SelectedFolderPie
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="p"></param>
        public void OnSelectedPieDataClick(object sender, ChartPoint point)
        {
            
            try
            {
                //Get selected item from DirectoryTree
                FileInfo item = selectedFolder.children.Where(x => x.name == point.SeriesView.Title).First() as FileInfo;

                // Make sure the file actually exists
                if (File.Exists(item.path) || Directory.Exists(item.path))
                {
                    //Get attributes for the selected item
                    FileAttributes attr = File.GetAttributes(item.path);

                    //If the selected item is a folder
                    if((attr & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        //Set the selected Folder to the selected item
                        selectedFolder = selectedFolder.children.Where(x => x.name == point.SeriesView.Title).First() as FolderInfo;

                        //Update the FolderContentsView
                        FolderContentsView.ItemsSource = selectedFolder.children;

                        //Refresh Pie
                        UpdateSelectedFolderPie();
                    }
                    //If the selected item is a file
                    else if ((attr & FileAttributes.Directory) != FileAttributes.Directory)
                    {
                        try
                        {
                            //Open it in its associated application
                            var p = new Process();
                            p.StartInfo = new ProcessStartInfo(item.path)
                            {
                                UseShellExecute = true
                            };
                            p.Start();
                        }
                        catch (System.ComponentModel.Win32Exception)
                        {
                            //User most likely attempted to open a file without an assigned application
                            //So open it with the 'how do you want to open this file' dialog
                            var p = new Process();
                            p.StartInfo = new ProcessStartInfo(item.path)
                            {
                                WindowStyle = ProcessWindowStyle.Normal,
                                Verb = "openas",
                                UseShellExecute = true,
                                ErrorDialog = true
                            };
                            p.Start();
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }

#endregion

        /// <summary>
        /// Scans a drive with the given name (Obtainable from SystemIO.DriveInfo)
        /// Should only be run from the ui by double clicking on a drive
        /// </summary>
        public async void ScanDrive(string DriveName)
        {
            try
            {

                // Prompt the user for if they actually wanted to scan this drive
                MessageDialogResult res = await ((MetroWindow)(Application.Current.MainWindow)).ShowMessageAsync("Confirm", $"Are you sure you want to scan this drive? ({DriveName})", MessageDialogStyle.AffirmativeAndNegative);

                // If they say yes
                if (res == MessageDialogResult.Affirmative)
                {
                    DriveInfo drive = DriveList.SelectedItem as DriveInfo;
                    Console.WriteLine($"User is scanning drive: {drive.Name}{drive.VolumeLabel}");
                    //Show the Bar
                    ProgBar.Visibility = Visibility.Visible;
                    //Scan the drive asynchronously then add the drive tree to the TreeView
                    Task scanTask = Task.Run(() => {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            //Remove Records of this drive if there are any
                            for (int i = 0; i < scannedDrives.Count; i++)
                            {
                                if (scannedDrives[i].name == drive.Name.Split("\\")[0])
                                {
                                    scannedDrives.Remove(scannedDrives[i]);
                                }
                            }
                        });

                        Scanning.DirectoryScanner.FindFiles(drive.Name, file => {
                            // Add the file to the scan
                            currentScan.files.Add(file);
                            OnFileFound(file);
                        });

                        //Add Drive tree to ui when finished then Release Working Resources (deallocate ram from _workingTree) when scan is finished
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            scannedDrives.Add(_workingTree.Value);  //Add drive to tree
                            ProgBar.Visibility = Visibility.Hidden; //Hide Progress Bar
                        });
                    });
                }
                
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e);
                RefreshDrives(this, null);
            }
        }

        /// <summary>
        /// Callback for ScanDrive() (is run once for every file and folder on the drive (including the root of the folder))
        /// </summary>
        /// <param name="foundFile"></param>
        /// <param name="isFirstFile"></param>
        /// <param name="isRoot"></param>
        public void OnFileFound(Scanning.File foundFile)
        {
            //If this is the root folder
            if (foundFile.isFirstRoot)
            {
                //If this is the second time this drive's root has been retuned (has the final size)
                if (foundFile.size > 0)
                {
                    //Update the size
                    _workingTree.Value.size = foundFile.size;
                }
                else //The size is 0 (this is the first time we have had this folder returned)
                {

                    //Initialise the working tree
                    _workingTree.Value = new FolderInfo(foundFile.size,
                    //This part is also jank. it finds the last string in an array of strings that isn't null
                    foundFile.path.Split("\\")[foundFile.path.Split("\\").Length-2]);

                }
            }
            else if (!foundFile.isFolder)
            {
                //It's a file so add it
                _workingTree.Value.AddFileAtPath(foundFile.size, foundFile.path);
            }
            else
            {
                //It's a folder so add it or update its size if it already exists
                _workingTree.Value.UpdateFolder(foundFile.size, foundFile.path);
            }
        }

        #region Ribbon Buttons

        /// <summary>
        /// Update the hidden file config from the checkbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void UpdateHiddenFiles(object sender, RoutedEventArgs e)
        {
            ConfigHandler.updateValue("hidden", HiddenFiles.IsChecked ? "1" : "0");
        }

        /// <summary>
        /// Switch current theme to specified theme and update config
        /// </summary>
        /// <param name="Sender"></param>
        /// <param name="e"></param>
        public void ThemeSwitch(dynamic Sender, RoutedEventArgs e = null)
        {
            //Get the name of the object the operation originated from
            //                             (this will be the theme to switch to)
            string theme = Sender.GetType() != typeof(string)
                ? ((String)(Sender.GetType().GetProperty("Header")).GetValue(Sender, null)).ToLower()
                : Sender;

            if (theme == "dark")
            {
                ConfigHandler.updateValue("theme", "dark");
                ThemeManager.Current.ChangeTheme(Application.Current, "Dark.Blue", false);
            }
            else if (theme == "light")
            {
                ConfigHandler.updateValue("theme", "light");
                ThemeManager.Current.ChangeTheme(Application.Current, "Light.Taupe", false);
            }
        }

        /// <Summary>
        /// Load a scan
        /// </summary>
        public void LoadScan(object Sender, RoutedEventArgs e)
        {
            // Configure the open file dialog box
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.DefaultExt = "drsn";
            dlg.Filter = "Scan Files (.drsn)|*.drsn";

            // Show the dialog box
            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                // Show the progress bar
                ProgBar.Visibility = Visibility.Visible;

                //Load the drives from the file asynchronously then add the drive tree to the TreeView
                Task scanTask = Task.Run(() => {
                    // Load the data from the file
                    currentScan.Load(dlg.FileName);

                    Application.Current.Dispatcher.Invoke(() =>
                        {
                            //Remove Records of this drive if there are any
                            for (int i = 0; i < scannedDrives.Count; i++)
                            {
                                if (scannedDrives[i].name == currentScan.files[0].path.Split("\\")[currentScan.files[0].path.Split("\\").Length-2])
                                {
                                    scannedDrives.Remove(scannedDrives[i]);
                                }
                            }
                        });

                    // Run the onfilefound function for each of the files found from the scanner load
                    currentScan.files.ForEach(OnFileFound);

                    //Add Drive tree to ui when finished then Release Working Resources (deallocate ram from _workingTree) when scan is finished
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        scannedDrives.Add(_workingTree.Value);  //Add drive to tree
                        ProgBar.Visibility = Visibility.Hidden; //Hide Progress Bar
                    });
                });
            }
        }

        /// <Summary>
        /// Save the current scan
        /// </summary>
        public void SaveScan(object Sender, RoutedEventArgs e)
        {
            // Configure the save file dialog box
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.DefaultExt = ".drsn";
            dlg.Filter = "Scan Files (.drsn)|*.drsn";

            // Show the dialog box
            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                currentScan.Save(dlg.FileName);
            }
        }

        /// <summary>
        /// Reboot as Administrator
        /// </summary>
        /// <param name="Sender"></param>
        /// <param name="e"></param>
        public void GetAdmin(object Sender, RoutedEventArgs e)
        {
            //Get application exe location
            string path = Process.GetCurrentProcess().MainModule.FileName;

            //Setup process info to have admin
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true,
                Verb = "runas"
            };

            try
            {
                //Start new instance
                Process.Start(psi);
                //Close original instance
                this.Close();
            }
            //Catch user cancelling UAC prompt
            catch (System.ComponentModel.Win32Exception) {}
        }
        #endregion
    }

}
