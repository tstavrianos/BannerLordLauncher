using System.Collections.Generic;

namespace BannerLord.Common.Xml
{
    public class UserGameTypeData
    {
        public List<UserModData> ModDatas { get; set; }

        public UserGameTypeData()
        {
            this.ModDatas = new List<UserModData>();
        }
    }
}