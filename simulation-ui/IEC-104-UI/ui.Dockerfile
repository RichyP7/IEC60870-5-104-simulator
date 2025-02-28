FROM node:lts AS frontend-build

WORKDIR /app

COPY package*.json ./
RUN npm install

COPY ./ ./
RUN npm run build --configuration=production

FROM nginx:alpine

# Copy the built Angular app to nginx's html directory
COPY --from=frontend-build /app/dist/iec-104-ui/browser /usr/share/nginx/html

# Create the entrypoint script for environment variables
RUN echo '#!/bin/sh' > /docker-entrypoint.sh \
    && echo 'echo "Generating env.js from env.template.js..."' >> /docker-entrypoint.sh \
    && echo 'envsubst < /usr/share/nginx/html/assets/env.template.js > /usr/share/nginx/html/assets/env.js' >> /docker-entrypoint.sh \
    && echo 'exec "$@"' >> /docker-entrypoint.sh

RUN dos2unix /docker-entrypoint.sh || true
RUN chmod +x /docker-entrypoint.sh

EXPOSE 80

# Run the entrypoint script before starting Nginx
ENTRYPOINT ["/docker-entrypoint.sh"]
CMD ["nginx", "-g", "daemon off;"]
