using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Quan_ly_kho
{
    public class SanPhamService
    {
        public class SanPham
        {
            public string MaSP { get; set; }
            public string TenSP { get; set; }
            public string NhomSP { get; set; }
            public string DonViTinh { get; set; } // Đã thêm
            public decimal GiaNhap { get; set; }
            public decimal GiaBan { get; set; }
            public int SoLuongTon { get; set; }
        }
        private const string Schema = "C##QUAN_LY_KHO";

        // Phương thức mở kết nối bảo mật (sử dụng tài khoản ứng dụng và SET ROLE)
        private OracleConnection GetSecureConnection()
        {
            // Kết nối bằng App User (C##QUAN_LY_KHO)
            string connString = Quan_ly_kho.Utilities.GetConnectionString("C##QUAN_LY_KHO", "123");
            OracleConnection conn = new OracleConnection(connString);

            try
            {
                conn.Open();

                // Kích hoạt ROLE hiện tại (RL_THUKHO phải được cấp quyền DML)
                string currentRole = Quan_ly_kho.SessionManager.CurrentRole;
                string rolePassword = (currentRole == "RL_THUKHO") ? "KhoPass2025" : (currentRole == "RL_ADMIN" ? "AdminPass2025" : "");

                string setRoleCommand = $"SET ROLE {currentRole} IDENTIFIED BY {rolePassword}";

                using (OracleCommand cmd = new OracleCommand(setRoleCommand, conn))
                {
                    cmd.ExecuteNonQuery();
                }
                return conn;
            }
            catch (OracleException ex)
            {
                // Xử lý lỗi phân quyền ngay tại đây (ví dụ: mật khẩu role sai)
                MessageBox.Show($"Lỗi kết nối bảo mật (SET ROLE): {ex.Message}", "Lỗi CSDL");
                conn.Close();
                throw;
            }
        }

        public DataTable GetAll()
        {
            DataTable dt = new DataTable();
            string query = $"SELECT MaSP, TenSP, NhomSP, DonViTinh, GiaNhap, GiaBan, SoLuongTon FROM {Schema}.SANPHAM ORDER BY MaSP";

            using (OracleConnection conn = GetSecureConnection())
            using (OracleDataAdapter adapter = new OracleDataAdapter(query, conn))
            {
                adapter.Fill(dt);
            }
            return dt;
        }

        public bool Add(SanPham sp)
        {
            string query = $@"
                INSERT INTO {Schema}.SANPHAM (MaSP, TenSP, NhomSP, DonViTinh, GiaNhap, GiaBan, SoLuongTon)
                VALUES (:MaSP, :TenSP, :NhomSP, :DonViTinh, :GiaNhap, :GiaBan, :SoLuongTon)";

            using (OracleConnection conn = GetSecureConnection())
            using (OracleCommand cmd = new OracleCommand(query, conn))
            {
                cmd.Parameters.Add("MaSP", sp.MaSP);
                cmd.Parameters.Add("TenSP", sp.TenSP);
                cmd.Parameters.Add("NhomSP", sp.NhomSP);
                cmd.Parameters.Add("DonViTinh", sp.DonViTinh);
                cmd.Parameters.Add("GiaNhap", sp.GiaNhap);
                cmd.Parameters.Add("GiaBan", sp.GiaBan);
                cmd.Parameters.Add("SoLuongTon", sp.SoLuongTon);

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool Update(SanPham sp)
        {
            // Chỉ cho phép sửa các trường không liên quan trực tiếp đến nghiệp vụ tồn kho
            string query = $@"
                UPDATE {Schema}.SANPHAM SET
                    TenSP = :TenSP, NhomSP = :NhomSP, DonViTinh = :DonViTinh,
                    GiaNhap = :GiaNhap, GiaBan = :GiaBan
                WHERE MaSP = :MaSP";

            using (OracleConnection conn = GetSecureConnection())
            using (OracleCommand cmd = new OracleCommand(query, conn))
            {
                cmd.Parameters.Add("TenSP", sp.TenSP);
                cmd.Parameters.Add("NhomSP", sp.NhomSP);
                cmd.Parameters.Add("DonViTinh", sp.DonViTinh);
                cmd.Parameters.Add("GiaNhap", sp.GiaNhap);
                cmd.Parameters.Add("GiaBan", sp.GiaBan);
                cmd.Parameters.Add("MaSP", sp.MaSP);

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool Delete(string maSP)
        {
            string query = $"DELETE FROM {Schema}.SANPHAM WHERE MaSP = :MaSP";

            using (OracleConnection conn = GetSecureConnection())
            using (OracleCommand cmd = new OracleCommand(query, conn))
            {
                cmd.Parameters.Add("MaSP", maSP);
                return cmd.ExecuteNonQuery() > 0;
            }
        }
    }
}
