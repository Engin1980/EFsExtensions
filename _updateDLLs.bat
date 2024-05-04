set "local=%cd%"

copy ..\ESystem.NET\_Release\* .\DLLs\
del .\DLLs\ESystem.WPF.dll
del .\DLLs\ESystem.WPF.pdb

copy ..\ESimConnect\_Release\ESimConnect\ESimConnect.* .\DLLs

pause