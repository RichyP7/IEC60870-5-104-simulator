# IEC60870-5-104-simulator
Containerized simulator for testing the industry protocol IEC 60870-5-101/104. The simulator acts currently only as IEC104 Server Application which opens a port on the container and enables a TCP/IEC104 Client to connect to it.

## Disclaimer
The project is still in development and not ready to use yet.

## Quickstart
Install docker to run the iec104 simulation

If you are using docker set DOCKER_BUILDKIT=1 for skipping unused build targets and use the testpart in the dockerfile optionally.

### Simulator 

```
docker build .
```

Useful flags:
```
--progress=plain --no-cache  to show command outputs
```
### Tests

```
cd PROJ_FOLDER
docker build --target test . 


```
