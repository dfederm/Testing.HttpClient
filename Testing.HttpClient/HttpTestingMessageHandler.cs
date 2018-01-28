// <copyright file="HttpTestingMessageHandler.cs" company="David Federman">
// Copyright (c) David Federman. All rights reserved.
// </copyright>

namespace Testing.HttpClient
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    internal sealed class HttpTestingMessageHandler : HttpMessageHandler
    {
        // We need to manage state across multiple collections, so thread-safe colletions won't be enough thread-safety.
        private readonly object lockObj = new object();

        private readonly Dictionary<RequestExpectation, Queue<TestRequest>> outstandingRequests;

        private readonly Dictionary<RequestExpectation, Queue<AutoResetEvent>> outstandingExpectations;

        private readonly HttpClientTestingFactorySettings settings;

        public HttpTestingMessageHandler(HttpClientTestingFactorySettings settings)
        {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));

            var comparer = new RequestExpectationEqualityComparer(this.settings);
            this.outstandingRequests = new Dictionary<RequestExpectation, Queue<TestRequest>>(comparer);
            this.outstandingExpectations = new Dictionary<RequestExpectation, Queue<AutoResetEvent>>(comparer);
        }

        public TestRequest Expect(RequestExpectation expectation)
        {
            AutoResetEvent autoResetEvent = null;
            try
            {
                lock (this.lockObj)
                {
                    // If the request was already made
                    if (this.TryGetMatchingItem(this.outstandingRequests, expectation, out var request))
                    {
                        return request;
                    }

                    // The request hasn't been made yet, so create a wait handle to signal when the request does come in
                    autoResetEvent = new AutoResetEvent(false);
                    if (!this.outstandingExpectations.TryGetValue(expectation, out var matchingExpectations))
                    {
                        matchingExpectations = new Queue<AutoResetEvent>();
                        this.outstandingExpectations.Add(expectation, matchingExpectations);
                    }

                    matchingExpectations.Enqueue(autoResetEvent);
                }

                // Wait for the request to come in for some timeout period.
                if (!autoResetEvent.WaitOne(this.settings.ExpectationMatchTimeout))
                {
                    lock (this.lockObj)
                    {
                        throw new InvalidOperationException($"Expected request was not matched: [{expectation}]. Outstanding requests: [{this.GetOutstandingRequestsDebugString()}]");
                    }
                }

                // It was signalled that the request was made, so try to get the matching request again. It should be there.
                lock (this.lockObj)
                {
                    if (this.TryGetMatchingItem(this.outstandingRequests, expectation, out var request))
                    {
                        return request;
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            $"A request matching an outstanding expectation was made, but the request could not be found. This is likely a bug. Please report this to the developer. " +
                            $"Expectation: [{expectation}]; " +
                            $"Outstanding expectations: [{this.GetOutstandingExpectationsDebugString()}]; " +
                            $"Outstanding requests: [{this.GetOutstandingRequestsDebugString()}]; ");
                    }
                }
            }
            finally
            {
                autoResetEvent?.Dispose();
            }
        }

        public void EnsureNoOutstandingRequests()
        {
            string outstandingRequestString;
            lock (this.lockObj)
            {
                outstandingRequestString = this.GetOutstandingRequestsDebugString();
            }

            if (outstandingRequestString.Length > 0)
            {
                throw new InvalidOperationException($"There are still outstanding requests: [{outstandingRequestString}]");
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
            timeoutCancellationSource.Token.Register(() => taskCompletionSource.TrySetException(new TimeoutException(string.Format(TimeoutErrorMessageFormat, expectation))), useSynchronizationContext: false);

            // Add to the outstanding requests
            var testRequest = new TestRequest(request, taskCompletionSource);
            lock (this.lockObj)
            {
                if (!this.outstandingRequests.TryGetValue(expectation, out var matchingRequests))
                {
                    matchingRequests = new Queue<TestRequest>();
                    this.outstandingRequests.Add(expectation, matchingRequests);
                }

                matchingRequests.Enqueue(testRequest);

                // Signal a waiting expectation, if one exists
                if (this.TryGetMatchingItem(this.outstandingExpectations, expectation, out var autoResetEvent))
                {
                    autoResetEvent.Set();
                }
            }

            return task;
        }

        // Only call when holding the lock
        private bool TryGetMatchingItem<T>(Dictionary<RequestExpectation, Queue<T>> items, RequestExpectation expectation, out T item)
        {
            if (items.TryGetValue(expectation, out var matchingItems) && matchingItems.Count > 0)
            {
                item = matchingItems.Dequeue();
                return true;
            }
            else
            {
                item = default(T);
                return false;
            }
        }

        // Only call when holding the lock
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

        // Only call when holding the lock
        private string GetOutstandingExpectationsDebugString()
        {
            var sb = new StringBuilder();

            var isFirst = true;
            foreach (var pair in this.outstandingExpectations)
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
