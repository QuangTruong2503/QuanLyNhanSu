using System.ComponentModel.DataAnnotations;

namespace QuanLyNhanSu.Models
{
    public class TokenAttendanceModel
    {
        [Key]
        public int Token_ID { get; set; }
        public required string Verify_Code { get; set; }
        public DateTime Expiration { get; set; }
    }
}
