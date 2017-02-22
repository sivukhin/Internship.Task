using System;
using HttpServerCore;
using StatisticServer.Modules;
using StatisticServer.Storage;

namespace StatisticServer
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var storage = new SimpleStorage();
            using (var server = new HttpServer(
                new HttpServerOptions {Prefix = "http://localhost:12345/"},
                new IServerModule[] { new UpdateStatisticModule(storage), new GetStatisticModule(storage)}))
            {
                server.Start();
                Console.ReadKey(true);
            }
        }
    }
}