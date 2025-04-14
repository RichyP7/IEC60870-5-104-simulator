export const environment = {
  production: false,
  development: true,

  API_BASE_PATH: "http://localhost:8080",
  BASE_PATH: "http://localhost:8090",
  environmentName:"DEV",
  API_ENDPOINT:
    typeof window !== "undefined" && (window as { [key: string]: any })["env"]?.API_ENDPOINT
      ? (window as { [key: string]: any })["env"].API_ENDPOINT
      : "http://localhost:8080/api/",
  HEALTH_ENDPOINT:
    typeof window !== "undefined" && (window as { [key: string]: any })["env"]?.HEALTH_ENDPOINT
      ? (window as { [key: string]: any })["env"].HEALTH_ENDPOINT
      : "http://localhost:8080/health/"
};
