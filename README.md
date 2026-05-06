# IEC60870-5-104-simulator

<img src="https://github.com/RichyP7/IEC60870-5-104-simulator/assets/14166202/f8c22afe-d4a0-4a1e-8655-43f6c3116cfe)" width=25% height=25%> <sup>[1](#myfootnote1)</sup>

## Overview

This repository contains an IEC 60870-104 protocol simulator written in .NET. The simulator acts as an IEC 104 server (controlled station) that listens on TCP port 2404 and allows IEC 104 clients to connect to it.

It is designed as a containerized application for testing the protocol in cloud-native scenarios. Whether you're testing your SCADA system, developing a new application, or learning the protocol, this simulator provides a convenient way to validate your implementation.

The application uses the [lib60870.NET](https://github.com/mz-automation) library by mz-automation.

### What is IEC 60870

Wikipedia:
> In electrical engineering and power system automation, the International Electrotechnical Commission 60870 standards define systems used for telecontrol (supervisory control and data acquisition). Such systems are used for controlling electric power transmission grids and other geographically widespread control systems. By use of standardized protocols, equipment from many different suppliers can be made to interoperate. IEC standard 60870 has six parts, defining general information related to the standard, operating conditions, electrical interfaces, performance requirements, and data transmission protocols. The 60870 standards are developed by IEC Technical Committee 57 (Working Group 03).

## Quickstart

### Prerequisites

- **Docker** or **Podman** installed

### Run with Docker

```bash
docker pull ghcr.io/richyp7/iec60870-5-104-simulator:main
docker run -p 2404:2404 -p 8080:8080 ghcr.io/richyp7/iec60870-5-104-simulator:main
```

### Run with Podman

```bash
podman pull ghcr.io/richyp7/iec60870-5-104-simulator:main
podman run -p 2404:2404 -p 8080:8080 ghcr.io/richyp7/iec60870-5-104-simulator:main
```

### Run with Docker Compose (includes UI)

```bash
docker compose up
```

This starts both the simulator backend (ports 2404 + 8080) and the configuration UI (port 4300).

### Build from source

```bash
docker build -t iec104-simulator .
docker run -p 2404:2404 -p 8080:8080 iec104-simulator
```

### Connect

Once running, connect your IEC 104 client to `localhost:2404`. The REST API is available at `http://localhost:8080/api/` and Swagger UI at `http://localhost:8080/swagger` (development mode).

## Configuration

### Datapoint Configuration

Datapoints are configured in `Configuration/SimulationOptions.json`. Each measure defines a Common Address (Ca), Object Address (Oa), and TypeId corresponding to an IEC 60870-5-104 data type.

```json
{
  "Iec104Simulation": {
    "CycleTimeMs": 5000,
    "DataPointConfiguration": {
      "Measures": [
        {
          "Id": "CA20_IOA25",
          "Name": "Tap Changer Position",
          "Ca": "20",
          "Oa": "25",
          "TypeId": 5,
          "Mode": "Periodic"
        }
      ],
      "Commands": [
        {
          "Id": "CA20_IOA2",
          "Name": "Tap Changer Command",
          "Ca": "20",
          "Oa": "2",
          "TypeId": 47,
          "ResponseId": "CA20_IOA25"
        }
      ]
    }
  }
}
```

### Simulation Modes

| Mode | Transmission | Description |
|------|-------------|-------------|
| `Static` | On startup / interrogation only | Fixed value, never sent on the periodic cycle |
| `Periodic` | Every cycle | Re-sends the current value with COT=PERIODIC |
| `RandomWalk` | Every cycle | Value steps randomly by up to `FluctuationRate`, clamped to `MinValue`/`MaxValue` |
| `GaussianNoise` | Every cycle | Gaussian noise around `BaseValue` ± `FluctuationRate`, bounded by `MinValue`/`MaxValue` |
| `PeriodicWave` | Every cycle | Positive half-sine wave with period `WavePeriodSeconds`, peak at `BaseValue` |
| `Profile` | Every cycle | Iterates through the inline `ProfileValues` array (loops continuously) |
| `EnergyCounter` | Every cycle | Accumulates energy from the linked data point (`LinkedDataPointId`) each cycle |
| `CounterOnDemand` | On interrogation only | Silently accumulates each cycle but only transmitted on GI/CI request |
| `CommandResponse` | On command receipt | Mirrors an incoming command ASDU as a measurement response |

### Initial Values

Each measure can optionally define an `InitValue` field that sets the value of the data point at startup. If omitted, a type-specific default is used.

| TypeId | Type | `InitValue` format | Example | Default |
|--------|------|--------------------|---------|---------|
| 1 | M_SP_NA_1 | `"true"` or `"false"` | `"true"` | `"false"` |
| 3 | M_DP_NA_1 | `"INTERMEDIATE"`, `"OFF"`, `"ON"`, or `"INDETERMINATE"` | `"INDETERMINATE"` | `"OFF"` |
| 5 | M_ST_NA_1 | Integer string in range -64 to 63 | `"5"` | `"0"` |
| 9 | M_ME_NA_1 | Float string (e.g. `"0.5"`) | `"0.5"` | `"0"` |
| 11 | M_ME_NB_1 | Integer string | `"100"` | `"0"` |
| 13 | M_ME_NC_1 | Float string (e.g. `"3.14"`) | `"3.14"` | `"0"` |

Example:

```json
{
  "Id": "CA20_IOA26",
  "Name": "Status Signal",
  "Ca": "20",
  "Oa": "26",
  "TypeId": 1,
  "InitValue": "true"
}
```

```json
{
  "Id": "CA20_IOA27",
  "Name": "Switchgear Status",
  "Ca": "20",
  "Oa": "27",
  "TypeId": 3,
  "InitValue": "INDETERMINATE"
}
```

### Profile Mode

For simulating realistic measurement curves (e.g. load over time), use the `Profile` mode with an inline `ProfileValues` array:

```json
{
  "Id": "CA20_IOA40",
  "Name": "Load Profile",
  "Ca": "20",
  "Oa": "40",
  "TypeId": 13,
  "Mode": "Profile",
  "ProfileValues": [0.0, 10.0, 25.0, 50.0, 75.0, 100.0, 75.0, 50.0, 25.0, 10.0]
}
```

Each cycle, the next value in the array is sent. When the end is reached, it loops back to the beginning. This mode is supported for measured value types (float, scaled, normalized, step position) but not for single/double points.

### Supported TypeIds

| TypeId | IEC Type | Description |
|--------|----------|-------------|
| 1 | M_SP_NA_1 | Single Point Information |
| 3 | M_DP_NA_1 | Double Point Information |
| 5 | M_ST_NA_1 | Step Position Information |
| 9 | M_ME_NA_1 | Normalized Measured Value |
| 11 | M_ME_NB_1 | Scaled Measured Value |
| 13 | M_ME_NC_1 | Short Floating Point Measured Value |
| 45 | C_SC_NA_1 | Single Command |
| 46 | C_DC_NA_1 | Double Command |
| 47 | C_RC_NA_1 | Regulating Step Command |
| 48 | C_SE_NA_1 | Set-point Normalized |
| 49 | C_SE_NB_1 | Set-point Scaled |

### Custom Configuration via Docker Volume Mount

To use your own configuration, mount your config files into the container:

```bash
docker run -p 2404:2404 -p 8080:8080 \
  -v ./my-config/SimulationOptions.json:/app/Configuration/SimulationOptions.json \
  ghcr.io/richyp7/iec60870-5-104-simulator:main
```

### REST API

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/DataPointConfigs` | List all configured datapoints |
| GET | `/api/DataPointConfigs/{Ca}/{Oa}` | Get a specific datapoint |
| POST | `/api/DataPointConfigs` | Create a new datapoint |
| PUT | `/api/DataPointConfigs/{Ca}/{Oa}` | Update datapoint simulation parameters |
| PUT | `/api/DataPointConfigs/{Ca}/{Oa}/simulation-mode` | Change simulation mode |
| DELETE | `/api/DataPointConfigs/{Ca}/{Oa}` | Delete a datapoint |
| GET | `/api/DataPointValues/{Ca}/{Oa}` | Get current value |
| POST | `/api/DataPointValues/{Ca}/{Oa}` | Send a value immediately |
| GET | `/api/Scenarios` | List all scenario states |
| POST | `/api/Scenarios/{name}/trigger` | Trigger a fault scenario (202 Accepted / 409 Conflict if already running) |
| GET | `/api/Status` | Simulator status (uptime, engine state, active IEC clients) |
| GET | `/health/ready` | Readiness check (server + connection) |
| GET | `/health/live` | Liveness check (server started) |

### SignalR Real-time Push

Connect to `ws://localhost:8080/hubs/simulation` for real-time updates:

| Event | Payload | Description |
|-------|---------|-------------|
| `FullSnapshot` | `DataPointUpdate[]` | Full datapoint snapshot pushed every cycle |
| `DataPointChanged` | `DataPointUpdate` | Single changed datapoint |
| `ScenarioUpdate` | `ScenarioStateDto` | Scenario status change (Idle/Running/Completed/Failed) |
| `ClientCountUpdate` | `int` | Number of connected IEC-104 clients changed |

## SCADA Demo

The built-in demo configuration (`SimulationOptions.json`) includes CA1 stations with realistic waveforms:

| ID | Name | IOA | Type | Mode | Description |
|----|------|-----|------|------|-------------|
| CA1_IOA101 | Transformer Breaker | 101 | M_DP_NA_1 | Static | Circuit breaker state (ON/OFF) |
| CA1_IOA102 | Active Power (W) | 102 | M_ME_NB_1 | GaussianNoise | Active power ~5000 W ± 150 |
| CA1_IOA103 | Fault Alarm | 103 | M_SP_NA_1 | Periodic | Fault alarm flag |
| CA1_IOA104 | Voltage (kV) | 104 | M_ME_NC_1 | GaussianNoise | Grid voltage ~110 kV ± 1.5 |
| CA1_IOA105 | Solar Power Output (W) | 105 | M_ME_NC_1 | PeriodicWave | Day-curve sine wave, peak 2500 W, period 24 h |
| CA1_IOA106 | Wind Power Output (W) | 106 | M_ME_NC_1 | RandomWalk | Random walk ~1200 W, bounded 0–3000 W |
| CA1_IOA107 | Energy Meter (Wh) | 107 | M_ME_NC_1 | EnergyCounter | Accumulates Wh from CA1_IOA102 each cycle |

### Running the Transformer-Trip Fault Scenario

The `ca1-transformer-trip` scenario simulates a fault followed by automatic recovery:

| Step | Delay | Action |
|------|-------|--------|
| 1 | 0 ms | Breaker → INTERMEDIATE state |
| 2 | 2 s | Active power → 0 W (load drop) |
| 3 | 3 s | Overload alarm → true |
| 4 | 3.5 s | Voltage → 0.0 kV (outage) |
| 5 | 5.5 s | Breaker → OFF (trip confirmed) |
| Recovery | +10 s | Restores power/alarm/voltage/breaker to pre-fault values |

**Trigger via REST:**
```bash
curl -X POST http://localhost:8080/api/Scenarios/ca1-transformer-trip/trigger
```

**Check status:**
```bash
curl http://localhost:8080/api/Scenarios
```

## Running Tests

```bash
docker build --target test .
```

Or with docker compose:

```bash
docker compose build
```

## Roadmap

| **Feature**                 | **Description**                                                                                   | **Status**       |
|-----------------------------|---------------------------------------------------------------------------------------------------|------------------|
| Basic Communication         | Establish communication channels between master and outstations.                                  | ✅ Implemented   |
| Command acknowledgement     | Acknowledge all command types                                                                     | ✅ Implemented   |
| Cyclic random Measurements  | Send cyclic random measurement for configured types                                               | ✅ Implemented   |
| Custom Simulation Profiles  | Iterate through predefined value profiles for measured values                                     | ✅ Implemented   |
| Configuration UI            | Web UI for configuration of simulator                                                             | ✅ Implemented   |
| Sim Measurements for Commands | Send Measurement types if a connected command gets sent                                         | ✅ Implemented   |
| ASDU Types                  | Support all ASDU types 1, 2, 30, 31                                                               | ⏳ In Progress   |
| Unit Testing                | Comprehensive unit tests to ensure reliability.                                                   | ⏳ In Progress   |
| Logging and Debugging       | Detailed logs for troubleshooting and debugging.                                                  | ⏳ In Progress   |
| Secure Authentication       | Implement secure authentication mechanisms.                                                       | Not started      |
| Performance Optimization    | Optimize performance for large-scale simulations.                                                 | Not started      |
| Real-time Monitoring        | Real-time visualization of communication exchanges.                                               | ✅ Implemented   |
| Fault Scenario Engine       | Timed multi-step fault sequences with auto-recovery via REST trigger.                             | ✅ Implemented   |
| Realism Simulation Modes    | GaussianNoise, PeriodicWave, RandomWalk, EnergyCounter, CounterOnDemand, Profile modes.            | ✅ Implemented   |

<a name="myfootnote1">1</a>: Supported by DALL-E 3
