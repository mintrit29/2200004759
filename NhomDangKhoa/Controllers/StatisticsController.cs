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
                   
                    TotalSalary = d.Employees.Sum(e => e.Salary ?? 0),
                   
                    TotalOfFemale = d.Employees.Count(e => e.Gender == false)
                })
                .ToListAsync();


            return View(statistics);
        }
    }
}