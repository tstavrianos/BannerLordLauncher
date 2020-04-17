using Splat;

namespace BannerLord.Common
{
    public interface IModManagerClient : IEnableLogger
    {
        bool CanInitialize(string configPath, string gamePath);

        bool CanRun();
        bool CanSave();

        bool CanMoveToTop(int idx);
        bool CanMoveUp(int idx);
        bool CanMoveDown(int idx);
        bool CanMoveToBottom(int idx);

        bool CanCheckAll();
        bool CanUncheckAll();
        bool CanInvertCheck();
    }
}
