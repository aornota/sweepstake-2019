module Aornota.Sweepstake2019.Server.Events.FixtureEvents

open Aornota.Sweepstake2019.Common.Domain.Fixture
open Aornota.Sweepstake2019.Common.Domain.Squad

open System

type FixtureEvent =
    | FixtureCreated of fixtureId : FixtureId * stage : Stage * homeParticipant : Participant * awayParticipant : Participant * kickOff : DateTimeOffset
    | ParticipantConfirmed of fixtureId : FixtureId * role : Role * squadId : SquadId
    | MatchEventAdded of fixtureId : FixtureId * matchEventId : MatchEventId * matchEvent : MatchEvent
    | MatchEventRemoved of fixtureId : FixtureId * matchEventId : MatchEventId
    | FixtureCancelled of fixtureId : FixtureId
    with
        member self.FixtureId =
            match self with
            | FixtureCreated (fixtureId, _, _, _, _) -> fixtureId
            | ParticipantConfirmed (fixtureId, _, _) -> fixtureId
            | MatchEventAdded (fixtureId, _, _) -> fixtureId
            | MatchEventRemoved (fixtureId, _) -> fixtureId
            | FixtureCancelled fixtureId -> fixtureId
