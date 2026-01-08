using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.UI.Xaml;

namespace MyShop.Services.Plugins;

/// <summary>
/// Service for loading and managing plugins with proper AssemblyLoadContext.
/// Supports WinUI 3 XAML resource resolution for plugin UserControls.
/// </summary>
public class PluginLoader
{
    private readonly Dictionary<string, PluginLoadContext> _loadContexts = new();
    private readonly Dictionary<string, object> _loadedPlugins = new();

    /// <summary>
    /// Loads a plugin from the specified path using AssemblyLoadContext.
    /// </summary>
    /// <typeparam name="T">The interface type the plugin should implement</typeparam>
    /// <param name="pluginPath">Full path to the plugin DLL</param>
    /// <param name="pluginTypeName">Optional type name to load. If null, finds first type implementing T</param>
    /// <returns>Instance of the plugin, or null if loading failed</returns>
    public T? LoadPlugin<T>(string pluginPath, string? pluginTypeName = null) where T : class
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"?? PluginLoader: Loading plugin from {pluginPath}");

            if (!File.Exists(pluginPath))
            {
                System.Diagnostics.Debug.WriteLine($"? Plugin file not found: {pluginPath}");
                return null;
            }

            // Create a unique key for this plugin
            string pluginKey = Path.GetFileNameWithoutExtension(pluginPath);

            // Check if already loaded
            if (_loadedPlugins.TryGetValue(pluginKey, out var existingPlugin))
            {
                System.Diagnostics.Debug.WriteLine($"? Plugin already loaded: {pluginKey}");
                return existingPlugin as T;
            }

            // Create a new AssemblyLoadContext for this plugin
            var loadContext = new PluginLoadContext(pluginPath);
            _loadContexts[pluginKey] = loadContext;

            // Load the plugin assembly
            var assembly = loadContext.LoadFromAssemblyPath(pluginPath);
            System.Diagnostics.Debug.WriteLine($"? Assembly loaded: {assembly.FullName}");

            // Find the plugin type
            Type? pluginType = null;

            if (!string.IsNullOrEmpty(pluginTypeName))
            {
                // Load specific type by name
                pluginType = assembly.GetType(pluginTypeName);
            }
            else
            {
                // Find first type that implements the interface
                pluginType = assembly.GetTypes()
                    .FirstOrDefault(t =>
                        !t.IsInterface &&
                        !t.IsAbstract &&
                        typeof(T).IsAssignableFrom(t));
            }

            if (pluginType == null)
            {
                System.Diagnostics.Debug.WriteLine($"? No type implementing {typeof(T).Name} found in assembly");
                System.Diagnostics.Debug.WriteLine($"   Available types: {string.Join(", ", assembly.GetTypes().Select(t => t.FullName))}");
                return null;
            }

            System.Diagnostics.Debug.WriteLine($"? Found plugin type: {pluginType.FullName}");

            // Create instance of the plugin
            var pluginInstance = Activator.CreateInstance(pluginType) as T;

            if (pluginInstance == null)
            {
                System.Diagnostics.Debug.WriteLine($"? Failed to create instance or cast to {typeof(T).Name}");
                return null;
            }

            // Cache the plugin instance
            _loadedPlugins[pluginKey] = pluginInstance;

            System.Diagnostics.Debug.WriteLine($"? Plugin loaded successfully: {pluginKey}");
            return pluginInstance;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"? Error loading plugin: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"   Stack trace: {ex.StackTrace}");
            return null;
        }
    }

    /// <summary>
    /// Unloads a plugin and its AssemblyLoadContext.
    /// Note: Only works if the context was created with isCollectible: true
    /// </summary>
    /// <param name="pluginKey">The plugin key (filename without extension)</param>
    public void UnloadPlugin(string pluginKey)
    {
        if (_loadContexts.TryGetValue(pluginKey, out var context))
        {
            _loadedPlugins.Remove(pluginKey);
            _loadContexts.Remove(pluginKey);
            
            // Note: Unloading is only possible if AssemblyLoadContext was created with isCollectible: true
            // For WinUI plugins, we keep them loaded for the app lifetime
            context.Unload();
            
            System.Diagnostics.Debug.WriteLine($"??? Plugin unloaded: {pluginKey}");
        }
    }

    /// <summary>
    /// Gets a loaded plugin by key.
    /// </summary>
    public T? GetPlugin<T>(string pluginKey) where T : class
    {
        if (_loadedPlugins.TryGetValue(pluginKey, out var plugin))
        {
            return plugin as T;
        }
        return null;
    }

    /// <summary>
    /// Checks if a plugin is loaded.
    /// </summary>
    public bool IsPluginLoaded(string pluginKey)
    {
        return _loadedPlugins.ContainsKey(pluginKey);
    }
}
