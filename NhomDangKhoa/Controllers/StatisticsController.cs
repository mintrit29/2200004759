using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NhomDangKhoa.Data;
using NhomDangKhoa.Models;
using System.Linq;
using System.Threading.Tasks;

namespace NhomDangKhoa.Controllers
{
    public class StatisticsController : Controller
    {
       
        private readonly _22bitv02EmployeeContext _context;

        public StatisticsController(_22bitv02EmployeeContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {

            var statistics = await _context.Departments
                .Include(d => d.Employees) 
                .Select(d => new DepartmentStatisticsViewModel
                {
                    DepartmentId = d.DepartmentId,
                    DepartmentName = d.DepartmentName,
                    NumberOfEmployees = d.Employees.Count(),
                    // Tính tổng lương, nếu không có nhân viên nào thì tổng lương là 0
                    TotalSalary = d.Employees.Sum(e => e.Salary ?? 0),
                    // Đếm số nhân viên nữ (giả định Gender: 0 là Nữ, 1 là Nam)
                    // Dựa trên dữ liệu mẫu của bạn, Gender là kiểu bool? (nullable boolean)
                    TotalOfFemale = d.Employees.Count(e => e.Gender == false)
                })
                .ToListAsync();

            // Gửi danh sách thống kê đến View
            return View(statistics);
        }
    }
}