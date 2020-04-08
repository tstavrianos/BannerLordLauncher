using System.Collections.Generic;
using System.Threading.Tasks;
using Steam.Common.WebAPI.Responses.ISteamRemoteStorage;

namespace Steam.Common.WebAPI.Requests
{
    public interface ISteamRemoteStorage
    {
        Task<GetPublishedFileDetailsResponse> GetPublishedFileDetailsAsync(string fileId);
        Task<GetPublishedFileDetailsResponse> GetPublishedFileDetailsAsync(ulong fileId);
        Task<GetPublishedFileDetailsResponse> GetPublishedFileDetailsAsync(IEnumerable<ulong> fileIds);
        Task<GetPublishedFileDetailsResponse> GetPublishedFileDetailsAsync(IReadOnlyList<string> fileIds);
    }
}