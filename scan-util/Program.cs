﻿using System;
using Scanner;

namespace scan_util
{
    class Program
    {
        static void Main(string[] args)
        {
            DirectoryScanner.FindFiles(@"K:\Coding\GitHub\Drive-Scan\scan-util\test\", file => {
                Console.WriteLine(file);
            });
        }
    }
}
