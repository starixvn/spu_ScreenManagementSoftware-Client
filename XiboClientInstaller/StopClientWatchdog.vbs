SET WshShell = CreateObject("WScript.Shell")
 
SET oExec = WshShell.Exec("taskkill /F /IM SMSClientWatchdog.exe")
 
SET oExec = Nothing
 
SET WshShell = Nothing