using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NhomDangKhoa.Data;
using NhomDangKhoa.Models;
using NhomDangKhoa.ViewModels;
using System;
using System.Collections.Generic;
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

        // GET: Employees
        public async Task<IActionResult> Index()
        {
            var data = _context.Employees.Include(e => e.Department);
            return View(await data.ToListAsync());
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

        // GET: Employees/Create
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


        // POST: Employees/Create
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
                var ext = Path.GetExtension(model.Photo.FileName).ToLowerInvariant();
                if (!new[] { ".jpg", ".jpeg", ".png" }.Contains(ext) || model.Photo.Length > 2 * 1024 * 1024)
                {
                    ModelState.AddModelError("Photo", "Chỉ chấp nhận ảnh .jpg/.png và nhỏ hơn 2MB");
                    model.Departments = _context.Departments.Select(d => new SelectListItem
                    {
                        Value = d.DepartmentId.ToString(),
                        Text = d.DepartmentName
                    }).ToList();
                    return View(model);
                }

                var uploadPath = Path.Combine(_env.WebRootPath, "images/photos");
                Directory.CreateDirectory(uploadPath);
                uniqueFileName = Guid.NewGuid() + ext;
                var filePath = Path.Combine(uploadPath, uniqueFileName);

                using var fs = new FileStream(filePath, FileMode.Create);
                await model.Photo.CopyToAsync(fs);
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
                PhotoImagePath = uniqueFileName != null ? "/images/photos/" + uniqueFileName : null
            };

            _context.Add(employee);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        // GET: Employees/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return NotFound();

            ViewData["DepartmentId"] = new SelectList(_context.Departments, "DepartmentId", "DepartmentName", employee.DepartmentId);
            return View(employee);
        }

        // POST: Employees/Edit/5
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

                employee.PhotoImagePath = result.FilePath;
            }

            if (ModelState.IsValid)
            {
                try
                {
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
                _context.Employees.Remove(employee);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.EmployeeId == id);
        }

        // ⬇️ Tiện ích upload ảnh
        private async Task<(bool IsSuccess, string? FilePath, string? ErrorMessage)> ProcessUploadImage(IFormFile photo)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(photo.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
                return (false, null, "Chỉ hỗ trợ ảnh .jpg, .jpeg, .png");

            if (photo.Length > 2 * 1024 * 1024)
                return (false, null, "Dung lượng ảnh tối đa là 2MB");

            var uploadsFolder = Path.Combine(_env.WebRootPath, "images/photos");
            Directory.CreateDirectory(uploadsFolder);

            var uniqueName = Guid.NewGuid().ToString() + extension;
            var filePath = Path.Combine(uploadsFolder, uniqueName);

            using var fs = new FileStream(filePath, FileMode.Create);
            await photo.CopyToAsync(fs);

            return (true, "/images/photos/" + uniqueName, null);
        }
    }
}
