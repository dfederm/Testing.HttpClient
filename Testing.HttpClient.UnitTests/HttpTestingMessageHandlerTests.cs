// <copyright file="HttpTestingMessageHandlerTests.cs" company="David Federman">
// Copyright (c) David Federman. All rights reserved.
// </copyright>

namespace Testing.HttpClient.UnitTests
{
    using System;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public sealed class HttpTestingMessageHandlerTests : IDisposable
    {
        private HttpClientTestingFactorySettings settings;
        private HttpTestingMessageHandler handler;

        [TestInitialize]
        public void TestInitialize()
        {
            this.settings = new HttpClientTestingFactorySettings();
            this.handler = new HttpTestingMessageHandler(this.settings);
        }

        public void Dispose() => this.handler.Dispose();

        [TestMethod]
        public void Constructor() => Assert.ThrowsException<ArgumentNullException>(() => new HttpTestingMessageHandler(null));

        [TestMethod]
        public void ExpectThrowsOnUnmatchingRequest() => Assert.ThrowsException<InvalidOperationException>(() => this.handler.Expect(new RequestExpectation(HttpMethod.Get, new Uri("https://www.foo.com"))));

        [TestMethod]
        public void ExpectThrowsOnPreviouslyMatchedRequest()
        {
            var uri = new Uri("https://www.foo.com");
            using (var httpClient = new HttpClient(this.handler))
            {
                var getTask = httpClient.GetAsync(uri);

                this.handler.Expect(new RequestExpectation(HttpMethod.Get, uri));
                Assert.ThrowsException<InvalidOperationException>(() => this.handler.Expect(new RequestExpectation(HttpMethod.Get, uri)));

                Assert.IsFalse(getTask.IsCompleted);
            }
        }

        [TestMethod]
        public void CancelledRequestsPropagateToResponseTask()
        {
            var uri = new Uri("https://www.foo.com");
            using (var httpClient = new HttpClient(this.handler))
            {
                var cancellationTokenSource = new CancellationTokenSource();
                var getTask = httpClient.GetAsync(uri, cancellationTokenSource.Token);
                cancellationTokenSource.Cancel();

                Assert.IsTrue(getTask.IsCompleted);
                Assert.IsTrue(getTask.IsCanceled);

                // Clear out the pending request
                this.handler.Expect(new RequestExpectation(HttpMethod.Get, uri));
            }
        }

        [TestMethod]
        public async Task RequestsTimeout()
        {
            var uri = new Uri("https://www.foo.com");
            using (var httpClient = new HttpClient(this.handler))
            {
                // Shorten the timeout to make the test faster
                this.settings.RequestTimeout = TimeSpan.FromMilliseconds(100);

                TimeoutException timeoutException = null;
                try
                {
                    // Awaiting the task which will never finish until it's expected
                    await httpClient.GetAsync(uri);
                }
                catch (TimeoutException e)
                {
                    timeoutException = e;
                }
                finally
                {
                    // Clear out the pending request
                    this.handler.Expect(new RequestExpectation(HttpMethod.Get, uri));
                }

                Assert.IsNotNull(timeoutException);
            }
        }

        [TestMethod]
        public void EnsureNoOutstandingRequests()
        {
            var uri = new Uri("https://www.foo.com");
            using (var httpClient = new HttpClient(this.handler))
            {
                httpClient.GetAsync(uri);

                Assert.ThrowsException<InvalidOperationException>(() => this.handler.EnsureNoOutstandingRequests());
                this.handler.Expect(new RequestExpectation(HttpMethod.Get, uri));
                this.handler.EnsureNoOutstandingRequests();
            }
        }
    }
}
