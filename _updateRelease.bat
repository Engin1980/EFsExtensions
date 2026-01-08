deltree -Y _Release
mkdir _Release
mkdir .\_Release\EFsExtensions
copy readme.md .\_Release\EFsExtensions\_Readme.md
copy License .\_Release\EFsExtensions\_License.txt
xcopy /e /i /Y .\EFsExtensions\bin\Debug\net8.0-windows7.0\* .\_Release\EFsExtensions
cd .\_Release
tar.exe -c -f EFsExtensions.zip EFsExtensions



pause
