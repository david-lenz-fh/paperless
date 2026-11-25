using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Minio;
using Minio.DataModel.Args;

namespace paperless_worker.services;

public class OcrService
{
    private readonly IMinioClient _minioClient;
    private readonly string _bucketName = "documents";

    private static readonly HashSet<string> SupportedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".bmp", ".tif", ".tiff", ".webp"
    };

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
            var cleanObjectKey = msg.ObjectKey.StartsWith("documents/")
                ? msg.ObjectKey.Substring(10)
                : msg.ObjectKey;

            // Echte Dateiendung ermitteln (Fallback auf .pdf)
            var extension = Path.GetExtension(cleanObjectKey);
            if (string.IsNullOrWhiteSpace(extension))
            {
                extension = Path.GetExtension(msg.FileName);
            }
            if (string.IsNullOrWhiteSpace(extension))
            {
                extension = ".pdf";
            }

            var inputPath = Path.Combine(workDir, $"input{extension.ToLowerInvariant()}");
            Console.WriteLine($"[OCR] Lade Objekt '{cleanObjectKey}' als '{inputPath}' herunter...");

            var getObjectArgs = new GetObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(cleanObjectKey)
                .WithCallbackStream(stream =>
                {
                    using var fileStream = new FileStream(inputPath, FileMode.Create, FileAccess.Write);
                    stream.CopyTo(fileStream);
                });
            await _minioClient.GetObjectAsync(getObjectArgs);

            var fileInfo = new FileInfo(inputPath);
            Console.WriteLine($"[OCR] Datei aus MinIO heruntergeladen. Pfad: {inputPath}, Größe: {fileInfo.Length} Bytes");

            if (fileInfo.Length == 0)
            {
                throw new InvalidOperationException("Die heruntergeladene Datei ist leer (0 Bytes)!");
            }

            var sb = new StringBuilder();

            // Fall 1: PDF -> Erst über Ghostscript in PNG-Seiten zerlegen
            if (extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("[OCR] PDF erkannt – konvertiere Seiten mit Ghostscript...");
                await RunAsync("gs", $"-dNOPAUSE -dBATCH -dQUIET -sDEVICE=png16m -r300 " +
                                     $"-sOutputFile=\"{workDir}/page-%03d.png\" \"{inputPath}\"");

                foreach (var png in Directory.GetFiles(workDir, "page-*.png").OrderBy(f => f))
                {
                    sb.AppendLine(await RunAsync("tesseract", $"\"{png}\" stdout -l eng+deu"));
                }
            }
            // Fall 2: Bild (z. B. Handyfoto) -> Ghostscript überspringen und direkt an Tesseract übergeben
            else if (SupportedImageExtensions.Contains(extension))
            {
                Console.WriteLine($"[OCR] Bild ({extension}) erkannt – starte Tesseract direkt...");
                sb.AppendLine(await RunAsync("tesseract", $"\"{inputPath}\" stdout -l eng+deu"));
            }
            else
            {
                throw new NotSupportedException($"Dateiformat '{extension}' wird für OCR nicht unterstützt.");
            }

            return sb.ToString();
        }
        finally
        {
            if (Directory.Exists(workDir))
            {
                Directory.Delete(workDir, recursive: true);
            }
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