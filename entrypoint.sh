#!/bin/sh
set -e

THEME_FOLDER="/app/wwwroot/_theme"

if [ -n "$THEME_FOLDER" ]; then
  if ! [ -n "$THEME_FOLDER/index.html" ]; then
    echo ""
  fi 
  echo ""
fi 

dotnet LiveDocs.Server.dll