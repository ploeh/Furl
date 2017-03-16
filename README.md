# Furl
Interact with HTTP resources using F# scripting

When developing REST APIs, you often need to test or verify them in various ad hoc manners. You could use cURL, but if you're more familiar with F# than Bash scripting, you can use Furl instead. This will also enable you to leverage JSON or XML **type providers** in your scripts or exploratory tests.

## Example

Imagine that you have an HTTP API that exposes resources for making restaurant reservations. For example, if you want to know how many free seats are available for a particular date, you can perform a simple `GET` request:

```fsharp
> get "http://localhost:56268/availability/2017/3/17" [] |> bodyText;;
> val it : string = "{"openings":[{"date":"2017-03-17","seats":10}]}"
```

## Use

Furl is a **single F# script file**, so you can load it into any FSI session and starting using it right away.

## Contributions

Furl has exactly the features I've needed so far. For the last half a year, I've only needed to do `GET` and `POST` HTTP requests, so those are the only HTTP methods available. If you need more features, please send a pull request, or [open an issue](https://github.com/ploeh/Furl/issues).

The philosophy behind Furl is to keep it simple and in a single file. Its purpose is to support exploratory testing, not to address every possible use case ever.

# Using type providers

Often, when interacting with REST APIs, you must either send JSON or XML, or you receive data in one of those formats.

With [F# Data](http://fsharp.github.io/FSharp.Data), you can use XML or JSON type providers to create even complex data structures, or to parse the responses you receive.

In order to use the XML or JSON type provider, you must first load *F# Data* in FSI:

```fsharp
> #r @".\packages\FSharp.Data\lib\net40\FSharp.Data.dll";;
```

When I develop REST APIs, I often write integration tests in F#. In such tests, I define the data formats in a stand-alone file, so that it's easy to load into FSI. Here's an example:

```fsharp
namespace Ploeh.Samples.BookingApi.BoundaryTests

open FSharp.Data

type ReservationJson = JsonProvider<"""
{
    "date": "some date",
    "name": "Mark Seemann",
    "email": "mark@example.org",
    "quantity": 4
}""">

type AvailabilityJson = JsonProvider<"""
{
    "openings": [
        {
            "date": "some date",
            "seats": 10
        }
    ]
}""">
```

If you have such a file, you can load it into your FSI session:

```fsharp
> #load @".\BookingApi\BookingApi.BoundaryTests\ProvidedTypes.fs";;
```

You can now start your local development server and start interacting with your REST API:

```fsharp
> let json = ReservationJson.Root ("2017-03-18", "Mark Seemann", "mark@example.com", 2) |> string;;
>
val json : string =
  "{
  "date": "2017-03-18",
  "name": "Mark Seemann",
  "ema"+[44 chars]

> post "http://localhost:56268/reservations" ["Content-Type", "application/json"] json;;
> val it : HttpResponseMessage =
  StatusCode: 200, ReasonPhrase: 'OK', Version: 1.1, Content: System.Net.Http.StreamContent,  
```

In the above example, I've truncated the response, because it actually gives you a lot of data.

If you want to query a resource and parse the response, that's easy to do as well:

```fsharp
> get "http://localhost:56268/availability/2017/3/18" [] |> bodyText |> AvailabilityJson.Parse;;
> val it : FSharp.Data.JsonProvider<...>.Root =
  {
  "openings": [
    {
      "date": "2017-03-18",
      "seats": 8
    }
  ]
}
```

These examples demonstrate how to interact with a local development server, but Furl can be used to interact with any HTTP-based API on your network, including the internet.