// <copyright file="HttpTestingMessageHandler.cs" company="David Federman">
// Copyright (c) David Federman. All rights reserved.
// </copyright>

namespace Testing.HttpClient
{
    using System;
    using System.Collections.Concurrent;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    internal sealed class HttpTestingMessageHandler : HttpMessageHandler
    {
        private readonly ConcurrentDictionary<RequestExpectation, ConcurrentBag<TestRequest>> outstandingRequests;

        private readonly HttpClientTestingFactorySettings settings;

        public HttpTestingMessageHandler(HttpClientTestingFactorySettings settings)
        {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));

            var comparer = new RequestExpectationEqualityComparer(this.settings);
            this.outstandingRequests = new ConcurrentDictionary<RequestExpectation, ConcurrentBag<TestRequest>>(comparer);
        }

        public TestRequest Expect(RequestExpectation expectation)
        {
            if (!this.outstandingRequests.TryGetValue(expectation, out var matchingRequests)
                || !matchingRequests.TryTake(out var request))
            {
                throw new InvalidOperationException($"Expected request was not matched: [{expectation}]. Outstanding requests: [{this.GetOutstandingRequestsDebugString()}]");
            }

            return request;
        }

        public void EnsureNoOutstandingRequests()
        {
            var outstandingRequestString = this.GetOutstandingRequestsDebugString();
            if (outstandingRequestString.Length > 0)
            {
                throw new InvalidOperationException($"There are still outstanding requests: [{this.GetOutstandingRequestsDebugString()}]");
            }
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var expectation = new RequestExpectation(request.Method, request.RequestUri);
            var taskCompletionSource = new TaskCompletionSource<HttpResponseMessage>();
            var task = taskCompletionSource.Task;

            // Propagate request cancellation
            cancellationToken.Register(() => taskCompletionSource.TrySetCanceled(), useSynchronizationContext: false);

            // Add timeout to protect against awaiting on this task without a matching expect
            const string TimeoutErrorMessageFormat = "The mock request [{0}] timed out. This either means that you are awaiting this http request task without " +
                "a matching expectation or your code is taking longer than the provided HttpClientTestingFactorySettings.RequestTimeout.";
            var timeoutCancellationSource = new CancellationTokenSource(this.settings.RequestTimeout);
            timeoutCancellationSource.Token.Register(
                () =>
                {
                    taskCompletionSource.TrySetException(new TimeoutException(string.Format(TimeoutErrorMessageFormat, expectation)));
                },
                useSynchronizationContext: false);

            var testRequest = new TestRequest(request, taskCompletionSource);
            this.outstandingRequests.GetOrAdd(expectation, key => new ConcurrentBag<TestRequest>()).Add(testRequest);
            return task;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            // Generally Dispose methods should not throw, but this is the best place to automatically perform this validation.
            if (this.settings.ThrowOnOutstandingRequests)
            {
                this.EnsureNoOutstandingRequests();
            }
        }

        private string GetOutstandingRequestsDebugString()
        {
            var sb = new StringBuilder();

            var isFirst = true;
            foreach (var pair in this.outstandingRequests)
            {
                var num = pair.Value.Count;
                for (var i = 0; i < num; i++)
                {
                    if (!isFirst)
                    {
                        sb.Append(", ");
                    }

                    sb.Append("[");
                    sb.Append(pair.Key.ToString());
                    sb.Append("]");

                    isFirst = false;
                }
            }

            return sb.ToString();
        }
    }
}
