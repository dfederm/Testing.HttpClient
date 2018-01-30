# Testing.HttpClient

[![NuGet](https://img.shields.io/nuget/v/Testing.HttpClient.svg)](https://www.nuget.org/packages/Testing.HttpClient)
[![NuGet](https://img.shields.io/nuget/dt/Testing.HttpClient.svg)](https://www.nuget.org/packages/Testing.HttpClient)

## Overview
This is a library to help simplify testing the .NET `HttpClient`. It is heavily influenced by other open source http testing libraries, most notably angular's http/testing.

The library ensures that the `HttpClient` never hits the network and allows your tests to assert expectations on the requests made through it, validate the requests, and provide mock respones.

## Getting started
To use Testing.HttpClient:

1. Ensure your existing unit test project is targeting a framework that supports [`netstandard2.0`](https://github.com/dotnet/standard/blob/master/docs/versions.md) (eg. .NET Core 2.0+ or .NET Framework 4.6.1+)
2. Update your unit test's csproj to reference the [`Testing.HttpClient`](https://www.nuget.org/packages/Testing.HttpClient) package:
```xml
<PackageReference Include="Testing.HttpClient" Version="1.0.0" />
```
3. Use the `HttpClientTestingFactory` class in your tests to create and configure an `HttpClient` your code under test can consume.

## Example
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
