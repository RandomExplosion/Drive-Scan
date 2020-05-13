using System;
using Scanner;

namespace scan_util
{
    class Program
    {
        static void Main(string[] args)
        {
            DirectoryScanner.FindFiles("W:\\", path => {
                Console.WriteLine(path);
            });
        }
    }
}
