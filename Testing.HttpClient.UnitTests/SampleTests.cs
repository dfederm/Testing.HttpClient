// <copyright file="SampleTests.cs" company="David Federman">
// Copyright (c) David Federman. All rights reserved.
// </copyright>

namespace Testing.HttpClient.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    // These tests are intended to be samples for how to use this library.
    [TestClass]
    public sealed class SampleTests
    {
        [TestMethod]
        public async Task FetchDataTest()
        {
            using (var http = new HttpClientTestingFactory())
            {
                var worker = new Worker(http.HttpClient);

                // Make the call, but don't await the task
                var resultTask = worker.FetchDataAsync();

                // Expect the request and respond to it
                var request = http.Expect("http://some-website.com/some-path");
                request.Respond(HttpStatusCode.OK, "123");

                // Await the result and assert on it
                var result = await resultTask;
                Assert.AreEqual(123, result);

                http.EnsureNoOutstandingRequests();
            }
        }

        [TestMethod]
        public async Task PostDataTest()
        {
            using (var http = new HttpClientTestingFactory())
            {
                var worker = new Worker(http.HttpClient);

                // Make the call, but don't await the task
                var resultTask = worker.PostDataAsync();

                // Expect the request, validate it, and respond to it
                var request = http.Expect(HttpMethod.Post, "http://some-website.com/some-path");
                Assert.AreEqual("some data", await request.Request.Content.ReadAsStringAsync());
                request.Respond(HttpStatusCode.OK);

                // Let the call finish
                await resultTask;

                http.EnsureNoOutstandingRequests();
            }
        }

        [TestMethod]
        public async Task FetchParallelDataTest()
        {
            using (var http = new HttpClientTestingFactory())
            {
                var worker = new Worker(http.HttpClient);

                // Make the call, but don't await the task
                var resultTask = worker.FetchParallelDataAsync();

                // Expect the requests and respond to them
                http.Expect("http://some-website.com/1").Respond("1");
                http.Expect("http://some-website.com/2").Respond("2");
                http.Expect("http://some-website.com/3").Respond("3");

                // Await the result and assert on it
                var result = await resultTask;
                Assert.AreEqual(6, result);

                http.EnsureNoOutstandingRequests();
            }
        }

        [TestMethod]
        public async Task FetchSequentialDataAsync()
        {
            using (var http = new HttpClientTestingFactory())
            {
                var worker = new Worker(http.HttpClient);

                // Make the call, but don't await the task
                var resultTask = worker.FetchSequentialDataAsync();

                // Expect the requests and respond to them
                http.Expect("http://some-website.com/items").Respond("1,2,3");
                http.Expect("http://some-website.com/items/3/subItems").Respond("4,5,6");
                http.Expect("http://some-website.com/items/3/subItems/6").Respond("item 3 subitem 6 data");

                // Await the result and assert on it
                var result = await resultTask;
                Assert.AreEqual("item 3 subitem 6 data", result);

                http.EnsureNoOutstandingRequests();
            }
        }

        // Example of a class being tested by this library
        private sealed class Worker
        {
            private readonly HttpClient httpClient;

            public Worker(HttpClient httpClient)
            {
                this.httpClient = httpClient;
                this.httpClient.BaseAddress = new Uri("http://some-website.com");
            }

            public async Task<int> FetchDataAsync()
            {
                using (var response = await this.httpClient.GetAsync("/some-path"))
                {
                    response.EnsureSuccessStatusCode();

                    var responseString = await response.Content.ReadAsStringAsync();
                    return int.TryParse(responseString, out var result) ? result : 0;
                }
            }

            public async Task PostDataAsync()
            {
                using (var response = await this.httpClient.PostAsync("/some-path", new StringContent("some data")))
                {
                    response.EnsureSuccessStatusCode();
                }
            }

            public async Task<int> FetchParallelDataAsync()
            {
                // Make some parallel calls
                var responses = await Task.WhenAll(
                    this.httpClient.GetAsync("/1"),
                    this.httpClient.GetAsync("/2"),
                    this.httpClient.GetAsync("/3"));

                // Aggregate the results
                var sum = 0;
                foreach (var response in responses)
                {
                    using (response)
                    {
                        response.EnsureSuccessStatusCode();

                        var responseString = await response.Content.ReadAsStringAsync();
                        sum += int.TryParse(responseString, out var result) ? result : 0;
                    }
                }

                return sum;
            }

            public async Task<string> FetchSequentialDataAsync()
            {
                // Make the first request
                IEnumerable<int> items;
                using (var itemsResponse = await this.httpClient.GetAsync("/items"))
                {
                    itemsResponse.EnsureSuccessStatusCode();
                    var itemsString = await itemsResponse.Content.ReadAsStringAsync();
                    items = itemsString
                        .Split(",")
                        .Select(str => int.TryParse(str, out var i) ? i : 0);
                }

                // Process the first result. Pretend it may do some async work and take a few cycles
                await Task.Delay(10);
                var newestItem = items.Max();

                // Make a second request which depends on the result of the first
                IEnumerable<int> subItems;
                using (var subItemsResponse = await this.httpClient.GetAsync($"/items/{newestItem}/subItems"))
                {
                    subItemsResponse.EnsureSuccessStatusCode();
                    var subItemsString = await subItemsResponse.Content.ReadAsStringAsync();
                    subItems = subItemsString
                        .Split(",")
                        .Select(str => int.TryParse(str, out var i) ? i : 0);
                }

                // Process the second result. Pretend it may do some async work and take a few cycles
                await Task.Delay(10);
                var newestSubItem = subItems.Max();

                // Make a thrid request which depends on the result of the first and second
                using (var subItemDetailResponse = await this.httpClient.GetAsync($"/items/{newestItem}/subItems/{newestSubItem}"))
                {
                    subItemDetailResponse.EnsureSuccessStatusCode();
                    return await subItemDetailResponse.Content.ReadAsStringAsync();
                }
            }
        }
    }
}
