using System.Runtime.InteropServices;

using unsafe AddEntryDelegate = delegate* unmanaged[Thiscall]<CChat*, int, byte*, byte*, uint, uint, void>;

[StructLayout(LayoutKind.Explicit, Size = 25622, Pack = 1)]
public unsafe partial struct CChat
{
    public static void AddEntry(EntryType type, string text, string prefix, uint textColor, uint prefixColor)
    {
        try
        {
            var _addEntry = (AddEntryDelegate)GetFunctionPtr("samp.dll", 0x67BE0);
            if ((uint)_addEntry == 0x67BE0) return;
            CChat* _instance = *(CChat**)GetFunctionPtr("samp.dll", 0x26EB80);
            using var textAnsi = AnsiString.Encode(text);
            using var prefixAnsi = AnsiString.Encode(prefix);
            _addEntry(_instance, (int)type, textAnsi, prefixAnsi, textColor, prefixColor);
        }
        catch { }
    }

    private static uint GetFunctionPtr(string moduleName, uint offset)
    {
        return GetModuleHandle(moduleName) + offset;
    }

    [LibraryImport("kernel32.dll", EntryPoint = "GetModuleHandleW")]
    internal static partial uint GetModuleHandle([MarshalAs(UnmanagedType.LPWStr)] string? lpModuleName);
}

public enum EntryType : int
{
    None = 0,
    Chat = 2,
    Info = 4,
    Debug = 8
}

internal readonly unsafe ref struct AnsiString
{
    public readonly byte* Pointer;
    public AnsiString(string? s)
    {
        Pointer = (byte*)Marshal.StringToHGlobalAnsi(s);
    }
    public void Dispose()
    {
        if (Pointer != null)
        {
            Marshal.FreeHGlobal((nint)Pointer);
        }
    }
    public static implicit operator byte*(AnsiString ansiString) => ansiString.Pointer;

    public static AnsiString Encode(string? s) => new(s);
    public static string? Decode(byte* pointer) => Marshal.PtrToStringAnsi((nint)pointer);
}