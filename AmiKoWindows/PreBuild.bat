REM Make sure this file is encoded as UTF-8 (no BOM)

ECHO "One is %1"
ECHO "Two is %2"

ECHO "List dir1:"
DIR "%1\AmiKoWindows\Properties\"

SET resxfile="%1\AmiKoWindows\Properties\Resources.resx"
IF EXIST %resxfile% (
  ECHO --- Deleting resource file ---
  DEL %resxfile%
)

ECHO "List dir2:"
DIR "%1\AmiKoWindows\Properties\"

ECHO --- Copying resource file ---

:: copy {de|fr}.resx to resx
(COPY /Y "%1\AmiKoWindows\Properties\%2" "%1\AmiKoWindows\Properties\Resources.resx")

ECHO "List dir3:"
DIR "%1\AmiKoWindows\Properties\"
