// <copyright file="RequestExpectation.cs" company="David Federman">
// Copyright (c) David Federman. All rights reserved.
// </copyright>

namespace Testing.HttpClient
{
    using System;
    using System.Net.Http;

    internal sealed class RequestExpectation
    {
        public RequestExpectation(HttpMethod method, Uri uri)
        {
            this.HttpMethod = method ?? throw new ArgumentNullException(nameof(method));
            this.Uri = uri ?? throw new ArgumentNullException(nameof(uri));
        }

        public HttpMethod HttpMethod { get; }

        public Uri Uri { get; }

        public override string ToString() => $"{this.HttpMethod} {this.Uri}";
    }
}
