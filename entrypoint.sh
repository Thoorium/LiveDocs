#!/bin/sh
set -e

WWW_ROOT="/app/wwwroot"
THEME_FOLDER="_theme"

if [ -f "/app/.application_lock" ]; then
  echo "Setup has already be done. Skipping."
else
  if [ -d "$WWW_ROOT/$THEME_FOLDER" ]; then
    echo "Theme folder found."
    if ! [ -f "$WWW_ROOT/$THEME_FOLDER/index.html" ]; then
      echo "No themed index.html file found. Applying css and javascript changes automatically."
      for i in $(find $WWW_ROOT/$THEME_FOLDER -name *.css -type f -exec basename {} \;)
      do
          CSS_FULLPATH="${i}"
          if [ -f "${WWW_ROOT}/${THEME_FOLDER}/$CSS_FULLPATH" ]; then
            echo "Writing theme css '${i}' to '$WWW_ROOT/index.html'."
            sed -i "/THEME_CSS_END/ i \ \ \ \ <link rel=\"stylesheet\" href=\"${THEME_FOLDER}/${i}\" />" "$WWW_ROOT/index.html"
          else
            echo "::error::Could not find theme css file '${CSS_FULLPATH}'."
            exit 1
          fi
      done

      for i in $(find $WWW_ROOT/$THEME_FOLDER -name *.js -type f -exec basename {} \;)
      do
          JS_FULLPATH="${WWW_ROOT}/${THEME_FOLDER}/${i}"
          if [ -f "$JS_FULLPATH" ]; then
            echo "Writing theme javascript '${i}' to '$WWW_ROOT/index.html'."
            sed -i "/THEME_JS_END/ i \ \ \ \ <script src=\"${THEME_FOLDER}/${i}\"><\/script>" "$WWW_ROOT/index.html"
          else
            echo "::error::Could not find theme javascript file '${JS_FULLPATH}'."
            exit 1
          fi
      done
    else
      echo "Themed index.html file found. Overwriting the default index.html file."
      cp "${WWW_ROOT}/${THEME_FOLDER}/index.html" "${WWW_ROOT}/index.html"
    fi
    
  fi 

  if [ -n "$APPLICATION_BASE_URL" ]; then
    echo "Updating application path to '$APPLICATION_BASE_URL'."
    sed -i "s/base href=\"\/\"/base href=\"\/"${APPLICATION_BASE_URL}"\/\"/g" "${WWW_ROOT}/index.html"
  fi 

  touch "/app/.application_lock"
  echo "Setup done."
fi

dotnet LiveDocs.Server.dll