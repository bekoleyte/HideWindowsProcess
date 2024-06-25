This program consists of 2 code segments. 
The first one injects into the taskmgr.exe process and ensures the initiation of the second DLL code. 
The second code, as an example, finds 'notepad.exe' and manipulates taskmgr.exe to hide the notepad.exe process. 
ConsoleApp1 is an exe file that performs the injection process. ConsoleApp2 is a DLL file that performs the hook process. 


Requirements:

1- Place the files ConsoleApp2.dll, ConsoleApp2.dll.config, and ConsoleApp2.pdb in the directory C:\Windows\system32 
(the reason for placing them in the system32 directory is that it is the directory of taskmgr.exe).


2- Run ConsoleApp1.exe as an administrator.


![Example](https://github.com/bekoleyte/HideWindowsProcess/blob/main/images/deneme.gif)
