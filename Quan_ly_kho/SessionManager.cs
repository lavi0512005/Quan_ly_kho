using System;
using Oracle.ManagedDataAccess.Client;


namespace Quan_ly_kho
{

    public static class SessionManager
    {
        public static string CurrentUserID { get; private set; }
        public static string CurrentUsername { get; private set; }
        public static string CurrentRole { get; private set; } // Vai trò chính (RL_ADMIN, RL_THUKHO, RL_KETOAN)

        // TÀI KHOẢN ỨNG DỤNG ĐỂ THỰC THI SET ROLE (C##QUAN_LY_KHO)
        public const string AppUsername = "C##QUAN_LY_KHO"; 
        public const string AppPassword = "123";           
        private const string Schema = "C##QUAN_LY_KHO";

        // Mật khẩu các ROLE (Cần khớp với ALTER ROLE ... IDENTIFIED BY ...)
        private static readonly System.Collections.Generic.Dictionary<string, string> RolePasswords =
            new System.Collections.Generic.Dictionary<string, string>
        {
        {"RL_ADMIN", "AdminPass2025"},
        {"RL_THUKHO", "KhoPass2025"},
        {"RL_KETOAN", "KetoanPass2025"}
        };

        // Bước 1: Xác thực mật khẩu băm và lấy thông tin UserID/Salt
        private static bool VerifyUserCredentials(string username, string password, out string userID, out string mainRole)
        {
            userID = null;
            mainRole = null;
            // Sử dụng tài khoản ứng dụng (C##QUAN_LY_KHO) để truy vấn thông tin xác thực
            string connString = Utilities.GetConnectionString(AppUsername, AppPassword);

            using (OracleConnection conn = new OracleConnection(connString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT UserID, PasswordHash, Salt, TrangThai FROM TAIKHOAN WHERE Username = :p_username";

                    using (OracleCommand cmd = new OracleCommand(query, conn))
                    {
                        cmd.Parameters.Add("p_username", OracleDbType.Varchar2).Value = username.ToUpper();

                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string dbUserID = reader["UserID"].ToString();
                                string dbHash = reader["PasswordHash"].ToString();
                                string dbSalt = reader["Salt"].ToString();
                                string trangThai = reader["TrangThai"].ToString();

                                if (trangThai != "DUYET")
                                {
                                    System.Windows.Forms.MessageBox.Show($"Tài khoản {trangThai.ToLower()} hoặc chưa được duyệt.");
                                    return false;
                                }

                                string inputHash = Utilities.HashPassword(password, dbSalt);

                                if (inputHash.Equals(dbHash, StringComparison.OrdinalIgnoreCase))
                                {
                                    userID = dbUserID;
                                    CurrentUsername = username;
                                    mainRole = GetUserRole(conn, dbUserID);
                                    return true;
                                }
                            }
                            System.Windows.Forms.MessageBox.Show("Tên đăng nhập hoặc mật khẩu không đúng.");
                            return false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show("Lỗi kết nối CSDL trong quá trình xác thực: " + ex.Message);
                    return false;
                }
            }
        }

        // Lấy ROLE chính của người dùng
        private static string GetUserRole(OracleConnection conn, string userID)
        {
            string roleQuery = "SELECT VAITRO FROM VAITRO WHERE UserID = :p_userID AND ROWNUM = 1";

            using (OracleCommand cmd = new OracleCommand(roleQuery, conn))
            {
                cmd.Parameters.Add("p_userID", OracleDbType.Varchar2).Value = userID;
                object result = cmd.ExecuteScalar();
                return result?.ToString();
            }
        }

        // Bước 2: Kích hoạt ROLE cho phiên làm việc
        public static bool ActivateRoleSession()
        {
            if (string.IsNullOrEmpty(CurrentRole) || !RolePasswords.ContainsKey(CurrentRole))
            {
                System.Windows.Forms.MessageBox.Show("Không thể xác định hoặc kích hoạt ROLE.");
                return false;
            }

            string rolePassword = RolePasswords[CurrentRole];
            string setRoleCommand = $"SET ROLE {CurrentRole} IDENTIFIED BY {rolePassword}";

            string connString = Utilities.GetConnectionString(AppUsername, AppPassword);
            using (OracleConnection conn = new OracleConnection(connString))
            {
                try
                {
                    conn.Open();
                    using (OracleCommand cmd = new OracleCommand(setRoleCommand, conn))
                    {
                        cmd.ExecuteNonQuery();
                        // Nếu thành công, phiên kết nối đã được kích hoạt quyền ROLE tương ứng.
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show($"Lỗi kích hoạt ROLE ({CurrentRole}): " + ex.Message);
                    return false;
                }
            }
        }

        // Phương thức chính cho Đăng nhập
        public static bool Login(string username, string password)
        {
            string userID;
            string mainRole;

            // 1. Xác thực bằng Username/Password (sử dụng hashing)
            if (VerifyUserCredentials(username, password, out userID, out mainRole))
            {
                CurrentUserID = userID;
                CurrentRole = mainRole;

                // 2. Kích hoạt ROLE (SET ROLE)
                return ActivateRoleSession();
            }
            return false;
        }
    }
}
