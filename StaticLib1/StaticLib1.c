// C only, since C++ requires CRT which conflicts with whatever ILC imports
#include <windows.h>
#include <stdlib.h>

UINT InstallCallHook(UINT targetAddress, UINT stolenByteCount, UINT injectedFunctionPtr)
{
    UINT bufferPtr = (UINT)calloc(stolenByteCount, sizeof(char));
    memcpy((void*)bufferPtr, (void*)targetAddress, stolenByteCount);

    DWORD oldProtect;
    VirtualProtect((void*)targetAddress, stolenByteCount, PAGE_READWRITE, &oldProtect);
    *(BYTE*)targetAddress = 0xE8;
    *(UINT*)(targetAddress + 1) = injectedFunctionPtr - (targetAddress + 5);
    for (UINT i = 5; i < stolenByteCount; i++)
    {
        *(BYTE*)(targetAddress + i) = 0x90;
    }
    VirtualProtect((void*)targetAddress, stolenByteCount, oldProtect, NULL);

    return bufferPtr;
}

BOOL WINAPI DllMain(HINSTANCE hinstDLL, DWORD fdwReason, LPVOID lpvReserved)
{
    DisableThreadLibraryCalls(hinstDLL);
    if (fdwReason != DLL_PROCESS_ATTACH) return TRUE;

    UINT injectedFunctionPtr = (UINT)GetProcAddress(hinstDLL, "HookMe");
    UINT targetAddress = 0x748A57; // 6-byte call to PeekMessageA in the main loop in gta_sa!_WinMain
    InstallCallHook(targetAddress, 6, injectedFunctionPtr);

    return TRUE;
}

