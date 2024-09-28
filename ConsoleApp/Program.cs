using System;
using Newtonsoft.Json;
using QuanLyNhanSu.Helpers;
class Program
{
    public class Questions
    {
        public string question_text { get; set; }
        public List<string> answers { get; set; }
        public string correct_answer { get; set; }
    }

    static void Main(string[] args)
    {
        var currentDate = DateTime.Now; 
        var day = currentDate.Day;
        var month = currentDate.Month;
        var year = currentDate.Year;
        var password = $"{day}{month}{year}";
        Console.WriteLine(password);
        

    }

}
