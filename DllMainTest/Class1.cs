using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public static class Program
{
    // To be safe and lazy, we wait the first 5 seconds, then send a message every 500 ms.
    static Stopwatch s = Stopwatch.StartNew();
    static int elapsedRequired = 5000;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)], EntryPoint = "HookMe")]
    public static int HookMe(nint lpMsg, nint hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg)
    {
        if(s.ElapsedMilliseconds > elapsedRequired)
        {
            CChat.AddEntry(EntryType.Chat, "Hi!", "Prefix", 0xFFFFFFFF, 0xFFFFFFFF);
            s.Restart();
            elapsedRequired = 500;
        }
        return PeekMessageA(lpMsg, hWnd, wMsgFilterMin, wMsgFilterMax, wRemoveMsg);
    }

    [DllImport("user32.dll")]
    private static extern int PeekMessageA(nint lpMsg, nint hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);
}

