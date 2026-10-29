# IEC104UI

Angular 20 single-page application (no SSR) using PrimeNG 20.

## Prerequisites

- Node.js 22 LTS
- npm

## Development server

```bash
npm ci
npm start
```

Navigate to `http://localhost:4200/`. The application will automatically reload if you change any of the source files. The UI expects the simulator backend API on `http://localhost:8080` (see `src/environments/`).

## Docker

The docker image "ui.dockerfile" can be used to start a ngnix container that hosts the UI.
Or use docker compose in root directory.

## Build

Run `ng build` to build the project. The build artifacts will be stored in the `dist/` directory.

## Further help

To get more help on the Angular CLI use `ng help` or go check out the [Angular CLI Overview and Command Reference](https://angular.dev/tools/cli) page.
