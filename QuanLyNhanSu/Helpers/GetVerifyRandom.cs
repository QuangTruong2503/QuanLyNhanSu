namespace QuanLyNhanSu.Helpers
{
    public class GetVerifyRandom
    {
        public static string GetVerificationCode()
        {
            // Define the character set for the verification code
            char[] chArray = "abcdefghijklmnopqrstuvwxyz1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            string str = string.Empty;
            Random random = new Random();

            // Generate a 10-character verification code
            for (int i = 0; i < 10; i++)
            {
                int index = random.Next(0, chArray.Length);

                // Ensure uniqueness of characters in the verification code
                if (!str.Contains(chArray[index].ToString()))
                {
                    str += chArray[index];
                }
                else
                {
                    i--; // Decrement the index to repeat the selection process
                }
            }
            return str;
        }

        public static string GenerateOTP()
        {
            const string characters = "1234567890";
            string otp = string.Empty;
            Random random = new Random();

            // Generate a 6-digit one-time password (OTP)
            for (int i = 0; i < 6; i++)
            {
                int index = random.Next(0, characters.Length);
                otp += characters[index];
            }

            return otp;
        }
    }
}
