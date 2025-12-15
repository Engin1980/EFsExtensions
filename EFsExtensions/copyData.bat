echo Copying data folder
cd
if not exist ".\bin\debug\net8.0-windows10.0.19041.0\Data" mkdir ".\bin\debug\net8.0-windows10.0.19041.0\Data"
copy ..\Data .\bin\debug\net8.0-windows10.0.19041.0\Data
