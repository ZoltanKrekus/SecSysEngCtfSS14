using System;
using System.Diagnostics;
using System.Threading;
using SpaceshipAI.UI.Backend;

namespace SpaceshipAI
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var serverThread = new Thread(() =>
            {
                var server = new Server(4545);
                server.Start();
            });
            serverThread.Start();

            Thread.Sleep(500);
            if (args.Length > 0 && args[0] == "daemon")
                return;
            var backend = new MainScreen();
            backend.Start();
            Process.GetCurrentProcess().Kill();
        }
    }
}