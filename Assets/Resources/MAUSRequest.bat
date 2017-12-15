@echo off
set transcript=%1
set wav=%2
set language=%3

@echo off
curl -v -X POST -H "content-type: multipart/form-data" -F LANGUAGE^=%language% -F TEXT^=@%transcript% -F SIGNAL^=@%wav% https://clarin.phonetik.uni-muenchen.de/BASWebServices/services/runMAUSBasic 2>&1 | findstr download >> response.xml



