#!/bin/bash
# dev.sh - Wrapper script that ensures dotnet process is killed when Tauri exits
# Fixes: https://github.com/tauri-apps/tauri/issues/2794

cleanup() {
    pkill -P $$ 2>/dev/null
}

trap cleanup EXIT SIGINT SIGTERM

dotnet run --project src/Sundy.csproj
