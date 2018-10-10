using System;
using System.Runtime.InteropServices;
using InElonWeTrust.Core.Services.Watchdog.Platforms;

namespace InElonWeTrust.Core.Services.Watchdog
{
    public class WatchdogFactory
    {
        public BaseWatchdog Create()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return new WindowsWatchdog();
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return new LinuxWatchdog();
            }

            throw new PlatformNotSupportedException();
        }
    }
}
