using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Serilog;

namespace WatchTower
{
    public static class WinProcessExtensions
    {
        [DllImport("Kernel32.dll")]
        private static extern uint QueryFullProcessImageName(IntPtr hProcess, uint flags, StringBuilder text, out uint size);


        public static string GetMainModuleFileName(this Process process)
        {
            try
            {
                if (process == null)
                    return string.Empty;

                uint nChars = 256;
                StringBuilder buffer = new StringBuilder((int)nChars);
                uint success = QueryFullProcessImageName(process.Handle, 0, buffer, out nChars);

                return success != 0 ? buffer.ToString() : null;
            }
            catch (DllNotFoundException e)
            {
                Log.Error(e, e.Message);
                return string.Empty;
            }

            //int error = Marshal.GetLastWin32Error();
            // return $"Error = {error} when calling GetProcessImageFileName";
        }
    }
}
