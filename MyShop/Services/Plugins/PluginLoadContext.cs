using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace MyShop.Services.Plugins;

/// <summary>
/// Custom AssemblyLoadContext for loading plugins with proper resource resolution.
/// This allows WinUI 3 XAML resources to be found when loading external plugin DLLs.
/// </summary>
public class PluginLoadContext : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver _resolver;
    private readonly string _pluginPath;

    public PluginLoadContext(string pluginPath) : base(isCollectible: false)
    {
        _pluginPath = pluginPath;
        _resolver = new AssemblyDependencyResolver(pluginPath);
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        // Try to resolve the assembly using the dependency resolver
        string? assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
        if (assemblyPath != null)
        {
            return LoadFromAssemblyPath(assemblyPath);
        }

        // If not found, check if it's a shared assembly (like Microsoft.WindowsAppSDK)
        // Let the default context handle these
        return null;
    }

    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        string? libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
        if (libraryPath != null)
        {
            return LoadUnmanagedDllFromPath(libraryPath);
        }

        return IntPtr.Zero;
    }
}
