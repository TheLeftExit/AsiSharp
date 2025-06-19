# DllMainTest

How to add a DllMain to your NativeAOT C# project:
- Create a static library in C and define your `DllMain` there.
	- Export a method in your C# module and resolve it via `GetProcAddress(hInstance, ...)` in your `DllMain`.
	- You can't invoke your imported method directly - runtime initialization under loader lock, boo-hoo. You'll have to install a hook or start a thread to call your imported method.
	- You can't use C++ - this forces your library to include runtime imports that may conflict with NativeAOT's imports, leading to 40-ish incomprehensible linker errors. Use the "Compile As" option in project settings to target C.
- In your `.csproj`, inject a target between [SetupOSSpecificProps](https://github.com/dotnet/runtime/blob/main/src/coreclr/nativeaot/BuildIntegration/Microsoft.NETCore.Native.Windows.targets) (which builds `LinkerArg`) and [LinkNative](https://github.com/dotnet/runtime/blob/main/src/coreclr/nativeaot/BuildIntegration/Microsoft.NETCore.Native.targets) (which consumes them).
	- Replace a reference to [dllmain.obj](https://github.com/dotnet/runtime/blob/main/src/coreclr/nativeaot/Bootstrap/dllmain/dllmain.cpp) with a reference to your C static library.
	- Force-include the exported `_DllMain@12` symbol.
- For convenience, use a `.props` file to share a variable path between the two projects so you don't have to copy the C library every time you build it.

This example creates an ASI mod for SAMP that just adds a chat message every 500 ms.