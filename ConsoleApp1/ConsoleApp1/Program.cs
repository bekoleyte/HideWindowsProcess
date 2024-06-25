using System;
using System.Diagnostics;
using System.IO;
using EasyHook;

class Program
{
    static void Main()
    {
        try
        {
            // Task Manager'ı başlat
            Process[] processes = Process.GetProcessesByName("taskmgr");
            if (processes.Length == 0)
            {
                Console.WriteLine("Task Manager (taskmgr.exe) not found.");
                return;
            }
            Process taskmgr = processes[0];

            // Task Manager'ın çalıştığı dizini al
            string targetDir = Path.GetDirectoryName(taskmgr.MainModule.FileName);

            // EasyHook'un bulunduğu dizini ve DLL dosyalarını hedef dizine kopyala
            string easyHookDir = Path.Combine(targetDir, "EasyHook");
            Directory.CreateDirectory(easyHookDir); // EasyHook klasörünü oluştur
            File.Copy("EasyHook64.dll", Path.Combine(easyHookDir, "EasyHook64.dll"), true);

            // Inject edilecek DLL dosyasının yolunu oluştur
            string injectionLibrary = Path.Combine(targetDir, "ConsoleApp2.dll");

            // Task Manager sürecine inject et
            RemoteHooking.Inject(
                taskmgr.Id,
                InjectionOptions.DoNotRequireStrongName,
                injectionLibrary,
                injectionLibrary,
                ""); // Injection parametreleri buraya yazılabilir

            Console.WriteLine("Injected ConsoleApp2.dll into Task Manager (taskmgr.exe).");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

        Console.ReadLine();
    }
}
