﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Azure.Health.DataServices.Pipelines;
using Azure.Health.DataServices.Channels;
using Azure.Health.DataServices.Filters;
using Azure.Health.DataServices.Tests.Assets;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Serilog;

namespace Azure.Health.DataServices.Tests.Proxy
{
    [TestClass]
    public class PipelineTests
    {
        private static readonly string logPath = "../../pipelinelog.txt";
        private static Microsoft.Extensions.Logging.ILogger<WebPipeline> logger;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            if (File.Exists(logPath))
            {
                File.Delete(logPath);
            }

            var slog = new LoggerConfiguration()
            .WriteTo.File(
            logPath,
            shared: true,
            flushToDiskInterval: TimeSpan.FromMilliseconds(10000))
            .MinimumLevel.Debug()
            .CreateLogger();

            Microsoft.Extensions.Logging.ILoggerFactory factory = LoggerFactory.Create(log =>
            {
                log.SetMinimumLevel(LogLevel.Information);
                log.AddConsole();
                log.AddSerilog(slog);
            });

            logger = factory.CreateLogger<WebPipeline>();
            factory.Dispose();
            Console.WriteLine(context.TestName);
        }

        [TestMethod]
        public async Task WebPipeline_Simple_Test()
        {
            string requestUriString = "http://example.org/path";
            IInputFilterCollection filters = new InputFilterCollection();
            IInputChannelCollection channels = new InputChannelCollection();
            filters.Add(new FakeFilter());
            channels.Add(new FakeChannel());

            IPipeline<HttpRequestMessage, HttpResponseMessage> pipeline = new WebPipeline(filters, channels);

            bool complete = false;
            pipeline.OnComplete += (a, args) =>
            {
                complete = true;
            };

            HttpRequestMessage request = new(HttpMethod.Get, requestUriString);
            HttpResponseMessage output = await pipeline.ExecuteAsync(request);
            Assert.IsTrue(complete, "Pipeline not signal complete.");
        }

        [TestMethod]
        public async Task WebPipeline_SimpleWithMultipleFilters_Test()
        {
            string requestUriString = "http://example.org/path";
            IInputFilterCollection filters = new InputFilterCollection();
            IInputChannelCollection channels = new InputChannelCollection();
            filters.Add(new FakeFilter());
            filters.Add(new FakeFilter());

            IPipeline<HttpRequestMessage, HttpResponseMessage> pipeline = new WebPipeline(filters, channels);

            bool complete = false;
            pipeline.OnComplete += (a, args) =>
            {
                complete = true;
            };

            HttpRequestMessage request = new(HttpMethod.Get, requestUriString);
            HttpResponseMessage output = await pipeline.ExecuteAsync(request);
            Assert.IsTrue(complete, "Pipeline not signal complete.");
        }

        [TestMethod]
        public async Task WebPipeline_SimpleWithMultipleAndFaultCaseNotExecuted_Filters_Test()
        {
            string requestUriString = "http://example.org/path";
            IInputFilterCollection filters = new InputFilterCollection();
            IInputChannelCollection channels = new InputChannelCollection();
            IFilter filter1 = new FakeFilter();
            IFilter filter2 = new FakeFilter(StatusType.Fault);

            filters.Add(filter1);
            filters.Add(filter2);

            IPipeline<HttpRequestMessage, HttpResponseMessage> pipeline = new WebPipeline(filters, channels, null, null, null, null, logger);

            bool complete = false;
            pipeline.OnComplete += (a, args) =>
            {
                complete = true;
            };

            HttpRequestMessage request = new(HttpMethod.Get, requestUriString);
            HttpResponseMessage output = await pipeline.ExecuteAsync(request);
            string copyPath = $"../../copypipelinelog.txt";
            File.Copy(logPath, copyPath, true);
            using StreamReader reader = new(copyPath);

            bool anyStatus = false;
            bool faultStatus = false;
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                anyStatus = anyStatus || line.Contains("executed with status Any");
                faultStatus = faultStatus || line.Contains("not executed due to status Fault");
            }
            Assert.IsTrue(complete, "Pipeline not signal complete.");
            Assert.IsTrue(anyStatus, "Any status not found.");
            Assert.IsTrue(faultStatus, "Fault status not found.");
        }

        [TestMethod]
        public async Task WebPipeline_SimpleWitNormalAndFaultCaseNotExecuted_Filters_Test()
        {
            string requestUriString = "http://example.org/path";
            IInputFilterCollection filters = new InputFilterCollection();
            IInputChannelCollection channels = new InputChannelCollection();
            IFilter filter1 = new FakeFilter(StatusType.Fault);
            IFilter filter2 = new FakeFilter(StatusType.Normal);

            filters.Add(filter1);
            filters.Add(filter2);

            IPipeline<HttpRequestMessage, HttpResponseMessage> pipeline = new WebPipeline(filters, channels, null, null, null, null, logger);

            bool complete = false;
            pipeline.OnComplete += (a, args) =>
            {
                complete = true;
            };

            HttpRequestMessage request = new(HttpMethod.Get, requestUriString);
            HttpResponseMessage output = await pipeline.ExecuteAsync(request);
            string copyPath = $"../../copypipelinelog.txt";
            File.Copy(logPath, copyPath, true);
            using StreamReader reader = new(copyPath);

            bool anyStatus = false;
            bool faultStatus = false;
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                anyStatus = anyStatus || line.Contains("executed with status Normal");
                faultStatus = faultStatus || line.Contains("not executed due to status Fault");
            }
            Assert.IsTrue(complete, "Pipeline not signal complete.");
            Assert.IsTrue(anyStatus, "Any status not found.");
            Assert.IsTrue(faultStatus, "Fault status not found.");
        }

        [TestMethod]
        public async Task WebPipeline_WithFilterError_Test()
        {
            string name = "ErrorFilter";
            bool fatal = true;
            string errorMessage = "Boom!";
            Exception error = new(errorMessage);
            HttpStatusCode code = HttpStatusCode.InternalServerError;
            string body = "stuff";
            var filter = new FakeFilterWithError(name, fatal, error, code, body);

            string requestUriString = "http://example.org/path";
            IInputFilterCollection filters = new InputFilterCollection
            {
                filter
            };
            IPipeline<HttpRequestMessage, HttpResponseMessage> pipeline = new WebPipeline(filters, null, null, null, null, null, null);
            bool complete = false;
            pipeline.OnError += (a, args) =>
            {
                Assert.AreEqual(errorMessage, args.Error.Message, "Error mismatch.");
                complete = true;
            };

            HttpRequestMessage request = new(HttpMethod.Get, requestUriString);
            _ = await pipeline.ExecuteAsync(request);
            Assert.IsTrue(complete, "Error was expected");
        }

        [TestMethod]
        public async Task WebPipeline_WithChannelErrorIgnored_Test()
        {
            string errorMessage = "Boom!";
            Exception error = new(errorMessage);
            var channel = new FakeChannelWithError(error);

            string requestUriString = "http://example.org/path";
            IInputChannelCollection channels = new InputChannelCollection
            {
                channel
            };
            IPipeline<HttpRequestMessage, HttpResponseMessage> pipeline = new WebPipeline(null, channels);
            bool trigger = false;
            pipeline.OnError += (a, args) =>
            {
                Assert.AreEqual(errorMessage, args.Error.Message, "Error mismatch.");
                trigger = true;
            };

            HttpRequestMessage request = new(HttpMethod.Get, requestUriString);
            _ = await pipeline.ExecuteAsync(request);
            Assert.IsTrue(trigger, "Error was not expected");
        }

        [TestMethod]
        public async Task WebPipeline_WithChannelErrorOmitted_Test()
        {
            string errorMessage = "Boom!";
            Exception error = new(errorMessage);
            var channel = new FakeChannelWithError(error);

            string requestUriString = "http://example.org/path";
            IInputChannelCollection channels = new InputChannelCollection
            {
                channel
            };
            IPipeline<HttpRequestMessage, HttpResponseMessage> pipeline = new WebPipeline(null, channels);
            bool complete = false;
            pipeline.OnComplete += (a, args) =>
            {
                complete = true;
            };
            pipeline.OnError += (a, args) =>
            {
                Assert.Fail("Unexpected error.");
            };

            HttpRequestMessage request = new(HttpMethod.Get, requestUriString);
            _ = await pipeline.ExecuteAsync(request);
            Assert.IsTrue(complete, "Fail to complete.");
        }





        [TestMethod]
        public async Task WebPipeline_NoContent_Test()
        {
            HttpMethod method = HttpMethod.Get;
            string requestUriString = "http://example.org/test";
            HttpRequestMessage request = new(method, requestUriString);
            IInputFilterCollection filters = new InputFilterCollection();
            IInputChannelCollection channels = new InputChannelCollection();
            filters.Add(new FakeFilter());
            channels.Add(new FakeChannel());

            IPipeline<HttpRequestMessage, HttpResponseMessage> pipeline = new WebPipeline(filters, channels);

            bool complete = false;
            pipeline.OnComplete += (a, args) =>
            {
                complete = true;
            };
            ;
            HttpResponseMessage response = await pipeline.ExecuteAsync(request);
            Assert.IsTrue(complete, "Pipeline not signal complete.");
            Assert.IsNotNull(response, "Response is null.");
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "Http status code mismatch.");
        }

        [TestMethod]
        public async Task WebPipeline_WithContent_Test()
        {
            string content = "{ \"property\": \"value\" }";
            HttpMethod method = HttpMethod.Get;
            string requestUriString = "http://example.org/test";
            HttpRequestMessage request = new(method, requestUriString);
            IInputFilterCollection filters = new InputFilterCollection();
            IInputChannelCollection channels = new InputChannelCollection();
            filters.Add(new FakeFilter());
            filters.Add(new FakeFilterWithContent());
            channels.Add(new FakeChannel());

            IPipeline<HttpRequestMessage, HttpResponseMessage> pipeline = new WebPipeline(filters, channels);

            bool complete = false;
            pipeline.OnComplete += (a, args) =>
            {
                complete = true;
            };

            HttpResponseMessage response = await pipeline.ExecuteAsync(request);
            Assert.IsNotNull(response, "Response is null.");
            Assert.IsTrue(complete, "Pipeline not complete.");
            string actualContent = await response.Content.ReadAsStringAsync();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "Http status code mismatch.");
            Assert.AreEqual(content, actualContent, "Content mismatch.");
        }

        [TestMethod]
        public async Task WebPipeline_ForcedError_Test()
        {
            FaultFilter filter = new();
            bool faulted = false;
            filter.OnFilterError += (a, args) =>
            {
                faulted = true;
            };
            IInputFilterCollection filters = new InputFilterCollection
            {
                filter
            };
            IPipeline<HttpRequestMessage, HttpResponseMessage> pipeline = new WebPipeline(filters);



            bool complete = false;
            pipeline.OnComplete += (a, args) =>
            {
                complete = true;
            };
            bool fault = false;
            pipeline.OnError += (a, args) =>
            {
                fault = true;
            };

            HttpMethod method = HttpMethod.Get;
            string requestUriString = "http://example.org/test";
            HttpRequestMessage request = new(method, requestUriString);
            var response = await pipeline.ExecuteAsync(request);
            Assert.IsTrue(faulted, "Not faulted.");
            Assert.IsFalse(complete, "Should not be complete.");
            Assert.IsTrue(fault, "Should have fault.");
            Assert.IsNotNull(response, "Response is null.");
            Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode, "Http status code mismatch.");



        }

        [TestMethod]
        public async Task FunctionPipeline_NoContent_Test()
        {
            string requestUriString = "http://example.org/test";
            FunctionContext funcContext = new FakeFunctionContext();
            List<KeyValuePair<string, string>> headerList = new();
            headerList.Add(new KeyValuePair<string, string>("Accept", "application/json"));
            HttpHeadersCollection headers = new();
            HttpRequestData request = new FakeHttpRequestData(funcContext, "GET", requestUriString, null, headers);
            IInputFilterCollection filters = new InputFilterCollection();
            IInputChannelCollection channels = new InputChannelCollection();
            filters.Add(new FakeFilter());
            channels.Add(new FakeChannel());

            IPipeline<HttpRequestData, HttpResponseData> pipeline = new AzureFunctionPipeline(filters, channels);

            bool complete = false;
            pipeline.OnComplete += (a, args) =>
            {
                complete = true;
            };

            HttpResponseData response = await pipeline.ExecuteAsync(request);
            Assert.IsTrue(complete, "Pipeline not signal complete.");
            Assert.IsNotNull(response, "Response is null.");
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "Http status code mismatch.");
        }

        [TestMethod]
        public async Task FunctionPipelineManager_WithContent_Test()
        {
            string content = "{ \"property\": \"value\" }";
            IInputFilterCollection filters = new InputFilterCollection();
            IInputChannelCollection channels = new InputChannelCollection();
            filters.Add(new FakeFilter());
            filters.Add(new FakeFilterWithContent());
            channels.Add(new FakeChannel());

            IPipeline<HttpRequestData, HttpResponseData> pipeline = new AzureFunctionPipeline(filters, channels);

            string requestUriString = "http://example.org/test";
            FunctionContext funcContext = new FakeFunctionContext();
            List<KeyValuePair<string, string>> headerList = new();
            headerList.Add(new KeyValuePair<string, string>("Accept", "application/json"));
            HttpHeadersCollection headers = new();
            HttpRequestData request = new FakeHttpRequestData(funcContext, "GET", requestUriString, null, headers);
            var response = await pipeline.ExecuteAsync(request);
            Assert.IsNotNull(response, "Response is null.");
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "Http status code mismatch.");
            Assert.AreEqual(content.Length, response.Body.Length, "Content length mismatch.");


        }

        [TestMethod]
        public async Task FunctionPipelineManager_ForcedError_Test()
        {
            string requestUriString = "http://example.org/test";
            FunctionContext funcContext = new FakeFunctionContext();
            List<KeyValuePair<string, string>> headerList = new();
            headerList.Add(new KeyValuePair<string, string>("Accept", "application/json"));
            HttpHeadersCollection headers = new();
            HttpRequestData request = new FakeHttpRequestData(funcContext, "GET", requestUriString, null, headers);

            IInputFilterCollection filters = new InputFilterCollection
            {
                new FaultFilter()
            };
            IPipeline<HttpRequestData, HttpResponseData> pipeline = new AzureFunctionPipeline(filters);

            bool complete = false;
            pipeline.OnComplete += (a, args) =>
            {
                complete = true;
            };
            bool fault = false;
            pipeline.OnError += (a, args) =>
            {
                fault = true;
            };

            var response = await pipeline.ExecuteAsync(request);
            Assert.IsFalse(complete, "Should not be complete.");
            Assert.IsTrue(fault, "Should have fault.");
            Assert.IsNotNull(response, "Response is null.");
            Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode, "Http status code mismatch.");
        }

    }
}
