# Glossary

The words this project uses in a specific way. Terms are defined once here and used
consistently in code, issues and ADRs.

## Protocol

**ASDU** — Application Service Data Unit. One unit of information on the IEC 60870-5-104
wire: a type identification, a cause of transmission, a common address, and one or more
information objects.

**Cause of transmission (COT)** — Why a frame was sent: periodic, spontaneous,
interrogated-by-station, activation, activation confirmation, activation termination. Two
frames carrying the same value are not the same frame if their COT differs.

**Station address (CA)** — Common address of the controlled station.

**Object address (IOA)** — Information object address, unique within a station.

## Domain

**Datapoint** — A configured IEC 104 information object: one type, one address, one value,
one simulation mode. Identified by `IecAddress` (station address + object address). The
pair is unique; a second datapoint at the same address is an error, not an overwrite.

**Datapoint value** — The current value of a datapoint, as an `IecValueObject`. The
concrete type must match the datapoint's `Iec104DataTypes`: a single-point datapoint holds a
single-point value. Mismatch is a programming error, not a runtime condition to tolerate.

**Simulation mode** — How the tick computes a datapoint's next value: `Static`, `Periodic`,
`RandomWalk`, `GaussianNoise`, `PeriodicWave`, `EnergyCounter`, `CounterOnDemand`,
`Profile`, `CommandResponse`.

**Command datapoint** — A datapoint that accepts a command ASDU from the client instead of
being computed by the tick.

**Response datapoint** — The datapoint a command datapoint mirrors back to the client. Linked
by address (`ResponseAddress`), resolved at use time, never held as an object reference.

**Tick** — One iteration of the simulation cycle: apply pending writes, compute values,
emit frames.

## Architecture

**Registry** — The module that owns which datapoints exist and what their values are. It
holds the uniqueness of `IecAddress`, the typing rules, and the command→response link.
Callers ask it; they do not hold or mutate its datapoints.

**Freeze** — A datapoint flag meaning the tick skips this datapoint. Used by scenarios to pin
a value while a fault is active.

**Snapshot** — A materialised list of datapoints handed to the tick for one iteration, so that
concurrent adds and removes cannot tear the iteration.

## Layers

**Domain** — Model and rules. No package dependencies, no framework, no transport.

**Infrastructure** — Adapters to things outside the process: the lib60870 server, file
configuration, SignalR publishing.

**API** — HTTP surface, DI composition, the hosted simulation engine.
