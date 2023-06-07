# IEC60870-5-104-simulator
Containerized simulator for testing the industry protocol IEC 60870-5-101/104

## Disclaimer
The project is still in development and not ready to use yet.


## How to run

### Tests

# inPROGRESS Tests are not executed yet
cd PROJ_FOLDER
docker build -f .\src\IEC60870-5-104-simulator.API\Dockerfile .\src --target test

### Simulator 
docker build -f .\src\IEC60870-5-104-simulator.API\Dockerfile .\src

Useful flags
--progress=plain --no-cache  to show command outputs