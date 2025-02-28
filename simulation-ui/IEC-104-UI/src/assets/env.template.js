(function(window) {
  window.env = window.env || {};

  // Environment variables
  window["env"].API_ENDPOINT = '${ENV_API_ENDPOINT}';
  window["env"].HEALTH_ENDPOINT = '${ENV_API_HEALTH_ENDPOINT}';

})(this);
