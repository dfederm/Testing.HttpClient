// <copyright file="HttpClientTestingFactorySettings.cs" company="David Federman">
// Copyright (c) David Federman. All rights reserved.
// </copyright>

namespace Testing.HttpClient
{
    using System;

    /// <summary>
    /// Settings that control the behavior of the <see cref="HttpClientTestingFactory"/>
    /// </summary>
    public sealed class HttpClientTestingFactorySettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether to ignore casing when matching Uris. Uris are technically case-sensitive, but in practice they are used as such. Defaults to true.
        /// </summary>
        public bool IgnoreUriCasing { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to automatically call <see cref="HttpClientTestingFactory.EnsureNoOutstandingRequests"/> when the <see cref="HttpClientTestingFactory"/> is disposed, which will throw and exception if are still outstanding requests. Defaults to true.
        /// </summary>
        public bool ThrowOnOutstandingRequests { get; set; } = true;

        /// <summary>
        /// Gets or sets the timeout after which to throw. Defaults to 10 seconds.
        /// </summary>
        /// <remarks>
        /// This is primarily in place to avoid awaiting http calls which are not properly expected.
        /// </remarks>
        public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(10);
    }
}
