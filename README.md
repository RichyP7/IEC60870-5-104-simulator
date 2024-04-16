# IEC60870-5-104-simulator

<img src="https://github.com/RichyP7/IEC60870-5-104-simulator/assets/14166202/f8c22afe-d4a0-4a1e-8655-43f6c3116cfe)" width=25% height=25%> <sup>[1](#myfootnote1)</sup>

## Overview

This repository contains an IEC 60870-104 protocol simulator written in .NET. The simulator allows you to emulate communication between an IEC 60870-104 master and outstations (slaves). Whether you're testing your SCADA system or developing a new application, this simulator provides a convenient way to validate your implementation.
Containerized simulator for testing the protocol in cloud native scenarios. The simulator currently is capable of being IEC104 Server Application which opens a port on the container and enables a TCP/IEC104 Client to connect to it.
Whether you’re testing your SCADA system or developing a new application, this simulator provides a convenient way to validate your implementation.

The application is a just as good as it´s core library, which is using the awesome work of [mz-automation](https://github.com/mz-automation)

### What is IEC 60870
Wikipedia:
> In electrical engineering and power system automation, the International Electrotechnical Commission 60870 standards define systems used for telecontrol (supervisory control and data acquisition). Such systems are used for controlling electric power transmission grids and other geographically widespread control systems. By use of standardized protocols, equipment from many different suppliers can be made to interoperate. IEC standard 60870 has six parts, defining general information related to the standard, operating conditions, electrical interfaces, performance requirements, and data transmission protocols. The 60870 standards are developed by IEC Technical Committee 57 (Working Group 03).

## Roadmap
The project is still in early development and there are a lot of feature missing. However, you can already do first test with it


| **Feature**                 | **Description**                                                                                   | **Status**       |
|-----------------------------|---------------------------------------------------------------------------------------------------|------------------|
| Basic Communication         | Establish communication channels between master and outstations.                                  | ✅ Implemented   |
| Command acknowledgkent      | Acknowledge all command types                                                                     | ✅ Implemented   |
| Cyclic random Measurments   | Send cyclic random measurement for configured types                                               | ✅ Implemented  |
| Sim Measurements for Commands | Send Measurement types if a connected command gets send                                         | ⏳ In Progress   |
| ASDU Types                  | Support all ASDU types 1, 2, 30, 31                                                     | ⏳ In Progress   |
| Unit Testing                | Comprehensive unit tests to ensure reliability.                                                  | ⏳ In Progress   |
| Logging and Debugging       | Detailed logs for troubleshooting and debugging.                                                 | ⏳ In Progress   |
| Custom Simulation profiles  | Allow users to add custom simulation profiles                                                         | Not started |
| Secure Authentication       | Implement secure authentication mechanisms.                                                       | Not started  |
| Performance Optimization    | Optimize performance for large-scale simulations.                                                | Not started  |
| Configuration UI            | Add UI for configuration of simulator          .                                                | Not started  |
| Real-time Monitoring        | Real-time visualization of communication exchanges.                                               |  Not started  |
| Error Handling              | Robust error handling for unexpected scenarios.                                                  |  Not started   |
| Documentation               | Detailed documentation for setup, usage, and customization.                                       |  Not started   |
| Community Contributions     | Welcome contributions from the community.                                                         | Not started  |

## Quickstart
Install docker to run the iec104 simulation

If you are using docker set DOCKER_BUILDKIT=1 for skipping unused build targets and use the testpart in the dockerfile optionally.

### Simulator 

```
docker pull ghcr.io/richyp7/iec60870-5-104-simulator:main
docker run ghcr.io/richyp7/iec60870-5-104-simulator:main
```
Currently it is just acknowledging commands. See [Roadmap](##Roadmap)

Useful flags:
```
--progress=plain --no-cache  to show command outputs
```

#### Initial Configuration

Supply configuration for Iec104 DataPoints via IConfiguration (e.g environment variables)
```
  "Iec104Simulation": {
    "CycleTimeMs": 10000,
    "DataPointConfiguration": {

      "Measures": [
        {
          "Id": "Point1",
          "Ca": "20",
          "Oa": "25",
          "TypeId": 5,
          "Mode": "Cyclic"
        },
```

### Tests

```
cd PROJ_FOLDER
docker build --target test . 
```



<a name="myfootnote1">1</a>: Supported by DALL·E 3

