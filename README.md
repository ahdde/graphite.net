## Synopsis

Client library for submitting data to and querying from [graphite](https://github.com/graphite-project/graphite-web)

## Code Example
### Submit metrics
```csharp
var client = new GraphiteClient("example.com");

var datapoints = = new[]
    {
        new Datapoint("data.server1.cpuUsage", 10, DateTime.Now),
        new Datapoint("data.server2.cpuUsage", 15, DateTime.Now)),
        new Datapoint("data.server3.cpuUsage", 20, DateTime.Now)),
    };

await client.SendAsync(datapoints);
```

### Query metrics
```csharp
//dynamically build targets - 'data.*.server[0-9].{cpu,ram,disk}.Usage'
var path = new GraphitePath("data")
    .WildcardPath()
    .Path("server").Range('0', '9')
    .ValuePath("cpu", "ram", "disk").Path("Usage");

//invoke functions on targets - alias(sum(data.*.server[0-9].{cpu,ram,disk}.Usage),"usage")
var function = path.Sum().Alias("usage");

//retrieve data from graphite
var client = new GraphiteClient("example.com");
var data = await client.GetMetricsDataAsync(function, "-1d")
```

For details take a look at the [tests](https://github.com/ahdde/graphite.net/blob/master/Graphite.Test/GraphitePathTest.cs)

## Motivation

All existing libraries we found supported only string parameters for querying graphite. To fix this we wrote our own statically typed client.

This client also supports sending batch data via the python pickle protocol (in addition to the plaintext protocol) (many thanks to [Pyrolite](https://github.com/irmen/Pyrolite)).

## Installation

Install via [nuget](https://www.nuget.org/packages/ahd.Graphite) (coming soon).

## Known Issues

Please be aware that this library might include functions which are not yet stable or miss functions from the latest stable release. To work around unsupported features you can always call .ToString() and used the inappropriately named function `GraphitePath.Parse()`.

## Contributing

We are happily accepting pull requests for any missing feature or enhancement. If anything is unclear, do not hesitate to open an [issue](https://github.com/ahdde/graphite.net/issues/new).

## License

Licensed under [MIT License](https://github.com/ahdde/graphite.net/blob/master/LICENSE.md)
