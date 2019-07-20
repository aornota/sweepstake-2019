module Aornota.Sweepstake2018.Server.DefaultData

open Aornota.Common.IfDebug
open Aornota.Common.Revision

open Aornota.Server.Common.Helpers

open Aornota.Sweepstake2018.Common.Domain.Core
open Aornota.Sweepstake2018.Common.Domain.Draft
open Aornota.Sweepstake2018.Common.Domain.Fixture
open Aornota.Sweepstake2018.Common.Domain.Squad
open Aornota.Sweepstake2018.Common.Domain.User
open Aornota.Sweepstake2018.Common.WsApi.ServerMsg
open Aornota.Sweepstake2018.Server.Agents.ConsoleLogger
open Aornota.Sweepstake2018.Server.Agents.Entities.Drafts
open Aornota.Sweepstake2018.Server.Agents.Entities.Fixtures
open Aornota.Sweepstake2018.Server.Agents.Entities.Squads
open Aornota.Sweepstake2018.Server.Agents.Entities.Users
open Aornota.Sweepstake2018.Server.Agents.Persistence
open Aornota.Sweepstake2018.Server.Authorization

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
let private egyptId = Guid "00000011-0000-0000-0000-000000000000" |> SquadId
let private russiaId = Guid "00000012-0000-0000-0000-000000000000" |> SquadId
let private saudiArabiaId = Guid "00000013-0000-0000-0000-000000000000" |> SquadId
let private uruguayId = Guid "00000014-0000-0000-0000-000000000000" |> SquadId
let private iranId = Guid "00000021-0000-0000-0000-000000000000" |> SquadId
let private moroccoId = Guid "00000022-0000-0000-0000-000000000000" |> SquadId
let private portugalId = Guid "00000023-0000-0000-0000-000000000000" |> SquadId
let private spainId = Guid "00000024-0000-0000-0000-000000000000" |> SquadId
let private australiaId = Guid "00000031-0000-0000-0000-000000000000" |> SquadId
let private denmarkId = Guid "00000032-0000-0000-0000-000000000000" |> SquadId
let private franceId = Guid "00000033-0000-0000-0000-000000000000" |> SquadId
let private peruId = Guid "00000034-0000-0000-0000-000000000000" |> SquadId
let private argentinaId = Guid "00000041-0000-0000-0000-000000000000" |> SquadId
let private croatiaId = Guid "00000042-0000-0000-0000-000000000000" |> SquadId
let private icelandId = Guid "00000043-0000-0000-0000-000000000000" |> SquadId
let private nigeriaId = Guid "00000044-0000-0000-0000-000000000000" |> SquadId
let private brazilId = Guid "00000051-0000-0000-0000-000000000000" |> SquadId
let private costaRicaId = Guid "00000052-0000-0000-0000-000000000000" |> SquadId
let private serbiaId = Guid "00000053-0000-0000-0000-000000000000" |> SquadId
let private switzerlandId = Guid "00000054-0000-0000-0000-000000000000" |> SquadId
let private germanyId = Guid "00000061-0000-0000-0000-000000000000" |> SquadId
let private mexicoId = Guid "00000062-0000-0000-0000-000000000000" |> SquadId
let private southKoreaId = Guid "00000063-0000-0000-0000-000000000000" |> SquadId
let private swedenId = Guid "00000064-0000-0000-0000-000000000000" |> SquadId
let private belgiumId = Guid "00000071-0000-0000-0000-000000000000" |> SquadId
let private englandId = Guid "00000072-0000-0000-0000-000000000000" |> SquadId
let private panamaId = Guid "00000073-0000-0000-0000-000000000000" |> SquadId
let private tunisiaId = Guid "00000074-0000-0000-0000-000000000000" |> SquadId
let private colombiaId = Guid "00000081-0000-0000-0000-000000000000" |> SquadId
let private japanId = Guid "00000082-0000-0000-0000-000000000000" |> SquadId
let private polandId = Guid "00000083-0000-0000-0000-000000000000" |> SquadId
let private senegalId = Guid "00000084-0000-0000-0000-000000000000" |> SquadId
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

        // #region: Create initial Squads - and subset of Players.

        // #region: Group A
        let egypt = SquadName "Egypt"
        let! result = nephTokens.CreateSquadToken |> ifToken (fun token -> (token, nephId, egyptId, egypt, GroupA, Seeding 22, CoachName "Héctor Cúper") |> squads.HandleCreateSquadCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateSquadCmdAsync (%A)" egypt)
        let russia = SquadName "Russia"
        let! result = nephTokens.CreateSquadToken |> ifToken (fun token -> (token, nephId, russiaId, russia, GroupA, Seeding 1, CoachName "Stanislav Cherchesov") |> squads.HandleCreateSquadCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateSquadCmdAsync (%A)" russia)
        let saudiArabia = SquadName "Saudi Arabia"
        let! result = nephTokens.CreateSquadToken |> ifToken (fun token -> (token, nephId, saudiArabiaId, saudiArabia, GroupA, Seeding 32, CoachName "Juan Antonio Pizzi") |> squads.HandleCreateSquadCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateSquadCmdAsync (%A)" saudiArabia)
        let uruguay = SquadName "Uruguay"
        let! result = nephTokens.CreateSquadToken |> ifToken (fun token -> (token, nephId, uruguayId, uruguay, GroupA, Seeding 15, CoachName "Óscar Tabárez") |> squads.HandleCreateSquadCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateSquadCmdAsync (%A)" uruguay)
        // #endregion
        // #region: Group B
        let iran = SquadName "Iran"
        let! result = nephTokens.CreateSquadToken |> ifToken (fun token -> (token, nephId, iranId, iran, GroupB, Seeding 24, CoachName "Carlos Queiroz") |> squads.HandleCreateSquadCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateSquadCmdAsync (%A)" iran)
        let morocco = SquadName "Morocco"
        let! result = nephTokens.CreateSquadToken |> ifToken (fun token -> (token, nephId, moroccoId, morocco, GroupB, Seeding 29, CoachName "Hervé Renard") |> squads.HandleCreateSquadCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateSquadCmdAsync (%A)" morocco)
        let portugal = SquadName "Portugal"
        let! result = nephTokens.CreateSquadToken |> ifToken (fun token -> (token, nephId, portugalId, portugal, GroupB, Seeding 4, CoachName "Fernando Santos") |> squads.HandleCreateSquadCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateSquadCmdAsync (%A)" portugal)
        let spain = SquadName "Spain"
        let! result = nephTokens.CreateSquadToken |> ifToken (fun token -> (token, nephId, spainId, spain, GroupB, Seeding 9, CoachName "Julen Lopetegui") |> squads.HandleCreateSquadCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateSquadCmdAsync (%A)" spain)
        // #endregion
        // #region: Group C
        let australia = SquadName "Australia"
        let! result = nephTokens.CreateSquadToken |> ifToken (fun token -> (token, nephId, australiaId, australia, GroupC, Seeding 27, CoachName "Bert van Marwijk") |> squads.HandleCreateSquadCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateSquadCmdAsync (%A)" australia)
        let denmark = SquadName "Denmark"
        let! result = nephTokens.CreateSquadToken |> ifToken (fun token -> (token, nephId, denmarkId, denmark, GroupC, Seeding 17, CoachName "Åge Hareide") |> squads.HandleCreateSquadCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateSquadCmdAsync (%A)" denmark)
        let france = SquadName "France"
        let! result = nephTokens.CreateSquadToken |> ifToken (fun token -> (token, nephId, franceId, france, GroupC, Seeding 8, CoachName "Didier Deschamps") |> squads.HandleCreateSquadCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateSquadCmdAsync (%A)" france)
        let peru = SquadName "Peru"
        let! result = nephTokens.CreateSquadToken |> ifToken (fun token -> (token, nephId, peruId, peru, GroupC, Seeding 10, CoachName "Ricardo Gareca") |> squads.HandleCreateSquadCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateUseHandleCreateSquadCmdAsyncrCmdAsync (%A)" peru)
        // #endregion
        // #region: Group D
        let argentina = SquadName "Argentina"
        let! result = nephTokens.CreateSquadToken |> ifToken (fun token -> (token, nephId, argentinaId, argentina, GroupD, Seeding 5, CoachName "Jorge Sampaoli") |> squads.HandleCreateSquadCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateSquadCmdAsync (%A)" argentina)
        let croatia = SquadName "Croatia"
        let! result = nephTokens.CreateSquadToken |> ifToken (fun token -> (token, nephId, croatiaId, croatia, GroupD, Seeding 16, CoachName "Zlatko Dalić") |> squads.HandleCreateSquadCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateSquadCmdAsync (%A)" croatia)
        let iceland = SquadName "Iceland"
        let! result = nephTokens.CreateSquadToken |> ifToken (fun token -> (token, nephId, icelandId, iceland, GroupD, Seeding 18, CoachName "Heimir Hallgrímsson") |> squads.HandleCreateSquadCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateSquadCmdAsync (%A)" iceland)
        let nigeria = SquadName "Nigeria"
        let! result = nephTokens.CreateSquadToken |> ifToken (fun token -> (token, nephId, nigeriaId, nigeria, GroupD, Seeding 26, CoachName "Gernot Rohr") |> squads.HandleCreateSquadCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateSquadCmdAsync (%A)" nigeria)
        // #endregion
        // #region: Group E
        let brazil = SquadName "Brazil"
        let! result = nephTokens.CreateSquadToken |> ifToken (fun token -> (token, nephId, brazilId, brazil, GroupE, Seeding 3, CoachName "Tite") |> squads.HandleCreateSquadCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateSquadCmdAsync (%A)" brazil)
        let costaRica = SquadName "Costa Rica"
        let! result = nephTokens.CreateSquadToken |> ifToken (fun token -> (token, nephId, costaRicaId, costaRica, GroupE, Seeding 19, CoachName "Óscar Ramírez") |> squads.HandleCreateSquadCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateSquadCmdAsync (%A)" costaRica)
        let serbia = SquadName "Serbia"
        let! result = nephTokens.CreateSquadToken |> ifToken (fun token -> (token, nephId, serbiaId, serbia, GroupE, Seeding 25, CoachName "Mladen Krstajić") |> squads.HandleCreateSquadCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateSquadCmdAsync (%A)" serbia)
        let switzerland = SquadName "Switzerland"
        let! result = nephTokens.CreateSquadToken |> ifToken (fun token -> (token, nephId, switzerlandId, switzerland, GroupE, Seeding 11, CoachName "Vladimir Petković") |> squads.HandleCreateSquadCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateSquadCmdAsync (%A)" switzerland)
        // #endregion
        // #region: Group F
        let germany = SquadName "Germany"
        let! result = nephTokens.CreateSquadToken |> ifToken (fun token -> (token, nephId, germanyId, germany, GroupF, Seeding 2, CoachName "Joachim Löw") |> squads.HandleCreateSquadCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateSquadCmdAsync (%A)" germany)
        let mexico = SquadName "Mexico"
        let! result = nephTokens.CreateSquadToken |> ifToken (fun token -> (token, nephId, mexicoId, mexico, GroupF, Seeding 14, CoachName "Juan Carlos Osorio") |> squads.HandleCreateSquadCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateSquadCmdAsync (%A)" mexico)
        let southKorea = SquadName "South Korea"
        let! result = nephTokens.CreateSquadToken |> ifToken (fun token -> (token, nephId, southKoreaId, southKorea, GroupF, Seeding 31, CoachName "Shin Tae-yong") |> squads.HandleCreateSquadCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateSquadCmdAsync (%A)" southKorea)
        let sweden = SquadName "Sweden"
        let! result = nephTokens.CreateSquadToken |> ifToken (fun token -> (token, nephId, swedenId, sweden, GroupF, Seeding 20, CoachName "Janne Andersson") |> squads.HandleCreateSquadCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateSquadCmdAsync (%A)" sweden)
        // #endregion
        // #region: Group G
        let belgium = SquadName "Belgium"
        let! result = nephTokens.CreateSquadToken |> ifToken (fun token -> (token, nephId, belgiumId, belgium, GroupG, Seeding 6, CoachName "Roberto Martínez") |> squads.HandleCreateSquadCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateSquadCmdAsync (%A)" belgium)
        let england = SquadName "England"
        let! result = nephTokens.CreateSquadToken |> ifToken (fun token -> (token, nephId, englandId, england, GroupG, Seeding 12, CoachName "Gareth Southgate") |> squads.HandleCreateSquadCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateSquadCmdAsync (%A)" england)
        // #region: England Players
        let jackButlandId, jackButland = Guid "00000072-0001-0000-0000-000000000000" |> PlayerId, PlayerName "Jack Butland"
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
        result |> logShouldSucceed (sprintf "HandleAddPlayerCmdAsync (%A %A)" england dannyWelbeck)
        // #endregion
        let panama = SquadName "Panama"
        let! result = nephTokens.CreateSquadToken |> ifToken (fun token -> (token, nephId, panamaId, panama, GroupG, Seeding 30, CoachName "Hernán Darío Gómez") |> squads.HandleCreateSquadCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateSquadCmdAsync (%A)" panama)
        let tunisia = SquadName "Tunisia"
        let! result = nephTokens.CreateSquadToken |> ifToken (fun token -> (token, nephId, tunisiaId, tunisia, GroupG, Seeding 21, CoachName "Nabil Maâloul") |> squads.HandleCreateSquadCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateSquadCmdAsync (%A)" tunisia)
        // #endregion
        // #region: Group H
        let colombia = SquadName "Colombia"
        let! result = nephTokens.CreateSquadToken |> ifToken (fun token -> (token, nephId, colombiaId, colombia, GroupH, Seeding 13, CoachName "José Pékerman") |> squads.HandleCreateSquadCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateUserCmdAsync (%A)" colombia)
        let japan = SquadName "Japan"
        let! result = nephTokens.CreateSquadToken |> ifToken (fun token -> (token, nephId, japanId, japan, GroupH, Seeding 28, CoachName "Akira Nishino") |> squads.HandleCreateSquadCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateUserCmdAsync (%A)" japan)
        let poland = SquadName "Poland"
        let! result = nephTokens.CreateSquadToken |> ifToken (fun token -> (token, nephId, polandId, poland, GroupH, Seeding 7, CoachName "Adam Nawałka") |> squads.HandleCreateSquadCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateUserCmdAsync (%A)" poland)
        let senegal = SquadName "Senegal"
        let! result = nephTokens.CreateSquadToken |> ifToken (fun token -> (token, nephId, senegalId, senegal, GroupH, Seeding 23, CoachName "Aliou Cissé") |> squads.HandleCreateSquadCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateUserCmdAsync (%A)" senegal)
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
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)

        let egyptVsUruguayKO = (2018, 06, 15, 12, 00) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 2u, Group GroupA, Confirmed egyptId, Confirmed uruguayId, egyptVsUruguayKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 2u)
        let gimenezId = Guid "0064a341-6544-43a7-a290-2f5f60c1e046" |> PlayerId
        let musleraId = Guid "70af641b-2756-470a-aa37-f717cef335cf" |> PlayerId
        let sánchezUruguayId = Guid "c911a019-bdd7-458d-94b0-da23ded6f82a" |> PlayerId
        let elShenawyId = Guid "b3865226-9a0d-4c5d-a4a0-5ddd4251273b" |> PlayerId
        let hegaziId = Guid "1d66d5bc-6fbc-45c8-be36-8e77b64e06ce" |> PlayerId
        let morsyId = Guid "d3dafd89-d889-4da2-b101-db9bcdeaaf9e" |> PlayerId
        let matchEvent = (uruguayId, gimenezId, sánchezUruguayId |> Some) |> Goal
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 2u, Rvn 1, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (egyptId, hegaziId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 2u, Rvn 2, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (egyptId, morsyId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 2u, Rvn 3, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (uruguayId, musleraId) |> CleanSheet
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 2u, Rvn 4, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (egyptId, elShenawyId) |> ManOfTheMatch
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 2u, Rvn 5, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)

        let russiaVsEgyptKO = (2018, 06, 19, 18, 00) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 17u, Group GroupA, Confirmed russiaId, Confirmed egyptId, russiaVsEgyptKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 17u)
        let fernandesId = Guid "f84b6eb1-dfa4-48cb-bc41-c5a36de875b8" |> PlayerId
        let kutepovId = Guid "a251d666-d1bc-4eb2-bc14-63bc15befb89" |> PlayerId
        let smolovId = Guid "34e642bf-8860-4948-9d31-49df81fcd610" |> PlayerId
        let fathyId = Guid "b9c37743-3fc6-4a30-b8a1-5e1fe82b3bad" |> PlayerId
        let salahId = Guid "b4b58dd3-12a7-4334-887b-0e7b4de60588" |> PlayerId
        let trézéguetId = Guid "d45709e5-0349-4773-8b52-7af10ab5584c" |> PlayerId
        let matchEvent = (egyptId, fathyId) |> OwnGoal
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 17u, Rvn 1, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (russiaId, cheryshevId, fernandesId |> Some) |> Goal
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 17u, Rvn 2, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (russiaId, dzyubaId, kutepovId |> Some) |> Goal
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 17u, Rvn 3, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (egyptId, salahId, Scored) |> Penalty
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 17u, Rvn 4, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (russiaId, smolovId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 17u, Rvn 5, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (egyptId, trézéguetId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 17u, Rvn 6, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (russiaId, cheryshevId) |> ManOfTheMatch
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 17u, Rvn 7, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)

        let uruguayVsSaudiArabiaKO = (2018, 06, 20, 15, 00) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 18u, Group GroupA, Confirmed uruguayId, Confirmed saudiArabiaId, uruguayVsSaudiArabiaKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 18u)
        let suárezId = Guid "41c4d5dd-92bf-4794-9872-7b6c092ae3ea" |> PlayerId
        let matchEvent = (uruguayId, suárezId, sánchezUruguayId |> Some) |> Goal
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 18u, Rvn 1, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (uruguayId, musleraId) |> CleanSheet
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 18u, Rvn 2, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (uruguayId, suárezId) |> ManOfTheMatch
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 18u, Rvn 3, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)

        let uruguayVsRussiaKO = (2018, 06, 25, 14, 00) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 33u, Group GroupA, Confirmed uruguayId, Confirmed russiaId, uruguayVsRussiaKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 33u)

        let saudiArabiaVsEgyptKO = (2018, 06, 25, 14, 00) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 34u, Group GroupA, Confirmed saudiArabiaId, Confirmed egyptId, saudiArabiaVsEgyptKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 34u)
        
        // #endregion
        // #region: Group B
        let moroccoVsIranKO = (2018, 06, 15, 15, 00) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 4u, Group GroupB, Confirmed moroccoId, Confirmed iranId, moroccoVsIranKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 4u)
        let ansarifardId = Guid "0cf94a5d-6b8d-469b-86af-744221a49852" |> PlayerId
        let beirandvandId = Guid "4362c0f2-c779-48ab-8110-4f605de3169c" |> PlayerId
        let jahanbakhshId = Guid "3cba3185-5d2a-45bc-b553-50e8aa933014" |> PlayerId
        let shojaeiId = Guid "e94367b0-39ed-4472-a28d-6d24823dc3cf" |> PlayerId
        let bouhaddouzId = Guid "998d8568-8d10-4951-bb3e-1d286a156733" |> PlayerId
        let elAhmadiId = Guid "a9d13a71-9465-46a4-bca4-edacf3ad54a3" |> PlayerId
        let haritId = Guid "d200ed46-9abb-4e00-8c79-766383cee6f5" |> PlayerId
        let matchEvent = (moroccoId, bouhaddouzId) |> OwnGoal
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 4u, Rvn 1, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (moroccoId, elAhmadiId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 4u, Rvn 2, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (iranId, shojaeiId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 4u, Rvn 3, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (iranId, jahanbakhshId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 4u, Rvn 4, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (iranId, ansarifardId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 4u, Rvn 5, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (iranId, beirandvandId) |> CleanSheet
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 4u, Rvn 6, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (moroccoId, haritId) |> ManOfTheMatch
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 4u, Rvn 7, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)

        let portugalVsSpainKO = (2018, 06, 15, 18, 00) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 3u, Group GroupB, Confirmed portugalId, Confirmed spainId, portugalVsSpainKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 3u)
        let fernandesBId = Guid "cfc16d20-25da-4115-b72b-7702559a2874" |> PlayerId
        let guedesId = Guid "6da73683-66e7-4947-98d9-1becfdd59d48" |> PlayerId
        let ronaldoId = Guid "c9938498-ebf5-4502-8b9e-a480e448c1ae" |> PlayerId
        let busquetsId = Guid "c84bc5da-5bbc-460e-b85f-f0575a31c3a9" |> PlayerId
        let costaSpainId = Guid "64b9e024-9ee5-4f94-9dd8-54006d196c2b" |> PlayerId
        let nachoId = Guid "78b464b4-93b7-44ed-8701-a611e7bcf024" |> PlayerId
        let matchEvent = (portugalId, ronaldoId, Scored) |> Penalty
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 3u, Rvn 1, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (spainId, costaSpainId, None) |> Goal
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 3u, Rvn 2, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (portugalId, ronaldoId, guedesId |> Some) |> Goal
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 3u, Rvn 3, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (spainId, costaSpainId, busquetsId |> Some) |> Goal
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 3u, Rvn 4, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (spainId, nachoId, None) |> Goal
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 3u, Rvn 5, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (portugalId, ronaldoId, None) |> Goal
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 3u, Rvn 6, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (portugalId, fernandesBId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 3u, Rvn 7, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (spainId, busquetsId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 3u, Rvn 8, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (portugalId, ronaldoId) |> ManOfTheMatch
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 3u, Rvn 9, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)

        let portugalVsMoroccoKO = (2018, 06, 20, 12, 00) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 19u, Group GroupB, Confirmed portugalId, Confirmed moroccoId, portugalVsMoroccoKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 19u)
        let moutinhoId = Guid "115dcbf3-3dc0-42f6-8b09-0f8a2fad682a" |> PlayerId
        let patricioId = Guid "f24e3dce-13ca-4bb2-9380-b7f699fd55e1" |> PlayerId
        let silvaId = Guid "7422ac07-78b4-4be9-9744-00ba7980a66c" |> PlayerId
        let benatiaId = Guid "bff84865-aa88-4d9d-b77c-00e6b90b1f49" |> PlayerId
        let matchEvent = (portugalId, ronaldoId, moutinhoId |> Some) |> Goal
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 19u, Rvn 1, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (portugalId, silvaId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 19u, Rvn 2, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (moroccoId, benatiaId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 19u, Rvn 3, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (portugalId, patricioId) |> CleanSheet
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 19u, Rvn 4, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (portugalId, ronaldoId) |> ManOfTheMatch
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 19u, Rvn 5, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)

        let iranVsSpainKO = (2018, 06, 20, 18, 00) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 20u, Group GroupB, Confirmed iranId, Confirmed spainId, iranVsSpainKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 20u)
        let amiriId = Guid "c9b55a95-95f1-4b3b-bc2e-dc8839364df1" |> PlayerId
        let ebrahimiId = Guid "02a8b632-7bbf-4eea-a807-5ed59ffb2b15" |> PlayerId
        let deGeaId = Guid "e91e34ea-3d77-4bda-8d11-8e63bf6a7deb" |> PlayerId
        let matchEvent = (spainId, costaSpainId, None) |> Goal
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 20u, Rvn 1, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (iranId, amiriId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 20u, Rvn 2, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (iranId, ebrahimiId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 20u, Rvn 3, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (spainId, deGeaId) |> CleanSheet
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 20u, Rvn 4, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (spainId, costaSpainId) |> ManOfTheMatch
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 20u, Rvn 5, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)

        let iranVsPortugalKO = (2018, 06, 25, 18, 00) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 35u, Group GroupB, Confirmed iranId, Confirmed portugalId, iranVsPortugalKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 35u)

        let spainVsMoroccoKO = (2018, 06, 25, 18, 00) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 36u, Group GroupB, Confirmed spainId, Confirmed moroccoId, spainVsMoroccoKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 36u)

        // #endregion
        // #region: Group C
        let franceVsAustraliaKO = (2018, 06, 16, 10, 00) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 5u, Group GroupC, Confirmed franceId, Confirmed australiaId, franceVsAustraliaKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 5u)
        let griezmannId = Guid "90a2d0bf-04d1-491e-a57c-0ff7563e198c" |> PlayerId
        let tolissoId = Guid "239346ef-e8bc-44c3-81b6-e5d4f240cca2" |> PlayerId
        let behichId = Guid "7ac6b397-f851-4b15-84aa-71bd18b1b032" |> PlayerId
        let jedinakId = Guid "6be8a6a0-65ee-4ac1-b873-f3e3ea7ecf2f" |> PlayerId
        let leckieId = Guid "e89efa5e-abb4-4971-87f0-097db673ca47" |> PlayerId
        let risdonId = Guid "80b67503-333e-4f57-9908-e1256508751b" |> PlayerId
        let matchEvent = (franceId, griezmannId, Scored) |> Penalty
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 5u, Rvn 1, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (australiaId, jedinakId, Scored) |> Penalty
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 5u, Rvn 2, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (australiaId, behichId) |> OwnGoal
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 5u, Rvn 3, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (franceId, tolissoId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 5u, Rvn 4, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (australiaId, leckieId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 5u, Rvn 5, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (australiaId, risdonId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 5u, Rvn 6, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (australiaId, behichId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 5u, Rvn 7, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (franceId, griezmannId) |> ManOfTheMatch
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 5u, Rvn 8, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)

        let peruVsDenmarkKO = (2018, 06, 16, 16, 00) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 6u, Group GroupC, Confirmed peruId, Confirmed denmarkId, peruVsDenmarkKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 6u)
        let delaneyId = Guid "3e7c1db3-83cf-4b3a-bf82-70485fc78982" |> PlayerId
        let eriksenId = Guid "fbf1ea6a-7f74-4f3a-af76-4a4fdad15e10" |> PlayerId
        let poulsenId = Guid "81797155-1c34-47d4-9fd4-ac08d3a52087" |> PlayerId
        let schmeichelId = Guid "346c1cef-91c5-413f-8ed2-b6188ad0e7d9" |> PlayerId
        let cuevaId = Guid "21b7386b-ddb1-4ab9-9e53-df38d8019466" |> PlayerId
        let tapiaId = Guid "f81f03b0-dedd-4baf-984e-91f9ed612af2" |> PlayerId
        let matchEvent = (peruId, cuevaId, Missed) |> Penalty
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 6u, Rvn 1, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (denmarkId, poulsenId, eriksenId |> Some) |> Goal
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 6u, Rvn 2, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (peruId, tapiaId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 6u, Rvn 3, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (denmarkId, delaneyId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 6u, Rvn 4, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (denmarkId, poulsenId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 6u, Rvn 5, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (denmarkId, schmeichelId) |> CleanSheet
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 6u, Rvn 6, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (denmarkId, poulsenId) |> ManOfTheMatch
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 6u, Rvn 7, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)

        let denmarkVsAustraliaKO = (2018, 06, 21, 12, 00) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 22u, Group GroupC, Confirmed denmarkId, Confirmed australiaId, denmarkVsAustraliaKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 22u)
        let jørgensenNId = Guid "a9ea63a8-b6d6-4d58-87dd-82d52fa772df" |> PlayerId
        let sistoId = Guid "4ab9cdbe-e581-4742-ac44-9e8787ca9824" |> PlayerId
        let matchEvent = (denmarkId, eriksenId, jørgensenNId |> Some) |> Goal
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 22u, Rvn 1, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (australiaId, jedinakId, Scored) |> Penalty
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 22u, Rvn 2, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (denmarkId, poulsenId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 22u, Rvn 3, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (denmarkId, sistoId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 22u, Rvn 4, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (denmarkId, eriksenId) |> ManOfTheMatch
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 22u, Rvn 5, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)

        let franceVsPeruKO = (2018, 06, 21, 15, 00) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 21u, Group GroupC, Confirmed franceId, Confirmed peruId, franceVsPeruKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 21u)
        let llorisId = Guid "03988f09-6419-484d-8e06-211b5a427fea" |> PlayerId
        let matuidiId = Guid "9624b28e-2c87-45a7-832e-d5bc0ddeab40" |> PlayerId
        let mbappéId = Guid "4b5c8405-cf0b-43fc-909f-6dd8a8362c81" |> PlayerId
        let pogbaId = Guid "2c7e2def-1297-4d4c-9983-ecc5e94e4198" |> PlayerId
        let aquinoId = Guid "f2d9c984-6ca6-49e0-a09b-2996d667b7cd" |> PlayerId
        let guerreroId = Guid "aea460a0-913b-4dee-b465-d51363de7f68" |> PlayerId
        let matchEvent = (franceId, mbappéId, None) |> Goal
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 21u, Rvn 1, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (franceId, matuidiId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 21u, Rvn 2, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (franceId, pogbaId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 21u, Rvn 3, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (peruId, guerreroId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 21u, Rvn 4, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (peruId, aquinoId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 21u, Rvn 5, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (franceId, llorisId) |> CleanSheet
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 21u, Rvn 6, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (franceId, mbappéId) |> ManOfTheMatch
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 21u, Rvn 7, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)

        let denmarkVsFranceKO = (2018, 06, 26, 14, 00) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 37u, Group GroupC, Confirmed denmarkId, Confirmed franceId, denmarkVsFranceKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 37u)

        let australiaVsPeruKO = (2018, 06, 26, 14, 00) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 38u, Group GroupC, Confirmed australiaId, Confirmed peruId, australiaVsPeruKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 38u)

        // #endregion
        // #region: Group D
        let argentinaVsIcelandKO = (2018, 06, 16, 13, 00) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 7u, Group GroupD, Confirmed argentinaId, Confirmed icelandId, argentinaVsIcelandKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 7u)
        let agueroId = Guid "f6148ce7-ab6e-49aa-b88e-06cdfe935a73" |> PlayerId
        let messiId = Guid "6ebded5a-ff2e-4bf9-b540-838c20a84f74" |> PlayerId
        let finnbogasonId = Guid "7b177ae2-a044-4ff3-bc76-93bc69ba2a5c" |> PlayerId
        let halldórssonId = Guid "037755b4-f38f-458e-9083-ce09f07252d3" |> PlayerId
        let matchEvent = (argentinaId, agueroId, None) |> Goal
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 7u, Rvn 1, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (icelandId, finnbogasonId, None) |> Goal
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 7u, Rvn 2, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (argentinaId, messiId, (icelandId, halldórssonId) |> Saved) |> Penalty
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 7u, Rvn 3, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (icelandId, halldórssonId) |> ManOfTheMatch
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 7u, Rvn 4, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)

        let croatiaVsNigeriaKO = (2018, 06, 16, 19, 00) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 8u, Group GroupD, Confirmed croatiaId, Confirmed nigeriaId, croatiaVsNigeriaKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 8u)
        let brozovićId = Guid "b473a9a8-2685-4d1d-b2ea-2bd8cf0c0cee" |> PlayerId
        let modrićId = Guid "62c929d8-dcff-4aec-919a-8f674037799b" |> PlayerId
        let rakitićId = Guid "6265d3f9-6e58-4e5a-9e6f-e0cb6097c4dd" |> PlayerId
        let subašićId = Guid "35976d44-1cf8-4e73-b4ed-c41826c2b2d2" |> PlayerId
        let eteboId = Guid "2d06a54e-2f11-44dd-8224-67d9f26a8893" |> PlayerId
        let troostEkongId = Guid "18c897bb-4e03-4df1-b066-380b98122978" |> PlayerId
        let matchEvent = (nigeriaId, eteboId) |> OwnGoal
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 8u, Rvn 1, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (croatiaId, modrićId, Scored) |> Penalty
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 8u, Rvn 2, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (croatiaId, brozovićId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 8u, Rvn 3, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (croatiaId, rakitićId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 8u, Rvn 4, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (nigeriaId, troostEkongId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 8u, Rvn 5, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (croatiaId, subašićId) |> CleanSheet
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 8u, Rvn 6, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (croatiaId, modrićId) |> ManOfTheMatch
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 8u, Rvn 7, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)

        let argentinaVsCroatiaKO = (2018, 06, 21, 18, 00) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 23u, Group GroupD, Confirmed argentinaId, Confirmed croatiaId, argentinaVsCroatiaKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 23u)
        let acuñaId = Guid "1eae6233-c07c-4545-ad28-a3d1b134e562" |> PlayerId
        let mercadoId = Guid "76f332ad-588c-4737-928a-5f3ea8d29378" |> PlayerId
        let otamendiId = Guid "3a6757f1-8198-450d-a54b-b56fa1b66aab" |> PlayerId
        let corlukaId = Guid "f6615095-48d5-40ae-89f7-0f59016488e9" |> PlayerId
        let kovačićId = Guid "2cf2512f-0270-490a-83e5-75377701a306" |> PlayerId
        let mandžukićId = Guid "0ced2454-77bb-461a-9907-37f6dfdf051f" |> PlayerId
        let rebićId = Guid "ae289f35-8ef2-4320-a65c-f7f41f3f07ad" |> PlayerId
        let vrsaljkoId = Guid "d46a99d1-72f8-44b7-b43e-c00dadaafe38" |> PlayerId
        let matchEvent = (croatiaId, rebićId, None) |> Goal
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 23u, Rvn 1, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (croatiaId, modrićId, brozovićId |> Some) |> Goal
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 23u, Rvn 2, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (croatiaId, rakitićId, kovačićId |> Some) |> Goal
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 23u, Rvn 3, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (argentinaId, mercadoId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 23u, Rvn 4, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (argentinaId, otamendiId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 23u, Rvn 5, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (argentinaId, acuñaId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 23u, Rvn 6, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (croatiaId, rebićId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 23u, Rvn 7, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (croatiaId, mandžukićId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 23u, Rvn 8, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (croatiaId, vrsaljkoId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 23u, Rvn 9, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (croatiaId, corlukaId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 23u, Rvn 10, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (croatiaId, subašićId) |> CleanSheet
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 23u, Rvn 11, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (croatiaId, modrićId) |> ManOfTheMatch
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 23u, Rvn 12, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)

        let nigeriaVsIcelandKO = (2018, 06, 22, 15, 00) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 24u, Group GroupD, Confirmed nigeriaId, Confirmed icelandId, nigeriaVsIcelandKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 24u)
        let idowuId = Guid "6ca5045a-73ad-4924-b27d-7ed832a95e5d" |> PlayerId
        let mosesId = Guid "33648061-2e23-4bda-811d-4879699d2e29" |> PlayerId
        let musaId = Guid "599cddb7-b2f1-4f4c-bae7-06dcb308dd56" |> PlayerId
        let omeruoId = Guid "4970c729-9f49-4c12-bd4a-9348adb49d60" |> PlayerId
        let uzohoId = Guid "8711403b-2c78-4fb6-b92e-7f7e25970b69" |> PlayerId
        let sigurðssonGId = Guid "84c1b314-b154-4a88-8c15-616ed4e570b6" |> PlayerId
        let matchEvent = (nigeriaId, musaId, mosesId |> Some) |> Goal
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 24u, Rvn 1, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (nigeriaId, musaId, omeruoId |> Some) |> Goal
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 24u, Rvn 2, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (icelandId, sigurðssonGId, Missed) |> Penalty
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 24u, Rvn 3, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (nigeriaId, idowuId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 24u, Rvn 4, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (nigeriaId, uzohoId) |> CleanSheet
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 24u, Rvn 5, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (nigeriaId, musaId) |> ManOfTheMatch
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 24u, Rvn 6, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)

        let nigeriaVsArgentinaKO = (2018, 06, 26, 18, 00) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 39u, Group GroupD, Confirmed nigeriaId, Confirmed argentinaId, nigeriaVsArgentinaKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 39u)

        let icelandVsCroatiaKO = (2018, 06, 26, 18, 00) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 40u, Group GroupD, Confirmed icelandId, Confirmed croatiaId, icelandVsCroatiaKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 40u)

        // #endregion
        // #region: Group E
        let costaRicaVsSerbiaKO = (2018, 06, 17, 12, 00) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 10u, Group GroupE, Confirmed costaRicaId, Confirmed serbiaId, costaRicaVsSerbiaKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 10u)
        let calvoId = Guid "48e7c084-b8bc-451f-8314-09a6434902fa" |> PlayerId
        let guzmánId = Guid "bd554d53-86ae-407c-9293-5f86351cab87" |> PlayerId
        let ivanovićId = Guid "95927330-428b-49e0-8e26-d96d48042b6f" |> PlayerId
        let kolarovId = Guid "98692a4a-6c96-40ae-bbe1-5f2b28c94cea" |> PlayerId
        let prijovićId = Guid "3111f500-31cd-4506-95e4-cd77bcf4d0e6" |> PlayerId
        let stojkovićId = Guid "8bffe0de-d307-4007-b722-13ddc615b387" |> PlayerId
        let matchEvent = (serbiaId, kolarovId, None) |> Goal
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 10u, Rvn 1, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (costaRicaId, calvoId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 10u, Rvn 2, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (costaRicaId, guzmánId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 10u, Rvn 3, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (serbiaId, ivanovićId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 10u, Rvn 4, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (serbiaId, prijovićId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 10u, Rvn 5, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (serbiaId, stojkovićId) |> CleanSheet
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 10u, Rvn 6, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (serbiaId, kolarovId) |> ManOfTheMatch
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 10u, Rvn 7, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)

        let brazilVsSwitzerlandKO = (2018, 06, 17, 18, 00) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 9u, Group GroupE, Confirmed brazilId, Confirmed switzerlandId, brazilVsSwitzerlandKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 9u)
        let casemiroId = Guid "68fa98df-0070-489b-ab31-3d85564ebd52" |> PlayerId
        let coutinhoId = Guid "df1c6d7b-e4ea-40f7-bc8f-69f5eb67be94" |> PlayerId
        let behramiId = Guid "be4b38c5-234c-47de-b150-546b1e691ce3" |> PlayerId
        let lichtsteinerId = Guid "bcc5aecd-8a03-43c1-b5a4-11740c982480" |> PlayerId
        let schärId = Guid "368aeb8e-c72b-4a62-874b-5798794a881e" |> PlayerId
        let shaqiriId = Guid "60fd83dd-3ec0-4fd8-be5a-43559ccf6c0f" |> PlayerId
        let zuberId = Guid "5e267e1a-1dc3-4f02-89b6-fa41c6eb0100" |> PlayerId
        let matchEvent = (brazilId, coutinhoId, None) |> Goal
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 9u, Rvn 1, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (switzerlandId, zuberId, shaqiriId |> Some) |> Goal
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 9u, Rvn 2, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (brazilId, casemiroId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 9u, Rvn 3, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (switzerlandId, lichtsteinerId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 9u, Rvn 4, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (switzerlandId, schärId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 9u, Rvn 5, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (switzerlandId, behramiId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 9u, Rvn 6, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (brazilId, coutinhoId) |> ManOfTheMatch
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 9u, Rvn 7, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)

        let brazilVsCostaRicaKO = (2018, 06, 22, 12, 00) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 25u, Group GroupE, Confirmed brazilId, Confirmed costaRicaId, brazilVsCostaRicaKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 25u)
        let alissonId = Guid "6550e6f4-95cd-4259-87a6-b1352bd91b3e" |> PlayerId
        let costaBrazilId = Guid "71eedd91-93e5-4ae8-b0b0-6d2d4ab1b040" |> PlayerId
        let jesusId = Guid "b7253293-5a37-4035-8031-8b509c1b36d2" |> PlayerId
        let neymarId = Guid "402fb8a9-b8de-4b71-9aae-0eef70925bcd" |> PlayerId
        let acostaId = Guid "568793a8-c178-4ec8-816c-ae8d1ee6e2c5" |> PlayerId
        let matchEvent = (brazilId, coutinhoId, jesusId |> Some) |> Goal
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 25u, Rvn 1, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (brazilId, neymarId, costaBrazilId |> Some) |> Goal
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 25u, Rvn 2, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (brazilId, coutinhoId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 25u, Rvn 3, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (brazilId, neymarId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 25u, Rvn 4, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (costaRicaId, acostaId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 25u, Rvn 5, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (brazilId, alissonId) |> CleanSheet
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 25u, Rvn 6, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (brazilId, coutinhoId) |> ManOfTheMatch
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 25u, Rvn 7, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)

        let serbiaVsSwitzerlandKO = (2018, 06, 22, 18, 00) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 26u, Group GroupE, Confirmed serbiaId, Confirmed switzerlandId, serbiaVsSwitzerlandKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 26u)
        let gavranovićId = Guid "622d079f-4ee2-437f-adc4-f1dff5e637f3" |> PlayerId
        let xhakaId = Guid "fd8d10a9-37b3-44c3-a17b-95ab5f4c0227" |> PlayerId
        let matićId = Guid "3e625c19-72ac-4ab5-8162-6785cb8cb188" |> PlayerId
        let milinkovićSavićId = Guid "25e2cfaf-8cc0-4b2e-acb2-fb51bc6be849" |> PlayerId
        let milivojevićId = Guid "141d8037-5750-4935-8130-0044a0d42350" |> PlayerId
        let mitrovićId = Guid "67204e21-720c-461a-9004-f1326fe6802d" |> PlayerId
        let tadićId = Guid "4db03b2c-008c-419e-b5e1-90d93725f58f" |> PlayerId
        let matchEvent = (serbiaId, mitrovićId, tadićId |> Some) |> Goal
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 26u, Rvn 1, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (switzerlandId, xhakaId, None) |> Goal
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 26u, Rvn 2, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (switzerlandId, shaqiriId, gavranovićId |> Some) |> Goal
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 26u, Rvn 3, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (serbiaId, milinkovićSavićId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 26u, Rvn 4, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (serbiaId, milivojevićId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 26u, Rvn 5, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (serbiaId, matićId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 26u, Rvn 6, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (serbiaId, mitrovićId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 26u, Rvn 7, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (switzerlandId, shaqiriId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 26u, Rvn 8, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (switzerlandId, shaqiriId) |> ManOfTheMatch
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 26u, Rvn 9, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)

        let serbiaVsBrazilKO = (2018, 06, 27, 18, 00) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 41u, Group GroupE, Confirmed serbiaId, Confirmed brazilId, serbiaVsBrazilKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 41u)

        let switzerlandVsCostaRicaKO = (2018, 06, 27, 18, 00) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 42u, Group GroupE, Confirmed switzerlandId, Confirmed costaRicaId, switzerlandVsCostaRicaKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 42u)

        // #endregion
        // #region: Group F
        let germanyVsMexicoKO = (2018, 06, 17, 15, 00) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 11u, Group GroupF, Confirmed germanyId, Confirmed mexicoId, germanyVsMexicoKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 11u)
        let hummelsId = Guid "f2501d38-bc99-41f4-a07e-39f9ac7db80e" |> PlayerId
        let müllerId = Guid "5e6eecd5-6baa-4a73-83dd-60df6f13d478" |> PlayerId
        let hernándezId = Guid "af3d298e-56ad-4a3b-913c-4826c14e07e7" |> PlayerId
        let herreraId = Guid "97a63f16-71db-43a2-9b72-ddea4ac76f34" |> PlayerId
        let lozanoId = Guid "7c979251-5a07-43e1-a6d8-f284f5e90ada" |> PlayerId
        let morenoId = Guid "c4af5497-ac6a-4624-8a72-2f0a22da8379" |> PlayerId
        let ochoaId = Guid "b5e1008a-1544-4d46-b1f2-6c4daebd8a02" |> PlayerId
        let matchEvent = (mexicoId, lozanoId, hernándezId |> Some) |> Goal
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 11u, Rvn 1, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (germanyId, hummelsId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 11u, Rvn 2, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (germanyId, müllerId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 11u, Rvn 3, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (mexicoId, herreraId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 11u, Rvn 4, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (mexicoId, morenoId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 11u, Rvn 5, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (mexicoId, ochoaId) |> CleanSheet
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 11u, Rvn 6, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (mexicoId, lozanoId) |> ManOfTheMatch
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 11u, Rvn 7, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)

        let swedenVsSouthKoreaKO = (2018, 06, 18, 12, 00) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 12u, Group GroupF, Confirmed swedenId, Confirmed southKoreaId, swedenVsSouthKoreaKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 12u)
        let claessonId = Guid "8a550de8-8b8b-40c2-8ebb-f320563fcb9f" |> PlayerId
        let granqvistId = Guid "12766393-1b57-4875-901a-576b3d9e525d" |> PlayerId
        let olsenId = Guid "42013589-ea1e-42bf-aaba-0fb2e51e883f" |> PlayerId
        let shinWookId = Guid "33c80682-a0b7-41bf-8f85-8842bd8d8a6c" |> PlayerId
        let heeChanId = Guid "198de87b-6f2c-44e7-8219-5598fb546e48" |> PlayerId
        let matchEvent = (swedenId, granqvistId, None) |> Goal
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 12u, Rvn 1, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (swedenId, claessonId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 12u, Rvn 2, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (southKoreaId, shinWookId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 12u, Rvn 3, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (southKoreaId, heeChanId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 12u, Rvn 4, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (swedenId, olsenId) |> CleanSheet
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 12u, Rvn 5, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (swedenId, granqvistId) |> ManOfTheMatch
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 12u, Rvn 6, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)

        let southKoreaVsMexicoKO = (2018, 06, 23, 15, 00) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 28u, Group GroupF, Confirmed southKoreaId, Confirmed mexicoId, southKoreaVsMexicoKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 28u)
        let heungMinId = Guid "b83a2c03-7416-49ad-a3aa-54cca6efa069" |> PlayerId
        let jaeSungId = Guid "72fa9c90-5d9b-42e4-8802-edd9d32be269" |> PlayerId
        let seungWooId = Guid "40163f86-a655-4423-84e4-4102f28ba72f" |> PlayerId
        let wooYoungId = Guid "bcc0019f-9a40-4883-9ac8-6e340fd008bc" |> PlayerId
        let yongId = Guid "f4c93506-52c7-421b-91cd-8b264e33a579" |> PlayerId
        let youngGwonId = Guid "271f6af0-5fdb-4ecf-b0a2-35349e885002" |> PlayerId
        let velaId = Guid "38e7264d-c22c-46a6-bd0a-2b6a87a885c4" |> PlayerId
        let matchEvent = (mexicoId, velaId, Scored) |> Penalty
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 28u, Rvn 1, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (mexicoId, hernándezId, lozanoId |> Some) |> Goal
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 28u, Rvn 2, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (southKoreaId, heungMinId, jaeSungId |> Some) |> Goal
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 28u, Rvn 3, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (southKoreaId, youngGwonId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 28u, Rvn 4, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (southKoreaId, yongId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 28u, Rvn 5, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (southKoreaId, seungWooId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 28u, Rvn 6, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (southKoreaId, wooYoungId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 28u, Rvn 7, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (mexicoId, hernándezId) |> ManOfTheMatch
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 28u, Rvn 8, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)

        let germanyVsSwedenKO = (2018, 06, 23, 18, 00) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 27u, Group GroupF, Confirmed germanyId, Confirmed swedenId, germanyVsSwedenKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 27u)
        let boatengId = Guid "018d93dd-ce9d-4379-8117-23d4af6dc7f4" |> PlayerId
        let gómezId = Guid "b0523780-a1ba-4a14-a27a-8ba1662b0a40" |> PlayerId
        let kroosId = Guid "57bdd513-4f1d-4064-a5c3-143d7ec0ea25" |> PlayerId
        let reusId = Guid "3abca99c-57d9-4c52-826c-401cd70f15d0" |> PlayerId
        let ekdalId = Guid "8301a692-f901-4c85-ad37-c7d545aeda33" |> PlayerId
        let larssonId = Guid "7f4e0de4-cd94-431b-abaf-5ff94f05f609" |> PlayerId
        let toivonenId = Guid "4e20bed2-fe6e-4fef-b101-2fd35c253a6d" |> PlayerId
        let matchEvent = (swedenId, toivonenId, claessonId |> Some) |> Goal
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 27u, Rvn 1, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (germanyId, reusId, gómezId |> Some) |> Goal
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 27u, Rvn 2, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (germanyId, kroosId, reusId |> Some) |> Goal
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 27u, Rvn 3, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (germanyId, boatengId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 27u, Rvn 4, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (germanyId, boatengId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 27u, Rvn 5, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (swedenId, ekdalId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 27u, Rvn 6, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (swedenId, larssonId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 27u, Rvn 7, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (germanyId, reusId) |> ManOfTheMatch
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 27u, Rvn 8, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)

        let southKoreaVsGermanyKO = (2018, 06, 27, 14, 00) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 43u, Group GroupF, Confirmed southKoreaId, Confirmed germanyId, southKoreaVsGermanyKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 43u)

        let mexicoVsSwedenKO = (2018, 06, 27, 14, 00) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 44u, Group GroupF, Confirmed mexicoId, Confirmed swedenId, mexicoVsSwedenKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 44u)

        // #endregion
        // #region: Group G
        let belgiumVsPanamaKO = (2018, 06, 18, 15, 00) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 13u, Group GroupG, Confirmed belgiumId, Confirmed panamaId, belgiumVsPanamaKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 13u)
        let courtoisId = Guid "1839b793-3798-445b-a7fe-47581883549b" |> PlayerId
        let deBruyneId = Guid "403802d5-e204-4f0c-b58c-abaa1f061075" |> PlayerId
        let hazardEId = Guid "3837ea91-aef0-4013-b62e-a78f1c931d9d" |> PlayerId
        let lukakuId = Guid "c2b6dadb-8f6e-4b8b-82f4-4ec4c5ac2d2f" |> PlayerId
        let mertensId = Guid "19d05166-b199-4600-86c6-236feb9a2a45" |> PlayerId
        let meunierId = Guid "20616381-eefe-4751-a513-744015181209" |> PlayerId
        let vertonghenId = Guid "4eca7575-a44b-49e6-8c4a-6cfac041f7fd" |> PlayerId
        let bárcenasId = Guid "928a2f7c-12cd-4bdc-8554-fa2d47c1ea4c" |> PlayerId
        let cooperId = Guid "f2692cfa-503b-44e7-96b5-386c08645459" |> PlayerId
        let davisId = Guid "54ccb9c9-8062-46c5-bc32-4d587b77b6c4" |> PlayerId
        let godoyId = Guid "7729b1d2-2bd7-40d9-83b6-9495f98c4764" |> PlayerId
        let murilloId = Guid "dbcb9bc0-ddf8-4350-bce2-614c5f689359" |> PlayerId
        let matchEvent = (belgiumId, mertensId, None) |> Goal
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 13u, Rvn 1, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (belgiumId, lukakuId, deBruyneId |> Some) |> Goal
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 13u, Rvn 2, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (belgiumId, lukakuId, hazardEId |> Some) |> Goal
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 13u, Rvn 3, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (belgiumId, meunierId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 13u, Rvn 4, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (belgiumId, vertonghenId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 13u, Rvn 5, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (belgiumId, deBruyneId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 13u, Rvn 6, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (panamaId, davisId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 13u, Rvn 7, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (panamaId, bárcenasId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 13u, Rvn 8, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (panamaId, cooperId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 13u, Rvn 9, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (panamaId, murilloId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 13u, Rvn 10, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (panamaId, godoyId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 13u, Rvn 11, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (belgiumId, courtoisId) |> CleanSheet
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 13u, Rvn 12, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (belgiumId, lukakuId) |> ManOfTheMatch
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 13u, Rvn 13, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)

        let tunisiaVsEnglandKO = (2018, 06, 18, 18, 00) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 14u, Group GroupG, Confirmed tunisiaId, Confirmed englandId, tunisiaVsEnglandKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 14u)
        let sassiId = Guid "2065805e-4e56-46c1-9197-d9c283eb1ffd" |> PlayerId
        let kaneId = Guid "00000072-0000-0000-0000-000000000001" |> PlayerId
        let maguireId = Guid "00000072-0000-0005-0000-000000000000" |> PlayerId
        let walkerId = Guid "00000072-0000-0009-0000-000000000000" |> PlayerId
        let matchEvent = (englandId, kaneId, None) |> Goal
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 14u, Rvn 1, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (tunisiaId, sassiId, Scored) |> Penalty
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 14u, Rvn 2, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (englandId, kaneId, maguireId |> Some) |> Goal
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 14u, Rvn 3, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (englandId, walkerId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 14u, Rvn 4, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (englandId, kaneId) |> ManOfTheMatch
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 14u, Rvn 5, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)

        let belgiumVsTunisiaKO = (2018, 06, 23, 12, 00) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 29u, Group GroupG, Confirmed belgiumId, Confirmed tunisiaId, belgiumVsTunisiaKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 29u)
        let alderweireldId = Guid "5dd9e0ff-c5bb-443d-89f2-1f721873a113" |> PlayerId
        let batshuayiId = Guid "4f08b1bb-7f2b-495c-b86f-6a6e13b00d46" |> PlayerId
        let tielemansId = Guid "0efcc1c2-5e72-4c61-8007-b11fdb76dc55" |> PlayerId
        let bronnId = Guid "1863c8ba-b2e5-4f1b-a778-4306378c1a32" |> PlayerId
        let khazriId = Guid "19c731b9-1e01-4433-b8c8-d6426f39b2f2" |> PlayerId
        let nagguezId = Guid "d390f862-6c01-4591-b421-a83480170a93" |> PlayerId
        let matchEvent = (belgiumId, hazardEId, Scored) |> Penalty
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 29u, Rvn 1, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (belgiumId, lukakuId, mertensId |> Some) |> Goal
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 29u, Rvn 2, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (tunisiaId, bronnId, khazriId |> Some) |> Goal
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 29u, Rvn 3, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (belgiumId, lukakuId, meunierId |> Some) |> Goal
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 29u, Rvn 4, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (belgiumId, hazardEId, alderweireldId |> Some) |> Goal
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 29u, Rvn 5, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (belgiumId, batshuayiId, tielemansId |> Some) |> Goal
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 29u, Rvn 6, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (tunisiaId, khazriId, nagguezId |> Some) |> Goal
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 29u, Rvn 7, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (tunisiaId, sassiId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 29u, Rvn 8, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (belgiumId, hazardEId) |> ManOfTheMatch
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 29u, Rvn 9, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)

        let englandVsPanamaKO = (2018, 06, 24, 12, 00) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 30u, Group GroupG, Confirmed englandId, Confirmed panamaId, englandVsPanamaKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 30u)
        let lindgardId = Guid "00000072-0000-0000-0004-000000000000" |> PlayerId
        let loftusCheekId = Guid "00000072-0000-0000-0005-000000000000" |> PlayerId
        let sterlingId = Guid "00000072-0000-0000-0000-000000000003" |> PlayerId
        let stonesId = Guid "00000072-0000-0007-0000-000000000000" |> PlayerId
        let trippierId = Guid "00000072-0000-0008-0000-000000000000" |> PlayerId
        let avilaId = Guid "fcd7cc93-2e55-4c06-b3c1-039e0b42f1f8" |> PlayerId
        let baloyId = Guid "428cc7ff-dec4-41d9-bf88-37e358fa31f4" |> PlayerId
        let escobarId = Guid "54bbc4c5-087d-4453-92cf-f8baecb80222" |> PlayerId
        let matchEvent = (englandId, stonesId, trippierId |> Some) |> Goal
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 30u, Rvn 1, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (englandId, kaneId, Scored) |> Penalty
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 30u, Rvn 2, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (englandId, lindgardId, sterlingId |> Some) |> Goal
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 30u, Rvn 3, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (englandId, stonesId, None) |> Goal
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 30u, Rvn 4, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (englandId, kaneId, Scored) |> Penalty
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 30u, Rvn 5, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (englandId, kaneId, loftusCheekId |> Some) |> Goal
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 30u, Rvn 6, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (panamaId, baloyId, avilaId |> Some) |> Goal
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 30u, Rvn 7, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (englandId, loftusCheekId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 30u, Rvn 8, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (panamaId, cooperId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 30u, Rvn 9, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (panamaId, escobarId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 30u, Rvn 10, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (panamaId, murilloId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 30u, Rvn 11, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (englandId, kaneId) |> ManOfTheMatch
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 30u, Rvn 12, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)

        let englandVsBelgiumKO = (2018, 06, 28, 18, 00) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 45u, Group GroupG, Confirmed englandId, Confirmed belgiumId, englandVsBelgiumKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 45u)

        let panamaVsTunisiaKO = (2018, 06, 28, 18, 00) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 46u, Group GroupG, Confirmed panamaId, Confirmed tunisiaId, panamaVsTunisiaKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 46u)

        // #endregion
        // #region: Group H
        let colombiaVsJapanKO = (2018, 06, 19, 12, 00) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 16u, Group GroupH, Confirmed colombiaId, Confirmed japanId, colombiaVsJapanKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 16u)
        let barriosId = Guid "e4d92de6-415d-41fb-9dac-57a3da0fd7a8" |> PlayerId
        let quinteroId = Guid "fe1d1306-5f64-4d33-8ba3-5d3d3cf7bbf6" |> PlayerId
        let rodríguezId = Guid "2d39c02d-2a69-4f95-9c19-ccc778cdf4bc" |> PlayerId
        let sánchezColombiaId = Guid "ecfb6653-a353-4fe8-bf57-006249a4eb1f" |> PlayerId
        let hondaId = Guid "5656320e-3655-403d-a959-bea9c3947f7b" |> PlayerId
        let kagawaId = Guid "1a929ac7-45e1-4507-8b19-4324d5023baa" |> PlayerId
        let kawashimaId = Guid "e756b578-1c0f-4af4-a861-7435a4f99353" |> PlayerId
        let osakoId = Guid "e309a71d-00dd-4e8a-b7a4-6dedfa525e3a" |> PlayerId
        let matchEvent = (japanId, kagawaId, Scored) |> Penalty
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 16u, Rvn 1, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (colombiaId, quinteroId, None) |> Goal
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 16u, Rvn 2, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (japanId, osakoId, hondaId |> Some) |> Goal
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 16u, Rvn 3, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (colombiaId, sánchezColombiaId) |> RedCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 16u, Rvn 4, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (colombiaId, barriosId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 16u, Rvn 5, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (colombiaId, rodríguezId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 16u, Rvn 6, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (japanId, kawashimaId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 16u, Rvn 7, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (japanId, osakoId) |> ManOfTheMatch
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 16u, Rvn 8, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)

        let polandVsSenegalKO = (2018, 06, 19, 15, 00) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 15u, Group GroupH, Confirmed polandId, Confirmed senegalId, polandVsSenegalKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 15u)
        let cionekId = Guid "8f41ad65-938d-4c96-b35e-ac9cc0244e95" |> PlayerId
        let grosickiId = Guid "f630a313-2209-4a9f-8853-6c0405f1906d" |> PlayerId
        let krychowiakId = Guid "9b24eabc-e61b-4ca0-8c23-300e937d3fc1" |> PlayerId
        let gueyeId = Guid "5dee97b6-4281-45b0-a895-5926c1ae56aa" |> PlayerId
        let niangId = Guid "45c81291-cc52-4489-bfb1-8cef8fe58b25" |> PlayerId
        let sanéId = Guid "3dd4ce75-e007-4b1d-a15b-a3ba4faf4a05" |> PlayerId
        let matchEvent = (polandId, cionekId) |> OwnGoal
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 15u, Rvn 1, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (senegalId, niangId, None) |> Goal
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 15u, Rvn 2, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (polandId, krychowiakId, grosickiId |> Some) |> Goal
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 15u, Rvn 3, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (polandId, krychowiakId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 15u, Rvn 4, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (senegalId, sanéId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 15u, Rvn 5, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (senegalId, gueyeId) |> YellowCard
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 15u, Rvn 6, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)
        let matchEvent = (senegalId, niangId) |> ManOfTheMatch
        let! result = nephTokens.ResultsAdminToken |> ifToken (fun token -> (token, nephId, fixtureId 15u, Rvn 7, matchEvent) |> fixtures.HandleAddMatchEventSpecialCmdAsync)
        result |> logShouldSucceed (sprintf "HandleAddMatchEventSpecialCmdAsync (%A)" matchEvent)

        let japanVsSenegalKO = (2018, 06, 24, 15, 00) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 32u, Group GroupH, Confirmed japanId, Confirmed senegalId, japanVsSenegalKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 32u)

        let polandVsColombiaKO = (2018, 06, 24, 18, 00) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 31u, Group GroupH, Confirmed polandId, Confirmed colombiaId, polandVsColombiaKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 31u)

        let japanVsPolandKO = (2018, 06, 28, 14, 00) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 47u, Group GroupH, Confirmed japanId, Confirmed polandId, japanVsPolandKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 47u)

        let senegalVsColombiaKO = (2018, 06, 28, 14, 00) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 48u, Group GroupH, Confirmed senegalId, Confirmed colombiaId, senegalVsColombiaKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 48u)

        // #endregion
        // #region: Round-of-16
        let winnerCVsRunnerUpDKO = (2018, 06, 30, 14, 00) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 50u, RoundOf16 50u, Unconfirmed (Winner (Group GroupC)), Unconfirmed (RunnerUp GroupD), winnerCVsRunnerUpDKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 50u)

        let winnerAVsRunnerUpBKO = (2018, 06, 30, 18, 00) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 49u, RoundOf16 49u, Unconfirmed (Winner (Group GroupA)), Unconfirmed (RunnerUp GroupB), winnerAVsRunnerUpBKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 49u)

        let winnerBVsRunnerUpAKO = (2018, 07, 01, 14, 00) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 51u, RoundOf16 51u, Unconfirmed (Winner (Group GroupB)), Unconfirmed (RunnerUp GroupA), winnerBVsRunnerUpAKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 51u)

        let winnerDVsRunnerUpCKO = (2018, 07, 01, 18, 00) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 52u, RoundOf16 52u, Unconfirmed (Winner (Group GroupD)), Unconfirmed (RunnerUp GroupC), winnerDVsRunnerUpCKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 52u)

        let winnerEVsRunnerUpFKO = (2018, 07, 02, 14, 00) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 53u, RoundOf16 53u, Unconfirmed (Winner (Group GroupE)), Unconfirmed (RunnerUp GroupF), winnerEVsRunnerUpFKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 53u)

        let winnerGVsRunnerUpHKO = (2018, 07, 02, 18, 00) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 54u, RoundOf16 54u, Unconfirmed (Winner (Group GroupG)), Unconfirmed (RunnerUp GroupH), winnerGVsRunnerUpHKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 54u)

        let winnerFVsRunnerUpEKO = (2018, 07, 03, 14, 00) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 55u, RoundOf16 55u, Unconfirmed (Winner (Group GroupF)), Unconfirmed (RunnerUp GroupE), winnerFVsRunnerUpEKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 55u)

        let winnerHVsRunnerUpGKO = (2018, 07, 03, 18, 00) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 56u, RoundOf16 56u, Unconfirmed (Winner (Group GroupH)), Unconfirmed (RunnerUp GroupG), winnerHVsRunnerUpGKO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 56u)

        // #endregion
        // #region: Quarter-finals
        let winner49VsWinner50KO = (2018, 07, 06, 14, 00) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 57u, QuarterFinal 1u, Unconfirmed (Winner (RoundOf16 49u)), Unconfirmed (Winner (RoundOf16 50u)), winner49VsWinner50KO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 57u)

        let winner53VsWinner54KO = (2018, 07, 06, 18, 00) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 58u, QuarterFinal 2u, Unconfirmed (Winner (RoundOf16 53u)), Unconfirmed (Winner (RoundOf16 54u)), winner53VsWinner54KO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 58u)

        let winner55VsWinner56KO = (2018, 07, 07, 14, 00) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 59u, QuarterFinal 3u, Unconfirmed (Winner (RoundOf16 55u)), Unconfirmed (Winner (RoundOf16 56u)), winner55VsWinner56KO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 59u)

        let winner51VsWinner52KO = (2018, 07, 07, 18, 00) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 60u, QuarterFinal 4u, Unconfirmed (Winner (RoundOf16 51u)), Unconfirmed (Winner (RoundOf16 52u)), winner51VsWinner52KO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 60u)

        // #endregion
        // #region: Semi-finals
        let winnerQF1VsWinnerQF2KO = (2018, 07, 10, 18, 00) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 61u, SemiFinal 1u, Unconfirmed (Winner (QuarterFinal 1u)), Unconfirmed (Winner (QuarterFinal 2u)), winnerQF1VsWinnerQF2KO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 61u)

        let winnerQF3VsWinnerQF4KO = (2018, 07, 11, 18, 00) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 62u, SemiFinal 2u, Unconfirmed (Winner (QuarterFinal 3u)), Unconfirmed (Winner (QuarterFinal 4u)), winnerQF3VsWinnerQF4KO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 62u)

        // #endregion
        // #region: Third/fourth place play-off
        let loserSF1VsLoserSF2KO = (2018, 07, 14, 14, 00) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 63u, ThirdPlacePlayOff, Unconfirmed (Loser 1u), Unconfirmed (Loser 2u), loserSF1VsLoserSF2KO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 63u)

        // #endregion
        // #region: Final
        let winnerSF1VsWinnerSF2KO = (2018, 07, 15, 15, 00) |> dateTimeOffsetUtc
        let! result = nephTokens.CreateFixtureToken |> ifToken (fun token -> (token, nephId, fixtureId 64u, Final, Unconfirmed (Winner (SemiFinal 1u)), Unconfirmed (Winner (SemiFinal 2u)), winnerSF1VsWinnerSF2KO) |> fixtures.HandleCreateFixtureCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateFixtureCmdAsync (match %i)" 64u)*)

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
        let draft1Starts, draft1Ends = (2018, 06, 07, 21, 30) |> dateTimeOffsetUtc, (2018, 06, 10, 17, 00) |> dateTimeOffsetUtc
        let draft1Type = (draft1Starts, draft1Ends) |> Constrained
        let! result = nephTokens.ProcessDraftToken |> ifToken (fun token -> (token, nephId, draft1Id, draft1Ordinal, draft1Type) |> drafts.HandleCreateDraftCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateDraftCmdAsync (%A %A)" draft1Id draft1Ordinal)
        let draft2Id, draft2Ordinal = Guid "00000000-0000-0000-0000-000000000002" |> DraftId, DraftOrdinal 2
        let draft2Starts, draft2Ends = (2018, 06, 10, 22, 30) |> dateTimeOffsetUtc, (2018, 06, 12, 17, 00) |> dateTimeOffsetUtc
        let draft2Type = (draft2Starts, draft2Ends) |> Constrained
        let! result = nephTokens.ProcessDraftToken |> ifToken (fun token -> (token, nephId, draft2Id, draft2Ordinal, draft2Type) |> drafts.HandleCreateDraftCmdAsync)
        result |> logShouldSucceed (sprintf "HandleCreateDraftCmdAsync (%A %A)" draft2Id draft2Ordinal)
        let draft3Id, draft3Ordinal = Guid "00000000-0000-0000-0000-000000000003" |> DraftId, DraftOrdinal 3
        let draft3Starts, draft3Ends = (2018, 06, 12, 22, 30) |> dateTimeOffsetUtc, (2018, 06, 14, 11, 00) |> dateTimeOffsetUtc
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
    do! createInitialDraftsEventsIfNecessary // note: although this can cause various events to be broadcast (DraftsRead | DraftEventWritten | &c.), no agents should yet be subscribed to these
    previousLogFilter |> consoleLogger.ChangeLogFilter }
