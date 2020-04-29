using System;
using System.Collections.Generic;
using Alphaleonis.Win32.Filesystem;
using System.Xml;
using Splat;

namespace BannerLord.Common.Xml
{
    public class Module
    {
        public string DirectoryName { get; set; }
        public string Name { get; set; }
        public string Id { get; set; }
        public ModuleVersion Version { get; set; }
        public bool Official { get; set; }
        public bool DefaultModule { get; set; }
        public bool SingleplayerModule { get; set; }
        public bool MultiplayerModule { get; set; }
        public List<string> DependedModules { get; set; }
        public List<SubModule> SubModules { get; set; }
        public List<SubModule> DelayedSubModules { get; set; }
        public List<string> OptionalDependModules { get; set; }

        public Module()
        {
            this.DependedModules = new List<string>();
            this.SubModules = new List<SubModule>();
            this.DelayedSubModules = new List<SubModule>();
            this.OptionalDependModules = new List<string>();
            this.Version = ModuleVersion.Empty;
        }

        public static Module Load(ModManager manager, string directoryName, string gamePath)
        {
            var file = Path.Combine(gamePath, "Modules", directoryName, "SubModule.xml");
            if (!File.Exists(file))
            {
                manager.Log().Warn($"File {file} does not exist");
                return null;
            }

            var document = new XmlDocument();
            try
            {
                document.Load(file);
            }
            catch (Exception ex)
            {
                manager.Log().Error($"Error while loading {file}", ex);

                return null;
            }

            var module = document.SelectSingleNode("Module");
            if (module != null)
            {
                var ret = new Module();
                ret.DirectoryName = directoryName;
                ret.Name = module.SelectSingleNode("Name")?.Attributes["value"]?.InnerText;
                ret.Id = module.SelectSingleNode("Id")?.Attributes["value"]?.InnerText;
                if (string.IsNullOrEmpty(ret.Name) || string.IsNullOrWhiteSpace(ret.Id))
                {
                    if (string.IsNullOrEmpty(ret.Name)) manager.Log().Error($"Invalid module name in {file}");
                    if (string.IsNullOrEmpty(ret.Id)) manager.Log().Error($"Invalid module id in {file}");
                    return null;
                }

                try
                {
                    ret.Version = ModuleVersion.FromString(module.SelectSingleNode("Version")?.Attributes["value"]?.InnerText);
                }
                catch (Exception e)
                {
                    manager.Log().Error(e, $"Version parsing issue in {file}");
                    ret.Version = ModuleVersion.Empty;
                }

                ret.Official = string.Equals(module.SelectSingleNode("Official")?.Attributes["value"]?.InnerText, "true", StringComparison.OrdinalIgnoreCase);
                ret.DefaultModule = string.Equals(module.SelectSingleNode("DefaultModule")?.Attributes["value"]?.InnerText, "true", StringComparison.OrdinalIgnoreCase);
                ret.SingleplayerModule = string.Equals(module.SelectSingleNode("SingleplayerModule")?.Attributes["value"]?.InnerText, "true", StringComparison.OrdinalIgnoreCase);
                ret.MultiplayerModule = string.Equals(module.SelectSingleNode("MultiplayerModule")?.Attributes["value"]?.InnerText, "true", StringComparison.OrdinalIgnoreCase);
                var dependedModules = module.SelectSingleNode("DependedModules");
                if (dependedModules != null)
                {
                    var dependedModulesList = dependedModules.SelectNodes("DependedModule");
                    for (var i = 0; i < dependedModulesList.Count; i++)
                    {
                        var value = dependedModulesList[i].Attributes["Id"]?.InnerText;
                        if (!string.IsNullOrEmpty(value)) ret.DependedModules.Add(value);
                    }
                    dependedModulesList = dependedModules.SelectNodes("OptionalDependModule");
                    for (var i = 0; i < dependedModulesList.Count; i++)
                    {
                        var value = dependedModulesList[i].Attributes["Id"]?.InnerText;
                        if (!string.IsNullOrEmpty(value)) ret.OptionalDependModules.Add(value);
                    }
                }
                var subModules = module.SelectSingleNode("SubModules");
                if (subModules != null)
                {
                    var subModulesList = subModules.SelectNodes("SubModule");
                    for (var i = 0; i < subModulesList.Count; i++)
                    {
                        var subModule = SubModule.Load(subModulesList[i]);
                        if (subModule != null) ret.SubModules.Add(subModule);
                    }
                }

                var delayedSubModules = module.SelectSingleNode("DelayedSubModules");
                if (delayedSubModules != null)
                {
                    var subModulesList = delayedSubModules.SelectNodes("SubModule");
                    for (var i = 0; i < subModulesList.Count; i++)
                    {
                        var subModule = SubModule.Load(subModulesList[i]);
                        if (subModule != null) ret.DelayedSubModules.Add(subModule);
                    }
                    subModulesList = delayedSubModules.SelectNodes("DelayedSubModule");
                    for (var i = 0; i < subModulesList.Count; i++)
                    {
                        var subModule = SubModule.Load(subModulesList[i]);
                        if (subModule != null) ret.DelayedSubModules.Add(subModule);
                    }

                }
                var optionalDependModules = module.SelectSingleNode("OptionalDependModules");
                if (optionalDependModules != null)
                {
                    var optionalDependModuleList = optionalDependModules.SelectNodes("OptionalDependModule");
                    for (var i = 0; i < optionalDependModuleList.Count; i++)
                    {
                        var value = optionalDependModuleList[i].Attributes["Id"]?.InnerText;
                        if (!string.IsNullOrEmpty(value)) ret.OptionalDependModules.Add(value);
                    }
                    optionalDependModuleList = dependedModules.SelectNodes("DependModule");
                    for (var i = 0; i < optionalDependModuleList.Count; i++)
                    {
                        var value = optionalDependModuleList[i].Attributes["Id"]?.InnerText;
                        if (!string.IsNullOrEmpty(value)) ret.OptionalDependModules.Add(value);
                    }
                }

                return ret;
            }
            manager.Log().Error($"Could not find module node in {file}");

            return null;
        }
    }
}