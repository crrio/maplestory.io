FROM microsoft/dotnet:2.1-sdk AS build-env
WORKDIR /app

COPY PKG1/PKG1.csproj ./PKG1/
RUN dotnet restore PKG1

# Copy csproj and restore as distinct layers
COPY maplestory.io/maplestory.io.csproj ./maplestory.io/
RUN dotnet restore maplestory.io

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out maplestory.io
COPY SHA1.hash /app/maplestory.io/out/SHA1.hash

# Build runtime image
FROM microsoft/dotnet:2.1-aspnetcore-runtime
WORKDIR /app
COPY maplestory.io/run.sh .
RUN chmod +x /app/run.sh
COPY maplestory.io/gms.aes .
COPY maplestory.io/kms.aes .
COPY --from=build-env /app/maplestory.io/out .
ENTRYPOINT ["sh", "run.sh"]
