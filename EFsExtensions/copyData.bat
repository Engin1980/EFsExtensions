echo Copying data folder
cd
if not exist ".\bin\debug\net6.0-windows\Data" mkdir ".\bin\debug\net6.0-windows\Data"
copy ..\Data .\bin\debug\net6.0-windows\Data
