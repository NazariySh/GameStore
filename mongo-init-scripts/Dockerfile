FROM mongo:8.0

COPY . /docker-entrypoint-initdb.d

RUN apt-get update && apt-get install -y dos2unix \
    && find /docker-entrypoint-initdb.d -type f -name "*.sh" -exec dos2unix {} \; \
    && chmod +x /docker-entrypoint-initdb.d/*.sh \
    && apt-get clean && rm -rf /var/lib/apt/lists/*