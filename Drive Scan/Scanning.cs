using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace Drive_Scan
{
    namespace Scanning
    {
        public class DirectoryScanner
        {
            /// <summary> Recursively get the files in a folder and call the callback function with each file or folder </summary>
            /// <param name="path">The path of the folder to start in</param>
            /// <param name="callback">The callback function for each file/folder, returns the file object with isFirstFile and isRoot</param>
            /// <param name="filterDirectory">Filter options for directories</param>
            /// <param name="filterFile">Filter options for files</param>
            /// <example><code>
            /// DirectoryScanner.FindFiles(@"C:\Program Files\", file => Console.WriteLine(file));
            /// </code></example>
            public static void FindFiles(string path, Action<File, bool, bool> callback, string filterFile = "?.*", string filterDirectory = "*")
            {
                foreach (File file in GetFiles(path))
                {
                   // Console.WriteLine(file);
                    callback(file, file.isFirstFile, file.isRoot);
                }
            }

            /// <summary> Returns an IEnumerable of a File object for each file in a specified directory (is recursive) </summary>
            /// <param name="path">The path of the folder to start in</param>
            static IEnumerable<File> GetFiles(string inPath)
            {
                // Read the config file for whether we show hidden files or not
                bool showHiddenFiles = Convert.ToBoolean(Convert.ToInt16(Config.ConfigHandler.readValue("hidden")));

                // Calculate the file enumeration options based on whether we are showing hidden files
                EnumerationOptions eopts = new EnumerationOptions();
                eopts.AttributesToSkip = (!showHiddenFiles ? FileAttributes.Hidden : 0) | FileAttributes.System;
                eopts.IgnoreInaccessible = true;
                eopts.RecurseSubdirectories = true;

                // Return the root directory with a size of 0
                yield return new File(0, inPath, true, true, true);

                // Recursively enumerate through all the files and directories in the target path
                foreach (string file in Directory.EnumerateFileSystemEntries(inPath, "*", eopts))
                {
                    // Get the file attributes of the found file
                    FileAttributes attr = System.IO.File.GetAttributes(file);

                    // Bitwise math to quickly determine if the given path is a directory
                    if ((attr & FileAttributes.Directory) == FileAttributes.Directory) {
                        /* Is a directory */
                        DirectoryInfo info = new DirectoryInfo(file);
                        // Calculate the size of the directory by doing a sum of all the child files
                        long size = info.EnumerateFiles("*", eopts).Sum(file => file.Length);
                        yield return new File(size, file, true);
                    } else {
                        /* Is not a directory (is a file) */
                        System.IO.FileInfo info = new System.IO.FileInfo(file);
                        yield return new File(info.Length, file, false);
                    }
                }

                // Return the size of the entire drive
                yield return new File((new DirectoryInfo(inPath)).EnumerateFiles("*", eopts).Sum(file => file.Length), inPath, true, true, true);
            }
        }

        public struct File
        {
            public long size;
            public string path;
            public bool isFolder;
            public bool isFirstFile;
            public bool isRoot;

            /// <param name="_size">The size of the file/folder</param>
            /// <param name="_path">The path of the file/folder</param>
            /// <param name="_isFolder">Whether or not it is a folder</param>
            /// <param name="_firstFile">Whether or not this is the first file</param>
            /// <param name="_isRoot">If it is a root folder</param>
            public File(long _size, string _path, bool _isFolder, bool _firstFile = false, bool _isRoot = false)
            {
                size = _size;
                path = _path;
                isFolder = _isFolder;
                isFirstFile = _firstFile;
                isRoot = _isRoot;
            }

            public override String ToString()
            {
                return $"\"{this.path}\" is {this.size} bytes ({(this.isFolder ? "folder" : "file")}, {(this.isFirstFile ? "first file" : "not first file")}, {(this.isRoot ? "root" : "not root")})";
            }
        }
    }
}