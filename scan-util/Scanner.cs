using System;
using System.IO;
using System.Collections.Generic;

// DriveInfo di = new DriveInfo(path);

namespace Scanner
{
    public static class DirectoryScanner
    {
        public static void FindFiles(string path, Action<File> callback)   
        {
            foreach (File file in GetFiles(path)) {
                callback(file);
            }
        }   

        private static IEnumerable<File> GetFiles(string path)
        {
            Queue<string> queue = new Queue<string>();
            queue.Enqueue(path);
            while (queue.Count > 0) 
            {
                long totalSize = 0;
                path = queue.Dequeue();

                try
                {
                    foreach (string subDir in Directory.GetDirectories(path)) 
                    {
                        queue.Enqueue(subDir);
                    }
                } catch (Exception) {}

                string[] files = null;
                try 
                {
                    files = Directory.GetFiles(path);
                } catch (Exception) {}

                if (files != null) 
                {
                    foreach (string file in files)
                    {
                        long size = new FileInfo(file).Length;
                        totalSize += size;
                        yield return new File(size, file, false);
                    }
                }

                yield return new File(totalSize, path, true);
            }
        }
    }

    public struct File
    {
        public long size;      
        public string path;
        public bool isFolder;
        public File(long s, string p, bool f)
        {
            size = s;
            path = p;
            isFolder = f;
        }

        public override String ToString() {
            return $"{this.path} is {this.size} bytes ({(this.isFolder ? "folder" : "file")})";
        }
    }
}