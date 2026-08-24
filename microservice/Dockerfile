FROM ubuntu:22.04

RUN dpkg --add-architecture i386 && \
    apt-get update && \
    apt-get install -y wine64 wine32 && \
    rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY /publish .
EXPOSE 5000
EXPOSE 5001
ENTRYPOINT ["wine", "WebApi.exe"]