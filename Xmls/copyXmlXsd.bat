cd ..\Xmls
cd

if not exist "..\Chlaot\bin\debug\net6.0-windows\Xmls" mkdir "..\Chlaot\bin\debug\net6.0-windows\Xmls"
copy .\Global\*.xml ..\Chlaot\bin\debug\net6.0-windows\Xmls
copy .\Copilot\*.xml ..\Chlaot\bin\debug\net6.0-windows\Xmls
copy .\Failures\*.xml ..\Chlaot\bin\debug\net6.0-windows\Xmls
copy .\Checklist\*.xml ..\Chlaot\bin\debug\net6.0-windows\Xmls
copy .\Affinity\*.xml ..\Chlaot\bin\debug\net6.0-windows\Xmls
copy .\RaaS\*.xml ..\Chlaot\bin\debug\net6.0-windows\Xmls

if not exist "..\Chlaot\bin\debug\net6.0-windows\Xmls\Xsds" mkdir "..\Chlaot\bin\debug\net6.0-windows\Xmls\Xsds"
copy .\Xsds\*.xsd ..\Chlaot\bin\debug\net6.0-windows\Xmls\Xsds

