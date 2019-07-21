module Aornota.Sweepstake2019.Server.Common.JsonConverter

open Aornota.Sweepstake2019.Common.Json

open Thoth.Json.Net

let toJson<'a> value = Json(Encode.Auto.toString<'a>(SPACE_COUNT, value))

let fromJson<'a> (Json json) =
    match Decode.Auto.fromString<'a> json with
    | Ok value -> value
    | Error error -> failwithf "Unable to deserialize %s -> %s" json error
