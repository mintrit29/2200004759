using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NhomDangKhoa.Data;
using NhomDangKhoa.Models;
using NhomDangKhoa.Models.ViewModels;

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

        // GET: Employees (tìm kiếm 2.4)
        public async Task<IActionResult> Index(string searchString)
        {
            var query = _context.Employees.Include(e => e.Department).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                var keyword = searchString.ToLower();
                query = query.Where(e =>
                    e.EmployeeName.ToLower().Contains(keyword) ||
                    e.Phone.ToLower().Contains(keyword) ||
                    e.Email.ToLower().Contains(keyword));
            }

            var result = await query
                .Select(e => new EmployeeListViewModel
                {
                    EmployeeId = e.EmployeeId,
                    EmployeeName = e.EmployeeName,
                    DepartmentName = e.Department.DepartmentName,
                    Gender = e.Gender == true ? "Male" : e.Gender == false ? "Female" : "Unknown",
                    PhotoImagePath = e.PhotoImagePath
                }).ToListAsync();

            ViewBag.SearchString = searchString;
            return View(result);
        }

        // GET: Employees/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var employee = await _context.Employees.Include(e => e.Department)
                .FirstOrDefaultAsync(e => e.EmployeeId == id);

            if (employee == null) return NotFound();
            return View(employee);
        }

        // GET: Employees/Create
        public IActionResult Create()
        {
            var vm = new EmployeeViewModel
            {
                Employee = new Employee(),
                Departments = _context.Departments.Select(d => new SelectListItem
                {
                    Value = d.DepartmentId.ToString(),
                    Text = d.DepartmentName
                })
            };
            return View(vm);
        }

        // POST: Employees/Create (2.3: upload ảnh)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmployeeViewModel vm, IFormFile? Photo)
        {
            if (Photo != null)
            {
                var result = await ProcessUploadImage(Photo);
                if (!result.IsSuccess)
                {
                    ModelState.AddModelError("Employee.PhotoImagePath", result.ErrorMessage!);
                    vm.Departments = LoadDepartments();
                    return View(vm);
                }

                vm.Employee.PhotoImagePath = result.FilePath;
            }

            if (ModelState.IsValid)
            {
                _context.Add(vm.Employee);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            vm.Departments = LoadDepartments();
            return View(vm);
        }

        // GET: Employees/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return NotFound();

            var vm = new EmployeeViewModel
            {
                Employee = employee,
                Departments = LoadDepartments()
            };
            return View(vm);
        }

        // POST: Employees/Edit/5 (có xử lý ảnh 2.3)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EmployeeViewModel vm, IFormFile? Photo)
        {
            if (id != vm.Employee.EmployeeId) return NotFound();

            if (Photo != null)
            {
                var result = await ProcessUploadImage(Photo);
                if (!result.IsSuccess)
                {
                    ModelState.AddModelError("Employee.PhotoImagePath", result.ErrorMessage!);
                    vm.Departments = LoadDepartments();
                    return View(vm);
                }

                vm.Employee.PhotoImagePath = result.FilePath;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(vm.Employee);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(vm.Employee.EmployeeId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            vm.Departments = LoadDepartments();
            return View(vm);
        }

        // GET: Employees/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var employee = await _context.Employees
                .Include(e => e.Department)
                .FirstOrDefaultAsync(e => e.EmployeeId == id);

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
                _context.Employees.Remove(employee);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EmployeeExists(int id) =>
            _context.Employees.Any(e => e.EmployeeId == id);

        // ✅ Tiện ích chung
        private IEnumerable<SelectListItem> LoadDepartments()
        {
            return _context.Departments.Select(d => new SelectListItem
            {
                Value = d.DepartmentId.ToString(),
                Text = d.DepartmentName
            }).ToList();
        }

        private async Task<(bool IsSuccess, string? FilePath, string? ErrorMessage)> ProcessUploadImage(IFormFile file)
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!new[] { ".jpg", ".jpeg", ".png" }.Contains(ext))
                return (false, null, "Chỉ nhận ảnh .jpg/.png");

            if (file.Length > 2 * 1024 * 1024)
                return (false, null, "Dung lượng ảnh tối đa là 2MB");

            var folder = Path.Combine(_env.WebRootPath, "images/photos");
            Directory.CreateDirectory(folder);
            var fileName = Guid.NewGuid() + ext;
            var path = Path.Combine(folder, fileName);

            using var fs = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(fs);

            return (true, "/images/photos/" + fileName, null);
        }
    }
}
