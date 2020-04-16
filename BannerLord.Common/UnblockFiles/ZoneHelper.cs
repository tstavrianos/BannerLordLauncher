using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using Splat;

namespace BannerLord.Common.UnblockFiles
{

    public static class ZoneHelper
    {
        public static string GetZone(string filename)
        {
            IPersistFile persistFile = null;
            IZoneIdentifier zoneId = null;
            try
            {
                persistFile = (IPersistFile)new PersistentZoneIdentifier();
                //const int mode = (int) (STGM.READWRITE | STGM.SHARE_EXCLUSIVE);
                const int mode = (int)STGM.READ;
                try
                {
                    persistFile.Load(filename, mode);
                }
                catch (FileNotFoundException)
                {
                    return "(none)";
                }
                catch (UnauthorizedAccessException)
                {
                    return "(access denied)";
                }

                zoneId = (IZoneIdentifier)persistFile;

                var getIdResult = zoneId.GetId(out var zone);
                return zone.ToString();
            }
            finally
            {
                if (persistFile != null)
                {
                    Marshal.ReleaseComObject(persistFile);
                }
                if (zoneId != null)
                {
                    Marshal.ReleaseComObject(zoneId);
                }
            }
        }

        public static void Remove(string filename, IEnableLogger logger)
        {
            IPersistFile persistFile = null;
            IZoneIdentifier zoneId = null;
            try
            {
                // need to cast because we can't directly implement the interface in C# code
                persistFile = (IPersistFile)new PersistentZoneIdentifier();
                const int mode = (int)(STGM.READWRITE | STGM.SHARE_EXCLUSIVE);

                URLZONE zone;
                try
                {
                    persistFile.Load(filename, mode);
                    // need to cast because we can't directly implement the interface in C# code
                    zoneId = (IZoneIdentifier)persistFile;
                    var getIdResult = zoneId.GetId(out zone);
                }
                catch (FileNotFoundException)
                {
                    return;
                }
                catch (UnauthorizedAccessException ex)
                {
                    logger.Log().Error($"No permission to unblock {filename}");
                    return;
                }
                catch (COMException e)
                {
                    logger.Log().Error(e);
                    return;
                }

                var removeResult = zoneId.Remove();

                persistFile.Save(filename, true);
            }
            finally
            {
                // don't forget to release the COM objects

                if (persistFile != null)
                {
                    Marshal.ReleaseComObject(persistFile);
                }
                if (zoneId != null)
                {
                    Marshal.ReleaseComObject(zoneId);
                }
            }
        }
    }
}
