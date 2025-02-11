export const environment = {
  production: true,
  API_ENDPOINT:
    typeof window !== "undefined" && (window as { [key: string]: any })["env"]?.API_ENDPOINT
      ? (window as { [key: string]: any })["env"].API_ENDPOINT
      : "http://localhost:8080/api/"
};
