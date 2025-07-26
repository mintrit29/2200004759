namespace NhomDangKhoa.Models.ViewModels
{
    public class EmployeeListViewModel
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string PhotoImagePath { get; set; }
        public decimal Salary { get; set; }
        public string DepartmentName { get; set; }
    }
}
