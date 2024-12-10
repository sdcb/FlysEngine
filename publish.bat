cd %cd%
dotnet publish FlysTest/FlysTest.csproj -r win-x64 -c Release -p:publishAot=true  -p:_SuppressWinFormsTrimError=true -o publish/win-x64
cd publish/win-x64
del /a /f /s /q "*.pdb"
echo publish success!
pause