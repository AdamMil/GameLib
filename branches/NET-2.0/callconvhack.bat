@echo off
"C:\Program Files\Microsoft Visual Studio 8\SDK\v2.0\Bin\ildasm.exe" /source /nobar /out:temp.il "%1\bin\%2\%3.dll"
perl "%1\callconvhack.pl" temp.il >hacked.il
"%windir%\Microsoft.NET\Framework\v2.0.40607\ilasm.exe" /quiet /dll /debug /out:"%1\bin\%2\%3.dll" /res:temp.res hacked.il >nul
copy /Y "%1\bin\%2\%3.dll" "%1\obj\%2\%3.dll"
copy /Y "%1\bin\%2\%3.pdb" "%1\obj\%2\%3.pdb"
del temp.il temp.res