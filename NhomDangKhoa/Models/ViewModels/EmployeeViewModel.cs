using Microsoft.AspNetCore.Mvc.Rendering;

namespace NhomDangKhoa.Models.ViewModels
{
    public class EmployeeViewModel
    {
        public Employee Employee { get; set; }

        // Danh sách department để bind dropdown
        public IEnumerable<SelectListItem> Departments { get; set; }
    }
}
