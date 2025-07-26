using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NhomDangKhoa.Data;
using NhomDangKhoa.Models;
using NhomDangKhoa.Models.ViewModels; // Or NhomDangKhoa.ViewModels depending on your project structure
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NhomDangKhoa.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly _22bitv02EmployeeContext _context;
        private readonly IWebHostEnvironment _env;

        public EmployeesController(_22bitv02EmployeeContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: Employees - Merged with Search functionality
        public async Task<IActionResult> Index(string searchString)
        {
            var employeesQuery = _context.Employees
                .Include(e => e.Department)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                string lower = searchString.ToLower();
                employeesQuery = employeesQuery.Where(e =>
                    e.EmployeeName.ToLower().Contains(lower) ||
                    e.Phone.ToLower().Contains(lower) ||
                    e.Email.ToLower().Contains(lower));
            }

            // Using a ViewModel for the list view is good practice
            var viewModelList = await employeesQuery
                .Select(e => new EmployeeListViewModel
                {
                    EmployeeId = e.EmployeeId,
                    EmployeeName = e.EmployeeName,
                    DepartmentName = e.Department.DepartmentName,
                    Gender = e.Gender == true ? "Male" : e.Gender == false ? "Female" : "Unknown",
                    PhotoImagePath = e.PhotoImagePath
                })
                .ToListAsync();

            ViewBag.SearchString = searchString;

            return View(viewModelList);
        }

        // GET: Employees/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var employee = await _context.Employees
                .Include(e => e.Department)
                .FirstOrDefaultAsync(m => m.EmployeeId == id);

            if (employee == null) return NotFound();

            return View(employee);
        }

        // GET: Employees/Create - From 'main' to support image upload
        public IActionResult Create()
        {
            var vm = new EmployeeViewModel
            {
                Departments = _context.Departments.Select(d => new SelectListItem
                {
                    Value = d.DepartmentId.ToString(),
                    Text = d.DepartmentName
                }).ToList()
            };

            return View(vm);
        }

        // POST: Employees/Create - From 'main' with full image upload logic
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmployeeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Departments = _context.Departments.Select(d => new SelectListItem
                {
                    Value = d.DepartmentId.ToString(),
                    Text = d.DepartmentName
                }).ToList();

                return View(model);
            }

            string? uniqueFileName = null;

            if (model.Photo != null)
            {
                // Using the helper method for consistency
                var uploadResult = await ProcessUploadImage(model.Photo);
                if (uploadResult.IsSuccess)
                {
                    uniqueFileName = uploadResult.FilePath;
                }
                else
                {
                    ModelState.AddModelError("Photo", uploadResult.ErrorMessage!);
                    model.Departments = _context.Departments.Select(d => new SelectListItem
                    {
                        Value = d.DepartmentId.ToString(),
                        Text = d.DepartmentName
                    }).ToList();
                    return View(model);
                }
            }

            var employee = new Employee
            {
                EmployeeName = model.EmployeeName,
                Gender = model.Gender,
                DateOfBirth = model.DateOfBirth,
                Email = model.Email,
                Phone = model.Phone,
                Salary = model.Salary,
                DepartmentId = model.DepartmentId,
                PhotoImagePath = uniqueFileName
            };

            _context.Add(employee);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        // GET: Employees/Edit/5 - From 'main'
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return NotFound();
            
            // This approach (using ViewData) is from 'main' and matches its POST Edit method
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "DepartmentId", "DepartmentName", employee.DepartmentId);
            return View(employee);
        }

        // POST: Employees/Edit/5 - From 'main' with image update logic
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Employee employee, IFormFile? Photo)
        {
            if (id != employee.EmployeeId) return NotFound();

            if (Photo != null)
            {
                var result = await ProcessUploadImage(Photo);
                if (!result.IsSuccess)
                {
                    ModelState.AddModelError("PhotoImagePath", result.ErrorMessage!);
                    ViewData["DepartmentId"] = new SelectList(_context.Departments, "DepartmentId", "DepartmentName", employee.DepartmentId);
                    return View(employee);
                }
                // TODO: Consider deleting the old image file from wwwroot
                employee.PhotoImagePath = result.FilePath;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // It's safer to attach and modify state for only the properties you want to change
                    // But for simplicity, Update() is used here as in the original code.
                    _context.Update(employee);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(employee.EmployeeId))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }
            
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "DepartmentId", "DepartmentName", employee.DepartmentId);
            return View(employee);
        }

        // GET: Employees/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var employee = await _context.Employees
                .Include(e => e.Department)
                .FirstOrDefaultAsync(m => m.EmployeeId == id);

            if (employee == null) return NotFound();

            return View(employee);
        }

        // POST: Employees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee != null)
            {
                // TODO: Consider deleting the image file from wwwroot when deleting an employee
                _context.Employees.Remove(employee);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.EmployeeId == id);
        }

        // ⬇️ Image upload utility from 'main'
        private async Task<(bool IsSuccess, string? FilePath, string? ErrorMessage)> ProcessUploadImage(IFormFile photo)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(photo.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
                return (false, null, "Chỉ hỗ trợ ảnh .jpg, .jpeg, .png");

            if (photo.Length > 2 * 1024 * 1024)
                return (false, null, "Dung lượng ảnh tối đa là 2MB");

            var uploadsFolder = Path.Combine(_env.WebRootPath, "images/photos");
            Directory.CreateDirectory(uploadsFolder); // Ensures the directory exists

            var uniqueName = Guid.NewGuid().ToString() + extension;
            var filePath = Path.Combine(uploadsFolder, uniqueName);

            using var fs = new FileStream(filePath, FileMode.Create);
            await photo.CopyToAsync(fs);

            // Return the web-accessible path
            return (true, "/images/photos/" + uniqueName, null);
        }
    }
}