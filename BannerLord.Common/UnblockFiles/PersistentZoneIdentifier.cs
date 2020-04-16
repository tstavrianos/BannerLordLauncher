using System.Runtime.InteropServices;

namespace BannerLord.Common.UnblockFiles
{
    [ComImport]
    [Guid("0968e258-16c7-4dba-aa86-462dd61e31a3")]
    // HKEY_CLASSES_ROOT\Wow6432Node\AppID\{0968e258-16c7-4dba-aa86-462dd61e31a3}

    // this is a ComImport so we don't actually implement the interfaces
    public class PersistentZoneIdentifier  //: IZoneIdentifier, IPersistFile
    {
    }
}
