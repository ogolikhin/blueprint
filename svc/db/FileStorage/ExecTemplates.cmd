@REM To easily run commands in Visual Studio, click Tools, External Tools and add a new item with command "C:\Windows\System32\cmd.exe", arguments "/C $(ItemPath)" and Initial directory "$(ItemDir)".
@REM Then, select the command to run, click Tools then click the new item you added.

FOR /R "." %%F IN (*.tt) DO "%COMMONPROGRAMFILES(x86)%\Microsoft Shared\TextTemplating\14.0\TextTransform.exe" %%F
