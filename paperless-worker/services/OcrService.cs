using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Minio;
using Minio.DataModel.Args;

namespace paperless_worker.services;

public class OcrService
{

    private readonly IMinioClient _minioClient;
    private readonly string _bucketName = "documents"; // Passe hier deinen Bucket-Namen an

    // Ein einziger, sauberer Konstruktor für Dependency Injection
    public OcrService(IMinioClient minioClient)
    {
        _minioClient = minioClient;
    }

    public async Task<string> ExtractTextAsync(DocumentUploadedMessage msg)
    {
        var workDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(workDir);
        try
        {
            var pdfPath = Path.Combine(workDir, "input.pdf");

            // minio download
            var getObjectArgs = new GetObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(msg.ObjectKey) // zb. "doc_12_v1.pdf"
                .WithCallbackStream(stream =>
                {
                    using var fileStream = new FileStream(pdfPath, FileMode.Create, FileAccess.Write);
                    stream.CopyTo(fileStream);
                });
            await _minioClient.GetObjectAsync(getObjectArgs);

            Console.WriteLine("testtestst222");
            var fileInfo = new FileInfo(pdfPath);
            Console.WriteLine($"[OCR] Datei aus MinIO heruntergeladen. Pfad: {pdfPath}, Größe: {fileInfo.Length} Bytes");

            if (fileInfo.Length == 0)
            {
                throw new InvalidOperationException("Die heruntergeladene Datei ist leer (0 Bytes)!");
            }

            //png erstellen mit ghostscript
            await RunAsync("gs", $"-dNOPAUSE -dBATCH -dQUIET -sDEVICE=png16m -r300 " +
                                 $"-sOutputFile=\"{workDir}/page-%03d.png\" \"{pdfPath}\"");

            var sb = new StringBuilder();
            foreach (var png in Directory.GetFiles(workDir, "page-*.png").OrderBy(f => f))
                sb.AppendLine(await RunAsync("tesseract", $"\"{png}\" stdout -l eng+deu"));

            return sb.ToString();
        }
        finally
        {
            Directory.Delete(workDir, recursive: true);
        }
    }

    private static async Task<string> RunAsync(string file, string args)
    {
        var psi = new ProcessStartInfo(file, args)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        using var p = Process.Start(psi) ?? throw new InvalidOperationException($"{file} nicht startbar");
        var stdout = p.StandardOutput.ReadToEndAsync();
        var stderr = p.StandardError.ReadToEndAsync();
        await p.WaitForExitAsync();
        if (p.ExitCode != 0)
            throw new InvalidOperationException($"{file} fehlgeschlagen: {await stderr}");
        return await stdout;
    }
}


/*await _minio.GetObjectAsync(new GetObjectArgs()
                .WithBucket("documents")
                .WithObject(msg.ObjectKey)
                .WithFile(pdfPath));*/