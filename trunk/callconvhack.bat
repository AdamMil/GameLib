@echo off
"C:\Program Files\Microsoft Visual Studio .NET 2003\SDK\v1.1\Bin\ildasm.exe" /source /nobar /out:temp.il %1\bin\%2\%3.dll >nul
perl %1\callconvhack.pl temp.il >hacked.il
"C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\ilasm.exe" /quiet /dll /debug /out:%1\bin\%2\%3.dll /res:temp.res hacked.il >nul
copy /Y %1\bin\%2\%3.dll %1\obj\%2\%3.dll
copy /Y %1\bin\%2\%3.pdb %1\obj\%2\%3.pdb
del temp.il temp.res