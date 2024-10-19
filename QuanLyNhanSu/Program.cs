using Microsoft.EntityFrameworkCore;
using QuanLyNhanSu.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
// Lấy chứng chỉ SSL từ biến môi trường
var sslCaCert = Environment.GetEnvironmentVariable("SSL_CA_CERT");
if (!string.IsNullOrEmpty(sslCaCert))
{
    // Tạo file tạm thời chứa nội dung chứng chỉ
    var caCertPath = "/tmp/ca.pem";  // Đường dẫn tạm thời
    System.IO.File.WriteAllText(caCertPath, sslCaCert);

    // Cập nhật chuỗi kết nối MySQL với đường dẫn chứng chỉ
    string connectionString = $"Server=earphone-store-quangtruong2503-a8ea.h.aivencloud.com;Port=22588;Database=QuanLyNhanSu;User=avnadmin;Password=AVNS_fkmO9iJEsLBXaYqmoUF;SslMode=REQUIRED;SslCa={caCertPath};";

    // Cấu hình DbContext với chuỗi kết nối MySQL
    builder.Services.AddDbContext<QuanLyNhanSuDbContext>(options =>
        options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
}
else
{
    builder.Services.AddDbContext<QuanLyNhanSuDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("MysqlConnection"),
        new MySqlServerVersion(new Version(8, 0, 29))));
}    


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Login}/{id?}");

app.Run();
