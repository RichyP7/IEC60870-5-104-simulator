export const environment = {
  production: true,
  API_ENDPOINT:
    typeof window !== "undefined" && (window as { [key: string]: any })["env"]?.API_ENDPOINT
      ? (window as { [key: string]: any })["env"].API_ENDPOINT
      : "http://localhost:8080/api/",
  HEALTH_ENDPOINT:
    typeof window !== "undefined" && (window as { [key: string]: any })["env"]?.HEALTH_ENDPOINT
      ? (window as { [key: string]: any })["env"].HEALTH_ENDPOINT
      : "http://localhost:8080/health/"
};
