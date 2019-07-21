module Aornota.Sweepstake2019.Ui.Common.JsonConverter

open Aornota.Sweepstake2019.Common.Json

open Thoth.Json

let inline toJson<'a> value = Encode.Auto.toString<'a>(SPACE_COUNT, value)

let inline fromJson<'a> json =
    match Decode.Auto.fromString<'a> json with
    | Ok value -> value
    | Error error -> failwithf "Unable to deserialize %s -> %s" json error
