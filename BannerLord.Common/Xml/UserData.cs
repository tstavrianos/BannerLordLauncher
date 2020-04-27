using System;
using Alphaleonis.Win32.Filesystem;
using System.Xml;
using System.Xml.Serialization;
using Splat;

namespace BannerLord.Common.Xml
{
    public class UserData : IEnableLogger
    {
        public GameType GameType { get; set; }
        public UserGameTypeData SingleplayerData { get; set; }
        public UserGameTypeData MultiplayerData { get; set; }

        public UserData()
        {
            this.GameType = GameType.Singleplayer;
            this.SingleplayerData = new UserGameTypeData();
            this.MultiplayerData = new UserGameTypeData();
        }

        public static UserData Load(ModManager manager, string file)
        {
            if (!File.Exists(file))
            {
                manager.Log().Warn($"File {file} does not exist");
                return null;
            }
            var xmlSerializer = new XmlSerializer(typeof(UserData));
            try
            {
                using var xmlReader = XmlReader.Create(file);
                return (UserData)xmlSerializer.Deserialize(xmlReader);
            }
            catch (Exception value)
            {
                manager.Log().Error($"Error loading {file}", value);
                return null;
            }
        }

        public void Save(string file)
        {
            var xmlSerializer = new XmlSerializer(typeof(UserData));
            try
            {
                if (File.Exists(file)) File.Delete(file);
                using var xmlWriter = XmlWriter.Create(file, new XmlWriterSettings
                {
                    Indent = true
                });
                xmlSerializer.Serialize(xmlWriter, this);
            }
            catch (Exception value)
            {
                this.Log().Error($"Error saving {file}", value);
            }
        }
    }
}