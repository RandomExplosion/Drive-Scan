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
                yield return new File(0, inPath, true, true, true);
                long totalSize = 0;
                bool firstFile = true;

                Queue<string> queue = new Queue<string>();
                queue.Enqueue(inPath);
                while (queue.Count > 0)
                {
                    string path = queue.Dequeue();
                    try
                    {
                        foreach (string subDir in Directory.GetDirectories(path))
                        {
                            queue.Enqueue(subDir);
                        }
                    } catch (UnauthorizedAccessException) { }

                    bool isFirstFile = firstFile;
                    if (firstFile) firstFile = false;

                    long folderSize = 0;
                    string[] files = null;
                    try
                    {
                        files = Directory.GetFiles(path);
                    } catch (UnauthorizedAccessException) { }

                    yield return new File(0, path, true);
                    if (files != null)
                    {
                        for (int i = 0; i < files.Length; i++)
                        {
                            System.IO.FileInfo file = new System.IO.FileInfo(files[i]);
                            folderSize += file.Length;
                            if (!file.Attributes.HasFlag(FileAttributes.Hidden))
                            {
                                yield return new File(file.Length, files[i], false, isFirstFile);
                            }
                        }
                    }

                    totalSize += folderSize;
                    yield return new File(folderSize, path, true, isFirstFile, path == inPath);
                }
                
                yield return new File(totalSize, inPath, true, true, true);
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