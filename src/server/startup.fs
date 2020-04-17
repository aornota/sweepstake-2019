module Aornota.Sweepstake2019.Server.Startup

open Aornota.Sweepstake2019.Server.WsMiddleware

open Giraffe

open System.IO

open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection

let publicPath =
    let publicPath = Path.Combine ("..", "ui/public") |> Path.GetFullPath // e.g. when served via webpack-dev-server
    if Directory.Exists publicPath then publicPath else "public" |> Path.GetFullPath // e.g. when published/deployed

type Startup(_configuration:IConfiguration) =
    member __.Configure(applicationBuilder:IApplicationBuilder) =
        let webApp = choose [ route "/" >=> htmlFile (Path.Combine (publicPath, "index.html")) ]
        applicationBuilder
            .UseStaticFiles()
            .UseWebSockets()
            .UseMiddleware<WsMiddleware>()
            .UseGiraffe(webApp)
    member __.ConfigureServices(services:IServiceCollection) =
        services.AddGiraffe() |> ignore
