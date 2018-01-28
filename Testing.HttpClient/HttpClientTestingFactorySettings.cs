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
        /// Gets or sets the timeout after which to throw. Defaults to 1 second.
        /// </summary>
        /// <remarks>
        /// This is primarily in place to avoid awaiting http calls which are not properly expected. Set this value higher
        /// if there may be longer delays between a request being made and the unit test being able to provide an expectation.
        /// </remarks>
        public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(1);

        /// <summary>
        /// Gets or sets the timeout for which to wait for a request to come matching an expectation. Defaults to 100 milliseconds.
        /// </summary>
        /// <remarks>
        /// This is primarily in place to support sequential or chained http calls where the unit test may not be able to know whether
        /// a later request has been made after an earlier one finishes. This is set to a fairly low value to avoid long delays in the
        /// unmatched expectation case. Set this value higher if there may be longer delays between two sequential requests.
        /// </remarks>
        public TimeSpan ExpectationMatchTimeout { get; set; } = TimeSpan.FromMilliseconds(100);
    }
}
