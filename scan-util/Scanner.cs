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
                path = queue.Dequeue();
                foreach (string subDir in Directory.GetDirectories(path)) 
                {
                    queue.Enqueue(subDir);
                }
                string[] files = null;
                files = Directory.GetFiles(path);
                if (files != null) 
                {
                    foreach (string file in files)
                    {
                        yield return new File(new FileInfo(file).Length, file, false);
                    }
                }
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
            return $"{this.path} is {this.size} kilobytes ({(this.isFolder ? "folder" : "file")})";
        }
    }
}