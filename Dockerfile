FROM microsoft/aspnetcore-build:2.0 AS build-env
WORKDIR /app

COPY PKG1/PKG1.csproj ./PKG1/
RUN dotnet restore PKG1

# Copy csproj and restore as distinct layers
COPY maplestory.io/maplestory.io.csproj ./maplestory.io/
RUN dotnet restore maplestory.io

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out maplestory.io

# Build runtime image
FROM microsoft/aspnetcore:2.0
WORKDIR /app
COPY maplestory.io/run.sh .
COPY --from=build-env /app/maplestory.io/out .
ENTRYPOINT ["sh", "run.sh"]
