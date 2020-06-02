using System;
using System.IO;
using System.Collections.Generic;

namespace Drive_Scan
{
    namespace Scanning
    {
        public class DirectoryScanner 
        {

            //ScaninProgress
            private static bool _scanInProgress;

            //Public Getter
            public static bool scanInProgress { get { return _scanInProgress; } }

            /// <summary> Recursively get the files in a folder and call the callback function with each file or folder </summary>
            /// <param name="path">The path of the folder to start in</param>
            /// <param name="callback">The callback function for each file/folder, returns the file object with isFirstFile and isRoot</param>
            /// <param name="filterDirectory">Filter options for directories</param>
            /// <param name="filterFile">Filter options for files</param>
            /// <example><code>
            /// DirectoryScanner.FindFiles(@"C:\Program Files\", file => Console.WriteLine(file));
            /// </code></example>
            public static async void FindFiles(string path, Action<File, bool, bool> callback, string filterFile = "?.*", string filterDirectory = "*")   
            {
                if (_scanInProgress)
                {
                    throw new System.InvalidOperationException("Another process is already running a scan!");
                }

                _scanInProgress = true;
                await foreach (File file in GetFiles(path, filterFile, filterDirectory)) 
                {
                    callback(file, file.isFirstFile, file.isRoot);
                }
                _scanInProgress = false;
            }   

            /// <summary> Returns an IEnumerable of a File object for each file in a specified directory (is recursive) </summary>
            /// <param name="path">The path of the folder to start in</param>
            private static async IAsyncEnumerable<File> GetFiles(string path, string filterFile, string filterDirectory)
            {
                yield return new File(0, path, true, true, true);

                // If it is the first file found
                bool firstFile = true;

                // Create directory queue
                Queue<string> queue = new Queue<string>();
                // Add starting folder to queue
                queue.Enqueue(path);

                // While there are folders in the queue
                while (queue.Count > 0) 
                {
                    // Create temp variable for storing the total folder size
                    long totalSize = 0;

                    // Remove folder from queue and get path
                    path = queue.Dequeue();

                    // Try catch incase the script runs into a permission error and can't read the file
                    try
                    {
                        // For every subfolder, add path to queue
                        foreach (string subDir in Directory.EnumerateDirectories(path/*, filterDirectory */)) 
                        {
                            queue.Enqueue(subDir);
                        }
                    } catch (Exception) {}

                    bool isFirstFile = firstFile;
                    if (firstFile) firstFile = false;

                    // Get all files in folder (this will not get directories)
                    foreach (string file in Directory.EnumerateFiles(path/*, filterFile*/))
                    {
                        // Check file size and yield 
                        long size = new System.IO.FileInfo(file).Length;
                        // Add file size to total directory size
                        totalSize += size;
                        
                        //Debugging
                        Console.WriteLine(new File(size, file, false, false, isFirstFile));

                        // Yield file object
                        yield return new File(size, file, false, false, isFirstFile);
                    }

                    // If not default filter or greater than one, will prevent empty folders from returning when a file filter is set
                    if (filterFile == "?.*" || totalSize > 0)
                    {
                        Console.WriteLine(new File(totalSize, path, true, path.Length == 3 && path.EndsWith(@":\"), isFirstFile));
                        // After all files, yield directory with total directory size
                        yield return new File(totalSize, path, true, path.Length == 3 && path.EndsWith(@":\"), isFirstFile);
                    }
                }
            }
        }

        public struct File
        {
            public long size;      
            public string path;
            public bool isFolder;
            public bool isFirstFile;
            public bool isRoot;

            /// <param name="s">The size of the file/folder</param>
            /// <param name="p">The path of the file/folder</param>
            /// <param name="f">Whether or not it is a folder</param>
            /// <param name="r">Whether or not this is the first file</param>
            /// <param name="ff">If it is a root folder</param>
            public File(long s, string p, bool f, bool r = false, bool ff = false)
            {
                size = s;
                path = p;
                isFolder = f;
                isRoot = r;
                isFirstFile = ff;
            }

            public override String ToString() 
            {
                return $"\"{this.path}\" is {this.size} bytes ({(this.isFolder ? "folder" : "file")}, {(this.isFirstFile ? "first file" : "not first file")}, {(this.isRoot ? "root" : "not root")})";
            }
        }
    }
}