@echo off
"C:\Program Files\Microsoft Visual Studio .NET\FrameworkSDK\Bin\ildasm.exe" /nobar /out:temp.il %1 >nul
perl callconvhack.pl <temp.il >hacked.il
"C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\ilasm.exe" /quiet /dll /out:%1 /res:temp.res hacked.il >nul
del temp.il temp.res hacked.il