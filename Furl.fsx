#r "System.Net.Http"

open System
open System.Net.Http

let private addHeader (headers : Headers.HttpHeaders) (name, value : string) =
    headers.Add (name, value)

let private addBody (req : HttpRequestMessage) headers body =
    req.Content <- new StringContent (body)
    let contentTypeHeader =
        headers |> List.tryFind (fun (n, _) -> n = "Content-Type")
    contentTypeHeader
    |> Option.iter (fun (_, v) -> req.Content.Headers.ContentType.MediaType <- v)

let result (t : System.Threading.Tasks.Task<_>) = t.Result

let composeMessage meth (url : Uri) headers body =
    let req = new HttpRequestMessage (meth, url)
    Option.iter (addBody req headers) body

    headers
    |> List.partition (fun (n, _) -> n = "Content-Type")
    |> snd
    |> List.iter (addHeader req.Headers)
    req

let get url headers =
    use client = new HttpClient ()
    // HttpMethod is qualified to avoid collision with FSharp.Data.HttpMethod,
    // if FSharp.Data is imported in a script as well as Furl.
    composeMessage Net.Http.HttpMethod.Get (Uri url) headers None
    |> client.SendAsync
    |> result

let post url headers body =
    use client = new HttpClient ()
    // HttpMethod is qualified to avoid collision with FSharp.Data.HttpMethod,
    // if FSharp.Data is imported in a script as well as Furl.
    composeMessage Net.Http.HttpMethod.Post (Uri url) headers (Some body)
    |> client.SendAsync
    |> result

let bodyText (resp : HttpResponseMessage) =
    resp.Content.ReadAsStringAsync().Result
