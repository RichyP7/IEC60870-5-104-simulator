# Architecture

Detailed internal architecture of the IEC 60870-5-104 Simulator.

```mermaid
flowchart LR
    subgraph Clients
        SCADA["IEC 104 Client / SCADA<br/>(TCP 2404)"]
        BROWSER["Browser<br/>(HTTP 4200 / 4300)"]
        RESTCLIENT["REST Client<br/>(HTTP 8080)"]
    end

    subgraph Simulator["IEC 60870-5-104 Simulator (.NET)"]
        subgraph API["API Layer"]
            CTRL["REST Controllers<br/>(DataPoints, Scenarios, Engine)"]
            HUB["SignalR Hub<br/>/hubs/simulation"]
            HEALTH["Health Checks<br/>/health/live · /health/ready"]
            ENGINE["Simulation Engine"]
        end
        subgraph Infra["Infrastructure Layer"]
            IEC["Iec104Service<br/>(lib60870.NET server)"]
            CYCLIC["Cyclic Simulation Service"]
            SCEN["Scenario Service"]
            CMD["Command Handler"]
        end
        REGISTRY["Datapoint Registry<br/>(measures + commands, in-memory)"]
        DOMAIN["Domain Layer<br/>(datapoint model, simulation modes, registry)"]
        CONFIG[("SimulationOptions.json")]
    end

    UI["Angular Web UI"]

    SCADA <-->|"IEC 60870-5-104"| IEC
    BROWSER --> UI
    UI -->|REST| CTRL
    UI <-->|WebSocket| HUB
    RESTCLIENT --> CTRL

    CTRL --> ENGINE
    ENGINE --> REGISTRY
    ENGINE --> CYCLIC
    ENGINE --> SCEN
    CYCLIC --> REGISTRY
    SCEN --> REGISTRY
    IEC --> CMD
    CMD --> REGISTRY
    REGISTRY --> IEC
    CYCLIC -.->|push updates| HUB
    SCEN -.->|scenario status| HUB
    CONFIG --> ENGINE
    Infra --> DOMAIN
    API --> DOMAIN
```

## Layers

- **API Layer** (`src/IEC60870-5-104-simulator.API`) — REST controllers, SignalR hub, health checks, and the simulation engine orchestration
- **Infrastructure Layer** (`src/IEC60870-5-104-simulator.Infrastructure`) — IEC 104 server (lib60870.NET), cyclic simulation, scenario execution, command handling
- **Domain Layer** (`src/IEC60870-5-104-simulator.Domain`) — datapoint model, simulation mode logic, and the `DatapointRegistry` that owns both measures and commands. No package dependencies.

## Datapoint ownership

`DatapointRegistry` is the single owner of datapoints: it holds the address uniqueness rule, the
value typing rule, and the command→response link. Readers get live references; every write goes
through a registry method. The rationale is recorded in
[ADR 0001](adr/0001-datapoint-registry.md).
