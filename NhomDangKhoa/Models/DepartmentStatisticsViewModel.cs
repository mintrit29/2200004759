namespace NhomDangKhoa.Models
{

    public class DepartmentStatisticsViewModel
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = "";
        public int NumberOfEmployees { get; set; }
        public decimal TotalSalary { get; set; }
        public int TotalOfFemale { get; set; }
    }
}