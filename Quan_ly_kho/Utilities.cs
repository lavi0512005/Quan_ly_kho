using System;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;
using Oracle.ManagedDataAccess.Client;

namespace Quan_ly_kho
{
    
    public static class Utilities
    {
        private static readonly string BaseConnectionString =
            ConfigurationManager.ConnectionStrings["OracleConnString"].ConnectionString;

        // Phương thức tạo chuỗi kết nối hoàn chỉnh với tên người dùng/mật khẩu
        public static string GetConnectionString(string username, string password)
        {
            // Sử dụng String.Format để điền Username ({0}) và Password ({1}) vào chuỗi kết nối
            return string.Format(BaseConnectionString, username, password);
        }

        // Phương thức Băm mật khẩu bằng SHA256 (Phải khớp với Oracle)
        public static string HashPassword(string password, string salt)
        {
            string combined = password + salt;
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // Băm chuỗi kết hợp
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(combined));

                // Chuyển kết quả băm sang chuỗi Hexadecimal (lowercase) để khớp với RAWTOHEX() của Oracle
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
