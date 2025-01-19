FROM node:lts AS frontend-build

WORKDIR /app

COPY package*.json ./

RUN npm install
COPY ./ .

RUN npm run build --prod

FROM nginx:alpine

# Copy the built Angular app to nginx's html directory
COPY --from=frontend-build /app/dist/iec-104-ui/browser /usr/share/nginx/html

EXPOSE 80

# Start nginx
CMD ["nginx", "-g", "daemon off;"]
