using System;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Newtonsoft.Json;
using QuanLyNhanSu.Helpers;

class Program
{
    // Hàm tính số ngày giữa hai ngày a và b
    static int TinhSoNgay(DateTime ngayA, DateTime ngayB)
    {
        // Tính sự khác biệt giữa hai ngày
        TimeSpan khoangThoiGian = ngayB - ngayA;

        // Trả về số ngày dưới dạng số nguyên
        return Math.Abs(khoangThoiGian.Days);
    }
    static void Main(string[] args)
    {
        var dateStart = new DateTime(2024, 09, 11, 0, 0, 0, DateTimeKind.Utc);
        var dateLeave = new DateTime(2024, 10, 10, 0, 0, 0, DateTimeKind.Utc);
        var date = TinhSoNgay(dateStart, dateLeave);
        Console.WriteLine(date);
    }

}
