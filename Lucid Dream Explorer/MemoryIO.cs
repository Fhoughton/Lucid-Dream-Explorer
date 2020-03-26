using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices; //DllImport

namespace Lucid_Dream_Explorer
{
    class MemoryIO
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int dwProcessId);

        [DllImport("Kernel32.dll")]
        static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, UInt32 nSize, ref UInt32 lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        public static extern Int32 CloseHandle(IntPtr hProcess);

        [DllImport("kernel32.dll")]
        static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);

        [DllImport("kernel32.dll")]
        static extern uint SuspendThread(IntPtr hThread);

        [DllImport("kernel32.dll")]
        static extern int ResumeThread(IntPtr hThread);

        [Flags]
        public enum ProcessAccessFlags : uint
        {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VMOperation = 0x00000008,
            VMRead = 0x00000010,
            VMWrite = 0x00000020,
            DupHandle = 0x00000040,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            Synchronize = 0x00100000
        }

        [Flags]
        private enum ThreadAccess : int
        {
            TERMINATE = (0x0001),
            SUSPEND_RESUME = (0x0002),
            GET_CONTEXT = (0x0008),
            SET_CONTEXT = (0x0010),
            SET_INFORMATION = (0x0020),
            QUERY_INFORMATION = (0x0040),
            SET_THREAD_TOKEN = (0x0080),
            IMPERSONATE = (0x0100),
            DIRECT_IMPERSONATION = (0x0200)
        }

        private IntPtr? hProcess = null;
        private IntPtr? dllBaseAdress = null;

        private static Dictionary<string, IntPtr?> StoredProcesses;
        private static Dictionary<string, int> StoredProcessIDs;
        private static Dictionary<string, IntPtr?> StoredModules;

        /**
         * Finds and connects to a process with the given name
         **/
        public MemoryIO(string ProcessName, bool relative = false, string ModuleName = "")
        {
            int ProcessID;
            if (StoredProcesses == null || StoredProcessIDs == null)
            {
                StoredProcesses = new Dictionary<string, IntPtr?>();
                StoredProcessIDs = new Dictionary<string, int>();
            }
            if (StoredProcesses.ContainsKey(ProcessName) && StoredProcessIDs.ContainsKey(ProcessName))
            {
                ProcessID = StoredProcessIDs[ProcessName];
                OpenProcess(ProcessAccessFlags.All, false, ProcessID);
                dllBaseAdress = StoredProcesses[ProcessName]; return;
            }
            Process[] processes = Process.GetProcessesByName(ProcessName);
            if (processes.Length <= 0) // No such process
            {
                hProcess = null;
                return;
            }
            Process process = processes[0]; //First matching name
            ProcessID = process.Id;
            hProcess = OpenProcess(ProcessAccessFlags.All, false, ProcessID); //Full access
            if (!relative && ModuleName == "") //Absolute address
            {
                dllBaseAdress = IntPtr.Zero; //No offset for a DLL file
            }
            else //Relative address
            {
                if (ModuleName == "") ModuleName = ProcessName + ".exe";
                try
                {
                    if (StoredModules == null)
                    {
                        StoredModules = new Dictionary<string, IntPtr?>();
                    }
                    if (StoredModules.ContainsKey(ProcessName))
                    {
                        dllBaseAdress = StoredModules[ProcessName]; return;
                    }
                    foreach (ProcessModule module in process.Modules)
                    {
                        if (module.ModuleName.ToLower() == ModuleName.ToLower())
                        {
                            dllBaseAdress = module.BaseAddress;
                            break;
                        }
                    }
                    StoredModules.Add(ProcessName, dllBaseAdress);
                    StoredProcesses.Add(ProcessName, dllBaseAdress);
                }
                catch (Exception)
                {
                    hProcess = null;
                    return;
                }
            }
        }

        ~MemoryIO()
        {
            if (hProcess != null)
                CloseHandle((IntPtr)hProcess);
        }

        public bool processOK()
        {
            return hProcess != null && dllBaseAdress != null;
        }

        public String MemoryRead(IntPtr adress, byte[] data)
        {
            uint bytesRead = 0;

            //Resolve virtual adress
            /*if ((int)adress < (int)hProcess)
            {
                adress += (int)hProcess;
            }*/
            if (hProcess == null) return "No process" + Environment.NewLine;
            if (dllBaseAdress == null) dllBaseAdress = (IntPtr)0;
            int tmp = (int)dllBaseAdress + (int)adress; //TODO
            adress = (IntPtr)tmp;
            ReadProcessMemory((IntPtr)hProcess, adress, data, (UInt32)data.Length, ref bytesRead);
            if (bytesRead == 0)
            {
                return "READ\t0x" + adress.ToString("X2") + " ERROR" + Environment.NewLine;
            }

            String hex = "";
            foreach (byte b in data)
            {
                hex += String.Format("{0:X2} ", b);
            }
            return "READ\t0x" + adress.ToString("X2") + ": 0x" + hex + Environment.NewLine;
        }

        //TODO stored dictionary with restore for Un-buttons
        //TODO verification with memoryread prior to write
        public String MemoryWrite(IntPtr adress, byte[] data)
        {
            int bytesWritten = 0;
            if (hProcess == null) return "No process" + Environment.NewLine;
            if (dllBaseAdress == null) dllBaseAdress = (IntPtr)0;
            int tmp = (int)dllBaseAdress + (int)adress; //TODO
            adress = (IntPtr)tmp;
            WriteProcessMemory((IntPtr)hProcess, adress, data, (UInt32)data.Length, out bytesWritten);
            if (bytesWritten == 0)
            {
                return "WRITE\t0x" + adress.ToString("X2") + " ERROR" + Environment.NewLine;
            }

            String hex = "";
            foreach (byte b in data)
            {
                hex += String.Format("{0:X2} ", b);
            }
            return "WRITE\t0x" + adress.ToString("X2") + ": 0x" + hex + Environment.NewLine;
        }
    }
}
