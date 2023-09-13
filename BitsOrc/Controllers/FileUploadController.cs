using BitsOrc.Interfaces;
using BitsOrc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;

namespace BitsOrc.Controllers
{
    public class FileUploadController : Controller
    {
        readonly IFileUploadService _streamFileUploadService;
        private string _FilePath;
        string[] _csvFiles;
        UserContext _db;

        public FileUploadController(IFileUploadService streamFileUploadService, UserContext db)
        {
            _streamFileUploadService = streamFileUploadService;
            _FilePath = "UploadedFiles/username.csv";
            _csvFiles = Directory.GetFiles("UploadedFiles", "*.csv");
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var users = await _db.Employees.ToListAsync();
            return View(users);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var user = await _db.Employees.FindAsync(id);
            if (user == null)
                return NotFound();
            return View(user);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var user = await _db.Employees.FindAsync(id);
            if (user == null)
                return NotFound();
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCompleted(int id)
        {
            var user = await _db.Employees.FindAsync(id);
            if (user != null)
            {
                _db.Employees.Remove(user);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Employee emp)
        {
            if (ModelState.IsValid)
            {
                _db.Employees.Update(emp);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(emp);
        }

        [ActionName("Index")]
        [HttpPost]
        public async Task<IActionResult> SaveFileToPhysicalFolder()
        {
            var boundary = HeaderUtilities.RemoveQuotes(
             MediaTypeHeaderValue.Parse(Request.ContentType).Boundary
            ).Value;

            var reader = new MultipartReader(boundary, Request.Body);

            var section = await reader.ReadNextSectionAsync();

            string response = string.Empty;
            try
            {
                if (await _streamFileUploadService.UploadFile(reader, section))
                {
                    ViewBag.Message = "File Upload Successful";
                    _FilePath = _csvFiles.FirstOrDefault() != null ? _csvFiles.FirstOrDefault() : _FilePath;
                    if (System.IO.File.Exists(_FilePath)) //foreach (string _FilePath in _csvFiles)
                    {
                        using (var read = new StreamReader(_FilePath))
                        {
                            read.ReadLine();

                            while (!read.EndOfStream)
                            {
                                var line = read.ReadLine();
                                var values = line.Split(';');

                                if (values.Length == 5)
                                {
                                    var user = new Employee
                                    {
                                        Name = values[0],
                                        DateOfBirth = DateTime.Parse(values[1]),
                                        Married = bool.Parse(values[2]),
                                        Phone = values[3],
                                        Salary = decimal.Parse(values[4])
                                    };
                                    _db.Employees.Add(user);
                                }
                            }
                            _db.SaveChangesAsync();
                        }
                    }
                    else
                    {
                        ViewBag.Message = "DB Save Failed";
                    }
                }

            }
            catch (Exception ex)
            {
                ViewBag.Message = "File Upload Failed";
            }
            return RedirectToAction("Index");
        }
    }
}
