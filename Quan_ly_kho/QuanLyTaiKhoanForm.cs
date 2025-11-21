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
    public partial class QuanLyTaiKhoanForm : Form
    {
        private const string Schema = "C##QUAN_LY_KHO";

        public QuanLyTaiKhoanForm()
        {
            this.Text = "Quản Trị Hệ Thống";
            this.ClientSize = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            InitializeAdminUI();

            // Đảm bảo Form chỉ dành cho ADMIN (Kiểm tra bảo mật lần nữa)
            if (Quan_ly_kho.SessionManager.CurrentRole != "RL_ADMIN")
            {
                MessageBox.Show("Truy cập bị từ chối. Form này chỉ dành cho Quản trị viên.", "Lỗi Bảo mật", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                this.Close();
            }
        }

        private void InitializeAdminUI()
        {
            // [Giữ nguyên code InitializeAdminUI]
            Label lblTitle = new Label();
            lblTitle.Text = "Quản Trị Hệ Thống";
            lblTitle.Font = new Font("Arial", 24, FontStyle.Bold);
            lblTitle.Location = new Point(250, 20);
            lblTitle.AutoSize = true;
            this.Controls.Add(lblTitle);

            Button btnLogout = new Button();
            btnLogout.Text = "Đăng xuất";
            btnLogout.Location = new Point(680, 20);
            btnLogout.Click += new EventHandler(this.btnLogout_Click);
            this.Controls.Add(btnLogout);

            Panel pnlMenu = new Panel();
            pnlMenu.BackColor = Color.LightGray;
            pnlMenu.Location = new Point(10, 60);
            pnlMenu.Size = new Size(150, 500);
            this.Controls.Add(pnlMenu);

            Button btnQuanLyTaiKhoan = new Button();
            btnQuanLyTaiKhoan.Text = "Quản lý tài khoản";
            btnQuanLyTaiKhoan.Location = new Point(10, 20);
            btnQuanLyTaiKhoan.Size = new Size(130, 40);
            btnQuanLyTaiKhoan.Click += new EventHandler(this.btnQuanLyTaiKhoan_Click);
            pnlMenu.Controls.Add(btnQuanLyTaiKhoan);

            Panel pnlMainContent = new Panel();
            pnlMainContent.Name = "pnlMainContent";
            pnlMainContent.BorderStyle = BorderStyle.FixedSingle;
            pnlMainContent.Location = new Point(170, 60);
            pnlMainContent.Size = new Size(600, 500);
            this.Controls.Add(pnlMainContent);
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Bạn có muốn đăng xuất không?", "Đăng xuất", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.Yes)
            {
                this.Close();
                Application.Restart();
            }
        }

        private void btnQuanLyTaiKhoan_Click(object sender, EventArgs e)
        {
            Panel pnlMainContent = (Panel)this.Controls.Find("pnlMainContent", true)[0];
            pnlMainContent.Controls.Clear();

            TabControl tabControl = new TabControl();
            tabControl.Dock = DockStyle.Fill;
            pnlMainContent.Controls.Add(tabControl);

            TabPage tabResetPassword = new TabPage("Cấp lại mật khẩu");
            tabControl.Controls.Add(tabResetPassword);
            InitializeResetPasswordTab(tabResetPassword);

            TabPage tabApproveAccount = new TabPage("Duyệt tài khoản");
            tabControl.Controls.Add(tabApproveAccount);
            InitializeApproveAccountTab(tabApproveAccount);
        }

        private void InitializeResetPasswordTab(TabPage tab)
        {
            // [Giữ nguyên code tạo UI Cấp lại mật khẩu]
            Label lblHeader = new Label(); lblHeader.Text = "Cấp lại mật khẩu"; lblHeader.Font = new Font("Arial", 16, FontStyle.Bold); lblHeader.Location = new Point(20, 20); lblHeader.AutoSize = true; tab.Controls.Add(lblHeader);
            Label lblUsername = new Label(); lblUsername.Text = "Tên đăng nhập:"; lblUsername.Location = new Point(20, 80); lblUsername.AutoSize = true; tab.Controls.Add(lblUsername);
            TextBox txtUsername = new TextBox(); txtUsername.Name = "txtUsername"; txtUsername.Location = new Point(150, 80); txtUsername.Size = new Size(200, 20); tab.Controls.Add(txtUsername);
            Label lblNewPassword = new Label(); lblNewPassword.Text = "Mật khẩu mới:"; lblNewPassword.Location = new Point(20, 120); lblNewPassword.AutoSize = true; tab.Controls.Add(lblNewPassword);
            TextBox txtNewPassword = new TextBox(); txtNewPassword.Name = "txtNewPassword"; txtNewPassword.Location = new Point(150, 120); txtNewPassword.Size = new Size(200, 20); txtNewPassword.PasswordChar = '*'; tab.Controls.Add(txtNewPassword);
            Button btnResetPassword = new Button(); btnResetPassword.Text = "Cấp lại mật khẩu"; btnResetPassword.Location = new Point(150, 160); btnResetPassword.Size = new Size(150, 30); btnResetPassword.Click += new EventHandler(this.btnResetPassword_Click); tab.Controls.Add(btnResetPassword);
        }

        private void btnResetPassword_Click(object sender, EventArgs e)
        {
            TabPage currentTab = (TabPage)((Button)sender).Parent;
            TextBox txtUsername = (TextBox)currentTab.Controls.Find("txtUsername", false)[0];
            TextBox txtNewPassword = (TextBox)currentTab.Controls.Find("txtNewPassword", false)[0];

            string username = txtUsername.Text.Trim().ToUpper(); // Chuyển sang HOA để khớp với CSDL
            string newPassword = txtNewPassword.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(newPassword))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ Tên đăng nhập và Mật khẩu mới.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                ResetPasswordForUser(username, newPassword);
                MessageBox.Show($"Mật khẩu cho tài khoản '{username}' đã được cấp lại thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                txtUsername.Clear();
                txtNewPassword.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể cấp lại mật khẩu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ResetPasswordForUser(string username, string newPassword)
        {
            // Tạo Salt và Hash mới
            string newSalt = Guid.NewGuid().ToString().Substring(0, 10);
            string newHashedPassword = Quan_ly_kho.Utilities.HashPassword(newPassword, newSalt); // Sử dụng Utilities.HashPassword

            // Kết nối bằng tài khoản ứng dụng chung
            string connString = Quan_ly_kho.Utilities.GetConnectionString(Quan_ly_kho.SessionManager.AppUsername, Quan_ly_kho.SessionManager.AppPassword);

            using (OracleConnection conn = new OracleConnection(connString))
            {
                conn.Open();
                // Kích hoạt ROLE ADMIN (Cần quyền này để sửa bảng TAIKHOAN)
                string setRoleCommand = $"SET ROLE {Quan_ly_kho.SessionManager.CurrentRole} IDENTIFIED BY AdminPass2025";
                using (OracleCommand cmdSetRole = new OracleCommand(setRoleCommand, conn))
                {
                    cmdSetRole.ExecuteNonQuery();
                }

                // SỬA ĐỔI: Thêm tiền tố Schema
                string query = $"UPDATE {Schema}.TAIKHOAN SET PasswordHash = :hash, Salt = :salt WHERE Username = :username";
                using (OracleCommand cmd = new OracleCommand(query, conn))
                {
                    cmd.Parameters.Add(new OracleParameter("hash", newHashedPassword.ToLower())); // Mã Hash luôn lưu trữ dạng lowercase
                    cmd.Parameters.Add(new OracleParameter("salt", newSalt));
                    cmd.Parameters.Add(new OracleParameter("username", username));
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        throw new Exception("Không tìm thấy tên đăng nhập.");
                    }
                }
            }
        }

        private void InitializeApproveAccountTab(TabPage tab)
        {
            // [Giữ nguyên code tạo UI Duyệt tài khoản]
            Label lblHeader = new Label(); lblHeader.Text = "Duyệt tài khoản mới và phân quyền"; lblHeader.Font = new Font("Arial", 16, FontStyle.Bold); lblHeader.Location = new Point(20, 20); lblHeader.AutoSize = true; tab.Controls.Add(lblHeader);
            DataGridView dgvPendingUsers = new DataGridView(); dgvPendingUsers.Name = "dgvPendingUsers"; dgvPendingUsers.Location = new Point(20, 60); dgvPendingUsers.Size = new Size(550, 300); dgvPendingUsers.ReadOnly = true; dgvPendingUsers.AutoGenerateColumns = true; dgvPendingUsers.SelectionMode = DataGridViewSelectionMode.FullRowSelect; tab.Controls.Add(dgvPendingUsers);
            Label lblRole = new Label(); lblRole.Text = "Chọn vai trò:"; lblRole.Location = new Point(20, 380); lblRole.AutoSize = true; tab.Controls.Add(lblRole);
            ComboBox cbxRole = new ComboBox(); cbxRole.Name = "cbxRole"; cbxRole.Location = new Point(120, 375); cbxRole.Size = new Size(150, 20);

            // SỬA ĐỔI: Sử dụng tên ROLE CSDL
            cbxRole.Items.AddRange(new object[] { "RL_THUKHO", "RL_KETOAN", "RL_ADMIN" });
            cbxRole.SelectedIndex = 0;
            tab.Controls.Add(cbxRole);

            Button btnApprove = new Button(); btnApprove.Text = "Duyệt và phân quyền"; btnApprove.Location = new Point(20, 420); btnApprove.Size = new Size(150, 30); btnApprove.Click += new EventHandler(this.btnApprove_Click); tab.Controls.Add(btnApprove);

            LoadPendingUsers(dgvPendingUsers);
        }

        private void LoadPendingUsers(DataGridView dgv)
        {
            // Kết nối bằng tài khoản ứng dụng chung
            string connString = Quan_ly_kho.Utilities.GetConnectionString(Quan_ly_kho.SessionManager.AppUsername, Quan_ly_kho.SessionManager.AppPassword);

            using (OracleConnection conn = new OracleConnection(connString))
            {
                try
                {
                    conn.Open();
                    // Kích hoạt ROLE ADMIN để có quyền SELECT TAIKHOAN
                    string setRoleCommand = $"SET ROLE {Quan_ly_kho.SessionManager.CurrentRole} IDENTIFIED BY AdminPass2025";
                    using (OracleCommand cmdSetRole = new OracleCommand(setRoleCommand, conn))
                    {
                        cmdSetRole.ExecuteNonQuery();
                    }

                    // SỬA ĐỔI: Thêm tiền tố Schema
                    string query = $"SELECT UserID, Username, TrangThai FROM {Schema}.TAIKHOAN WHERE TrangThai = 'CHO_DUYET'";
                    using (OracleCommand cmd = new OracleCommand(query, conn))
                    {
                        using (OracleDataAdapter adapter = new OracleDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);
                            dgv.DataSource = dt;
                            dgv.Columns["UserID"].HeaderText = "ID";
                            dgv.Columns["Username"].HeaderText = "Tên Đăng nhập";
                            dgv.Columns["TrangThai"].Visible = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi tải danh sách tài khoản chờ duyệt: " + ex.Message, "Lỗi DB", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnApprove_Click(object sender, EventArgs e)
        {
            TabPage currentTab = (TabPage)((Button)sender).Parent;
            DataGridView dgvPendingUsers = (DataGridView)currentTab.Controls.Find("dgvPendingUsers", false)[0];
            ComboBox cbxRole = (ComboBox)currentTab.Controls.Find("cbxRole", false)[0];

            if (dgvPendingUsers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn một tài khoản để duyệt.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (cbxRole.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn vai trò.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string userId = dgvPendingUsers.SelectedRows[0].Cells["UserID"].Value.ToString();
            string selectedRole = cbxRole.SelectedItem.ToString();

            try
            {
                ApproveAccountAndAssignRole(userId, selectedRole);
                MessageBox.Show($"Tài khoản {userId} đã được duyệt và phân quyền '{selectedRole}' thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadPendingUsers(dgvPendingUsers); // Tải lại lưới
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể duyệt tài khoản: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApproveAccountAndAssignRole(string userId, string role)
        {
            // Kết nối bằng tài khoản ứng dụng chung
            string connString = Quan_ly_kho.Utilities.GetConnectionString(Quan_ly_kho.SessionManager.AppUsername, Quan_ly_kho.SessionManager.AppPassword);

            using (OracleConnection conn = new OracleConnection(connString))
            {
                conn.Open();
                // Kích hoạt ROLE ADMIN
                string setRoleCommand = $"SET ROLE {Quan_ly_kho.SessionManager.CurrentRole} IDENTIFIED BY AdminPass2025";
                using (OracleCommand cmdSetRole = new OracleCommand(setRoleCommand, conn))
                {
                    cmdSetRole.ExecuteNonQuery();
                }

                OracleTransaction transaction = conn.BeginTransaction();
                try
                {
                    // 1. Cập nhật trạng thái TAIKHOAN
                    string updateQuery = $"UPDATE {Schema}.TAIKHOAN SET TrangThai = 'DA_DUYET' WHERE UserID = :userId";
                    using (OracleCommand updateCmd = new OracleCommand(updateQuery, conn))
                    {
                        updateCmd.Parameters.Add(new OracleParameter("userId", userId));
                        updateCmd.Transaction = transaction;
                        updateCmd.ExecuteNonQuery();
                    }

                    // 2. Xóa vai trò cũ (đảm bảo sạch)
                    string deleteRoleQuery = $"DELETE FROM {Schema}.VAITRO WHERE UserID = :userId";
                    using (OracleCommand deleteCmd = new OracleCommand(deleteRoleQuery, conn))
                    {
                        deleteCmd.Parameters.Add(new OracleParameter("userId", userId));
                        deleteCmd.Transaction = transaction;
                        deleteCmd.ExecuteNonQuery();
                    }

                    // 3. Chèn vai trò mới
                    string insertRoleQuery = $"INSERT INTO {Schema}.VAITRO (VaiTro, UserID) VALUES (:role, :userId)";
                    using (OracleCommand insertCmd = new OracleCommand(insertRoleQuery, conn))
                    {
                        insertCmd.Parameters.Add(new OracleParameter("role", role));
                        insertCmd.Parameters.Add(new OracleParameter("userId", userId));
                        insertCmd.Transaction = transaction;
                        insertCmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    if (transaction != null) transaction.Rollback();
                    throw new Exception($"Lỗi thực hiện giao tác: {ex.Message}", ex); // Ném lỗi ra ngoài để hiển thị
                }
            }
        }
    }
}
