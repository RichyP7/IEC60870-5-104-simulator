/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./src/**/*.{html,ts}", // Ensure Tailwind scans your Angular components
    "./node_modules/tailwindcss-primeui/**/*.js" // Include PrimeNG Tailwind components
  ],
  theme: {
    extend: {},
  },
  plugins: [require('tailwindcss-primeui')],
}

