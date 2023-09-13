using Microsoft.AspNetCore.WebUtilities;

namespace BitsOrc.Interfaces
{
    public interface IFileUploadService
    {
        Task<bool> UploadFile(MultipartReader reader, MultipartSection section);
    }
}
