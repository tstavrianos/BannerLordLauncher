namespace BannerLord.Common.Xml
{
    public class UserModData
    {
        public UserModData() {}
        
        public UserModData(string id, bool isSelected)
        {
            this.Id = id;
            this.IsSelected = isSelected;
        }

        public string Id { get; set; }
        public bool IsSelected { get; set; }
        
    }
}