using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace TwitchDownloader.Utilities;

public static partial class NativeMethods
{
    [SupportedOSPlatform("windows")]
    public static partial class Windows
    {
        [LibraryImport("user32.dll", EntryPoint = "MessageBoxW", SetLastError = true,
            StringMarshalling = StringMarshalling.Utf16)]
        public static partial int MessageBox(nint hWnd, string text, string caption, uint type);
    }
}