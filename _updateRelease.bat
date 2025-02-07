deltree -Y _Release
mkdir _Release
mkdir .\_Release\Chlaot
copy readme.md .\_Release\Chlaot\_Readme.md
copy License .\_Release\Chlaot\_License.txt
xcopy /e /i /Y Chlaot\bin\debug\net6.0-windows\* .\_Release\Chlaot
cd .\_Release
tar.exe -c -f Chlaot.zip Chlaot



pause
