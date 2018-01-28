// <copyright file="TestRequestTests.cs" company="David Federman">
// Copyright (c) David Federman. All rights reserved.
// </copyright>

namespace Testing.HttpClient.UnitTests
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Testing.HttpClient;

    [TestClass]
    public sealed class TestRequestTests : IDisposable
    {
        private HttpRequestMessage requestMessage = new HttpRequestMessage();
        private TaskCompletionSource<HttpResponseMessage> taskCompletionSource = new TaskCompletionSource<HttpResponseMessage>();

        public void Dispose() => this.requestMessage.Dispose();

        [TestMethod]
        public void Constructor()
        {
            // Validation
            Assert.ThrowsException<ArgumentNullException>(() => new TestRequest(null, this.taskCompletionSource));
            Assert.ThrowsException<ArgumentNullException>(() => new TestRequest(this.requestMessage, null));

            // Public members
            var testRequest = new TestRequest(this.requestMessage, this.taskCompletionSource);
            Assert.AreEqual(this.requestMessage, testRequest.Request);
        }

        [TestMethod]
        public void Respond()
        {
            var response = this.ValidateAndGetResponse(testRequest => testRequest.Respond());
            Assert.AreEqual(TestRequest.DefaultStatusCode, response.StatusCode);
            Assert.IsNull(response.Content);
        }

        [TestMethod]
        public void RespondWithIntStatusCode()
        {
            const int StatusCode = 400;
            var response = this.ValidateAndGetResponse(testRequest => testRequest.Respond(StatusCode));
            Assert.AreEqual((HttpStatusCode)StatusCode, response.StatusCode);
            Assert.IsNull(response.Content);
        }

        [TestMethod]
        public void RespondWithHttpStatusCode()
        {
            const HttpStatusCode StatusCode = HttpStatusCode.InternalServerError;
            var response = this.ValidateAndGetResponse(testRequest => testRequest.Respond(StatusCode));
            Assert.AreEqual(StatusCode, response.StatusCode);
            Assert.IsNull(response.Content);
        }

        [TestMethod]
        public async Task RespondWithStreamBody()
        {
            const string BodyString = "Test String";
            var stream = new MemoryStream(Encoding.Default.GetBytes(BodyString));
            var response = this.ValidateAndGetResponse(testRequest => testRequest.Respond(stream));

            Assert.AreEqual(TestRequest.DefaultStatusCode, response.StatusCode);
            Assert.IsNotNull(response.Content);
            Assert.AreEqual(BodyString, await response.Content.ReadAsStringAsync());
        }

        [TestMethod]
        public async Task RespondWithStringBody()
        {
            const string BodyString = "Test String";
            var response = this.ValidateAndGetResponse(testRequest => testRequest.Respond(BodyString));

            Assert.AreEqual(TestRequest.DefaultStatusCode, response.StatusCode);
            Assert.IsNotNull(response.Content);
            Assert.AreEqual(BodyString, await response.Content.ReadAsStringAsync());
        }

        [TestMethod]
        public async Task RespondWithHttpStatusCodeAndStreamBody()
        {
            const HttpStatusCode StatusCode = HttpStatusCode.Created;
            const string BodyString = "Test String";
            var stream = new MemoryStream(Encoding.Default.GetBytes(BodyString));
            var response = this.ValidateAndGetResponse(testRequest => testRequest.Respond(StatusCode, stream));

            Assert.AreEqual(StatusCode, response.StatusCode);
            Assert.IsNotNull(response.Content);
            Assert.AreEqual(BodyString, await response.Content.ReadAsStringAsync());
        }

        [TestMethod]
        public async Task RespondWithHttpStatusCodeAndStringBody()
        {
            const HttpStatusCode StatusCode = HttpStatusCode.NoContent;
            const string BodyString = "Test String";
            var response = this.ValidateAndGetResponse(testRequest => testRequest.Respond(StatusCode, BodyString));

            Assert.AreEqual(StatusCode, response.StatusCode);
            Assert.IsNotNull(response.Content);
            Assert.AreEqual(BodyString, await response.Content.ReadAsStringAsync());
        }

        [TestMethod]
        public void RespondWithHttpResponseMessage()
        {
            var responseMessage = new HttpResponseMessage();
            var response = this.ValidateAndGetResponse(testRequest => testRequest.Respond(responseMessage));

            Assert.AreEqual(responseMessage, response);
        }

        [TestMethod]
        public void RespondThrowsWhenCalledMultipleTimes()
        {
            var testRequest = new TestRequest(this.requestMessage, this.taskCompletionSource);
            testRequest.Respond();
            Assert.ThrowsException<InvalidOperationException>(() => testRequest.Respond());
            Assert.ThrowsException<InvalidOperationException>(() => testRequest.Respond());
        }

        [TestMethod]
        public void RespondThrowsWhenRequestIsCancelled()
        {
            var testRequest = new TestRequest(this.requestMessage, this.taskCompletionSource);
            this.taskCompletionSource.SetCanceled();
            Assert.ThrowsException<InvalidOperationException>(() => testRequest.Respond());
        }

        [TestMethod]
        public void RespondThrowsWhenRequestIsAlreadyFaulted()
        {
            var testRequest = new TestRequest(this.requestMessage, this.taskCompletionSource);
            this.taskCompletionSource.SetException(new Exception());
            Assert.ThrowsException<InvalidOperationException>(() => testRequest.Respond());
        }

        [TestMethod]
        public void RespondThrowsWhenRequestIsAlreadyCompleted()
        {
            var testRequest = new TestRequest(this.requestMessage, this.taskCompletionSource);
            this.taskCompletionSource.SetResult(new HttpResponseMessage());
            Assert.ThrowsException<InvalidOperationException>(() => testRequest.Respond());
        }

        private HttpResponseMessage ValidateAndGetResponse(Action<TestRequest> respond)
        {
            var testRequest = new TestRequest(this.requestMessage, this.taskCompletionSource);
            var task = this.taskCompletionSource.Task;

            Assert.IsFalse(task.IsCompleted);
            respond(testRequest);
            Assert.IsTrue(task.IsCompleted);

            return task.Result;
        }
    }
}
