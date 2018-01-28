// <copyright file="RequestExpectationEqualityComparerTests.cs" company="David Federman">
// Copyright (c) David Federman. All rights reserved.
// </copyright>

namespace Testing.HttpClient.UnitTests
{
    using System;
    using System.Net.Http;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public sealed class RequestExpectationEqualityComparerTests
    {
        [TestMethod]
        public void Constructor() => Assert.ThrowsException<ArgumentNullException>(() => new RequestExpectationEqualityComparer(null));

        [TestMethod]
        public void HashCodeIgnoringUriCasing()
        {
            var comparer = new RequestExpectationEqualityComparer(new HttpClientTestingFactorySettings { IgnoreUriCasing = true });

            // Because GetHashCode may result in different values across different platforms or even processes
            // we can't actually do much validation here beyond asserting general contracts.
            var expectation = new RequestExpectation(HttpMethod.Get, new Uri("https://www.foo.com"));

            // Consistency
            Assert.AreEqual(comparer.GetHashCode(expectation), comparer.GetHashCode(expectation));

            // Equivalence
            var expectationCopy = new RequestExpectation(HttpMethod.Get, new Uri("https://www.foo.com"));
            Assert.AreEqual(comparer.GetHashCode(expectation), comparer.GetHashCode(expectationCopy));

            // Case-insensitivity
            var expectationCaseDifferent = new RequestExpectation(HttpMethod.Get, new Uri("https://www.FOO.com"));
            Assert.AreEqual(comparer.GetHashCode(expectation), comparer.GetHashCode(expectationCopy));

            // Non-colliding
            var otherExpectation = new RequestExpectation(HttpMethod.Post, new Uri("https://www.bar.com"));
            Assert.AreNotEqual(comparer.GetHashCode(expectation), comparer.GetHashCode(otherExpectation));
        }

        [TestMethod]
        public void HashCodeStrictUriCasing()
        {
            var comparer = new RequestExpectationEqualityComparer(new HttpClientTestingFactorySettings { IgnoreUriCasing = false });

            // Because GetHashCode may result in different values across different platforms or even processes
            // we can't actually do much validation here beyond asserting general contracts.
            var expectation = new RequestExpectation(HttpMethod.Get, new Uri("https://www.foo.com/bar"));

            // Consistency
            Assert.AreEqual(comparer.GetHashCode(expectation), comparer.GetHashCode(expectation));

            // Equivalence
            Assert.AreEqual(comparer.GetHashCode(expectation), comparer.GetHashCode(new RequestExpectation(HttpMethod.Get, new Uri("https://www.foo.com/bar"))));

            // Case-sensitivity
            Assert.AreNotEqual(comparer.GetHashCode(expectation), comparer.GetHashCode(new RequestExpectation(HttpMethod.Get, new Uri("https://www.foo.com/BAR"))));

            // Non-colliding
            Assert.AreNotEqual(comparer.GetHashCode(expectation), comparer.GetHashCode(new RequestExpectation(HttpMethod.Post, new Uri("https://www.baz.com/bar"))));
        }

        [TestMethod]
        public void EqualsIgnoringUriCasing()
        {
            var comparer = new RequestExpectationEqualityComparer(new HttpClientTestingFactorySettings { IgnoreUriCasing = true });
            var expectation = new RequestExpectation(HttpMethod.Get, new Uri("https://www.foo.com"));

            Assert.IsTrue(comparer.Equals(null, null));
            Assert.IsFalse(comparer.Equals(expectation, null));
            Assert.IsFalse(comparer.Equals(null, expectation));
            Assert.IsTrue(comparer.Equals(expectation, expectation));
            Assert.IsTrue(comparer.Equals(expectation, new RequestExpectation(HttpMethod.Get, new Uri("https://www.foo.com"))));
            Assert.IsTrue(comparer.Equals(expectation, new RequestExpectation(HttpMethod.Get, new Uri("https://www.FOO.com"))));
            Assert.IsFalse(comparer.Equals(expectation, new RequestExpectation(HttpMethod.Get, new Uri("https://www.bar.com"))));
            Assert.IsFalse(comparer.Equals(expectation, new RequestExpectation(HttpMethod.Post, new Uri("https://www.foo.com"))));
        }

        [TestMethod]
        public void EqualsStrictUriCasing()
        {
            var comparer = new RequestExpectationEqualityComparer(new HttpClientTestingFactorySettings { IgnoreUriCasing = false });
            var expectation = new RequestExpectation(HttpMethod.Get, new Uri("https://www.foo.com/bar"));

            Assert.IsTrue(comparer.Equals(null, null));
            Assert.IsFalse(comparer.Equals(expectation, null));
            Assert.IsFalse(comparer.Equals(null, expectation));
            Assert.IsTrue(comparer.Equals(expectation, expectation));
            Assert.IsTrue(comparer.Equals(expectation, new RequestExpectation(HttpMethod.Get, new Uri("https://www.foo.com/bar"))));
            Assert.IsFalse(comparer.Equals(expectation, new RequestExpectation(HttpMethod.Get, new Uri("https://www.foo.com/BAR"))));
            Assert.IsFalse(comparer.Equals(expectation, new RequestExpectation(HttpMethod.Get, new Uri("https://www.bar.com/bar"))));
            Assert.IsFalse(comparer.Equals(expectation, new RequestExpectation(HttpMethod.Post, new Uri("https://www.foo.com/bar"))));
        }
    }
}
