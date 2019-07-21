module Aornota.Sweepstake2019.Ui.Theme.Shared

open Aornota.Sweepstake2019.Ui.Theme.Default
open Aornota.Sweepstake2019.Ui.Theme.Dark

let getTheme useDefaultTheme = if useDefaultTheme then themeDefault else themeDark
