module Aornota.Sweepstake2018.UI.Pages.Squads.Render

open Aornota.Common.UnitsOfMeasure

open Aornota.UI.Common.LazyViewOrHMR
open Aornota.UI.Common.TimestampHelper
open Aornota.UI.Render.Bulma
open Aornota.UI.Render.Common
open Aornota.UI.Theme.Common
open Aornota.UI.Theme.Render.Bulma
open Aornota.UI.Theme.Shared

open Aornota.Sweepstake2018.Common.Domain.Core
open Aornota.Sweepstake2018.Common.Domain.Draft
open Aornota.Sweepstake2018.Common.Domain.Fixture
open Aornota.Sweepstake2018.Common.Domain.Squad
open Aornota.Sweepstake2018.Common.Domain.User
open Aornota.Sweepstake2018.UI.Pages.Squads.Common
open Aornota.Sweepstake2018.UI.Shared

open System

module Rct = Fable.Helpers.React

let private playerTypes = [ Goalkeeper ; Defender ; Midfielder ; Forward ]

let private playerTypeSortOrder playerType = match playerType with | Goalkeeper -> 1 | Defender -> 2 | Midfielder -> 3 | Forward -> 4
let private playerTypeRadios selectedPlayerType disabledPlayerType disableAll dispatch =
    let onChange playerType = (fun _ -> playerType |> dispatch)
    playerTypes
    |> List.sortBy playerTypeSortOrder
    |> List.map (fun playerType ->
        let isSelected = playerType |> Some = selectedPlayerType
        let disabled = disableAll || playerType |> Some = disabledPlayerType
        let onChange = if isSelected || disabled then ignore else playerType |> onChange
        radioInline (playerType |> playerTypeText) isSelected disabled onChange)
    |> List.collect id

let private nonWithdrawnCount squad =
    match squad with
    | Some squad ->
        squad.PlayerDic |> List.ofSeq |> List.filter (fun (KeyValue (_, player)) -> match player.PlayerStatus with | PlayerStatus.Active -> true | Withdrawn _ -> false) |> List.length
    | None -> 0

let private renderAddPlayersModal (useDefaultTheme, squadDic:SquadDic, addPlayersState:AddPlayersState) dispatch =
    let theme = getTheme useDefaultTheme
    let squadId = addPlayersState.SquadId
    let squad = if squadId |> squadDic.ContainsKey then squadDic.[squadId] |> Some else None
    let titleText =
        match squad with
        | Some squad ->
            let (SquadName squadName) = squad.SquadName
            sprintf "Add player/s for %s" squadName
        | None -> "Add player/s" // note: should never happen
    let onDismiss = match addPlayersState.AddPlayerStatus with | Some AddPlayerPending -> None | Some _ | None -> (fun _ -> CancelAddPlayers |> dispatch) |> Some
    let playerNames = match squad with | Some squad -> squad.PlayerDic |> playerNames | None -> []
    let nonWithdrawnCount = squad |> nonWithdrawnCount
    let squadIsFull = nonWithdrawnCount >= MAX_PLAYERS_PER_SQUAD
    let isAddingPlayer, addPlayerInteraction, onEnter =
        let addPlayer = (fun _ -> AddPlayer |> dispatch)
        match addPlayersState.AddPlayerStatus with
        | Some AddPlayerPending -> true, Loading, ignore
        | Some (AddPlayerFailed _) | None ->
            match validatePlayerName playerNames (PlayerName addPlayersState.NewPlayerNameText), squadIsFull with
            | None, false -> false, Clickable (addPlayer, None), addPlayer
            | _ -> false, NotEnabled None, ignore
    let errorText = match addPlayersState.AddPlayerStatus with | Some (AddPlayerFailed errorText) -> errorText |> Some | Some AddPlayerPending | None -> None
    let (PlayerId newPlayerKey) = addPlayersState.NewPlayerId
    let body = [
        if squadIsFull then
            yield notification theme notificationWarning [ [ str squadIsFullText ] |> para theme paraCentredSmallest ]
            yield br
        match errorText with
        | Some errorText ->
            yield notification theme notificationDanger [ [ str errorText ] |> para theme paraDefaultSmallest ]
            yield br
        | None -> ()
        yield [ str "Please enter the name and position for the new player" ] |> para theme paraCentredSmaller
        yield br
        // TODO-NMB-MEDIUM: Finesse layout / alignment - and add labels?...
        yield field theme { fieldDefault with Grouped = Centred |> Some } [
            textBox theme newPlayerKey addPlayersState.NewPlayerNameText (iconMaleSmall |> Some) false addPlayersState.NewPlayerNameErrorText [] true isAddingPlayer
                (NewPlayerNameTextChanged >> dispatch) onEnter ]
        yield field theme { fieldDefault with Grouped = Centred |> Some } [
            yield! playerTypeRadios (addPlayersState.NewPlayerType |> Some) None isAddingPlayer (NewPlayerTypeChanged >> dispatch) ]
        yield field theme { fieldDefault with Grouped = Centred |> Some } [ [ str "Add player" ] |> button theme { buttonLinkSmall with Interaction = addPlayerInteraction } ] ]
    cardModal theme [ [ bold titleText ] |> para theme paraCentredSmall ] onDismiss body

let private renderChangePlayerNameModal (useDefaultTheme, squadDic:SquadDic, changePlayerNameState:ChangePlayerNameState) dispatch =
    let theme = getTheme useDefaultTheme
    let squadId = changePlayerNameState.SquadId
    let squad = if squadId |> squadDic.ContainsKey then squadDic.[squadId] |> Some else None
    let playerId = changePlayerNameState.PlayerId
    let player = match squad with | Some squad -> (if playerId |> squad.PlayerDic.ContainsKey then squad.PlayerDic.[playerId] |> Some else None) | None -> None
    let currentPlayerName, titleText =
        match player with
        | Some player ->
            let (PlayerName playerName) = player.PlayerName
            player.PlayerName |> Some, sprintf "Edit name for %s" playerName
        | None -> None, "Edit player name" // note: should never happen
    let onDismiss = match changePlayerNameState.ChangePlayerNameStatus with | Some ChangePlayerNamePending -> None | Some _ | None -> (fun _ -> CancelChangePlayerName |> dispatch) |> Some
    let playerNames = match squad with | Some squad -> squad.PlayerDic |> playerNames | None -> []
    let isChangingPlayerName, changePlayerNameInteraction, onEnter =
        let changePlayerName = (fun _ -> ChangePlayerName |> dispatch)
        match changePlayerNameState.ChangePlayerNameStatus with
        | Some ChangePlayerNamePending -> true, Loading, ignore
        | Some (ChangePlayerNameFailed _) | None ->
            let isSame = match currentPlayerName with | Some playerName -> playerName = (PlayerName changePlayerNameState.PlayerNameText) | None -> false
            match validatePlayerName playerNames (PlayerName changePlayerNameState.PlayerNameText), isSame with
            | None, false -> false, Clickable (changePlayerName, None), changePlayerName
            | _ -> false, NotEnabled None, ignore
    let errorText = match changePlayerNameState.ChangePlayerNameStatus with | Some (ChangePlayerNameFailed errorText) -> errorText |> Some | Some ChangePlayerNamePending | None -> None
    let (PlayerId playerKey) = changePlayerNameState.PlayerId
    let body = [
        match errorText with
        | Some errorText ->
            yield notification theme notificationDanger [ [ str errorText ] |> para theme paraDefaultSmallest ]
            yield br
        | None -> ()
        yield [ str "Please enter the new name for the player" ] |> para theme paraCentredSmaller
        yield br
        // TODO-NMB-MEDIUM: Finesse layout / alignment - and add labels?...
        yield field theme { fieldDefault with Grouped = Centred |> Some } [
            textBox theme playerKey changePlayerNameState.PlayerNameText (iconMaleSmall |> Some) false changePlayerNameState.PlayerNameErrorText [] true isChangingPlayerName
                (PlayerNameTextChanged >> dispatch) onEnter ]
        yield field theme { fieldDefault with Grouped = Centred |> Some } [ [ str "Edit name" ] |> button theme { buttonLinkSmall with Interaction = changePlayerNameInteraction } ] ]
    cardModal theme [ [ bold titleText ] |> para theme paraCentredSmall ] onDismiss body

let private renderChangePlayerTypeModal (useDefaultTheme, squadDic:SquadDic, changePlayerTypeState:ChangePlayerTypeState) dispatch =
    let theme = getTheme useDefaultTheme
    let squadId = changePlayerTypeState.SquadId
    let squad = if squadId |> squadDic.ContainsKey then squadDic.[squadId] |> Some else None
    let playerId = changePlayerTypeState.PlayerId
    let player = match squad with | Some squad -> (if playerId |> squad.PlayerDic.ContainsKey then squad.PlayerDic.[playerId] |> Some else None) | None -> None
    let currentPlayerType, titleText =
        match player with
        | Some player ->
            let (PlayerName playerName) = player.PlayerName
            player.PlayerType |> Some, sprintf "Change position for %s" playerName
        | None -> None, "Change player position" // note: should never happen
    let onDismiss = match changePlayerTypeState.ChangePlayerTypeStatus with | Some ChangePlayerTypePending -> None | Some _ | None -> (fun _ -> CancelChangePlayerType |> dispatch) |> Some
    let isChangingPlayerType, changePlayerTypeInteraction, onEnter =
        let changePlayerType = (fun _ -> ChangePlayerType |> dispatch)
        match changePlayerTypeState.ChangePlayerTypeStatus with
        | Some ChangePlayerTypePending -> true, Loading, ignore
        | Some (ChangePlayerTypeFailed _) | None ->
            let isValid = match changePlayerTypeState.PlayerType with | Some playerType -> playerType |> Some <> currentPlayerType | None -> false
            if isValid |> not then false, NotEnabled None, ignore
            else false, Clickable (changePlayerType, None), changePlayerType
    let errorText = match changePlayerTypeState.ChangePlayerTypeStatus with | Some (ChangePlayerTypeFailed errorText) -> errorText |> Some | Some ChangePlayerTypePending | None -> None
    let body = [
        match errorText with
        | Some errorText ->
            yield notification theme notificationDanger [ [ str errorText ] |> para theme paraDefaultSmallest ]
            yield br
        | None -> ()
        yield [ str "Please choose the new position for the player" ] |> para theme paraCentredSmaller
        yield br
        // TODO-NMB-MEDIUM: Finesse layout / alignment - and add labels?...
        yield field theme { fieldDefault with Grouped = Centred |> Some } [
            yield! playerTypeRadios changePlayerTypeState.PlayerType currentPlayerType isChangingPlayerType (PlayerTypeChanged >> dispatch) ]
        yield field theme { fieldDefault with Grouped = Centred |> Some } [ [ str "Change position" ] |> button theme { buttonLinkSmall with Interaction = changePlayerTypeInteraction } ] ]
    cardModal theme [ [ bold titleText ] |> para theme paraCentredSmall ] onDismiss body

let private renderWithdrawPlayerModal (useDefaultTheme, squadDic:SquadDic, withdrawPlayerState:WithdrawPlayerState) dispatch =
    let theme = getTheme useDefaultTheme
    let squadId = withdrawPlayerState.SquadId
    let squad = if squadId |> squadDic.ContainsKey then squadDic.[squadId] |> Some else None
    let playerId = withdrawPlayerState.PlayerId
    let player = match squad with | Some squad -> (if playerId |> squad.PlayerDic.ContainsKey then squad.PlayerDic.[playerId] |> Some else None) | None -> None
    let titleText =
        match player with
        | Some player ->
            let (PlayerName playerName) = player.PlayerName
            sprintf "Withdraw %s" playerName
        | None -> "Withdraw player" // note: should never happen
    let confirmInteraction, onDismiss =
        let confirm = (fun _ -> ConfirmWithdrawPlayer |> dispatch)
        let cancel = (fun _ -> CancelWithdrawPlayer |> dispatch)
        match withdrawPlayerState.WithdrawPlayerStatus with
        | Some WithdrawPlayerPending -> Loading, None
        | Some (WithdrawPlayerFailed _) | None -> Clickable (confirm, None), cancel |> Some
    let errorText = match withdrawPlayerState.WithdrawPlayerStatus with | Some (WithdrawPlayerFailed errorText) -> errorText |> Some | Some WithdrawPlayerPending | None -> None
    let warning = [
        [ bold "Are you sure you want to withdraw this player?" ] |> para theme paraCentredSmaller
        br
        [ str "Please note that this action is irreversible." ] |> para theme paraCentredSmallest ]
    let body = [
        match errorText with
        | Some errorText ->
            yield notification theme notificationDanger [ [ str errorText ] |> para theme paraDefaultSmallest ]
            yield br
        | None -> ()
        yield notification theme notificationWarning warning
        yield br
        yield field theme { fieldDefault with Grouped = Centred |> Some } [
            [ str "Withdraw player" ] |> button theme { buttonLinkSmall with Interaction = confirmInteraction } ] ]
    cardModal theme [ [ bold titleText ] |> para theme paraCentredSmall ] onDismiss body

let private renderEliminateSquadModal (useDefaultTheme, squadDic:SquadDic, eliminateSquadState:EliminateSquadState) dispatch =
    let theme = getTheme useDefaultTheme
    let squadId = eliminateSquadState.SquadId
    let squad = if squadId |> squadDic.ContainsKey then squadDic.[squadId] |> Some else None
    let titleText =
        match squad with
        | Some squad ->
            let (SquadName squadName) = squad.SquadName
            sprintf "Eliminate %s" squadName
        | None -> "Eliminate team" // note: should never happen
    let confirmInteraction, onDismiss =
        let confirm = (fun _ -> ConfirmEliminateSquad |> dispatch)
        let cancel = (fun _ -> CancelEliminateSquad |> dispatch)
        match eliminateSquadState.EliminateSquadStatus with
        | Some EliminateSquadPending -> Loading, None
        | Some (EliminateSquadFailed _) | None -> Clickable (confirm, None), cancel |> Some
    let errorText = match eliminateSquadState.EliminateSquadStatus with | Some (EliminateSquadFailed errorText) -> errorText |> Some | Some EliminateSquadPending | None -> None
    let warning = [
        [ bold "Are you sure you want to eliminate this team?" ] |> para theme paraCentredSmaller
        br
        [ str "Please note that this action is irreversible." ] |> para theme paraCentredSmallest ]
    let body = [
        match errorText with
        | Some errorText ->
            yield notification theme notificationDanger [ [ str errorText ] |> para theme paraDefaultSmallest ]
            yield br
        | None -> ()
        yield notification theme notificationWarning warning
        yield br
        yield field theme { fieldDefault with Grouped = Centred |> Some } [
            [ str "Eliminate team" ] |> button theme { buttonLinkSmall with Interaction = confirmInteraction } ] ]
    cardModal theme [ [ bold titleText ] |> para theme paraCentredSmall ] onDismiss body

let private renderFreePickModal (useDefaultTheme, squadDic:SquadDic, freePickState:FreePickState) dispatch =
    let theme = getTheme useDefaultTheme
    let draftPickText = freePickState.DraftPick |> draftPickText squadDic
    let confirmInteraction, onDismiss =
        let confirm = (fun _ -> ConfirmFreePick |> dispatch)
        let cancel = (fun _ -> CancelFreePick |> dispatch)
        match freePickState.FreePickStatus with
        | Some FreePickPending -> Loading, None
        | Some (FreePickFailed _) | None -> Clickable (confirm, None), cancel |> Some
    let errorText = match freePickState.FreePickStatus with | Some (FreePickFailed errorText) -> errorText |> Some | Some FreePickPending | None -> None
    let warning = [
        [ bold (sprintf "Are you sure you want to pick %s?" draftPickText) ] |> para theme paraCentredSmaller
        br
        [ str "Please note that this action is irreversible." ] |> para theme paraCentredSmallest ]
    let body = [
        match errorText with
        | Some errorText ->
            yield notification theme notificationDanger [ [ str errorText ] |> para theme paraDefaultSmallest ]
            yield br
        | None -> ()
        yield notification theme notificationWarning warning
        yield br
        yield field theme { fieldDefault with Grouped = Centred |> Some } [
            [ str (sprintf "Pick %s" draftPickText) ] |> button theme { buttonLinkSmall with Interaction = confirmInteraction } ] ]
    cardModal theme [ [ bold (sprintf "Pick %s" draftPickText) ] |> para theme paraCentredSmall ] onDismiss body

let private group (squadDic:SquadDic) squadId = match squadId with | Some squadId when squadId |> squadDic.ContainsKey -> squadDic.[squadId].Group |> Some | Some _ | None -> None

let private defaultSquadId (squadDic:SquadDic) group =
    let groupSquads = squadDic |> List.ofSeq |> List.map (fun (KeyValue (squadId, squad)) -> squadId, squad) |> List.filter (fun (_, squad) -> squad.Group = group)
    match groupSquads |> List.sortBy (fun (_, squad) -> squad.SquadName) with | (squadId, _) :: _ -> squadId |> Some | [] -> None

let private groupTab currentGroup dispatch group =
    let isActive = match currentGroup with | Some currentGroup when currentGroup = group -> true | Some _ | None -> false
    { IsActive = isActive ; TabText = group |> groupText ; TabLinkType = ClickableLink (fun _ -> group |> ShowGroup |> dispatch ) }

let private squadTabs currentSquadId dispatch (squadDic:SquadDic) =
    let squadTab (squadId, squad) =
        let (SquadName squadName) = squad.SquadName
        let isActive = match currentSquadId with | Some currentSquadId when currentSquadId = squadId -> true | Some _ | None -> false
        { IsActive = isActive ; TabText = squadName ; TabLinkType = ClickableLink (fun _ -> squadId |> ShowSquad |> dispatch ) }
    match currentSquadId |> group squadDic with
    | Some group ->
        let groupSquads = squadDic |> List.ofSeq |> List.map (fun (KeyValue (squadId, squad)) -> squadId, squad) |> List.filter (fun (_, squad) -> squad.Group = group)
        groupSquads |> List.map squadTab
    | None -> []

let private activeDraftIdAndOrdinalAndIsOpen authUser currentDraft =
    let canDraft =
        match authUser with
        | Some authUser -> match authUser.Permissions.DraftPermission with | Some userId when userId = authUser.UserId -> true | Some _ | None -> false
        | None -> false
    match canDraft, currentDraft with
    | true, Some (draftId, draft) ->
        match draft.DraftStatus with
        | Opened _ -> (draftId, draft.DraftOrdinal, true) |> Some
        | PendingProcessing _ -> (draftId, draft.DraftOrdinal, false) |> Some
        | _ -> None
    | _ -> None
let private freePickDraftId authUser currentDraft =
    let canDraft =
        match authUser with
        | Some authUser -> match authUser.Permissions.DraftPermission with | Some userId when userId = authUser.UserId -> true | Some _ | None -> false
        | None -> false
    match canDraft, currentDraft with
    | true, Some (draftId, draft) ->
        match draft.DraftStatus with
        | FreeSelection -> draftId |> Some
        | _ -> None
    | _ -> None

let private rank userDraftPick (userDraftPickDic:UserDraftPickDic) = if userDraftPick |> userDraftPickDic.ContainsKey then userDraftPickDic.[userDraftPick] |> Some else None

let private pendingTeamPick squadId (pendingPicks:PendingPick list) =
    pendingPicks |> List.tryFind (fun pendingPick ->
        match pendingPick.UserDraftPick with | TeamPick pickSquadId when pickSquadId = squadId -> true | _ -> false)
let private pendingPlayerPick playerId (pendingPicks:PendingPick list) =
    pendingPicks |> List.tryFind (fun pendingPick ->
        match pendingPick.UserDraftPick with | PlayerPick (_, pickPlayerId) when pickPlayerId = playerId -> true | _ -> false)

let private selectedForDraftTag theme (draftOrdinal:DraftOrdinal) rank =
    let tagText = sprintf "Selected for %s: rank #%i" (draftOrdinal |> draftTextLower) rank
    [ [ str tagText ] |> tag theme { tagInfo with IsRounded = false } ] |> para theme { paraDefaultSmallest with ParaAlignment = RightAligned }

let private draftLeftAndRight theme draftId draftOrdinal isOpen needsMorePicks userDraftPick (rank:int option) (pendingPick:PendingPick option) dispatch =
    let canInteract = isOpen && needsMorePicks
    let addText, removeText = sprintf "Add to %s" (draftOrdinal |> draftTextLower), "Remove"
    let paraRight = { paraDefaultSmallest with ParaAlignment = RightAligned }
    let draftLeft =
        match rank with
        | Some rank -> if canInteract then rank |> selectedForDraftTag theme draftOrdinal |> Some else None
        | None ->
            match pendingPick with
            | Some pendingPick ->
                match pendingPick.PendingPickStatus with
                | Adding -> if canInteract then [ [ str addText ] |> button theme { buttonLinkSmall with Interaction = Loading } ] |> para theme paraRight |> Some else None
                | Removing -> None
            | None ->
                if canInteract then
                    let onClick = (fun _ -> (draftId, userDraftPick) |> AddToDraft |> dispatch)
                    [ [ str addText ] |> button theme { buttonLinkSmall with Interaction = Clickable (onClick, None) } ] |> para theme paraRight |> Some
                else None
    let draftRight =
        match rank with
        | Some _ ->
            match pendingPick with
            | Some pendingPick ->
                match pendingPick.PendingPickStatus with
                | Adding -> None
                | Removing -> if canInteract then [ [ str removeText ] |> button theme { buttonDangerSmall with Interaction = Loading } ] |> para theme paraDefaultSmallest |> Some else None
            | None ->
                if canInteract then
                    let onClick = (fun _ -> (draftId, userDraftPick) |> RemoveFromDraft |> dispatch)
                    [ [ str removeText ] |> button theme { buttonDangerSmall with Interaction = Clickable (onClick, None) } ] |> para theme paraDefaultSmallest |> Some
                else None
        | None -> None
    draftLeft, draftRight

let private freePick theme draftId needsMorePicks draftPick dispatch =
    let pickType = match draftPick with | TeamPicked _ -> "team" | PlayerPicked _ -> "player"
    if needsMorePicks then
        let onClick = (fun _ -> (draftId, draftPick) |> ShowFreePickModal |> dispatch)
        [ [ str (sprintf "Pick %s" pickType) ] |> para theme paraCentredSmallest ] |> link theme (ClickableLink onClick) |> Some
    else None

// #region customAgo
let private customAgo (timestamp:DateTime) =
#if TICK
    timestamp |> ago
#else
    sprintf "on %s" (timestamp |> dateAndTimeText)
#endif
// #endregion

let private pickedByTag theme (userDic:UserDic) (authUser:AuthUser option) (pickedBy:PickedBy option) =
    match pickedBy with
    | Some (userId, draftOrdinal, timestamp) ->
        let (UserName userName) = userId |> userName userDic
        let pickedBy =
            match draftOrdinal with
            | Some draftOrdinal -> [ div divDefault [ bold userName ; str (sprintf " (%s)" (draftOrdinal |> draftTextLower)) ] ]
            | None -> [ div divDefault [ bold userName ; str (sprintf " (%s)" (customAgo timestamp.LocalDateTime)) ] ]
        let tagData = match authUser with | Some authUser when authUser.UserId = userId -> tagSuccess | Some _ | None -> tagPrimary
        pickedBy |> tag theme { tagData with IsRounded = false } |> Some
    | None -> None

let private score (points:int<point>) (pickedByPoints:int<point> option) pickedByUserId (userDic:UserDic) =
    let pointsOnly =
        let pointsText = sprintf "%i" (int points)
        if points > 0<point> then bold pointsText else if points < 0<point> then italic pointsText else str pointsText
    match pickedByPoints, pickedByUserId with
    | Some pickedByPoints, Some pickedByUserId ->
        if points = pickedByPoints then pointsOnly
        else
            let (UserName userName) = pickedByUserId |> userName userDic
            let pickedByPointsText = sprintf "%i" (int pickedByPoints)
            let pickedByPoints = if pickedByPoints > 0<point> then bold pickedByPointsText else if pickedByPoints < 0<point> then italic pickedByPointsText else str pickedByPointsText
            let pickedByUser = str (sprintf " for %s)" userName)
            div divDefault [ pointsOnly ; str " (" ; pickedByPoints ; pickedByUser ]
    | _ -> pointsOnly

let private renderSquad (useDefaultTheme, squadId, squad, currentDraft, pickedCounts, userDraftPickDic, pendingPicks, userDic:UserDic, fixtureDic:FixtureDic, authUser) dispatch =
    let theme = getTheme useDefaultTheme
    let pickedTeamCount, _, _ = pickedCounts
    let needsTeam = pickedTeamCount < MAX_TEAM_PICKS
    let (SquadName squadName), (CoachName coachName), (Seeding seeding) = squad.SquadName, squad.CoachName, squad.Seeding
    let canEliminate =
        match authUser with
        | Some authUser -> match authUser.Permissions.SquadPermissions with | Some squadPermissions -> squadPermissions.EliminateSquadPermission | None -> false
        | None -> false
    let eliminated = if squad.Eliminated then [ [ str "Eliminated" ] |> tag theme { tagWarning with IsRounded = false } ] |> para theme paraDefaultSmallest |> Some else None
    let eliminate =
        if canEliminate && squad.Eliminated |> not then
            let pendingFixtures =
                fixtureDic |> List.ofSeq |> List.filter (fun (KeyValue (_, fixture)) ->
                    match fixture.HomeParticipant, fixture.AwayParticipant with
                    | Confirmed otherSquadId, _ when otherSquadId = squadId -> match fixture.MatchResult with | Some _ -> false | None -> true
                    | _, Confirmed otherSquadId when otherSquadId = squadId -> match fixture.MatchResult with | Some _ -> false | None -> true
                    | _ -> false)
            if pendingFixtures.Length = 0 then
                let onClick = (fun _ -> squadId |> ShowEliminateSquadModal |> dispatch)
                [ [ str "Eliminate" ] |> para theme { paraDefaultSmallest with ParaAlignment = RightAligned } ] |> link theme (ClickableLink onClick) |> Some
            else None
        else None
    let draftLeft, draftRight =
        let isPicked = match squad.PickedBy with | Some _ -> true | None -> false
        let activeDraftIdAndOrdinalAndIsOpen = activeDraftIdAndOrdinalAndIsOpen authUser currentDraft
        let freePickDraftId = freePickDraftId authUser currentDraft
        match isPicked, activeDraftIdAndOrdinalAndIsOpen, freePickDraftId with
        | false, Some (draftId, draftOrdinal, isOpen), _ ->
            let userDraftPick = squadId |> TeamPick
            let rank = userDraftPickDic |> rank userDraftPick
            draftLeftAndRight theme draftId draftOrdinal isOpen needsTeam userDraftPick rank (pendingPicks |> pendingTeamPick squadId) dispatch
        | false, _, Some draftId ->
            let draftPick = squadId |> TeamPicked
            freePick theme draftId needsTeam draftPick dispatch, None
        | _ -> None, None
    let pickedByTag = squad.PickedBy |> pickedByTag theme userDic authUser
    let pickedByUserId, pickedDate = match squad.PickedBy with | Some (userId, _, date) -> userId |> Some, date |> Some | None -> None, None
    let points, pickedByPoints = teamPoints fixtureDic squadId pickedDate
    let score = score points pickedByPoints pickedByUserId userDic
    div divCentred [
        table theme false { tableDefault with IsNarrow = true ; IsFullWidth = true } [
            thead [ 
                tr false [
                    th [ [ bold "Team"] |> para theme paraDefaultSmallest ]
                    th []
                    th [ [ bold "Seeding" ] |> para theme paraCentredSmallest ]
                    th [ [ bold "Coach" ] |> para theme paraDefaultSmallest ]
                    th []
                    th []
                    th [ [ bold "Picked by" ] |> para theme paraDefaultSmallest ]
                    th [ [ bold "Score" ] |> para theme { paraDefaultSmallest with ParaAlignment = RightAligned } ]
                    th [] ] ]
            tbody [
                tr false [
                    td [ [ str squadName ] |> para theme paraDefaultSmallest ]
                    td [ Rct.ofOption eliminated ]
                    td [ [ str (sprintf "%i" seeding) ] |> para theme paraCentredSmallest ]
                    td [ [ str coachName ] |> para theme paraDefaultSmallest ]
                    td [ Rct.ofOption draftLeft ]
                    td [ Rct.ofOption draftRight ]
                    td [ Rct.ofOption pickedByTag ]
                    td [ [ score ] |> para theme { paraDefaultSmallest with ParaAlignment = RightAligned } ]
                    td [ Rct.ofOption eliminate ] ] ] ] ]

let private renderPlayers (useDefaultTheme, playerDic:PlayerDic, squadId, squad, currentDraft, pickedCounts, userDraftPickDic, pendingPicks, userDic:UserDic, fixtureDic:FixtureDic, authUser) dispatch =
    let theme = getTheme useDefaultTheme
    let _, pickedGoalkeeperCount, pickedOutfieldPlayerCount = pickedCounts
    let needsGoalkeeper = pickedGoalkeeperCount < MAX_GOALKEEPER_PICKS
    let needsOutfieldPlayers = MAX_OUTFIELD_PLAYER_PICKS - pickedOutfieldPlayerCount > 0
    let canEdit, canWithdraw =
        match authUser with
        | Some authUser ->
            match authUser.Permissions.SquadPermissions with
            | Some squadPermissions -> squadPermissions.AddOrEditPlayerPermission, squadPermissions.WithdrawPlayerPermission
            | None -> false, false
        | None -> false, false
    let editName playerId =
        if canEdit then
            let onClick = (fun _ -> (squadId, playerId) |> ShowChangePlayerNameModal |> dispatch)
            [ [ str "Edit name" ] |> para theme paraDefaultSmallest ] |> link theme (ClickableLink onClick) |> Some
        else None
    let changePosition playerId =
        if canEdit then
            let onClick = (fun _ -> (squadId, playerId) |> ShowChangePlayerTypeModal |> dispatch)
            [ [ str "Change position" ] |> para theme paraDefaultSmallest ] |> link theme (ClickableLink onClick) |> Some
        else None
    let isWithdrawnAndDate player = match player.PlayerStatus with | PlayerStatus.Active -> false, None | Withdrawn dateWithdrawn -> true, dateWithdrawn
    let withdrawn player =
        let isWithdrawn, dateWithdrawn = player |> isWithdrawnAndDate
        if isWithdrawn then
            let withdrawnText = match dateWithdrawn with | Some dateWithdrawn -> sprintf "Withdrawn %s" (customAgo dateWithdrawn.LocalDateTime) | None -> "Withdrawn"
            [ [ str withdrawnText ] |> tag theme { tagWarning with IsRounded = false } ] |> para theme paraDefaultSmallest |> Some
        else None
    let withdraw playerId player =
        let isWithdrawn, _ = player |> isWithdrawnAndDate
        if canWithdraw && squad.Eliminated |> not && isWithdrawn |> not then
            let onClick = (fun _ -> (squadId, playerId) |> ShowWithdrawPlayerModal |> dispatch)
            [ [ str "Withdraw" ] |> para theme { paraDefaultSmallest with ParaAlignment = RightAligned } ] |> link theme (ClickableLink onClick) |> Some
        else None
    let draftLeftAndRight playerId (player:Player) =
        let isPicked = match player.PickedBy with | Some _ -> true | None -> false
        let isWithdrawn, _ = player |> isWithdrawnAndDate
        let needsMorePicks = match player.PlayerType with | Goalkeeper -> needsGoalkeeper | _ -> needsOutfieldPlayers
        let activeDraftIdAndOrdinalAndIsOpen = activeDraftIdAndOrdinalAndIsOpen authUser currentDraft
        let freePickDraftId = freePickDraftId authUser currentDraft
        match isPicked, isWithdrawn, activeDraftIdAndOrdinalAndIsOpen, freePickDraftId with
        | false, false, Some (draftId, draftOrdinal, isOpen), _ ->
            let userDraftPick = (squadId, playerId) |> PlayerPick
            let rank = userDraftPickDic |> rank userDraftPick
            draftLeftAndRight theme draftId draftOrdinal isOpen needsMorePicks userDraftPick (rank) (pendingPicks |> pendingPlayerPick playerId) dispatch
        | false, false, _, Some draftId ->
            let draftPick = (squadId, playerId) |> PlayerPicked
            freePick theme draftId needsMorePicks draftPick dispatch, None
        | _ -> None, None
    let playerRow (playerId, player) =
        let (PlayerName playerName), playerTypeText = player.PlayerName, player.PlayerType |> playerTypeText
        let draftLeft, draftRight = draftLeftAndRight playerId player
        let pickedBy = player.PickedBy |> pickedByTag theme userDic authUser
        let pickedByUserId, pickedDate = match player.PickedBy with | Some (userId, _, date) -> userId |> Some, date |> Some | None -> None, None
        let points, pickedByPoints = playerPoints fixtureDic (squadId, playerId) pickedDate
        let score = score points pickedByPoints pickedByUserId userDic
        tr false [
            td [ [ str playerName ] |> para theme paraDefaultSmallest ]
            td [ Rct.ofOption (editName playerId) ]
            td [ Rct.ofOption (withdrawn player) ]
            td [ [ str playerTypeText ] |> para theme paraCentredSmallest ]
            td [ Rct.ofOption (changePosition playerId) ]
            td [ Rct.ofOption draftLeft ]
            td [ Rct.ofOption draftRight ]
            td [ Rct.ofOption pickedBy ]
            td [ [ score ] |> para theme { paraDefaultSmallest with ParaAlignment = RightAligned } ]
            td [ Rct.ofOption (withdraw playerId player) ] ]
    let players = playerDic |> List.ofSeq |> List.map (fun (KeyValue (playerId, player)) -> (playerId, player)) |> List.sortBy (fun (_, player) ->
        player.PlayerType |> playerTypeSortOrder, player.PlayerName)
    let playerRows = players |> List.map playerRow
    div divCentred [
        if playerDic.Count > 0 then
            yield table theme false { tableDefault with IsNarrow = true ; IsFullWidth = true } [
                thead [ 
                    tr false [
                        th [ [ bold "Player" ] |> para theme paraDefaultSmallest ]
                        th []
                        th []
                        th [ [ bold "Position" ] |> para theme paraCentredSmallest ]
                        th []
                        th []
                        th []
                        th [ [ bold "Picked by" ] |> para theme paraDefaultSmallest ]
                        th [ [ bold "Score" ] |> para theme { paraDefaultSmallest with ParaAlignment = RightAligned } ]
                        th [] ] ]
                tbody [ yield! playerRows ] ]
        else yield [ str "Player details coming soon" ] |> para theme paraCentredSmaller ]    

let private addPlayers theme squadId squad authUser dispatch =
    let nonWithdrawnCount = squad |> Some |> nonWithdrawnCount
    let paraAddPlayers = { paraDefaultSmallest with ParaAlignment = RightAligned }
    match squad.Eliminated, authUser with
    | false, Some authUser ->
        match authUser.Permissions.SquadPermissions with
        | Some squadPermissions ->
            if squadPermissions.AddOrEditPlayerPermission then
                if nonWithdrawnCount < MAX_PLAYERS_PER_SQUAD  then
                    [ [ str "Add player/s" ] |> para theme paraAddPlayers ] |> link theme (ClickableLink (fun _ -> squadId |> ShowAddPlayersModal |> dispatch)) |> Some
                else
                    [ italic squadIsFullText ] |> para theme paraAddPlayers |> Some
            else None
        | None -> None
    | _, _ -> None

let render (useDefaultTheme, state, authUser:AuthUser option, squadsProjection:Projection<_ * SquadDic>, usersProjection:Projection<_ * UserDic>, fixturesProjection:Projection<_ * FixtureDic>, draftsProjection:Projection<_ * DraftDic * CurrentUserDraftDto option> option, hasModal) dispatch =
    let theme = getTheme useDefaultTheme
    columnContent [
        yield [ bold "Squads" ] |> para theme paraCentredSmall
        yield hr theme false
        match squadsProjection, usersProjection, fixturesProjection with
        | Pending, _, _ | _, Pending, _ | _, _, Pending ->
            yield div divCentred [ icon iconSpinnerPulseLarge ]
        | Failed, _, _ | _, Failed, _ | _, _, Failed -> // note: should never happen
            yield [ str "This functionality is not currently available" ] |> para theme { paraCentredSmallest with ParaColour = SemanticPara Danger ; Weight = Bold }
        | Ready (_, squadDic), Ready (_, userDic), Ready (_, fixtureDic) ->
            let currentDraft, userDraftPickDic =
                match draftsProjection with
                | Some draftsProjection ->
                    match draftsProjection with
                    | Ready (_, draftDic, currentUserDraftDto) -> draftDic |> currentDraft, currentUserDraftDto |> userDraftPickDic
                    | Pending | Failed -> None, UserDraftPickDic ()
                | None -> None, UserDraftPickDic ()
            let currentGroup, currentSquadId =
                match state.CurrentSquadId with
                | Some currentSquadId -> currentSquadId |> Some |> group squadDic, currentSquadId |> Some
                | None ->
                    let currentGroup = match state.CurrentGroup with | Some group -> group | None -> GroupA
                    let currentSquadId = currentGroup |> defaultSquadId squadDic
                    currentGroup |> Some, currentSquadId
            let groupTabs = groups |> List.map (groupTab currentGroup dispatch)
            let squadTabs = squadDic |> squadTabs currentSquadId dispatch
            let pickedCounts =
                match authUser with
                | Some authUser ->
                    let squad, players = authUser.UserId |> pickedByUser squadDic
                    (squad, players) |> pickedCounts
                | None -> 0, 0, 0
            match hasModal, state.AddPlayersState with
            | false, Some addPlayersState ->
                yield div divDefault [ lazyViewOrHMR2 renderAddPlayersModal (useDefaultTheme, squadDic, addPlayersState) (AddPlayersInput >> dispatch) ]
            | _ -> ()
            match hasModal, state.ChangePlayerNameState with
            | false, Some changePlayerNameState ->
                yield div divDefault [ lazyViewOrHMR2 renderChangePlayerNameModal (useDefaultTheme, squadDic, changePlayerNameState) (ChangePlayerNameInput >> dispatch) ]
            | _ -> ()
            match hasModal, state.ChangePlayerTypeState with
            | false, Some changePlayerTypeState ->
                yield div divDefault [ lazyViewOrHMR2 renderChangePlayerTypeModal (useDefaultTheme, squadDic, changePlayerTypeState) (ChangePlayerTypeInput >> dispatch) ]
            | _ -> ()
            match hasModal, state.WithdrawPlayerState with
            | false, Some withdrawPlayerState ->
                yield div divDefault [ lazyViewOrHMR2 renderWithdrawPlayerModal (useDefaultTheme, squadDic, withdrawPlayerState) (WithdrawPlayerInput >> dispatch) ]
            | _ -> ()           
            match hasModal, state.EliminateSquadState with
            | false, Some eliminateSquadState ->
                yield div divDefault [ lazyViewOrHMR2 renderEliminateSquadModal (useDefaultTheme, squadDic, eliminateSquadState) (EliminateSquadInput >> dispatch) ]
            | _ -> ()
            match hasModal, state.FreePickState with
            | false, Some freePickState ->
                yield div divDefault [ lazyViewOrHMR2 renderFreePickModal (useDefaultTheme, squadDic, freePickState) (FreePickInput >> dispatch) ]
            | _ -> ()
            yield div divCentred [ tabs theme { tabsDefault with Tabs = groupTabs } ]
            match squadTabs with
            | _ :: _ ->
                yield div divCentred [ tabs theme { tabsDefault with TabsSize = Normal ; Tabs = squadTabs } ]
            | [] -> () // note: should never happen           
            match currentSquadId with
            | Some currentSquadId when currentSquadId |> squadDic.ContainsKey ->
                let squad = squadDic.[currentSquadId]
                let pendingPicks = state.PendingPicksState.PendingPicks
                yield br
                yield lazyViewOrHMR2 renderSquad (useDefaultTheme, currentSquadId, squad, currentDraft, pickedCounts, userDraftPickDic, pendingPicks, userDic, fixtureDic, authUser) dispatch
                yield br
                yield lazyViewOrHMR2 renderPlayers (useDefaultTheme, squad.PlayerDic, currentSquadId, squad, currentDraft, pickedCounts, userDraftPickDic, pendingPicks, userDic, fixtureDic, authUser) dispatch
                yield Rct.ofOption (addPlayers theme currentSquadId squad authUser dispatch)
            | Some _ | None -> () ] // note: should never happen
