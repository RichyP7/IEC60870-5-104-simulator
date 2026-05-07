(function(window) {
  window.env = window.env || {};

  // Environment variables
  window["env"].API_ENDPOINT = 'http://localhost:8080/api/';
  window["env"].HEALTH_ENDPOINT = 'http://localhost:8080/health/';
  window["env"].HUB_ENDPOINT = 'http://localhost:8080/hubs/simulation';
})(this);
