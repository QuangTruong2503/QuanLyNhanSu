using System;
using QuanLyNhanSu.Helpers;
class Program
{
    static void Main(string[] args)
    {
        string passwordDatabase = "Ia8RfjT05lxrj0fxjZ5aJmYA7ixq+ZaiEGNvdtzk1mP7fmFh";
        string password = "MatKhauCuaBan";
        string hashedPassword = PasswordHasher.HashPassword(password);
        bool verifyPassword = PasswordHasher.VerifyPassword(password, passwordDatabase);
        Console.WriteLine($"Mật khẩu đã hash: {hashedPassword}");
        Console.WriteLine($"Kết quả kiểm tra mật khẩu: {verifyPassword}");
    }

}
