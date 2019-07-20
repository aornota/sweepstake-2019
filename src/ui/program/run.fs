module Aornota.Sweepstake2018.UI.Program.Run

#if TICK
open Aornota.Common.UnitsOfMeasure
#endif

#if TICK
open Aornota.Sweepstake2018.UI.Program.Common
#endif
open Aornota.Sweepstake2018.UI.Program.Render
open Aornota.Sweepstake2018.UI.Program.State

open Aornota.UI.Common.Marked

open Elmish
#if DEBUG
open Elmish.Debug
#endif
#if HMR
open Elmish.HMR
#endif
open Elmish.React

open Fable.Core.JsInterop
#if TICK
open Fable.Import
#endif

#if TICK
let [<Literal>] private SECONDS_PER_TICK = 1<second/tick>

let private ticker dispatch =
    let secondsPerTick = if SECONDS_PER_TICK > 0<second/tick> then SECONDS_PER_TICK else 1<second/tick>
    let millisecondsPerTick = ((float secondsPerTick) * 1.<second/tick>) * MILLISECONDS_PER_SECOND
    Browser.window.setInterval (fun _ -> 
        dispatch Tick
    , int millisecondsPerTick) |> ignore

let private tickSubscription (_:State) = ticker |> Cmd.ofSub
#endif

Marked.Globals.marked.setOptions (unbox (createObj [ "sanitize" ==> true ])) |> ignore // note: "sanitize" ensures Html rendered as text

Program.mkProgram initialize transition render
#if TICK
|> Program.withSubscription tickSubscription
#endif
#if DEBUG
|> Program.withConsoleTrace
#endif
#if HMR
|> Program.withHMR // note: HMR can cause weird behaviour if TICK is also defined
#endif
|> Program.withReact "elmish-app" // note: needs to match id of div in index.html
#if DEBUG
//|> Program.withDebugger // TODO-NMB-LOW: Uncomment once installed https://github.com/zalmoxisus/redux-devtools-extension?...
#endif
|> Program.run
