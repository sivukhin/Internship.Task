using System;
using Internship.Modules;
using Internship.Storage;

namespace Internship
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var storage = new SimpleStorage();
            using (var server = new StatisticsServer(
                new ServerOptions {Prefix = "http://127.0.0.1:12345/"},
                new IServerModule[]
                {
                    new HelloWorldModule(),
                    new UpdateStatisticModule(storage), 
                }))
            {
                server.StartListen();
                Console.ReadKey(true);
            }
        }
    }
}