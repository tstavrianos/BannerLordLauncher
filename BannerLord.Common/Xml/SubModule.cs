using System;
using System.Collections.Generic;
using System.Xml;

namespace BannerLord.Common.Xml
{
    public class SubModule
    {
        public string Name { get; set; }
        public string DLLName { get; set; }
        public string SubModuleClassType { get; set; }
        public List<(SubModuleTags Key, string Value)> Tags { get; set; }
        public List<string> Assemblies { get; set; }

        public SubModule()
        {
            this.Tags = new List<(SubModuleTags Key, string Value)>();
            this.Assemblies = new List<string>();
        }

        public static SubModule Load(XmlNode node)
        {
            if (node == null) return null;
            var ret = new SubModule();
            ret.Name = node.SelectSingleNode("Name")?.Attributes["value"]?.InnerText;
            ret.DLLName = node.SelectSingleNode("DLLName")?.Attributes["value"]?.InnerText;
            ret.SubModuleClassType = node.SelectSingleNode("SubModuleClassType")?.Attributes["value"]?.InnerText;
            var assemblies = node.SelectSingleNode("Assemblies");
            if (assemblies != null)
            {
                var assembliesList = assemblies.SelectNodes("Assembly");
                for (var i = 0; i < assembliesList.Count; i++)
                {
                    var value = assembliesList[i].Attributes["value"]?.InnerText;
                    if(!string.IsNullOrEmpty(value)) ret.Assemblies.Add(value);
                }
            }
            var tags = node.SelectSingleNode("Tags");
            if (tags != null)
            {
                var tagsList = tags.SelectNodes("Tag");
                for (var i = 0; i < tagsList.Count; i++)
                {
                    var key = tagsList[i].Attributes["key"]?.InnerText;
                    if (!string.IsNullOrEmpty(key) && Enum.TryParse<SubModuleTags>(key, out var keyEnum))
                    {
                        var value = tagsList[i].Attributes["value"]?.InnerText;
                        ret.Tags.Add((keyEnum, value));
                    }
                }
            }
            return ret;
        }
    }
}