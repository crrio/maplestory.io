#!/bin/sh

export DOTNET_PID=$(cd /app && dotnet maplestory.io.dll &)
