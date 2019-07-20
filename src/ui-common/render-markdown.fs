module Aornota.UI.Common.Render.Markdown

open Aornota.Common.Markdown

open Aornota.UI.Common.Marked
open Aornota.UI.Render.Bulma
open Aornota.UI.Render.Common
open Aornota.UI.Theme.Common

open Fable.Core
module Rct = Fable.Helpers.React
open Fable.Helpers.React.Props

[<Pojo>]
type private DangerousInnerHtml = { __html : string }

let [<Literal>] private MARKDOWN_CLASS = "markdown"

let contentFromMarkdown' theme inNotification (Markdown markdown) =
    let (ThemeClass className) = theme.ThemeClass
    let customClasses = [
        yield MARKDOWN_CLASS
        if inNotification then yield sprintf "%s-in-notification" className else yield className ]
    let customClass = match customClasses with | _ :: _ -> ClassName (String.concat SPACE customClasses) |> Some | [] -> None
    content [
        Rct.div [
            match customClass with | Some customClass -> yield customClass :> IHTMLProp | None -> ()
            yield DangerouslySetInnerHTML { __html = Marked.Globals.marked.parse markdown } :> IHTMLProp ] [] ]

let contentFromMarkdown theme markdown = markdown |> contentFromMarkdown' theme false

let notificationContentFromMarkdown theme markdown = markdown |> contentFromMarkdown' theme true
