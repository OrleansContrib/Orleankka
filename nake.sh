#!/bin/bash
dotnet nake -- -f $(pwd)/Nake.csx -d $(pwd) "$@"