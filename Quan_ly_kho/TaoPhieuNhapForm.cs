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
    public partial class TaoPhieuNhapForm : Form
    {
        // Khai báo Controls
        private TextBox txtMaPN, txtNV, txtMaNCC, txtMaSP, txtSoLuong, txtDonGia;
        private DateTimePicker dtpNgayNhap;
        private Button btnOk;

        // Định nghĩa hằng số CSDL
        private const string Schema = "C##QUAN_LY_KHO";

        public TaoPhieuNhapForm()
        {
            this.StartPosition = FormStartPosition.CenterScreen;
            InitializeComponents();

            // Tự động điền Mã Nhân Viên (UserID của người đăng nhập)
            // LƯU Ý: Phải đảm bảo CurrentUserID là string (hoặc gọi .ToString())
            txtNV.Text = SessionManager.CurrentUserID.ToString();
            txtNV.ReadOnly = true;
        }

        private void InitializeComponents()
        {
            this.Text = "Tạo Phiếu Nhập Hàng Mới";
            this.Size = new Size(380, 480);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            int y = 20;
            int xLabel = 10;
            int xText = 120;
            int widthText = 200;

            // MaPN
            this.Controls.Add(new Label() { Text = "Mã Phiếu Nhập:", Location = new Point(xLabel, y) });
            txtMaPN = new TextBox() { Location = new Point(xText, y - 3), Width = widthText, Name = "txtMaPN" };
            this.Controls.Add(txtMaPN);

            y += 35;
            // NhanVienNhap (Mã Nhân Viên)
            this.Controls.Add(new Label() { Text = "Mã Nhân Viên:", Location = new Point(xLabel, y) });
            txtNV = new TextBox() { Location = new Point(xText, y - 3), Width = widthText, Name = "txtNV" };
            this.Controls.Add(txtNV);

            y += 35;
            // MaNCC (Mã Nhà Cung Cấp)
            this.Controls.Add(new Label() { Text = "Mã Nhà CC:", Location = new Point(xLabel, y) });
            txtMaNCC = new TextBox() { Location = new Point(xText, y - 3), Width = widthText, Name = "txtMaNCC" };
            this.Controls.Add(txtMaNCC);

            y += 35;
            // NgayNhap (Ngày Nhập)
            this.Controls.Add(new Label() { Text = "Ngày Nhập:", Location = new Point(xLabel, y) });
            dtpNgayNhap = new DateTimePicker() { Location = new Point(xText, y - 3), Width = widthText, Format = DateTimePickerFormat.Short };
            this.Controls.Add(dtpNgayNhap);

            y += 35;
            this.Controls.Add(new Label() { Text = "--- Chi Tiết Sản Phẩm ---", Location = new Point(xLabel, y), Font = new Font(this.Font, FontStyle.Bold) });

            y += 35;
            // MaSP
            this.Controls.Add(new Label() { Text = "Mã Sản Phẩm:", Location = new Point(xLabel, y) });
            txtMaSP = new TextBox() { Location = new Point(xText, y - 3), Width = widthText, Name = "txtMaSP" };
            this.Controls.Add(txtMaSP);

            y += 35;
            // SoLuong (Số Lượng Nhập)
            this.Controls.Add(new Label() { Text = "Số Lượng Nhập:", Location = new Point(xLabel, y) });
            txtSoLuong = new TextBox() { Location = new Point(xText, y - 3), Width = widthText, Name = "txtSoLuong" };
            this.Controls.Add(txtSoLuong);

            y += 35;
            // DonGia (Đơn Giá Nhập)
            this.Controls.Add(new Label() { Text = "Đơn Giá Nhập:", Location = new Point(xLabel, y) });
            txtDonGia = new TextBox() { Location = new Point(xText, y - 3), Width = widthText, Name = "txtDonGia" };
            this.Controls.Add(txtDonGia);

            // Nút OK/Nhập
            btnOk = new Button() { Text = "Tạo Phiếu Nhập", Location = new Point(xText, y + 50), Size = new Size(120, 30), BackColor = Color.LightBlue };
            btnOk.Click += BtnOk_Click;
            this.Controls.Add(btnOk);
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtMaPN.Text) ||
                string.IsNullOrWhiteSpace(txtNV.Text) ||
                string.IsNullOrWhiteSpace(txtMaNCC.Text) ||
                string.IsNullOrWhiteSpace(txtMaSP.Text) ||
                string.IsNullOrWhiteSpace(txtSoLuong.Text) ||
                string.IsNullOrWhiteSpace(txtDonGia.Text))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ Mã Phiếu, Nhân Viên, Mã Nhà CC, Mã SP, Số Lượng và Đơn Giá.", "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!int.TryParse(txtSoLuong.Text, out int soLuong) || soLuong <= 0)
            {
                MessageBox.Show("Số Lượng Nhập phải là một số nguyên dương.", "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtSoLuong.Focus();
                return false;
            }

            if (!decimal.TryParse(txtDonGia.Text, out decimal donGia) || donGia <= 0)
            {
                MessageBox.Show("Đơn Giá Nhập phải là một số dương.", "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtDonGia.Focus();
                return false;
            }

            return true;
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            if (!ValidateInput()) return;

            string connString = Quan_ly_kho.Utilities.GetConnectionString(SessionManager.AppUsername, SessionManager.AppPassword);

            using (OracleConnection conn = new OracleConnection(connString))
            {
                OracleTransaction transaction = null;

                try
                {
                    conn.Open();

                    // BƯỚC 1: KÍCH HOẠT ROLE (SET ROLE)
                    // Sử dụng ROLE của người dùng hiện tại (RL_THUKHO)
                    string currentRole = Quan_ly_kho.SessionManager.CurrentRole;
                    string rolePassword = (currentRole == "RL_THUKHO") ? "KhoPass2025" : "AdminPass2025";
                    string setRoleCommand = $"SET ROLE {currentRole} IDENTIFIED BY {rolePassword}";

                    using (OracleCommand cmdSetRole = new OracleCommand(setRoleCommand, conn))
                    {
                        cmdSetRole.ExecuteNonQuery();
                    }

                    transaction = conn.BeginTransaction(IsolationLevel.ReadCommitted);

                    // Lấy dữ liệu từ controls
                    string maPN = txtMaPN.Text.Trim();
                    string nvNhap = txtNV.Text.Trim();
                    string maNCC = txtMaNCC.Text.Trim();
                    DateTime ngayNhap = dtpNgayNhap.Value;
                    string maSP = txtMaSP.Text.Trim();
                    int soLuong = int.Parse(txtSoLuong.Text);
                    decimal donGia = decimal.Parse(txtDonGia.Text);

                    // 1. CHÈN VÀO PHIEUNHAP (Phần Header)
                    string insertPNQuery = $@"
                        INSERT INTO {Schema}.PHIEUNHAP (MaPN, NgayNhap, NhanVienNhap, MaNCC)
                        VALUES (:MaPN, :NgayNhap, :NhanVienNhap, :MaNCC)";

                    using (OracleCommand cmdPN = new OracleCommand(insertPNQuery, conn))
                    {
                        cmdPN.Transaction = transaction;
                        cmdPN.Parameters.Add(new OracleParameter("MaPN", maPN));
                        cmdPN.Parameters.Add(new OracleParameter("NgayNhap", ngayNhap));
                        cmdPN.Parameters.Add(new OracleParameter("NhanVienNhap", nvNhap));
                        cmdPN.Parameters.Add(new OracleParameter("MaNCC", maNCC));
                        cmdPN.ExecuteNonQuery();
                    }

                    // 2. CHÈN VÀO CT_PHIEUNHAP (Phần Chi Tiết)
                    // Thao tác INSERT này sẽ kích hoạt TRIGGER trg_ctpn_update_tonkho (Làm tăng SL Tồn)
                    string insertCTPNQuery = $@"
                        INSERT INTO {Schema}.CT_PHIEUNHAP (MaPN, MaSP, SoLuong, DonGia)
                        VALUES (:MaPN_CT, :MaSP, :SoLuong, :DonGia)";

                    using (OracleCommand cmdCTPN = new OracleCommand(insertCTPNQuery, conn))
                    {
                        cmdCTPN.Transaction = transaction;
                        cmdCTPN.Parameters.Add(new OracleParameter("MaPN_CT", maPN));
                        cmdCTPN.Parameters.Add(new OracleParameter("MaSP", maSP));
                        cmdCTPN.Parameters.Add(new OracleParameter("SoLuong", soLuong));
                        cmdCTPN.Parameters.Add(new OracleParameter("DonGia", donGia));
                        cmdCTPN.ExecuteNonQuery();
                    }

                    // 3. Commit Transaction
                    transaction.Commit();

                    MessageBox.Show("Tạo Phiếu Nhập thành công. Tồn kho đã được cập nhật.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                catch (OracleException ex)
                {
                    if (transaction != null) transaction.Rollback();

                    // Xử lý lỗi bảo mật
                    if (ex.Number == 1031)
                    {
                        MessageBox.Show("LỖI BẢO MẬT: Bạn không có đủ quyền (ORA-01031). Hãy đảm bảo vai trò RL_THUKHO được cấp quyền INSERT.", "Lỗi Phân quyền", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    // Xử lý lỗi Khóa chính trùng lặp (trùng MaPN)
                    else if (ex.Number == 1)
                    {
                        MessageBox.Show($"Lỗi: Mã Phiếu Nhập '{txtMaPN.Text}' đã tồn tại.", "Lỗi Khóa chính", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        // Lỗi Khóa ngoại, v.v.
                        string errorMessage = $"Lỗi CSDL Oracle (Mã lỗi: {ex.Number}): {ex.Message.Substring(0, Math.Min(ex.Message.Length, 200))}...";
                        MessageBox.Show(errorMessage, "Lỗi CSDL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    if (transaction != null) transaction.Rollback();
                    MessageBox.Show("Lỗi không xác định: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}