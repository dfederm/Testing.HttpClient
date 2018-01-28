# Testing.HttpClient
This is a library to help simplify testing the .NET `HttpClient`. It is heavily influenced by other open source http testing libraries, most notably angular's http/testing.

# Usage

```cs
[TestMethod]
public async Task ExampleTest()
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
    }
}

```

More code [samples](Testing.HttpClient.UnitTests/SampleTests.cs).
