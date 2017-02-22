using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using RestSharp;

namespace HttpServerCore.Tests
{
    [TestFixture]
    public class HttpServerCoreTests
    {
        private string prefix = "http://localhost:12345/unit_test/";
        private RestClient client;
        private IHttpServer server;
        private IServerModule defaultModule;

        private static IServerModule CreateFakeModule(Func<IRequest, Task<IRequest>> moduleFunction)
        {
            var module = A.Fake<IServerModule>();
            A.CallTo(() => module.ProcessRequest(A<IRequest>._)).ReturnsLazily(moduleFunction);
            return module;
        }

        private static IServerModule CreateFakeModule()
        {
            return CreateFakeModule(Task.FromResult);
        }
        
        [SetUp]
        public void SetUp()
        {
            defaultModule = CreateFakeModule();
        }

        public void Prepare(params IServerModule[] modules)
        {
            client = new RestClient(prefix);
            server = new HttpServer(
                new HttpServerOptions {Prefix = prefix},
                modules);
        }

        [TearDown]
        public void TearDown()
        {
            server.Dispose();
        }

        [Test]
        public void ServerIsSilent_WhenNotStarted()
        {
            Prepare(defaultModule);

            client.Execute(new RestRequest("/", Method.GET));

            A.CallTo(() => defaultModule.ProcessRequest(A<IRequest>._)).MustNotHaveHappened();
        }

        [Test]
        public void ServerWorks_WhenStarted()
        {
            Prepare(defaultModule);
            server.Start();

            client.Execute(new RestRequest("/", Method.GET));

            A.CallTo(() => defaultModule.ProcessRequest(A<IRequest>._)).MustHaveHappened();
        }

        [Test]
        public void ServerSilent_WhenStopped()
        {
            Prepare(defaultModule);
            server.Start();
            server.Stop();

            client.Execute(new RestRequest("/", Method.GET));

            A.CallTo(() => defaultModule.ProcessRequest(A<IRequest>._)).MustNotHaveHappened();
        }

        [Test]
        public void ServerCallsModules_InOrderTheyWerePresented()
        {
            var callOrder = new List<int>();
            var firstFake = CreateFakeModule(request => { callOrder.Add(1); return Task.FromResult(request); });
            var secondFake = CreateFakeModule(request => { callOrder.Add(2); return Task.FromResult(request); });
            Prepare(firstFake, secondFake);
            server.Start();

            client.Execute(new RestRequest("/", Method.GET));

            callOrder.ShouldBeEquivalentTo(new[] {1, 2}, options => options.WithStrictOrdering());
        }

        [Test]
        public void ServerCanRegiserModule()
        {
            var registredFake = CreateFakeModule();
            Prepare(defaultModule);
            server.RegisterModule(registredFake);
            server.Start();

            client.Execute(new RestRequest("/", Method.GET));

            A.CallTo(() => registredFake.ProcessRequest(A<IRequest>._)).MustHaveHappened();
        }
    }
}
