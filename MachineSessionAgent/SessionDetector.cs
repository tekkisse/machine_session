using System.Runtime.InteropServices;

public sealed class SessionDetector
{
    public IReadOnlyList<string> GetLoggedInUsers()
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        IntPtr server = IntPtr.Zero;
        IntPtr sessionInfoPtr = IntPtr.Zero;

        try
        {
            if (!WTSEnumerateSessions(server, 0, 1, out sessionInfoPtr, out int count))
                return result.ToList();

            int size = Marshal.SizeOf<WTS_SESSION_INFO>();

            for (int i = 0; i < count; i++)
            {
                var info = Marshal.PtrToStructure<WTS_SESSION_INFO>(
                    sessionInfoPtr + i * size);

                if (info.State != WTS_CONNECTSTATE_CLASS.WTSActive)
                    continue;

                string user = QueryString(info.SessionID, WTS_INFO_CLASS.WTSUserName);
                if (!string.IsNullOrWhiteSpace(user))
                    result.Add(user);
            }
        }
        finally
        {
            if (sessionInfoPtr != IntPtr.Zero)
                WTSFreeMemory(sessionInfoPtr);
        }

        return result.ToList();
    }

    private static string QueryString(int sessionId, WTS_INFO_CLASS infoClass)
    {
        IntPtr buffer;
        int bytes;

        if (!WTSQuerySessionInformation(
            IntPtr.Zero, sessionId, infoClass, out buffer, out bytes))
            return string.Empty;

        string value = Marshal.PtrToStringAnsi(buffer) ?? "";
        WTSFreeMemory(buffer);
        return value;
    }

    #region Native

    [DllImport("wtsapi32.dll")]
    static extern bool WTSEnumerateSessions(
        IntPtr hServer, int Reserved, int Version,
        out IntPtr ppSessionInfo, out int pCount);

    [DllImport("wtsapi32.dll")]
    static extern bool WTSQuerySessionInformation(
        IntPtr hServer, int sessionId, WTS_INFO_CLASS infoClass,
        out IntPtr ppBuffer, out int pBytesReturned);

    [DllImport("wtsapi32.dll")]
    static extern void WTSFreeMemory(IntPtr pointer);

    struct WTS_SESSION_INFO
    {
        public int SessionID;
        public string pWinStationName;
        public WTS_CONNECTSTATE_CLASS State;
    }

    enum WTS_CONNECTSTATE_CLASS
    {
        WTSActive = 0,
        WTSDisconnected = 4
    }

    enum WTS_INFO_CLASS
    {
        WTSUserName = 5
    }

    #endregion
}
