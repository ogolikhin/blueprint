@echo off
SETLOCAL
SET TEXT_TRANSFORM="%COMMONPROGRAMFILES(x86)%\Microsoft Shared\TextTemplating\14.0\TextTransform.exe" 
@echo on

%TEXT_TRANSFORM% AdminStorage_Instance.tt

%TEXT_TRANSFORM% Migration\7.0.1.0\7.0.1.0.tt
%TEXT_TRANSFORM% AdminStorage_Migration.tt

Pause
Exit