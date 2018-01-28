// <copyright file="TestRequest.cs" company="David Federman">
// Copyright (c) David Federman. All rights reserved.
// </copyright>

namespace Testing.HttpClient
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;

    /// <summary>
    /// An object that encapsulates a matched request, as well as exposes various methods to respond to the request.
    /// </summary>
    public sealed class TestRequest
    {
        internal const HttpStatusCode DefaultStatusCode = HttpStatusCode.OK;

        private readonly TaskCompletionSource<HttpResponseMessage> taskCompletionSource;

        private bool hasResponded;

        // For usage clarity, only allow our library to create these
        internal TestRequest(HttpRequestMessage requestMessage, TaskCompletionSource<HttpResponseMessage> taskCompletionSource)
        {
            this.Request = requestMessage ?? throw new ArgumentNullException(nameof(requestMessage));
            this.taskCompletionSource = taskCompletionSource ?? throw new ArgumentNullException(nameof(taskCompletionSource));
        }

        /// <summary>
        /// Gets the matched request.
        /// </summary>
        public HttpRequestMessage Request { get; }

        /// <summary>
        /// Responds to the request with an empty OK response.
        /// </summary>
        public void Respond() => this.Respond(DefaultStatusCode);

        /// <summary>
        /// Responds to the request with the provided status code.
        /// </summary>
        /// <param name="statusCode">Response status code</param>
        public void Respond(int statusCode) => this.Respond((HttpStatusCode)statusCode);

        /// <summary>
        /// Responds to the request with the provided status code.
        /// </summary>
        /// <param name="statusCode">Response status code</param>
        public void Respond(HttpStatusCode statusCode) => this.Respond(new HttpResponseMessage(statusCode));

        /// <summary>
        /// Responds to the request with the provided response body.
        /// </summary>
        /// <param name="body">The response boddy as a stream</param>
        public void Respond(Stream body) => this.Respond(DefaultStatusCode, body);

        /// <summary>
        /// Responds to the request with the provided response body.
        /// </summary>
        /// <param name="body">The response boddy as a string</param>
        public void Respond(string body) => this.Respond(DefaultStatusCode, body);

        /// <summary>
        /// Responds to the request with the provided status code and response body.
        /// </summary>
        /// <param name="statusCode">Response status code</param>
        /// <param name="body">The response boddy as a stream</param>
        public void Respond(HttpStatusCode statusCode, Stream body) => this.Respond(new HttpResponseMessage(statusCode) { Content = new StreamContent(body) });

        /// <summary>
        /// Responds to the request with the provided status code and response body.
        /// </summary>
        /// <param name="statusCode">Response status code</param>
        /// <param name="body">The response boddy as a string</param>
        public void Respond(HttpStatusCode statusCode, string body) => this.Respond(new HttpResponseMessage(statusCode) { Content = new StringContent(body) });

        /// <summary>
        /// Responds to the request with the provided status code.
        /// </summary>
        /// <param name="response">Response message</param>
        public void Respond(HttpResponseMessage response)
        {
            if (this.hasResponded)
            {
                throw new InvalidOperationException("This request has already been responded to.");
            }

            if (this.taskCompletionSource.Task.IsCanceled)
            {
                throw new InvalidOperationException("This request was cancelled and cannot be responded to.");
            }

            if (this.taskCompletionSource.Task.IsFaulted)
            {
                throw new InvalidOperationException("This task associated with this request is unexpectedly faulted.", this.taskCompletionSource.Task.Exception);
            }

            if (this.taskCompletionSource.Task.IsCompleted)
            {
                throw new InvalidOperationException("This task associated with this request is unexpectedly already complete.");
            }

            this.taskCompletionSource.SetResult(response);
            this.hasResponded = true;
        }
    }
}
