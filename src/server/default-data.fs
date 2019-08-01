module Aornota.Sweepstake2019.Server.DefaultData

open Aornota.Sweepstake2019.Common.Domain.Core
open Aornota.Sweepstake2019.Common.Domain.Draft
open Aornota.Sweepstake2019.Common.Domain.Fixture
open Aornota.Sweepstake2019.Common.Domain.Squad
open Aornota.Sweepstake2019.Common.Domain.User
open Aornota.Sweepstake2019.Common.IfDebug
open Aornota.Sweepstake2019.Common.Revision
open Aornota.Sweepstake2019.Common.WsApi.ServerMsg
open Aornota.Sweepstake2019.Server.Agents.ConsoleLogger
open Aornota.Sweepstake2019.Server.Agents.Entities.Drafts
open Aornota.Sweepstake2019.Server.Agents.Entities.Fixtures
open Aornota.Sweepstake2019.Server.Agents.Entities.Squads
open Aornota.Sweepstake2019.Server.Agents.Entities.Users
open Aornota.Sweepstake2019.Server.Agents.Persistence
open Aornota.Sweepstake2019.Server.Authorization
open Aornota.Sweepstake2019.Server.Common.Helpers

open System
open System.IO

let private deleteExistingUsersEvents = ifDebug false false // note: should *not* generally set to true for Release (and only with caution for Debug!)
let private deleteExistingSquadsEvents = ifDebug false false // note: should *not* generally set to true for Release (and only with caution for Debug!)
let private deleteExistingFixturesEvents = ifDebug false false // note: should *not* generally set to true for Release (and only with caution for Debug!)
let private deleteExistingDraftsEvents = ifDebug false false // note: should *not* generally set to true for Release (and only with caution for Debug!)

let private log category = (Host, category) |> consoleLogger.Log

let private logResult shouldSucceed scenario result =
    match shouldSucceed, result with
    | true, Ok _ -> sprintf "%s -> succeeded (as expected)" scenario |> Verbose |> log
    | true, Error error -> sprintf "%s -> unexpectedly failed -> %A" scenario error |> Danger |> log
    | false, Ok _ -> sprintf "%s -> unexpectedly succeeded" scenario |> Danger |> log
    | false, Error error -> sprintf "%s -> failed (as expected) -> %A" scenario error |> Verbose |> log
let private logShouldSucceed scenario result = result |> logResult true scenario
let private logShouldFail scenario result = result |> logResult false scenario

let private delete dir =
    Directory.GetFiles dir |> Array.iter File.Delete
    Directory.Delete dir

let private ifToken fCmdAsync token = async { return! match token with | Some token -> token |> fCmdAsync | None -> NotAuthorized |> AuthCmdAuthznError |> Error |> thingAsync }

let private superUser = SuperUser
let private nephId = Guid.Empty |> UserId
let private nephTokens = permissions nephId superUser |> UserTokens

// #region SquadIds
let private irelandId = Guid "00000011-0000-0000-0000-000000000000" |> SquadId
let private japanId = Guid "00000012-0000-0000-0000-000000000000" |> SquadId
let private russiaId = Guid "00000013-0000-0000-0000-000000000000" |> SquadId
let private samoaId = Guid "00000014-0000-0000-0000-000000000000" |> SquadId
let private scotlandId = Guid "00000015-0000-0000-0000-000000000000" |> SquadId
let private canadaId = Guid "00000021-0000-0000-0000-000000000000" |> SquadId
let private italyId = Guid "00000022-0000-0000-0000-000000000000" |> SquadId
let private namibiaId = Guid "00000023-0000-0000-0000-000000000000" |> SquadId
let private newZealandId = Guid "00000024-0000-0000-0000-000000000000" |> SquadId
let private southAfricaId = Guid "00000025-0000-0000-0000-000000000000" |> SquadId
let private argentinaId = Guid "00000031-0000-0000-0000-000000000000" |> SquadId
let private englandId = Guid "00000032-0000-0000-0000-000000000000" |> SquadId
let private franceId = Guid "00000033-0000-0000-0000-000000000000" |> SquadId
let private tongaId = Guid "00000034-0000-0000-0000-000000000000" |> SquadId
let private unitedStatesId = Guid "00000035-0000-0000-0000-000000000000" |> SquadId
let private australiaId = Guid "00000041-0000-0000-0000-000000000000" |> SquadId
let private fijiId = Guid "00000042-0000-0000-0000-000000000000" |> SquadId
let private georgiaId = Guid "00000043-0000-0000-0000-000000000000" |> SquadId
let private uruguayId = Guid "00000044-0000-0000-0000-000000000000" |> SquadId
let private walesId = Guid "00000045-0000-0000-0000-000000000000" |> SquadId
// #endregion

let private createInitialUsersEventsIfNecessary = async {
    let usersDir = directory EntityType.Users

    // #region: Force re-creation of initial User/s events if directory already exists (if requested)
    if deleteExistingUsersEvents && Directory.Exists usersDir then
        sprintf "deleting existing User/s events -> %s" usersDir |> Info |> log
        delete usersDir
    // #endregion

    if Directory.Exists usersDir then sprintf "preserving existing User/s events -> %s" usersDir |> Info |> log
    else
        sprintf "creating initial User/s events -> %s" usersDir |> Info |> log
        "starting Users agent" |> Info |> log
        () |> users.Start
        // Note: Send dummy OnUsersEventsRead to Users agent to ensure that it transitions [from pendingOnUsersEventsRead] to managingUsers; otherwise HandleCreateUserCmdAsync (&c.) would be ignored (and block).
        "sending dummy OnUsersEventsRead to Users agent" |> Info |> log
        [] |> users.OnUsersEventsRead

        // #region: Create initial SuperUser | Administators - and a couple of Plebs
        let neph = UserName "neph"
        let dummyPassword = Password "password"
        let! result = nephTokens.CreateUserToken |> ifToken (fun token -> (token, nephId, nephId, neph, dummyPassword, superUser) |> users.HandleCreateUserCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateUserCmdAsync (%A)" neph)
        let administrator = Administrator
        let rosieId, rosie = Guid "ffffffff-0001-0000-0000-000000000000" |> UserId, UserName "rosie"
        let! result = nephTokens.CreateUserToken |> ifToken (fun token -> (token, nephId, rosieId, rosie, dummyPassword, administrator) |> users.HandleCreateUserCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateUserCmdAsync (%A)" rosie)
        let hughId, hugh = Guid "ffffffff-0002-0000-0000-000000000000" |> UserId, UserName "hugh"
        let! result = nephTokens.CreateUserToken |> ifToken (fun token -> (token, nephId, hughId, hugh, dummyPassword, administrator) |> users.HandleCreateUserCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateUserCmdAsync (%A)" hugh)
        let pleb = Pleb
        let robId, rob = Guid "ffffffff-ffff-0001-0000-000000000000" |> UserId, UserName "rob"
        let! result = nephTokens.CreateUserToken |> ifToken (fun token -> (token, nephId, robId, rob, dummyPassword, pleb) |> users.HandleCreateUserCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateUserCmdAsync (%A)" rob)
        let joshId, josh = Guid "ffffffff-ffff-0002-0000-000000000000" |> UserId, UserName "josh"
        let! result = nephTokens.CreateUserToken |> ifToken (fun token -> (token, nephId, joshId, josh, dummyPassword, pleb) |> users.HandleCreateUserCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateUserCmdAsync (%A)" josh)
        // #endregion

        // #region: TEMP-NMB: Test various scenarios (note: expects initial SuperUser | Administrators | Plebs to have been created)...
        (*let newDummyPassword = Password "drowssap"
        let rosieTokens = permissions rosieId adminType |> UserTokens
        let willId, will = Guid "ffffffff-ffff-0003-0000-000000000000" |> UserId, UserName "will"
        let personaNonGrataId, personaNonGrata = Guid "ffffffff-ffff-ffff-0001-000000000000" |> UserId, UserName "persona non grata"
        let unknownUserId, unknownUser = Guid.NewGuid () |> UserId, UserName "unknown"
        let personaNonGrataTokens = permissions personaNonGrataId PersonaNonGrata |> UserTokens
        let unknownUserTokens = permissions unknownUserId Pleb |> UserTokens
        // Test HandleSignInCmdAsync:
        let! result = (neph, dummyPassword) |> users.HandleSignInCmdAsync
        result |> logShouldSucceed (sprintf "HandleSignInCmdAsync (%A)" neph)
        let! result = (UserName String.Empty, dummyPassword) |> users.HandleSignInCmdAsync
        result |> logShouldFail "HandleSignInCmdAsync (invalid userName: blank)"
        let! result = (UserName "bob", dummyPassword) |> users.HandleSignInCmdAsync
        result |> logShouldFail "HandleSignInCmdAsync (invalid userName: too short)"
        let! result = (neph, Password String.Empty) |> users.HandleSignInCmdAsync
        result |> logShouldFail "HandleSignInCmdAsync (invalid password: blank)"
        let! result = (neph, Password "1234") |> users.HandleSignInCmdAsync
        result |> logShouldFail "HandleSignInCmdAsync (invalid password: too short)"
        let! result = (neph, Password "PASSWORD") |> users.HandleSignInCmdAsync
        result |> logShouldFail "HandleSignInCmdAsync (incorrect password: case-sensitive)"
        let! result = (unknownUser, dummyPassword) |> users.HandleSignInCmdAsync
        result |> logShouldFail "HandleSignInCmdAsync (unknown userName)"
        let! result = nephTokens.CreateUserToken |> ifToken (fun token -> (token, nephId, personaNonGrataId, personaNonGrata, dummyPassword, PersonaNonGrata) |> users.HandleCreateUserCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateUserCmdAsync (%A)" personaNonGrata)
        let! result = (personaNonGrata, dummyPassword) |> users.HandleSignInCmdAsync
        result |> logShouldFail "HandleSignInCmdAsync (PersonaNonGrata)"
        // Test HandleChangePasswordCmdAsync:
        let! result = nephTokens.ChangePasswordToken |> ifToken (fun token -> (token, nephId, initialRvn, newDummyPassword) |> users.HandleChangePasswordCmdAsync)
        result |> logShouldSucceed (sprintf "HandleChangePasswordCmdAsync (%A)" neph)
        let! result = rosieTokens.ChangePasswordToken |> ifToken (fun token -> (token, hughId, Rvn 2, newDummyPassword) |> users.HandleChangePasswordCmdAsync)
        result |> logShouldFail "HandleChangePasswordCmdAsync (invalid ChangePasswordToken: userId differs from auditUserId)"
        let! result = personaNonGrataTokens.ChangePasswordToken |> ifToken (fun token -> (token, personaNonGrataId, Rvn 2, newDummyPassword) |> users.HandleChangePasswordCmdAsync)
        result |> logShouldFail "HandleChangePasswordCmdAsync (no ChangePasswordToken)"
        let! result = unknownUserTokens.ChangePasswordToken |> ifToken (fun token -> (token, unknownUserId, Rvn 2, newDummyPassword) |> users.HandleChangePasswordCmdAsync)
        result |> logShouldFail "HandleChangePasswordCmdAsync (unknown auditUserId)"
        let! result = nephTokens.ChangePasswordToken |> ifToken (fun token -> (token, nephId, Rvn 2, Password String.Empty) |> users.HandleChangePasswordCmdAsync)
        result |> logShouldFail "HandleChangePasswordCmdAsync (invalid password: blank)"
        let! result = nephTokens.ChangePasswordToken |> ifToken (fun token -> (token, nephId, Rvn 2, Password "1234") |> users.HandleChangePasswordCmdAsync)
        result |> logShouldFail "HandleChangePasswordCmdAsync (invalid password: too short)"
        let! result = nephTokens.ChangePasswordToken |> ifToken (fun token -> (token, nephId, Rvn 2, newDummyPassword) |> users.HandleChangePasswordCmdAsync)
        result |> logShouldFail "HandleChangePasswordCmdAsync (invalid password: same as current)"
        let! result = nephTokens.ChangePasswordToken |> ifToken (fun token -> (token, nephId, Rvn 3, Password "pa$$word") |> users.HandleChangePasswordCmdAsync)
        result |> logShouldFail "HandleChangePasswordCmdAsync (invalid current Rvn)"
        // Test HandleCreateUserCmdAsync:
        let! result = rosieTokens.CreateUserToken |> ifToken (fun token -> (token, rosieId, willId, will, dummyPassword, Pleb) |> users.HandleCreateUserCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateUserCmdAsync (%A)" will)
        let! result = rosieTokens.CreateUserToken |> ifToken (fun token -> (token, rosieId, unknownUserId, unknownUser, dummyPassword, Administrator) |> users.HandleCreateUserCmdAsync)
        result |> logShouldFail "HandleCreateUserCmdAsync (invalid CreateUserToken: UserType not allowed)"
        let! result = personaNonGrataTokens.CreateUserToken |> ifToken (fun token -> (token, personaNonGrataId, unknownUserId, unknownUser, dummyPassword, Administrator) |> users.HandleCreateUserCmdAsync)
        result |> logShouldFail "HandleCreateUserCmdAsync (no CreateUserToken)"
        let! result = nephTokens.CreateUserToken |> ifToken (fun token -> (token, nephId, unknownUserId, UserName String.Empty, dummyPassword, Administrator) |> users.HandleCreateUserCmdAsync)
        result |> logShouldFail "HandleCreateUserCmdAsync (invalid userName: blank)"
        let! result = nephTokens.CreateUserToken |> ifToken (fun token -> (token, nephId, unknownUserId, UserName "bob", dummyPassword, Administrator) |> users.HandleCreateUserCmdAsync)
        result |> logShouldFail "HandleCreateUserCmdAsync (invalid userName: too short)"
        let! result = nephTokens.CreateUserToken |> ifToken (fun token -> (token, nephId, unknownUserId, unknownUser, Password String.Empty, Administrator) |> users.HandleCreateUserCmdAsync)
        result |> logShouldFail "HandleCreateUserCmdAsync (invalid password: blank)"
        let! result = nephTokens.CreateUserToken |> ifToken (fun token -> (token, nephId, unknownUserId, unknownUser, Password "1234", Administrator) |> users.HandleCreateUserCmdAsync)
        result |> logShouldFail "HandleCreateUserCmdAsync (invalid password: too short)"
        let! result = nephTokens.CreateUserToken |> ifToken (fun token -> (token, nephId, rosieId, rosie, dummyPassword, Administrator) |> users.HandleCreateUserCmdAsync)
        result |> logShouldFail "HandleCreateUserCmdAsync (userId already exists)"
        let! result = nephTokens.CreateUserToken |> ifToken (fun token -> (token, nephId, unknownUserId, rosie, dummyPassword, Administrator) |> users.HandleCreateUserCmdAsync)
        result |> logShouldFail "HandleCreateUserCmdAsync (userName already exists)"
        // Test HandleResetPasswordCmdAsync:
        let! result = nephTokens.ResetPasswordToken |> ifToken (fun token -> (token, nephId, rosieId, initialRvn, newDummyPassword) |> users.HandleResetPasswordCmdAsync)
        result |> logShouldSucceed (sprintf "HandleResetPasswordCmdAsync (%A)" will)
        let! result = rosieTokens.ResetPasswordToken |> ifToken (fun token -> (token, rosieId, willId, initialRvn, newDummyPassword) |> users.HandleResetPasswordCmdAsync)
        result |> logShouldSucceed (sprintf "HandleResetPasswordCmdAsync (%A)" will)
        let! result = nephTokens.ResetPasswordToken |> ifToken (fun token -> (token, nephId, nephId, Rvn 2, dummyPassword) |> users.HandleResetPasswordCmdAsync)
        result |> logShouldFail "HandleResetPasswordCmdAsync (invalid ResetPasswordToken: valid UserTarget is NotSelf)"
        let! result = rosieTokens.ResetPasswordToken |> ifToken (fun token -> (token, rosieId, nephId, Rvn 2, dummyPassword) |> users.HandleResetPasswordCmdAsync)
        result |> logShouldFail "HandleResetPasswordCmdAsync (invalid ResetPasswordToken: UserType for UserTarget not allowed)"
        let! result = personaNonGrataTokens.ResetPasswordToken |> ifToken (fun token -> (token, personaNonGrataId, nephId, Rvn 2, dummyPassword) |> users.HandleResetPasswordCmdAsync)
        result |> logShouldFail "HandleResetPasswordCmdAsync (no ResetPasswordToken)"
        let! result = nephTokens.ResetPasswordToken |> ifToken (fun token -> (token, nephId, unknownUserId, initialRvn, newDummyPassword) |> users.HandleResetPasswordCmdAsync)
        result |> logShouldFail "HandleResetPasswordCmdAsync (unknown userId)"
        let! result = nephTokens.ResetPasswordToken |> ifToken (fun token -> (token, nephId, willId, Rvn 2, Password String.Empty) |> users.HandleResetPasswordCmdAsync)
        result |> logShouldFail "HandleResetPasswordCmdAsync (invalid password; blank)"
        let! result = nephTokens.ResetPasswordToken |> ifToken (fun token -> (token, nephId, willId, Rvn 2, Password "1234") |> users.HandleResetPasswordCmdAsync)
        result |> logShouldFail "HandleResetPasswordCmdAsync (invalid password; too short)"
        let! result = nephTokens.ResetPasswordToken |> ifToken (fun token -> (token, nephId, willId, Rvn 0, Password "pa$$word") |> users.HandleResetPasswordCmdAsync)
        result |> logShouldFail "HandleResetPasswordCmdAsync (invalid current Rvn)"
        // Test HandleChangeUserTypeCmdAsync:
        let! result = nephTokens.ChangeUserTypeToken |> ifToken (fun token -> (token, nephId, hughId, initialRvn, Pleb) |> users.HandleChangeUserTypeCmdAsync)
        result |> logShouldSucceed (sprintf "HandleChangeUserTypeCmdAsync (%A %A)" hugh Pleb)
        let! result = nephTokens.ChangeUserTypeToken |> ifToken (fun token -> (token, nephId, nephId, Rvn 2, Administrator) |> users.HandleChangeUserTypeCmdAsync)
        result |> logShouldFail "HandleChangeUserTypeCmdAsync (invalid ChangeUserTypeToken: valid UserTarget is NotSelf)"
        // Note: Cannot test "UserType for UserTarget not allowed" or "UserType not allowed" as only SuperUsers have ChangeUserTypePermission - and they have it for all UserTypes.
        let! result = rosieTokens.ChangeUserTypeToken |> ifToken (fun token -> (token, rosieId, nephId, Rvn 2, Administrator) |> users.HandleChangeUserTypeCmdAsync)
        result |> logShouldFail "HandleChangeUserTypeCmdAsync (no ChangeUserTypeToken)"
        let! result = nephTokens.ChangeUserTypeToken |> ifToken (fun token -> (token, nephId, unknownUserId, initialRvn, Administrator) |> users.HandleChangeUserTypeCmdAsync)
        result |> logShouldFail "HandleChangeUserTypeCmdAsync (unknown userId)"
        let! result = nephTokens.ChangeUserTypeToken |> ifToken (fun token -> (token, nephId, hughId, Rvn 2, Pleb) |> users.HandleChangeUserTypeCmdAsync)
        result |> logShouldFail "HandleChangeUserTypeCmdAsync (invalid userType: same as current)"
        let! result = nephTokens.ChangeUserTypeToken |> ifToken (fun token -> (token, nephId, hughId, initialRvn, SuperUser) |> users.HandleChangeUserTypeCmdAsync)
        result |> logShouldFail "HandleChangeUserTypeCmdAsync (invalid current Rvn)"*)
        // #endregion

        // Note: Reset Users agent [to pendingOnUsersEventsRead] so that it handles subsequent UsersEventsRead event appropriately (i.e. from readPersistedEvents).
        "resetting Users agent" |> Info |> log
        () |> users.Reset
    return () }

let private createInitialSquadsEventsIfNecessary = async {
    let squadsDir = directory EntityType.Squads

    // #region: Force re-creation of initial Squad/s events if directory already exists (if requested)
    if deleteExistingSquadsEvents && Directory.Exists squadsDir then
        sprintf "deleting existing Squad/s events -> %s" squadsDir |> Info |> log
        delete squadsDir
    // #endregion

    if Directory.Exists squadsDir then sprintf "preserving existing Squad/s events -> %s" squadsDir |> Info |> log
    else
        sprintf "creating initial Squad/s events -> %s" squadsDir |> Info |> log
        "starting Squads agent" |> Info |> log
        () |> squads.Start
        // Note: Send dummy OnSquadsEventsRead to Squads agent to ensure that it transitions [from pendingOnSquadsEventsRead] to managingSquads; otherwise HandleCreateSquadCmdAsync (&c.) would be ignored (and block).
        "sending dummy OnSquadsEventsRead to Squads agent" |> Info |> log
        [] |> squads.OnSquadsEventsRead

        // #region: Create initial Squads.

        // TODO-NMB: Add Players dev/test data?...

        // #region: Group A - Ireland | Scotland | Japan | Russia | Samoa
        let ireland = SquadName "Ireland"
        let! result = nephTokens.CreateSquadToken |> ifToken (fun token -> (token, nephId, irelandId, ireland, GroupA, Seeding 4, CoachName "TODO-NMB...Ireland coach") |> squads.HandleCreateSquadCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateSquadCmdAsync (%A)" ireland)
        let scotland = SquadName "Scotland"
        let! result = nephTokens.CreateSquadToken |> ifToken (fun token -> (token, nephId, scotlandId, scotland, GroupA, Seeding 5, CoachName "TODO-NMB...Scotland coach") |> squads.HandleCreateSquadCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateSquadCmdAsync (%A)" scotland)
        let japan = SquadName "Japan"
        let! result = nephTokens.CreateSquadToken |> ifToken (fun token -> (token, nephId, japanId, japan, GroupA, Seeding 11, CoachName "TODO-NMB...Japan coach") |> squads.HandleCreateSquadCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateSquadCmdAsync (%A)" japan)
        let russia = SquadName "Russia"
        let! result = nephTokens.CreateSquadToken |> ifToken (fun token -> (token, nephId, russiaId, russia, GroupA, Seeding 999, CoachName "TODO-NMB...Russia coach") |> squads.HandleCreateSquadCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateSquadCmdAsync (%A)" russia)
        let samoa = SquadName "Samoa"
        let! result = nephTokens.CreateSquadToken |> ifToken (fun token -> (token, nephId, samoaId, samoa, GroupA, Seeding 999, CoachName "TODO-NMB...Samoa coach") |> squads.HandleCreateSquadCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateSquadCmdAsync (%A)" samoa)
        // #endregion
        // #region: Group B - New Zealand | South Africa | Italy | Namibia | Canada
        let newZealand = SquadName "New Zealand"
        let! result = nephTokens.CreateSquadToken |> ifToken (fun token -> (token, nephId, newZealandId, newZealand, GroupB, Seeding 1, CoachName "TODO-NMB...New Zealand coach") |> squads.HandleCreateSquadCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateSquadCmdAsync (%A)" newZealand)
        let southAfrica = SquadName "South Africa"
        let! result = nephTokens.CreateSquadToken |> ifToken (fun token -> (token, nephId, southAfricaId, southAfrica, GroupB, Seeding 7, CoachName "TODO-NMB...South Africa coach") |> squads.HandleCreateSquadCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateSquadCmdAsync (%A)" southAfrica)
        let italy = SquadName "Italy"
        let! result = nephTokens.CreateSquadToken |> ifToken (fun token -> (token, nephId, italyId, italy, GroupB, Seeding 15, CoachName "TODO-NMB...Italy coach") |> squads.HandleCreateSquadCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateSquadCmdAsync (%A)" italy)
        let namibia = SquadName "Namibia"
        let! result = nephTokens.CreateSquadToken |> ifToken (fun token -> (token, nephId, namibiaId, namibia, GroupB, Seeding 999, CoachName "TODO-NMB...Namibia coach") |> squads.HandleCreateSquadCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateSquadCmdAsync (%A)" namibia)
        let canada = SquadName "Canada"
        let! result = nephTokens.CreateSquadToken |> ifToken (fun token -> (token, nephId, canadaId, canada, GroupB, Seeding 999, CoachName "TODO-NMB...Canada coach") |> squads.HandleCreateSquadCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateSquadCmdAsync (%A)" canada)
        // #endregion
        // #region: Group C - England | France | Argentina | United States | Tonga
        let england = SquadName "England"
        let! result = nephTokens.CreateSquadToken |> ifToken (fun token -> (token, nephId, englandId, england, GroupC, Seeding 2, CoachName "TODO-NMB...England coach") |> squads.HandleCreateSquadCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateSquadCmdAsync (%A)" england)
        let france = SquadName "France"
        let! result = nephTokens.CreateSquadToken |> ifToken (fun token -> (token, nephId, franceId, france, GroupC, Seeding 6, CoachName "TODO-NMB...France coach") |> squads.HandleCreateSquadCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateSquadCmdAsync (%A)" france)
        let argentina = SquadName "Argentina"
        let! result = nephTokens.CreateSquadToken |> ifToken (fun token -> (token, nephId, argentinaId, argentina, GroupC, Seeding 9, CoachName "TODO-NMB...Argentina coach") |> squads.HandleCreateSquadCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateSquadCmdAsync (%A)" argentina)
        let unitedStates = SquadName "United States"
        let! result = nephTokens.CreateSquadToken |> ifToken (fun token -> (token, nephId, unitedStatesId, unitedStates, GroupC, Seeding 999, CoachName "TODO-NMB...United States coach") |> squads.HandleCreateSquadCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateUseHandleCreateSquadCmdAsyncrCmdAsync (%A)" unitedStates)
        let tonga = SquadName "Tonga"
        let! result = nephTokens.CreateSquadToken |> ifToken (fun token -> (token, nephId, tongaId, tonga, GroupC, Seeding 999, CoachName "TODO-NMB...Tonga coach") |> squads.HandleCreateSquadCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateUseHandleCreateSquadCmdAsyncrCmdAsync (%A)" tonga)
        // #endregion
        // #region: Group D - Australia | Wales | Georgia | Fiji | Uruguay
        let australia = SquadName "Australia"
        let! result = nephTokens.CreateSquadToken |> ifToken (fun token -> (token, nephId, australiaId, australia, GroupD, Seeding 3, CoachName "TODO-NMB...Australia coach") |> squads.HandleCreateSquadCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateSquadCmdAsync (%A)" australia)
        let wales = SquadName "Wales"
        let! result = nephTokens.CreateSquadToken |> ifToken (fun token -> (token, nephId, walesId, wales, GroupD, Seeding 8, CoachName "TODO-NMB...Wales coach") |> squads.HandleCreateSquadCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateSquadCmdAsync (%A)" wales)
        let georgia = SquadName "Georgia"
        let! result = nephTokens.CreateSquadToken |> ifToken (fun token -> (token, nephId, georgiaId, georgia, GroupD, Seeding 12, CoachName "TODO-NMB...Georgia coach") |> squads.HandleCreateSquadCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateSquadCmdAsync (%A)" georgia)
        let fiji = SquadName "Fiji"
        let! result = nephTokens.CreateSquadToken |> ifToken (fun token -> (token, nephId, fijiId, fiji, GroupD, Seeding 999, CoachName "TODO-NMB...Fiji coach") |> squads.HandleCreateSquadCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateSquadCmdAsync (%A)" fiji)
        let uruguay = SquadName "Uruguay"
        let! result = nephTokens.CreateSquadToken |> ifToken (fun token -> (token, nephId, uruguayId, uruguay, GroupD, Seeding 999, CoachName "TODO-NMB...Uruguay coach") |> squads.HandleCreateSquadCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateSquadCmdAsync (%A)" uruguay)
        // #endregion

        // #region: TEMP-NMB: (England) Players...
        (* let jackButlandId, jackButland = Guid "00000072-0001-0000-0000-000000000000" |> PlayerId, PlayerName "Jack Butland"
        let! result = nephTokens.AddOrEditPlayerToken |> ifToken (fun token -> (token, nephId, englandId, initialRvn, jackButlandId, jackButland, Goalkeeper) |> squads.HandleAddPlayerCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddPlayerCmdAsync (%A %A)" england jackButland)
        let jordanPickfordId, jordanPickford = Guid "00000072-0002-0000-0000-000000000000" |> PlayerId, PlayerName "Jordan Pickford"
        let! result = nephTokens.AddOrEditPlayerToken |> ifToken (fun token -> (token, nephId, englandId, Rvn 2, jordanPickfordId, jordanPickford, Goalkeeper) |> squads.HandleAddPlayerCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddPlayerCmdAsync (%A %A)" england jordanPickford)
        let nickPopeId, nickPope = Guid "00000072-0003-0000-0000-000000000000" |> PlayerId, PlayerName "Nick Pope"
        let! result = nephTokens.AddOrEditPlayerToken |> ifToken (fun token -> (token, nephId, englandId, Rvn 3, nickPopeId, nickPope, Goalkeeper) |> squads.HandleAddPlayerCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddPlayerCmdAsync (%A %A)" england nickPope)
        let trentAlexanderArnoldId, trentAlexanderArnold = Guid "00000072-0000-0001-0000-000000000000" |> PlayerId, PlayerName "Trent Alexander-Arnold"
        let! result = nephTokens.AddOrEditPlayerToken |> ifToken (fun token -> (token, nephId, englandId, Rvn 4, trentAlexanderArnoldId, trentAlexanderArnold, Defender) |> squads.HandleAddPlayerCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddPlayerCmdAsync (%A %A)" england trentAlexanderArnold)
        let garyCahillId, garyCahill = Guid "00000072-0000-0002-0000-000000000000" |> PlayerId, PlayerName "Gary Cahill"
        let! result = nephTokens.AddOrEditPlayerToken |> ifToken (fun token -> (token, nephId, englandId, Rvn 5, garyCahillId, garyCahill, Defender) |> squads.HandleAddPlayerCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddPlayerCmdAsync (%A %A)" england garyCahill)
        let fabianDelphId, fabianDelph = Guid "00000072-0000-0003-0000-000000000000" |> PlayerId, PlayerName "Fabian Delph"
        let! result = nephTokens.AddOrEditPlayerToken |> ifToken (fun token -> (token, nephId, englandId, Rvn 6, fabianDelphId, fabianDelph, Defender) |> squads.HandleAddPlayerCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddPlayerCmdAsync (%A %A)" england fabianDelph)
        let philJonesId, philJones = Guid "00000072-0000-0004-0000-000000000000" |> PlayerId, PlayerName "Phil Jones"
        let! result = nephTokens.AddOrEditPlayerToken |> ifToken (fun token -> (token, nephId, englandId, Rvn 7, philJonesId, philJones, Defender) |> squads.HandleAddPlayerCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddPlayerCmdAsync (%A %A)" england philJones)
        let harryMaguireId, harryMaguire = Guid "00000072-0000-0005-0000-000000000000" |> PlayerId, PlayerName "Harry Maguire"
        let! result = nephTokens.AddOrEditPlayerToken |> ifToken (fun token -> (token, nephId, englandId, Rvn 8, harryMaguireId, harryMaguire, Defender) |> squads.HandleAddPlayerCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddPlayerCmdAsync (%A %A)" england harryMaguire)
        let dannyRoseId, dannyRose = Guid "00000072-0000-0006-0000-000000000000" |> PlayerId, PlayerName "Danny Rose"
        let! result = nephTokens.AddOrEditPlayerToken |> ifToken (fun token -> (token, nephId, englandId, Rvn 9, dannyRoseId, dannyRose, Defender) |> squads.HandleAddPlayerCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddPlayerCmdAsync (%A %A)" england dannyRose)
        let johnStonesId, johnStones = Guid "00000072-0000-0007-0000-000000000000" |> PlayerId, PlayerName "John Stones"
        let! result = nephTokens.AddOrEditPlayerToken |> ifToken (fun token -> (token, nephId, englandId, Rvn 10, johnStonesId, johnStones, Defender) |> squads.HandleAddPlayerCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddPlayerCmdAsync (%A %A)" england johnStones)
        let kieranTrippierId, kieranTrippier = Guid "00000072-0000-0008-0000-000000000000" |> PlayerId, PlayerName "Kieran Trippier"
        let! result = nephTokens.AddOrEditPlayerToken |> ifToken (fun token -> (token, nephId, englandId, Rvn 11, kieranTrippierId, kieranTrippier, Defender) |> squads.HandleAddPlayerCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddPlayerCmdAsync (%A %A)" england kieranTrippier)
        let kyleWalkerId, kyleWalker = Guid "00000072-0000-0009-0000-000000000000" |> PlayerId, PlayerName "Kyle Walker"
        let! result = nephTokens.AddOrEditPlayerToken |> ifToken (fun token -> (token, nephId, englandId, Rvn 12, kyleWalkerId, kyleWalker, Defender) |> squads.HandleAddPlayerCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddPlayerCmdAsync (%A %A)" england kyleWalker)
        let ashleyYoungId, ashleyYoung = Guid "00000072-0000-0010-0000-000000000000" |> PlayerId, PlayerName "Ashley Young"
        let! result = nephTokens.AddOrEditPlayerToken |> ifToken (fun token -> (token, nephId, englandId, Rvn 13, ashleyYoungId, ashleyYoung, Defender) |> squads.HandleAddPlayerCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddPlayerCmdAsync (%A %A)" england ashleyYoung)
        let deleAlliId, deleAlli = Guid "00000072-0000-0000-0001-000000000000" |> PlayerId, PlayerName "Dele Alli"
        let! result = nephTokens.AddOrEditPlayerToken |> ifToken (fun token -> (token, nephId, englandId, Rvn 14, deleAlliId, deleAlli, Midfielder) |> squads.HandleAddPlayerCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddPlayerCmdAsync (%A %A)" england deleAlli)
        let ericDierId, ericDier = Guid "00000072-0000-0000-0002-000000000000" |> PlayerId, PlayerName "Eric Dier"
        let! result = nephTokens.AddOrEditPlayerToken |> ifToken (fun token -> (token, nephId, englandId, Rvn 15, ericDierId, ericDier, Midfielder) |> squads.HandleAddPlayerCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddPlayerCmdAsync (%A %A)" england ericDier)
        let jordanHendersonId, jordanHenderson = Guid "00000072-0000-0000-0003-000000000000" |> PlayerId, PlayerName "Jordan Henderson"
        let! result = nephTokens.AddOrEditPlayerToken |> ifToken (fun token -> (token, nephId, englandId, Rvn 16, jordanHendersonId, jordanHenderson, Midfielder) |> squads.HandleAddPlayerCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddPlayerCmdAsync (%A %A)" england jordanHenderson)
        let jesseLindgardId, jesseLindgard = Guid "00000072-0000-0000-0004-000000000000" |> PlayerId, PlayerName "Jesse Lindgard"
        let! result = nephTokens.AddOrEditPlayerToken |> ifToken (fun token -> (token, nephId, englandId, Rvn 17, jesseLindgardId, jesseLindgard, Midfielder) |> squads.HandleAddPlayerCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddPlayerCmdAsync (%A %A)" england jesseLindgard)
        let rubenLoftusCheekId, rubenLoftusCheek = Guid "00000072-0000-0000-0005-000000000000" |> PlayerId, PlayerName "Ruben Loftus-Cheek"
        let! result = nephTokens.AddOrEditPlayerToken |> ifToken (fun token -> (token, nephId, englandId, Rvn 18, rubenLoftusCheekId, rubenLoftusCheek, Midfielder) |> squads.HandleAddPlayerCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddPlayerCmdAsync (%A %A)" england rubenLoftusCheek)
        let harryKaneId, harryKane = Guid "00000072-0000-0000-0000-000000000001" |> PlayerId, PlayerName "Harry Kane"
        let! result = nephTokens.AddOrEditPlayerToken |> ifToken (fun token -> (token, nephId, englandId, Rvn 19, harryKaneId, harryKane, Forward) |> squads.HandleAddPlayerCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddPlayerCmdAsync (%A %A)" england harryKane)
        let marcusRashfordId, marcusRashford = Guid "00000072-0000-0000-0000-000000000002" |> PlayerId, PlayerName "Marcus Rashford"
        let! result = nephTokens.AddOrEditPlayerToken |> ifToken (fun token -> (token, nephId, englandId, Rvn 20, marcusRashfordId, marcusRashford, Forward) |> squads.HandleAddPlayerCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddPlayerCmdAsync (%A %A)" england marcusRashford)
        let raheemSterlingId, raheemSterling = Guid "00000072-0000-0000-0000-000000000003" |> PlayerId, PlayerName "Raheem Sterling"
        let! result = nephTokens.AddOrEditPlayerToken |> ifToken (fun token -> (token, nephId, englandId, Rvn 21, raheemSterlingId, raheemSterling, Forward) |> squads.HandleAddPlayerCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddPlayerCmdAsync (%A %A)" england raheemSterling)
        let jamieVardyId, jamieVardy = Guid "00000072-0000-0000-0000-000000000004" |> PlayerId, PlayerName "Jamie Vardy"
        let! result = nephTokens.AddOrEditPlayerToken |> ifToken (fun token -> (token, nephId, englandId, Rvn 22, jamieVardyId, jamieVardy, Forward) |> squads.HandleAddPlayerCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddPlayerCmdAsync (%A %A)" england jamieVardy)
        let dannyWelbeckId, dannyWelbeck = Guid "00000072-0000-0000-0000-000000000005" |> PlayerId, PlayerName "Danny Welbeck"
        let! result = nephTokens.AddOrEditPlayerToken |> ifToken (fun token -> (token, nephId, englandId, Rvn 23, dannyWelbeckId, dannyWelbeck, Forward) |> squads.HandleAddPlayerCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddPlayerCmdAsync (%A %A)" england dannyWelbeck) *)
        // #endregion

        // #endregion

        // Note: Reset Squads agent [to pendingOnSquadsEventsRead] so that it handles subsequent SquadsEventsRead event appropriately (i.e. from readPersistedEvents).
        "resetting Squads agent" |> Info |> log
        () |> squads.Reset
    return () }

let private createInitialFixturesEventsIfNecessary = async {
    let fixtureId matchNumber =
        if matchNumber < 10u then sprintf "00000000-0000-0000-0000-00000000000%i" matchNumber |> Guid |> FixtureId
        else if matchNumber < 100u then sprintf "00000000-0000-0000-0000-0000000000%i" matchNumber |> Guid |> FixtureId
        else FixtureId.Create ()

    let fixturesDir = directory EntityType.Fixtures

    // #region: Force re-creation of initial Fixture/s events if directory already exists (if requested)
    if deleteExistingFixturesEvents && Directory.Exists fixturesDir then
        sprintf "deleting existing Fixture/s events -> %s" fixturesDir |> Info |> log
        delete fixturesDir
    // #endregion

    if Directory.Exists fixturesDir then sprintf "preserving existing Fixture/s events -> %s" fixturesDir |> Info |> log
    else
        sprintf "creating initial Fixture/s events -> %s" fixturesDir |> Info |> log
        "starting Fixtures agent" |> Info |> log
        () |> fixtures.Start
        // Note: Send dummy OnFixturesEventsRead to Users agent to ensure that it transitions [from pendingOnFixturesEventsRead] to managingFixtures; otherwise HandleCreateFixtureCmdAsync would be ignored (and block).
        "sending dummy OnFixturesEventsRead to Fixtures agent" |> Info |> log
        [] |> fixtures.OnFixturesEventsRead

        // #region: Group A
        let japanVsRussialKO = (2019, 09, 20, 10, 45) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 1u, Group GroupA, Confirmed japanId, Confirmed russiaId, japanVsRussialKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 1u)
        let irelandVsScotlandKO = (2019, 09, 22, 07, 45) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 2u, Group GroupA, Confirmed irelandId, Confirmed scotlandId, irelandVsScotlandKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 2u)
        let russiaVsSamoaKO = (2019, 09, 24, 10, 15) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 3u, Group GroupA, Confirmed russiaId, Confirmed samoaId, russiaVsSamoaKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 3u)
        let japanVsIrelandKO = (2019, 09, 28, 07, 15) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 4u, Group GroupA, Confirmed japanId, Confirmed irelandId, japanVsIrelandKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 4u)
        let scotlandVsSamoaKO = (2019, 09, 30, 10, 15) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 5u, Group GroupA, Confirmed scotlandId, Confirmed samoaId, scotlandVsSamoaKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 5u)
        let irelandVsRussiaKO = (2019, 10, 03, 10, 15) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 6u, Group GroupA, Confirmed irelandId, Confirmed russiaId, irelandVsRussiaKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 6u)
        let japanVsSamoaKO = (2019, 10, 05, 10, 30) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 7u, Group GroupA, Confirmed japanId, Confirmed samoaId, japanVsSamoaKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 7u)
        let scotlandVsRussiaKO = (2019, 10, 09, 07, 15) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 8u, Group GroupA, Confirmed scotlandId, Confirmed russiaId, scotlandVsRussiaKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 8u)
        let irelandVsSamoaKO = (2019, 10, 12, 10, 45) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 9u, Group GroupA, Confirmed irelandId, Confirmed samoaId, irelandVsSamoaKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 9u)
        let japanVsScotlandKO = (2019, 10, 13, 10, 45) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 10u, Group GroupA, Confirmed japanId, Confirmed scotlandId, japanVsScotlandKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 10u)
        // #region: TEMP-NMB: (Russia vs. Saudi Arabia) MatchEvents...
        (*let russiaVsSaudiArabiaKO = (2018, 06, 14, 15, 00) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 1u, Group GroupA, Confirmed russiaId, Confirmed saudiArabiaId, russiaVsSaudiArabiaKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 1u)
        let akinfeevId = Guid "a35cb647-5972-4976-b22e-970ccdb87181" |> PlayerId
        let cheryshevId = Guid "16b0a497-dde1-4db3-9d46-43c3470ee852" |> PlayerId
        let dzyubaId = Guid "b9866cf7-53e1-4776-99a8-1417f6199201" |> PlayerId
        let gazinskyId = Guid "e11061d7-d159-4d85-b626-8b1beb8a8f59" |> PlayerId
        let golovinId = Guid "9758f2ce-d5b4-4457-a82b-4229a6f4ace7" |> PlayerId
        let zobninId = Guid "1d601493-2c20-438f-a52e-c068263ce6d2" |> PlayerId
        let alJassimId = Guid "b96a43b1-a04b-4438-9928-b40c315e9e5a" |> PlayerId
        let matchEvent = (russiaId, gazinskyId, golovinId |> Some) |> Goal
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 1u, Rvn 1, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (russiaId, cheryshevId, zobninId |> Some) |> Goal
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 1u, Rvn 2, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (russiaId, dzyubaId, golovinId |> Some) |> Goal
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 1u, Rvn 3, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (russiaId, cheryshevId, dzyubaId |> Some) |> Goal
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 1u, Rvn 4, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (russiaId, golovinId, None) |> Goal
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 1u, Rvn 5, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (russiaId, golovinId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 1u, Rvn 6, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (saudiArabiaId, alJassimId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 1u, Rvn 7, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (russiaId, akinfeevId) |> CleanSheet
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 1u, Rvn 8, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (russiaId, cheryshevId) |> ManOfTheMatch
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 1u, Rvn 9, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)*)
        // #endregion

        // #endregion
        // #region: Group B
        let newZealandVsSouthAfricaKO = (2019, 09, 21, 09, 45) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 11u, Group GroupB, Confirmed newZealandId, Confirmed southAfricaId, newZealandVsSouthAfricaKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 11u)
        let italyVsNamibiaKO = (2019, 09, 22, 05, 15) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 12u, Group GroupB, Confirmed italyId, Confirmed namibiaId, italyVsNamibiaKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 12u)
        let italyVsCanadaKO = (2019, 09, 26, 07, 45) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 13u, Group GroupB, Confirmed italyId, Confirmed canadaId, italyVsCanadaKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 13u)
        let southAfricaVsNamibiaKO = (2019, 09, 28, 09, 45) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 14u, Group GroupB, Confirmed southAfricaId, Confirmed namibiaId, southAfricaVsNamibiaKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 14u)
        let newZealandVsCanadaKO = (2019, 10, 02, 10, 15) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 15u, Group GroupB, Confirmed newZealandId, Confirmed canadaId, newZealandVsCanadaKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 15u)
        let southAfricaVsItalyKO = (2019, 10, 04, 09, 45) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 16u, Group GroupB, Confirmed southAfricaId, Confirmed italyId, southAfricaVsItalyKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 16u)
        let newZealandVsNamibiaKO = (2019, 10, 06, 04, 45) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 17u, Group GroupB, Confirmed newZealandId, Confirmed namibiaId, newZealandVsNamibiaKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 17u)
        let southAfricaVsCanadaKO = (2019, 10, 08, 10, 15) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 18u, Group GroupB, Confirmed southAfricaId, Confirmed canadaId, southAfricaVsCanadaKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 18u)
        let newZealandVsItalyKO = (2019, 10, 12, 04, 45) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 19u, Group GroupB, Confirmed newZealandId, Confirmed italyId, newZealandVsNamibiaKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 19u)
        let namibiaVsCanadaKO = (2019, 10, 13, 03, 15) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 20u, Group GroupB, Confirmed namibiaId, Confirmed canadaId, namibiaVsCanadaKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 20u)
        // #endregion
        // #region: Group C
        let franceVsArgentinaKO = (2019, 09, 21, 07, 15) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 21u, Group GroupC, Confirmed franceId, Confirmed argentinaId, franceVsArgentinaKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 21u)
        let englandVsTongaKO = (2019, 09, 22, 10, 15) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 22u, Group GroupC, Confirmed englandId, Confirmed tongaId, englandVsTongaKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 22u)
        let englandVsUnitedStatesKO = (2019, 09, 26, 10, 45) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 23u, Group GroupC, Confirmed englandId, Confirmed unitedStatesId, englandVsUnitedStatesKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 23u)
        let argentinaVsTongaKO = (2019, 09, 28, 04, 45) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 24u, Group GroupC, Confirmed argentinaId, Confirmed tongaId, argentinaVsTongaKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 24u)
        let franceVsUnitedStatesKO = (2019, 10, 02, 07, 45) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 25u, Group GroupC, Confirmed franceId, Confirmed unitedStatesId, franceVsUnitedStatesKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 25u)
        let englandVsArgentinaKO = (2019, 10, 05, 08, 00) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 26u, Group GroupC, Confirmed englandId, Confirmed argentinaId, englandVsArgentinaKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 26u)
        let franceVsTongaKO = (2019, 10, 06, 07, 45) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 27u, Group GroupC, Confirmed franceId, Confirmed tongaId, franceVsTongaKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 27u)
        let argentinaVsUnitedStatesKO = (2019, 10, 09, 04, 45) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 28u, Group GroupC, Confirmed argentinaId, Confirmed unitedStatesId, argentinaVsUnitedStatesKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 28u)
        let englandVsFranceKO = (2019, 10, 12, 08, 15) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 29u, Group GroupC, Confirmed englandId, Confirmed franceId, englandVsFranceKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 29u)
        let unitedStatesVsTongaKO = (2019, 10, 13, 05, 45) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 30u, Group GroupC, Confirmed unitedStatesId, Confirmed tongaId, unitedStatesVsTongaKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 30u)
        // #endregion
        // #region: Group D
        let australiaVsFijiKO = (2019, 09, 21, 04, 45) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 31u, Group GroupD, Confirmed australiaId, Confirmed fijiId, australiaVsFijiKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 31u)
        let walesVsGeorgiaKO = (2019, 09, 23, 10, 15) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 32u, Group GroupD, Confirmed walesId, Confirmed georgiaId, walesVsGeorgiaKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 32u)
        let fijiVsUruguayKO = (2019, 09, 25, 05, 15) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 33u, Group GroupD, Confirmed fijiId, Confirmed uruguayId, fijiVsUruguayKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 33u)
        let georgiaVsUruguayKO = (2019, 09, 29, 05, 15) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 34u, Group GroupD, Confirmed georgiaId, Confirmed uruguayId, georgiaVsUruguayKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 34u)
        let australiaVsWalesKO = (2019, 09, 29, 07, 45) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 35u, Group GroupD, Confirmed australiaId, Confirmed walesId, australiaVsWalesKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 35u)
        let georgiaVsFijiKO = (2019, 10, 03, 05, 15) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 36u, Group GroupD, Confirmed georgiaId, Confirmed fijiId, georgiaVsFijiKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 36u)
        let australiaVsUruguayKO = (2019, 10, 05, 05, 15) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 37u, Group GroupD, Confirmed australiaId, Confirmed uruguayId, australiaVsUruguayKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 37u)
        let walesVsFijiKO = (2019, 10, 09, 09, 45) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 38u, Group GroupD, Confirmed walesId, Confirmed fijiId, walesVsFijiKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 38u)
        let australiaVsGeorgiaKO = (2019, 10, 11, 10, 15) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 39u, Group GroupD, Confirmed australiaId, Confirmed georgiaId, australiaVsGeorgiaKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 39u)
        let walesVsUruguayKO = (2019, 10, 13, 08, 15) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 40u, Group GroupD, Confirmed walesId, Confirmed uruguayId, walesVsUruguayKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 40u)
        // #endregion
        // #region: Quarter-finals
        let winnerCVsRunnerUpDKO = (2019, 10, 19, 05, 15) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 41u, QuarterFinal 1u, Unconfirmed (StageWinner (Group GroupC)), Unconfirmed (GroupRunnerUp GroupD), winnerCVsRunnerUpDKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 41u)
        let winnerBVsRunnerUpAKO = (2019, 10, 19, 10, 15) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 42u, QuarterFinal 2u, Unconfirmed (StageWinner (Group GroupB)), Unconfirmed (GroupRunnerUp GroupA), winnerBVsRunnerUpAKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 42u)
        let winnerDVsRunnerUpCKO = (2019, 10, 20, 05, 15) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 43u, QuarterFinal 3u, Unconfirmed (StageWinner (Group GroupD)), Unconfirmed (GroupRunnerUp GroupC), winnerDVsRunnerUpCKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 43u)
        let winnerAVsRunnerUpBKO = (2019, 10, 20, 10, 15) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 44u, QuarterFinal 4u, Unconfirmed (StageWinner (Group GroupA)), Unconfirmed (GroupRunnerUp GroupB), winnerAVsRunnerUpBKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 44u)
        // #endregion
        // #region: Semi-finals
        let winnerQF1VsWinnerQF2KO = (2019, 10, 26, 08, 00) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 45u, SemiFinal 1u, Unconfirmed (StageWinner (QuarterFinal 1u)), Unconfirmed (StageWinner (QuarterFinal 2u)), winnerQF1VsWinnerQF2KO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 45u)
        let winnerQF3VsWinnerQF4KO = (2019, 10, 27, 09, 00) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 46u, SemiFinal 2u, Unconfirmed (StageWinner (QuarterFinal 3u)), Unconfirmed (StageWinner (QuarterFinal 4u)), winnerQF3VsWinnerQF4KO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 46u)
        // #endregion
        // #region: "Bronze" final
        let loserSF1VsLoserSF2KO = (2019, 11, 01, 09, 00) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 47u, BronzeFinal, Unconfirmed (SemiFinalLoser 1u), Unconfirmed (SemiFinalLoser 2u), loserSF1VsLoserSF2KO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 47u)
        // #endregion
        // #region: Final
        let winnerSF1VsWinnerSF2KO = (2019, 11, 02, 09, 00) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 48u, Final, Unconfirmed (StageWinner (SemiFinal 1u)), Unconfirmed (StageWinner (SemiFinal 2u)), winnerSF1VsWinnerSF2KO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 48u)
        // #endregion

        // Note: Reset Fixtures agent [to pendingOnFixturesEventsRead] so that it handles subsequent FixturesEventsRead event appropriately (i.e. from readPersistedEvents).
        "resetting Fixtures agent" |> Info |> log
        () |> fixtures.Reset
    return () }

let private createInitialDraftsEventsIfNecessary = async {
    let draftsDir = directory EntityType.Drafts

    // #region: Force re-creation of initial Draft/s events if directory already exists (if requested)
    if deleteExistingDraftsEvents && Directory.Exists draftsDir then
        sprintf "deleting existing Draft/s events -> %s" draftsDir |> Info |> log
        delete draftsDir
    // #endregion

    if Directory.Exists draftsDir then sprintf "preserving existing Draft/s events -> %s" draftsDir |> Info |> log
    else
        sprintf "creating initial Draft/s events -> %s" draftsDir |> Info |> log
        "starting Drafts agent" |> Info |> log
        () |> drafts.Start
        // Note: Send dummy OnSquadsRead | OnDraftsEventsRead | OnUserDraftsEventsRead to Drafts agent to ensure that it transitions [from pendingAllRead] to managingDrafts; otherwise HandleCreateDraftCmdAsync would be ignored (and block).
        "sending dummy OnSquadsRead | OnDraftsEventsRead | OnUserDraftsEventsRead to Drafts agent" |> Info |> log
        [] |> drafts.OnSquadsRead
        [] |> drafts.OnDraftsEventsRead
        [] |> drafts.OnUserDraftsEventsRead

        let draft1Id, draft1Ordinal = Guid "00000000-0000-0000-0000-000000000001" |> DraftId, DraftOrdinal 1
        let draft1Starts, draft1Ends = (2019, 09, 07, 09, 00) |> dateTimeOffsetUtc, (2019, 09, 13, 19, 00) |> dateTimeOffsetUtc
        let draft1Type = (draft1Starts, draft1Ends) |> Constrained
        let! result = nephTokens.ProcessDraftToken |> ifToken (fun token -> (token, nephId, draft1Id, draft1Ordinal, draft1Type) |> drafts.HandleCreateDraftCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateDraftCmdAsync (%A %A)" draft1Id draft1Ordinal)
        let draft2Id, draft2Ordinal = Guid "00000000-0000-0000-0000-000000000002" |> DraftId, DraftOrdinal 2
        let draft2Starts, draft2Ends = (2019, 09, 14, 09, 00) |> dateTimeOffsetUtc, (2019, 09, 16, 19, 00) |> dateTimeOffsetUtc
        let draft2Type = (draft2Starts, draft2Ends) |> Constrained
        let! result = nephTokens.ProcessDraftToken |> ifToken (fun token -> (token, nephId, draft2Id, draft2Ordinal, draft2Type) |> drafts.HandleCreateDraftCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateDraftCmdAsync (%A %A)" draft2Id draft2Ordinal)
        let draft3Id, draft3Ordinal = Guid "00000000-0000-0000-0000-000000000003" |> DraftId, DraftOrdinal 3
        let draft3Starts, draft3Ends = (2019, 09, 17, 09, 00) |> dateTimeOffsetUtc, (2019, 09, 18, 19, 00) |> dateTimeOffsetUtc
        let draft3Type = (draft3Starts, draft3Ends) |> Constrained
        let! result = nephTokens.ProcessDraftToken |> ifToken (fun token -> (token, nephId, draft3Id, draft3Ordinal, draft3Type) |> drafts.HandleCreateDraftCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateDraftCmdAsync (%A %A)" draft3Id draft3Ordinal)
        let draft4Id, draft4Ordinal = Guid "00000000-0000-0000-0000-000000000004" |> DraftId, DraftOrdinal 4
        let draft4Type = Unconstrained
        let! result = nephTokens.ProcessDraftToken |> ifToken (fun token -> (token, nephId, draft4Id, draft4Ordinal, draft4Type) |> drafts.HandleCreateDraftCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateDraftCmdAsync (%A %A)" draft4Id draft4Ordinal)

        // Note: Reset Drafts agent [to pendingAllRead] so that it handles subsequent DraftsEventsRead event (&c.) appropriately (i.e. from readPersistedEvents).
        "resetting Drafts agent" |> Info |> log
        () |> drafts.Reset
    return () }

let createInitialPersistedEventsIfNecessary = async {
    "creating initial persisted events (if necessary)" |> Info |> log
    let previousLogFilter = () |> consoleLogger.CurrentLogFilter
    let customLogFilter = "createInitialPersistedEventsIfNecessary", function | Host -> allCategories | Entity _ -> allExceptVerbose | _ -> onlyWarningsAndWorse
    customLogFilter |> consoleLogger.ChangeLogFilter
    do! createInitialUsersEventsIfNecessary // note: although this can cause various events to be broadcast (UsersRead | UserEventWritten | &c.), no agents should yet be subscribed to these
    do! createInitialSquadsEventsIfNecessary // note: although this can cause various events to be broadcast (SquadsRead | SquadEventWritten | &c.), no agents should yet be subscribed to these
    do! createInitialFixturesEventsIfNecessary // note: although this can cause various events to be broadcast (FixturesRead | FixtureEventWritten | &c.), no agents should yet be subscribed to these
    // TODO-NMB...do! createInitialDraftsEventsIfNecessary // note: although this can cause various events to be broadcast (DraftsRead | DraftEventWritten | &c.), no agents should yet be subscribed to these
    previousLogFilter |> consoleLogger.ChangeLogFilter }
