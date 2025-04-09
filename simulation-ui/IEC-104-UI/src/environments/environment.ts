export const environment = {
  production: true,
  API_BASE_PATH: "http://localhost:8090",
  BASE_PATH: "http://localhost:8090",
  API_ENDPOINT:
    typeof window !== "undefined" && (window as { [key: string]: any })["env"]?.API_ENDPOINT
      ? (window as { [key: string]: any })["env"].API_ENDPOINT
      : "http://localhost:8090/api/",
  HEALTH_ENDPOINT:
    typeof window !== "undefined" && (window as { [key: string]: any })["env"]?.HEALTH_ENDPOINT
      ? (window as { [key: string]: any })["env"].HEALTH_ENDPOINT
      : "http://localhost:8090/health/"
};
