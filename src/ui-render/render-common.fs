module Aornota.Sweepstake2019.Ui.Render.Common

open Browser.Types

open Fable.React
open Fable.React.Props

type Alignment = | Centred | LeftAligned | RightAligned | Justified | FullWidth

type DivData = {
    DivCustomClass : string option
    IsCentred : bool
    PadV : int option
    PadH : int option }

let [<Literal>] private KEYBOARD_CODE__ENTER = 13.
let [<Literal>] private KEYBOARD_CODE__ESCAPE = 27.

let [<Literal>] CENTRED_CLASS = "centered" (* sic *)
let [<Literal>] SPACE = " "

let private padStyle padV padH =
    let padding =
        match padV, padH with
        | Some padV, Some padH -> sprintf "%ipx %ipx" padV padH
        | Some padV, None -> sprintf "%ipx 0" padV
        | None, Some padH -> sprintf "0 %ipx" padH
        | None, None -> "0 0"
    Style [ Padding padding ]

let str text = str text
let strongEm text = strong [] [ em [] [ str text ] ]
let strong text = strong [] [ str text ]
let em text = em [] [ str text ]
let br = br []

let div divData children =
    let customClasses = [
        match divData.DivCustomClass with | Some divCustomClass -> yield divCustomClass | None -> ()
        if divData.IsCentred then yield CENTRED_CLASS ]
    let customClass = match customClasses with | _ :: _ -> Some (ClassName (String.concat SPACE customClasses)) | [] -> None
    div [
        match customClass with | Some customClass -> yield customClass :> IHTMLProp | None -> ()
        yield padStyle divData.PadV divData.PadH :> IHTMLProp
    ] children

let divDefault = { DivCustomClass = None ; IsCentred = false ; PadV = None ; PadH = None }
let divCentred = { divDefault with IsCentred = true }

let divVerticalSpace height = div { divDefault with PadV = Some (height / 2) } [ str SPACE ]

let divEmpty = div divDefault []

let onEnterPressed onEnter =
    OnKeyDown (fun (ev:KeyboardEvent) ->
        match ev with
        | _ when ev.keyCode = KEYBOARD_CODE__ENTER ->
            ev.preventDefault ()
            onEnter ()
        | _ -> ())
let onEscapePressed onEscape =
    OnKeyDown(fun (ev:KeyboardEvent) ->
        match ev with
        | _ when ev.keyCode = KEYBOARD_CODE__ESCAPE ->
            ev.preventDefault()
            onEscape()
        | _ -> ())
