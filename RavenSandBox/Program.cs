using System;
using System.Threading;
using HttpServerCore;
using NLog;
using StatisticServer.Modules;
using StatisticServer.Storage;

namespace RavenSandBox
{
    class Program
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static void Main(string[] args)
        {
            logger.Info("Application started");
            try
            {
                RunCli();
            }
            catch (Exception exception)
            {
                logger.Fatal(exception, "Application crashed");
            }
        }

        private static void RunCli()
        {
            
        }
    }
}
