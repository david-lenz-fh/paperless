using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace paperless_worker.services;

public class OcrService
{
    /*
    //private readonly IMinioClient _minio;
    //public OcrService(IMinioClient minio) => _minio = minio;

    */

    public async Task<string> ExtractTextAsync(DocumentUploadedMessage msg)
    {
        var workDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(workDir);
        try
        {
            var pdfPath = Path.Combine(workDir, "input.pdf");

            // === TEMPORÄRER ERSATZ FÜR MINIO-DOWNLOAD ===
            // testpdf hernehmen
            string localTestFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test.pdf");
            if (File.Exists(localTestFile))
            {
                File.Copy(localTestFile, pdfPath, overwrite: true);
            }
            else
            {
                throw new FileNotFoundException("Lege bitte eine 'test.pdf' in das Ausgabeverzeichnis (oder Projektverzeichnis)!");
            }
            
            // erstelle aus jeder pdf seite ein png
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