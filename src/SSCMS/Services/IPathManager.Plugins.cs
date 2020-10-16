﻿namespace SSCMS.Services
{
    public partial interface IPathManager
    {
        string GetPackagesPath(params string[] paths);

        string PluginsPath { get; }

        string GetPluginPath(string pluginId, params string[] paths);

        string GetPluginNuspecPath(string pluginId);

        string GetPluginDllDirectoryPath(string pluginId);
    }
}
