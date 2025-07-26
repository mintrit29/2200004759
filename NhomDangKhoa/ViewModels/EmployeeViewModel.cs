using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace NhomDangKhoa.ViewModels;

public class EmployeeViewModel
{
    public int EmployeeId { get; set; }

    [Required]
    [StringLength(100)]
    public string EmployeeName { get; set; } = string.Empty;

    public bool? Gender { get; set; }

    [DataType(DataType.Date)]
    public DateOnly? DateOfBirth { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public decimal? Salary { get; set; }

    public int DepartmentId { get; set; }

    public IFormFile? Photo { get; set; }

    public string? PhotoImagePath { get; set; }

    public IEnumerable<SelectListItem>? Departments { get; set; }
}
