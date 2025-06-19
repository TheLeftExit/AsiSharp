#include <windows.h>
#include <memory>

UINT InstallCallHook(UINT targetAddress, UINT stolenByteCount, UINT injectedFunctionPtr)
{
    UINT bufferPtr = (UINT)calloc(stolenByteCount, sizeof(char));
    memcpy(_Notnull_(void*)bufferPtr, (void*)targetAddress, stolenByteCount);
    DWORD oldProtect;
    VirtualProtect((void*)targetAddress, stolenByteCount, PAGE_READWRITE, &oldProtect);
    *(BYTE*)targetAddress = 0xE8; // JMP instruction
    *(UINT*)(targetAddress + 1) = injectedFunctionPtr - (targetAddress + 5);
    for (UINT i = 5; i < stolenByteCount; i++)
    {
        *(BYTE*)(targetAddress + i) = 0x90;
    }
    VirtualProtect(_Notnull_(void*)targetAddress, stolenByteCount, oldProtect, nullptr);

    return bufferPtr;
}

BOOL WINAPI DllMain(HINSTANCE hinstDLL, DWORD fdwReason, LPVOID lpvReserved)
{
    DisableThreadLibraryCalls(hinstDLL);

    UINT injectedFunctionPtr = (UINT)GetProcAddress(hinstDLL, "HookMe");
    UINT targetAddress = (UINT)GetModuleHandle(NULL) + 0x748A57 - 0x400000;
    InstallCallHook(injectedFunctionPtr, 6, targetAddress);

    return true;
}