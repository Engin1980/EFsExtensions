cd ..\Xmls
cd

set "OUT_DIR=..\EFsExtensions\bin\debug\net8.0-windows10.0.19041.0\Xmls"
set "OUT_DIR_XSD=%OUTDIR%\Xsds"

if not exist %OUT_DIR% mkdir %OUT_DIR%
copy .\Global\*.xml %OUT_DIR%
copy .\Copilot\*.xml %OUT_DIR%
copy .\Failures\*.xml %OUT_DIR%
copy .\Checklist\*.xml %OUT_DIR%
copy .\Affinity\*.xml %OUT_DIR%
copy .\RaaS\*.xml %OUT_DIR%

if not exist %OUT_DIR%\Xsds mkdir %OUT_DIR%\Xsds
copy .\Xsds\*.xsd %OUT_DIR%\Xsds
