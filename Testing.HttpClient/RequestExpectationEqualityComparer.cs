// <copyright file="RequestExpectationEqualityComparer.cs" company="David Federman">
// Copyright (c) David Federman. All rights reserved.
// </copyright>

namespace Testing.HttpClient
{
    using System;
    using System.Collections.Generic;

    internal sealed class RequestExpectationEqualityComparer : IEqualityComparer<RequestExpectation>
    {
        private readonly HttpClientTestingFactorySettings settings;

        public RequestExpectationEqualityComparer(HttpClientTestingFactorySettings settings)
        {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        private StringComparer UriStringComparer => this.settings.IgnoreUriCasing ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;

        public bool Equals(RequestExpectation x, RequestExpectation y)
        {
            if (x == y)
            {
                return true;
            }

            if (x == null || y == null)
            {
                return false;
            }

            return x.HttpMethod == y.HttpMethod
                && this.UriStringComparer.Equals(x.Uri.AbsoluteUri, y.Uri.AbsoluteUri);
        }

        public int GetHashCode(RequestExpectation obj) => (obj.HttpMethod.GetHashCode() * 27) ^ this.UriStringComparer.GetHashCode(obj.Uri.AbsoluteUri);
    }
}
