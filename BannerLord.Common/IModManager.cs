using Splat;

namespace BannerLord.Common
{
    public interface IModManager : IEnableLogger
    {
        bool Initialize(string configPath, string gamePath, out string errorMessage);


        bool Save(out string errorMessage);
        bool Run(out string errorMessage);

        bool MoveToTop(int idx, out string errorMessage);
        bool MoveUp(int idx, out string errorMessage);
        bool MoveDown(int idx, out string errorMessage);
        bool MoveToBottom(int idx, out string errorMessage);

        bool CheckAll(out string errorMessage);
        bool UncheckAll(out string errorMessage);
        bool InvertCheck(out string errorMessage);

        bool Sort(out string errorMessage);
    }
}
