cd ..\Xmls
cd

if not exist "..\EFsExtensions\bin\debug\net6.0-windows\Xmls" mkdir "..\EFsExtensions\bin\debug\net6.0-windows\Xmls"
copy .\Global\*.xml ..\EFsExtensions\bin\debug\net6.0-windows\Xmls
copy .\Copilot\*.xml ..\EFsExtensions\bin\debug\net6.0-windows\Xmls
copy .\Failures\*.xml ..\EFsExtensions\bin\debug\net6.0-windows\Xmls
copy .\Checklist\*.xml ..\EFsExtensions\bin\debug\net6.0-windows\Xmls
copy .\Affinity\*.xml ..\EFsExtensions\bin\debug\net6.0-windows\Xmls
copy .\RaaS\*.xml ..\EFsExtensions\bin\debug\net6.0-windows\Xmls

if not exist "..\EFsExtensions\bin\debug\net6.0-windows\Xmls\Xsds" mkdir "..\EFsExtensions\bin\debug\net6.0-windows\Xmls\Xsds"
copy .\Xsds\*.xsd ..\EFsExtensions\bin\debug\net6.0-windows\Xmls\Xsds
