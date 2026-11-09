using System.IO;
using System.Threading.Tasks;

namespace business_layer.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> UploadFileAsync(string objectName, Stream data, long size, string contentType);
        Task<Stream> DownloadFileAsync(string objectName);
        Task DeleteFileAsync(string objectName);
    }
}
