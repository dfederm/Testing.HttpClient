// <copyright file="RequestExpectationTests.cs" company="David Federman">
// Copyright (c) David Federman. All rights reserved.
// </copyright>

namespace Testing.HttpClient.UnitTests
{
    using System;
    using System.Net.Http;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public sealed class RequestExpectationTests
    {
        private HttpMethod method = HttpMethod.Get;
        private Uri uri = new Uri("https://www.foo.com");

        [TestMethod]
        public void Constructor()
        {
            // Validation
            Assert.ThrowsException<ArgumentNullException>(() => new RequestExpectation(null, this.uri));
            Assert.ThrowsException<ArgumentNullException>(() => new RequestExpectation(this.method, null));

            // Public members
            var expectation = new RequestExpectation(this.method, this.uri);
            Assert.AreEqual(this.method, expectation.HttpMethod);
            Assert.AreEqual(this.uri, expectation.Uri);
        }

        [TestMethod]
        public void Stringification()
        {
            Assert.AreEqual("GET https://www.foo.com/", new RequestExpectation(HttpMethod.Get, new Uri("https://www.foo.com")).ToString());
            Assert.AreEqual("POST https://www.bar.com/", new RequestExpectation(HttpMethod.Post, new Uri("https://www.bar.com")).ToString());
            Assert.AreEqual("DELETE https://www.baz.com/", new RequestExpectation(HttpMethod.Delete, new Uri("https://www.baz.com")).ToString());
        }
    }
}
