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
    "CycleTimeMs": 10000,
    "DataPointConfiguration": {
      "Measures": [
        {
          "Id": "Point1",
          "Ca": "20",
          "Oa": "25",
          "TypeId": 13,
          "Mode": "Cyclic"
        }
      ],
      "Commands": [
        {
          "Id": "Command1",
          "Ca": "100",
          "Oa": "25",
          "TypeId": 45,
          "ResponseId": "Point1"
        }
      ]
    }
  }
}
```

### Simulation Modes

| Mode | Description |
|------|-------------|
| `None` | Static value, no cyclic transmission |
| `Cyclic` | Generates a new random value every cycle |
| `CyclicStatic` | Re-sends the current value every cycle |
| `PredefinedProfile` | Iterates through a predefined list of values from a profile (loops continuously) |

### Predefined Profiles

For simulating realistic measurement curves (e.g. current, power over time), use the `PredefinedProfile` mode. Profiles are defined in `Configuration/Profiles.json`:

```json
{
  "Profiles": {
    "TestProfile": [0.0, 10.0, 25.0, 50.0, 75.0, 100.0, 75.0, 50.0, 25.0, 10.0]
  }
}
```

Reference the profile by name in your datapoint configuration:

```json
{
  "Id": "PROFILE1",
  "Ca": "20",
  "Oa": "40",
  "TypeId": 13,
  "Mode": "PredefinedProfile",
  "ProfileName": "TestProfile"
}
```

Each cycle, the next value in the profile is sent. When the end is reached, it loops back to the beginning. This mode is supported for measured value types (float, scaled, normalized, step position) but not for single/double points.

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
  -v ./my-config/Profiles.json:/app/Configuration/Profiles.json \
  ghcr.io/richyp7/iec60870-5-104-simulator:main
```

### REST API

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/DataPointConfigs` | List all configured datapoints |
| GET | `/api/DataPointConfigs/{Ca}/{Oa}` | Get a specific datapoint |
| POST | `/api/DataPointConfigs` | Create a new datapoint |
| PUT | `/api/DataPointConfigs/{Ca}/{Oa}/simulation-mode` | Change simulation mode |
| DELETE | `/api/DataPointConfigs/{Ca}/{Oa}` | Delete a datapoint |
| GET | `/api/DataPointValues/{Ca}/{Oa}` | Get current value |
| POST | `/api/DataPointValues/{Ca}/{Oa}` | Send a value immediately |
| GET | `/health/ready` | Readiness check (server + connection) |
| GET | `/health/live` | Liveness check (server started) |

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
| Sim Measurements for Commands | Send Measurement types if a connected command gets sent                                         | ⏳ In Progress   |
| ASDU Types                  | Support all ASDU types 1, 2, 30, 31                                                               | ⏳ In Progress   |
| Unit Testing                | Comprehensive unit tests to ensure reliability.                                                   | ⏳ In Progress   |
| Logging and Debugging       | Detailed logs for troubleshooting and debugging.                                                  | ⏳ In Progress   |
| Secure Authentication       | Implement secure authentication mechanisms.                                                       | Not started      |
| Performance Optimization    | Optimize performance for large-scale simulations.                                                 | Not started      |
| Real-time Monitoring        | Real-time visualization of communication exchanges.                                               | Not started      |

<a name="myfootnote1">1</a>: Supported by DALL-E 3
