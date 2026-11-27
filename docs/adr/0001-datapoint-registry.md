# ADR 0001 — One datapoint registry, live references, writes owned by the module

- **Status**: Accepted
- **Date**: 2026-11-27
- **Branch**: `refactor/datapoint-registry`

## Context

Two modules held the same `Iec104DataPoint` instances:

- `Iec104ConfigurationService` (`Domain/Service`) with dictionaries `dataPoints` and
  `commandDataPoints`
- `IecValueLocalStorageRepository` (`Infrastructure`) with `StoredDataPoints`

`Iec104ConfigurationService.InitStorage()` passed each stored instance straight into the
repository, and `DataPointConfigService.CreateDataPoint()` did the same on the runtime path.
The result was one object filed in two dictionaries, mutated in place from three threads
(tick loop, scenario `Task.Run`, HTTP request), and read through two different
"list everything" calls that answered differently depending on which module was asked.

Two concrete consequences:

1. `DataPointConfigService.DeleteDataPoint()` removed the measure from the repository and the
   config dictionary, but `Iec104CommandDataPointConfig.SimulatedDataPoint` still referenced
   it. `MirroredResponseFactory` then dereferenced a datapoint that no longer existed.
2. `AddDataPoint()` mutated the caller's object (`Value` parsing from `InitString`), so the
   config dictionary observed a mutation performed by the repository.

## Decision

**One module owns datapoints.** `DatapointRegistry` in the Domain layer replaces both
dictionaries and both interfaces (`IIecValueRepository`, `IIec104ConfigurationService`). It
exposes two narrow interfaces, both implemented by one internal class:

- `IDatapointRegistry` — measures: `Add`, `Remove`, `Get`, `SetValue`, `SetMode`,
  `AdjustStep`, `SetFrozen`, `Snapshot`
- `ICommandRegistry` — commands: `AddCommand`, `Resolve`, `CommandSnapshot`

The split exists because the two halves have disjoint callers, so each caller learns only the
half it uses. One class behind two seams is deliberate: the command/response link needs the
same address space as the measures, so putting them behind separate implementations would
reintroduce a cross-module reference.

**Live references across the seam, writes owned by the module.** `Get` returns the stored
object; callers may read it freely. No caller may mutate it: every write goes through a
registry method. This was chosen over returning immutable snapshots because the tick calls
`Get` once per datapoint per cycle (a hot path), and over leaving setters in place because
that is precisely what caused the bug.

**`Snapshot()` materialises the list once per tick.** `ConcurrentDictionary` makes
enumeration safe but not the objects; a materialised list gives a stable iteration so a
concurrent add cannot tear it.

**The command→response link is an address, not a reference.** `ResponseAddress` replaces
`SimulatedDataPoint`. Resolution happens at use time through the registry, so a deleted
response datapoint produces a clean miss instead of a null dereference, and the link survives
serialisation.

**Domain stays dependency-free.** `DatapointRegistry` takes no package references. DI
registration lives in Infrastructure.

**`AdjustStep(address, delta)` absorbs the ±1 arithmetic and the ±64/63 clamp** that was split
across `MirroredResponseFactory`.

## Consequences

- `Iec104ConfigurationService`, `IecValueLocalStorageRepository`, `IIecValueRepository` and
  `IIec104ConfigurationService` are deleted.
- Config loading (parsing `Iec104SimulationOptions` into domain objects, validating duplicate
  addresses and dangling `LinkedDataPointId`) moves out of the `BackgroundService` into a
  module that can be tested directly.
- Four infrastructure test files are rewritten to the new interface;
  `ScenarioServiceTest` keeps its `Times.Exactly(2)` assertion, which is the only thing
  pinning "two steps → two writes".
- `Domain` still has zero `PackageReference`s — a constraint worth checking in review.
- Deferred: immutable `DatapointSnapshot` for readers belongs to the single-writer tick
  (candidate 5), not here.

## Alternatives rejected

- **Keep both dictionaries, synchronise them.** The referential-integrity fanout would have
  to be re-implemented for every future link.
- **Immutable snapshots across the seam now.** Correct end state, but it belongs with the tick
  rewrite; paying it here would allocate on the hot path for no current benefit.
- **Keep all 14 repository methods for a minimal diff.** The typed getters and setters exist
  only because callers could not express the value type; every caller already knows it
  statically.
