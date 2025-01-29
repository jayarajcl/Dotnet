using MicroFocus.InsecureWebApp.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace MicroFocus.InsecureWebApp.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class PrescriptionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private const string PRESCRIPTION_LOCATION = "Files\\Prescriptions\\";

        public PrescriptionController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public ContentResult GetSearchText(string sSearchText)
        {

            return Content("Prescription Search By : " + sSearchText, "text/html");
        }

        [HttpGet("GetPrescription")]
        public List<Models.Prescription> GetPrescription(string sSearchText)
        {
            List<Models.Prescription> pres;
            if (string.IsNullOrEmpty(sSearchText))
            {
                pres = _context.Prescription.ToList();
            }
            else
            {
                pres = _context.Prescription.Where(m => m.DocName.Contains(sSearchText)).ToList();
            }
            return pres; //Ok(pres);
        }


        [HttpGet("GetDoctorName")]
        public Models.Prescription GetDoctorNameByPresId(int iPresId)
        {
            var pres = _context.Prescription.Where(m => m.ID.Equals(iPresId)).FirstOrDefault();
            return pres;
        }

        [HttpPost("UploadXml")]
        public IActionResult UploadXml(string sPath)
        {
            XmlDocument xdoc = new XmlDocument();
            xdoc.XmlResolver = new XmlUrlResolver();
            xdoc.LoadXml(sPath);

            return Content(xdoc.InnerText);
        }

        [HttpPost("UploadFile")]
       public async Task<bool> UploadFile(IFormFile file, string sPath)
{
    bool blnResult = false;
    try
    {
        string baseDirectory = Path.GetFullPath("C:\\SecureBaseDirectory");
        string fullPath = Path.GetFullPath(sPath);

        if (!fullPath.StartsWith(baseDirectory))
            throw new UnauthorizedAccessException("Access to the path is denied.");

        if (!Directory.Exists(baseDirectory))
            throw new DirectoryNotFoundException("Base directory does not exist.");

        string fileName = Path.GetFileName(sPath);
        if (!Regex.IsMatch(fileName, @"^[a-zA-Z0-9]+\.(txt|jpg|png)$"))
            throw new InvalidDataException("Invalid file name or extension.");

        using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }
        blnResult = true;
    }
    catch (UnauthorizedAccessException)
    {
        throw;
    }
    catch (DirectoryNotFoundException)
    {
        throw;
    }
    catch (InvalidDataException)
    {
        throw;
    }
    catch (Exception ex)
    {
        throw new FileLoadException(ex.Message);
    }
    return blnResult;
}
        [HttpPost("UpdateXml")]
        public async Task<bool> UpdateXml(string sFileName, string xmlContent)
        {
            bool retFunc = false;
            if (!string.IsNullOrEmpty(xmlContent))
            {
                await Task.Delay(100);
                string path = Path.Combine(Directory.GetCurrentDirectory(), "Files"+ Path.DirectorySeparatorChar +"Prescriptions" + Path.DirectorySeparatorChar) + sFileName;

                XmlDocument document = new XmlDocument();
                document.Load(path);

                document.InnerXml = xmlContent;
                document.Save(path);
                retFunc = true;
            }
            return retFunc;
        }

        [HttpGet("DownloadFile")]
        public FileResult DownloadFile(string fileName)
{
    string baseDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Files", "Prescriptions");
    if (!Directory.Exists(baseDirectory))
    {
        throw new DirectoryNotFoundException("The base directory does not exist.");
    }

    string[] allowedExtensions = { ".pdf", ".txt", ".docx" };
    string fileExtension = Path.GetExtension(fileName);

    if (!Regex.IsMatch(fileName, @"^[a-zA-Z0-9]+\.[a-zA-Z0-9]+$") || !allowedExtensions.Contains(fileExtension))
    {
        throw new ArgumentException("Invalid file name or extension.");
    }

    string fullPath = Path.GetFullPath(Path.Combine(baseDirectory, fileName));

    if (!fullPath.StartsWith(baseDirectory))
    {
        throw new UnauthorizedAccessException("Access to the path is denied.");
    }

    if (!System.IO.File.Exists(fullPath))
    {
        throw new FileNotFoundException("The file does not exist.");
    }

    byte[] bytes = System.IO.File.ReadAllBytes(fullPath);
    return File(bytes, "application/octet-stream", fileName);
}

    }
}
