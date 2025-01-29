using Microsoft.AspNetCore.Mvc;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Tar;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace MicroFocus.InsecureWebApp.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class FileController : Controller
    {
        private const string PRESCRIPTION_LOCATION = "Files\\Prescriptions\\";

        [HttpPost("UploadFile")]
       public async Task<IActionResult> UploadFile(IFormFile file, string zipFileName, string targetDir = "")
{
    string baseDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Files", "Prescriptions");
    if (!Directory.Exists(baseDirectory))
    {
        return BadRequest("Base directory does not exist.");
    }

    if (string.IsNullOrEmpty(zipFileName) || !Regex.IsMatch(zipFileName, @"^[a-zA-Z0-9]+\.(zip)$"))
    {
        return BadRequest("Invalid file name.");
    }

    string zipFilePath = Path.Combine(baseDirectory, zipFileName);
    using (var stream = new FileStream(zipFilePath, FileMode.Create))
    {
        await file.CopyToAsync(stream);
    }

    if (string.IsNullOrEmpty(targetDir))
    {
        targetDir = baseDirectory;
    }

    string targetDirectory = Path.GetFullPath(Path.Combine(baseDirectory, targetDir));
    if (!targetDirectory.StartsWith(baseDirectory))
    {
        return BadRequest("Invalid target directory.");
    }

    if (!Directory.Exists(targetDirectory))
    {
        return BadRequest("Target directory does not exist.");
    }

    FastZip fastZip = new FastZip();
    fastZip.ExtractZip(zipFilePath, targetDirectory, null);

    return Ok("File extracted at : " + targetDirectory);
}
    }
}
