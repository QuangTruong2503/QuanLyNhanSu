using Microsoft.EntityFrameworkCore;
using QuanLyNhanSu.Models;

namespace QuanLyNhanSu.Data
{
    public class QuanLyNhanSuDbContext : DbContext
    {
        public QuanLyNhanSuDbContext(DbContextOptions<QuanLyNhanSuDbContext> options)
            : base(options)
        {
        }

        public DbSet<departmentsModel> departments { get; set; }
        //Bảng employees
        public DbSet<EmployeesModel> employees { get; set; }
        //Bảng positions
        public DbSet<PositionModel> positions { get; set; }

    }
}
