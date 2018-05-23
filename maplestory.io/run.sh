#!/bin/sh

dotnet maplestory.io.dll >> /var/log/"$(echo $HOSTNAME).log"
