// <copyright file="HttpClientTestingFactoryTests.cs" company="David Federman">
// Copyright (c) David Federman. All rights reserved.
// </copyright>

namespace Testing.HttpClient.UnitTests
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public sealed class HttpClientTestingFactoryTests : IDisposable
    {
        private readonly HttpClientTestingFactory factory = new HttpClientTestingFactory();

        public void Dispose() => this.factory.Dispose();

        [TestMethod]
        public void ExpectThrowsOnInvalidArguments()
        {
            Assert.ThrowsException<ArgumentNullException>(() => this.factory.Expect((string)null));
            Assert.ThrowsException<ArgumentNullException>(() => this.factory.Expect((Uri)null));
            Assert.ThrowsException<ArgumentNullException>(() => this.factory.Expect(null, new Uri("https://www.foo.com")));
            Assert.ThrowsException<ArgumentException>(() => this.factory.Expect("not a uri"));
            Assert.ThrowsException<ArgumentException>(() => this.factory.Expect(new Uri("/foo", UriKind.Relative)));
        }

        [TestMethod]
        public void ExpectThrowsWhenDisposed()
        {
            this.factory.Dispose();
            Assert.ThrowsException<ObjectDisposedException>(() => this.factory.Expect(new Uri("https://www.foo.com")));
        }
    }
}
