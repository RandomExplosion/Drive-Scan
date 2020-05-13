using System;

namespace Scanner
{
    public static class DirectoryScanner
    {
        public static void FindFiles(string path, Action<File> callback)   
        {
            File test = new File(53252, path, false);
            callback(test);
        }     
    }

    public struct File
    {
        public ulong size;      // kilobytes 
        public string path;
        public bool isFolder;
        public File(ulong s, string p, bool f)
        {
            size = s;
            path = p;
            isFolder = f;
        }

        public override String ToString() {
            return $"{this.path} {this.size} kilobytes ({(this.isFolder ? "folder" : "file")})";
        }
    }
}