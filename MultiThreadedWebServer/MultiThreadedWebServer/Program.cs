using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace MultiThreadedWebServer
{
    class Program
    {
        static readonly string RootFolder = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..");
        static void Main()
        {
            if (!Directory.Exists(RootFolder))
                Directory.CreateDirectory(RootFolder);

            ThreadPool.QueueUserWorkItem(_ => Server.StartWebServer());

            Console.WriteLine("Web server started.");
            Console.ReadKey();
		}
    }
}
