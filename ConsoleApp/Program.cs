using System;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Newtonsoft.Json;
using QuanLyNhanSu.Helpers;

class Program
{
    static void Main(string[] args)
    {
        var dateJoin = DateTime.Now;
        var dateLeave = DateTime.Now.AddHours(8);
        var workTime = dateLeave.Hour - dateJoin.Hour;
        Console.WriteLine(workTime);
    }

}
