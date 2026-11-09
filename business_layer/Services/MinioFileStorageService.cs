using System;
using System.IO;
using System.Threading.Tasks;
using business_layer.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Minio;
using Minio.DataModel.Args;

namespace business_layer
{
    public class MinioFileStorageService : IFileStorageService
    {
        private readonly IMinioClient _minioClient;
        private readonly string _bucketName;
        private readonly ILogger<MinioFileStorageService> _logger;

        public MinioFileStorageService(IMinioClient minioClient, IConfiguration configuration, ILogger<MinioFileStorageService> logger)
        {
            _minioClient = minioClient;
            _bucketName = configuration["Minio:BucketName"] ?? "documents";
            _logger = logger;
        }

        public async Task<string> UploadFileAsync(string objectName, Stream data, long size, string contentType)
        {
            _logger.LogInformation("Prüfe MinIO Bucket '{BucketName}'...", _bucketName);

            bool found = await _minioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(_bucketName));
            if (!found)
            {
                _logger.LogInformation("Bucket '{BucketName}' existiert nicht. Erstelle Bucket...", _bucketName);
                await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(_bucketName));
            }

            // Stream-Position zurücksetzen
            if (data.CanSeek)
            {
                data.Position = 0;
            }

            var putObjectArgs = new PutObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(objectName)
                .WithStreamData(data)
                .WithObjectSize(size)
                .WithContentType(contentType);

            await _minioClient.PutObjectAsync(putObjectArgs);
            _logger.LogInformation("Datei '{ObjectName}' ({Size} Bytes) erfolgreich in MinIO Bucket '{BucketName}' gespeichert.",
                objectName, size, _bucketName);

            return $"{_bucketName}/{objectName}";
        }

        public async Task<Stream> DownloadFileAsync(string objectName)
        {
            var memoryStream = new MemoryStream();
            var getObjectArgs = new GetObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(objectName)
                .WithCallbackStream(stream => stream.CopyTo(memoryStream));

            await _minioClient.GetObjectAsync(getObjectArgs);
            memoryStream.Position = 0;
            return memoryStream;
        }

        public async Task DeleteFileAsync(string objectName)
        {
            var removeArgs = new RemoveObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(objectName);

            await _minioClient.RemoveObjectAsync(removeArgs);
            _logger.LogInformation("Datei '{ObjectName}' aus MinIO gelöscht.", objectName);
        }
    }
}