using System;
using System.Collections.Generic;
using System.IO;

namespace Drive_Scan
{
    namespace Scanning
    {
        public static class DirectoryScanner
        {
            //ScaninProgress
            private static bool _scanInProgress;

            //Public Getter
            public static bool scanInProgress { get { return _scanInProgress; } }

            /// <summary> Recursively get the files in a folder and call the callback function with each file or folder </summary>
            /// <param name="path">The path of the folder to start in</param>
            /// <param name="callback">The callback function for each file/folder</param>
            /// <example><code>
            /// DirectoryScanner.FindFiles(@"K:\Coding\GitHub\", file => Console.WriteLine(file));
            /// </code></example>
            public static void FindFiles(string path, Action<File> callback)   
            {
                _scanInProgress = true;
                foreach (File file in GetFiles(path)) 
                {
                    callback(file);
                }
                _scanInProgress = false;
            }   

            /// <summary> Returns an IEnumerable of a File object for each file in a specified directory (is recursive) </summary>
            /// <param name="path">The path of the folder to start in</param>
            private static IEnumerable<File> GetFiles(string path)
            {
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
                        foreach (string subDir in Directory.GetDirectories(path)) 
                        {
                            queue.Enqueue(subDir);
                        }
                    } catch (Exception) {}

                    // Get all files in folder (this will not get directories)
                    string[] files = null;
                    try 
                    {
                        files = Directory.GetFiles(path);
                    } catch (Exception) {}

                    // If any files were found
                    if (files != null) 
                    {
                        foreach (string file in files)
                        {
                            // Check file size and yield 
                            long size = new System.IO.FileInfo(file).Length;
                            // Add file size to total directory size
                            totalSize += size;
                            // Yield file object
                            yield return new File(size, file, false);
                        }
                    }

                    // After all files, yield directory with total directory size
                    yield return new File(totalSize, path, true);
                }
            }
        }

        public struct File
        {
            public long size;      
            public string path;
            public bool isFolder;

            /// <param name="s">The size of the file/folder</param>
            /// <param name="p">The path of the file/folder</param>
            /// <param name="f">Whether or not it is a folder</param>
            public File(long s, string p, bool f)
            {
                size = s;
                path = p;
                isFolder = f;
            }

            public override String ToString() {
                return $"\"{this.path}\" is {this.size} bytes ({(this.isFolder ? "folder" : "file")})";
            }
        }
    }
}