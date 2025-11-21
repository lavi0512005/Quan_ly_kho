using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Quan_ly_kho
{
    public partial class Login : Form
    {
        public Login()
        {
            InitializeComponent();
            txtPassword.UseSystemPasswordChar = true;
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ Tài khoản và Mật khẩu.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Gọi phương thức Login trong SessionManager
            if (SessionManager.Login(username, password))
            {
                MessageBox.Show($"Đăng nhập thành công với vai trò: {SessionManager.CurrentRole}", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Mở Main Form
                MainForm mainForm = new MainForm();
                mainForm.Show();
                this.Hide();
            }
            // Nếu thất bại, SessionManager đã hiển thị thông báo lỗi chi tiết hơn
        }

        

    }
}
