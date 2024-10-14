using Microsoft.CodeAnalysis.Elfie.Diagnostics;
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
        //Bảng Role
        public DbSet<RoleModel> role { get; set; }
        //Bảng Attendence
        public DbSet<AttendanceModel> attendances { get; set; }

        public DbSet<AttendanceStatusModel> attendance_status { get; set; }

        public DbSet<BonusModel> bonuses { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Define the Departments table
            modelBuilder.Entity<departmentsModel>(entity =>
            {
                entity.ToTable("departments");
                entity.HasKey(e => e.department_id);
                entity.Property(e => e.department_id).HasColumnName("department_id");
                entity.Property(e => e.department_name).HasColumnName("department_name").IsRequired();
            });

            // Define the RoleTable
            modelBuilder.Entity<RoleModel>(entity =>
            {
                entity.ToTable("role");
                entity.HasKey(e => e.Role_Id);
                entity.Property(e => e.Role_Id).HasColumnName("role_id");
                entity.Property(e => e.Role_Name).HasColumnName("role_name").IsRequired();
            });

            // Define the Employees table with foreign keys
            modelBuilder.Entity<EmployeesModel>(entity =>
            {
                entity.ToTable("employees");
                entity.HasKey(e => e.employee_id);
                entity.Property(e => e.employee_id).HasColumnName("employee_id");
                entity.Property(e => e.first_name).HasColumnName("first_name").IsRequired();
                entity.Property(e => e.last_name).HasColumnName("last_name").IsRequired();
                entity.Property(e => e.email).HasColumnName("email");
                entity.Property(e => e.phone).HasColumnName("phone").IsRequired();
                entity.Property(e => e.hashed_password).HasColumnName("hashed_password").IsRequired();
                entity.Property(e => e.date_of_birth).HasColumnName("date_of_birth");
                entity.Property(e => e.gender).HasColumnName("gender");
                entity.Property(e => e.hire_date).HasColumnName("hire_date").IsRequired();
                entity.Property(e => e.expired_date).HasColumnName("expired_date");
                entity.Property(e => e.position).HasColumnName("position");
                entity.Property(e => e.department_id).HasColumnName("department_id");
                entity.Property(e => e.role_id).HasColumnName("role_id").HasDefaultValue(1);

                // Foreign key to Department
                entity.HasOne(e => e.departments)
                      .WithMany()
                      .HasForeignKey(e => e.department_id)
                      .OnDelete(DeleteBehavior.Cascade);

                // Foreign key to RoleTable
                entity.HasOne(e => e.Role)
                      .WithMany()
                      .HasForeignKey(e => e.role_id);
            });

            // Define the Salaries table with foreign key to Employees
            modelBuilder.Entity<SalaryModel>(entity =>
            {
                entity.ToTable("salaries");
                entity.HasKey(e => e.Salary_Id);
                entity.Property(e => e.Salary_Id).HasColumnName("salary_id");
                entity.Property(e => e.Employee_Id).HasColumnName("employee_id");
                entity.Property(e => e.Base_Salary).HasColumnName("base_salary");
                entity.Property(e => e.Bonus).HasColumnName("bonus");
                entity.Property(e => e.Total_Salary).HasColumnName("total_salary");
                entity.Property(e => e.Salary_Date).HasColumnName("salary_date");

                // Foreign key to Employee
                entity.HasOne(e => e.Employee)
                      .WithMany()
                      .HasForeignKey(e => e.Employee_Id)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Define the Positions table with foreign key to Departments
            modelBuilder.Entity<PositionModel>(entity =>
            {
                entity.ToTable("positions");
                entity.HasKey(e => e.position_id);
                entity.Property(e => e.position_id).HasColumnName("position_id");
                entity.Property(e => e.position_name).HasColumnName("position_name");
                entity.Property(e => e.base_salary).HasColumnName("base_salary");
                entity.Property(e => e.department_id).HasColumnName("department_id");

                // Foreign key to Department
                entity.HasOne(e => e.departments)
                      .WithMany()
                      .HasForeignKey(e => e.department_id)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Define the Bonuses table with foreign key to Employees
            modelBuilder.Entity<BonusModel>(entity =>
            {
                entity.ToTable("bonuses");
                entity.HasKey(e => e.Bonus_Id);
                entity.Property(e => e.Bonus_Id).HasColumnName("bonus_id");
                entity.Property(e => e.Employee_Id).HasColumnName("employee_id");
                entity.Property(e => e.Bonus_Amount).HasColumnName("bonus_amount").IsRequired();
                entity.Property(e => e.Bonus_Date).HasColumnName("bonus_date").IsRequired();
                entity.Property(e => e.Reason).HasColumnName("reason").IsRequired();

                // Foreign key to Employee
                entity.HasOne(e => e.Employee)
                      .WithMany()
                      .HasForeignKey(e => e.Employee_Id)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Define the Attendances table with foreign key to Employees
            modelBuilder.Entity<AttendanceModel>(entity =>
            {
                entity.ToTable("attendances");
                entity.HasKey(e => e.Attendance_Id);
                entity.Property(e => e.Attendance_Id).HasColumnName("attendance_id");
                entity.Property(e => e.Employee_Id).HasColumnName("employee_id").IsRequired();
                entity.Property(e => e.Attendance_Date).HasColumnName("attendance_date").IsRequired();
                entity.Property(e => e.status_id).HasColumnName("status_id").IsRequired();

                // Foreign key to Employee
                entity.HasOne(e => e.Employee)
                      .WithMany()
                      .HasForeignKey(e => e.Employee_Id)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.AttendanceStatus)
                      .WithMany()
                      .HasForeignKey(e => e.status_id);
            });

            // Define the Deductions table with foreign key to Employees
            modelBuilder.Entity<DeductionModel>(entity =>
            {
                entity.ToTable("deductions");
                entity.HasKey(e => e.Deduction_Id);
                entity.Property(e => e.Deduction_Id).HasColumnName("deduction_id");
                entity.Property(e => e.Employee_Id).HasColumnName("employee_id").IsRequired();
                entity.Property(e => e.Deduction_Amount).HasColumnName("deduction-amount").IsRequired();
                entity.Property(e => e.Deduction_Date).HasColumnName("deduction_date").IsRequired();
                entity.Property(e => e.Reason).HasColumnName("reason").IsRequired();

                // Foreign key to Employee
                entity.HasOne(e => e.Employee)
                      .WithMany()
                      .HasForeignKey(e => e.Employee_Id)
                      .OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<AttendanceStatusModel>(entity =>
            {
                entity.ToTable("attendance_status");
                entity.HasKey(e => e.status_id);
                entity.Property(e => e.status_id).HasColumnName("status_id");
                entity.Property(e => e.status_name).HasColumnName("status_name");
            });
        }

    }
}
