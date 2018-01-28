// <copyright file="HttpClientTestingFactory.cs" company="David Federman">
// Copyright (c) David Federman. All rights reserved.
// </copyright>

namespace Testing.HttpClient
{
    using System;
    using System.Net.Http;

    /// <summary>
    /// Provides a mechanism for creating an <see cref="HttpClient"/> for unit testing purposes, including expecting and providing mock responses to requests.
    /// </summary>
    public sealed class HttpClientTestingFactory : IDisposable
    {
        private readonly HttpTestingMessageHandler httpMessageHandler;

        private bool isDisposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpClientTestingFactory"/> class.
        /// </summary>
        /// <param name="settings">Settings to control the behavior of this object</param>
        public HttpClientTestingFactory(HttpClientTestingFactorySettings settings = null)
        {
            this.httpMessageHandler = new HttpTestingMessageHandler(settings ?? new HttpClientTestingFactorySettings());
            this.HttpClient = new HttpClient(this.httpMessageHandler);
        }

        /// <summary>
        /// Gets the HttpClient to provide to the code being tested.
        /// </summary>
        public HttpClient HttpClient { get; }

        /// <summary>
        /// Expect a GET request to have been made which matches the given uriString and return its mock.
        /// If multiple requests match, one will be returned. Each request may only be expected once and will only be returned once.
        /// If no matching request has been made, this method will throw a <see cref="InvalidOperationException"/>.
        /// </summary>
        /// <param name="uriString">The expected uri as a string</param>
        /// <returns>A <see cref="TestRequest"/> object matching the provided values</returns>
#pragma warning disable CA2234 // Using the more convenient overload which in turn calls the correct Uri-based overload
        public TestRequest Expect(string uriString) => this.Expect(HttpMethod.Get, uriString);
#pragma warning restore CA2234

        /// <summary>
        /// Expect a GET request to have been made which matches the given Uri and return its mock.
        /// If multiple requests match, one will be returned. Each request may only be expected once and will only be returned once.
        /// If no matching request has been made, this method will throw a <see cref="InvalidOperationException"/>.
        /// </summary>
        /// <param name="uri">The expected uri</param>
        /// <returns>A <see cref="TestRequest"/> object matching the provided values</returns>
        public TestRequest Expect(Uri uri) => this.Expect(HttpMethod.Get, uri);

        /// <summary>
        /// Expect a request to have been made which matches the given method and uriString, and return its mock.
        /// If multiple requests match, one will be returned. Each request may only be expected once and will only be returned once.
        /// If no matching request has been made, this method will throw a <see cref="InvalidOperationException"/>.
        /// </summary>
        /// <param name="method">The expected http method</param>
        /// <param name="uriString">The expected uri as a string</param>
        /// <returns>A <see cref="TestRequest"/> object matching the provided values</returns>
        public TestRequest Expect(HttpMethod method, string uriString)
        {
            if (uriString == null)
            {
                throw new ArgumentNullException(nameof(uriString));
            }

            if (!Uri.TryCreate(uriString, UriKind.RelativeOrAbsolute, out var uri))
            {
                throw new ArgumentException("The provided value is not a valid Uri.", nameof(uriString));
            }

            return this.Expect(method, uri);
        }

        /// <summary>
        /// Expect a request to have been made which matches the given method and Uri, and return its mock.
        /// If multiple requests match, one will be returned. Each request may only be expected once and will only be returned once.
        /// If no matching request has been made, this method will throw a <see cref="InvalidOperationException"/>.
        /// </summary>
        /// <param name="method">The expected http method</param>
        /// <param name="uri">The expected uri</param>
        /// <returns>A <see cref="TestRequest"/> object matching the provided values</returns>
        public TestRequest Expect(HttpMethod method, Uri uri)
        {
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }

            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            if (!uri.IsAbsoluteUri)
            {
                throw new ArgumentException($"The provided Uri is not absolute.", nameof(uri));
            }

            if (this.isDisposed)
            {
                throw new ObjectDisposedException("This object has previously been disposed.");
            }

            var expectation = new RequestExpectation(method, uri);

            return this.httpMessageHandler.Expect(expectation);
        }

        /// <summary>
        /// Throws an exception if there are still outstanding requests.
        /// </summary>
        public void EnsureNoOutstandingRequests() => this.httpMessageHandler.EnsureNoOutstandingRequests();

        /// <inheritdoc/>
        public void Dispose()
        {
            if (!this.isDisposed)
            {
                this.httpMessageHandler.Dispose();
                this.HttpClient.Dispose();

                this.isDisposed = true;
            }
        }
    }
}
