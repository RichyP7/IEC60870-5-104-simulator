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
            REPO["Value Repository<br/>(in-memory)"]
        end
        DOMAIN["Domain Layer<br/>(datapoint model, simulation modes)"]
        CONFIG[("SimulationOptions.json")]
    end

    UI["Angular Web UI"]

    SCADA <-->|"IEC 60870-5-104"| IEC
    BROWSER --> UI
    UI -->|REST| CTRL
    UI <-->|WebSocket| HUB
    RESTCLIENT --> CTRL

    CTRL --> ENGINE
    ENGINE --> CYCLIC
    ENGINE --> SCEN
    CYCLIC --> REPO
    SCEN --> REPO
    IEC --> CMD
    CMD --> REPO
    REPO --> IEC
    CYCLIC -.->|push updates| HUB
    SCEN -.->|scenario status| HUB
    CONFIG --> ENGINE
    Infra --> DOMAIN
    API --> DOMAIN
```

## Layers

- **API Layer** (`src/IEC60870-5-104-simulator.API`) — REST controllers, SignalR hub, health checks, and the simulation engine orchestration
- **Infrastructure Layer** (`src/IEC60870-5-104-simulator.Infrastructure`) — IEC 104 server (lib60870.NET), cyclic simulation, scenario execution, command handling, value storage
- **Domain Layer** (`src/IEC60870-5-104-simulator.Domain`) — datapoint model and simulation mode logic
