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
    public partial class TaoPhieuXuatForm : Form
    {
        // Khai báo Controls
        private TextBox txtMaPX, txtNV, txtMaNM, txtMaSP, txtSoLuong, txtDonGia;
        private DateTimePicker dtpNgayXuat;
        private Button btnOk;

        // Định nghĩa hằng số CSDL
        private const string Schema = "C##QUAN_LY_KHO";
        private const string AppUsername = "C##QUAN_LY_KHO";
        private const string AppPassword = "123";

        public TaoPhieuXuatForm()
        {
            this.StartPosition = FormStartPosition.CenterScreen;
            InitializeComponents();

            // Tự động điền Mã Nhân Viên (UserID của người đăng nhập)
            txtNV.Text = SessionManager.CurrentUserID;
            txtNV.ReadOnly = true;
        }

        private void InitializeComponents()
        {
            this.Text = "Tạo Phiếu Xuất Hàng Mới";
            this.Size = new Size(380, 480);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            int y = 20;
            int xLabel = 10;
            int xText = 120;
            int widthText = 200;

            // [PHẦN KHỞI TẠO CONTROLS - GIỮ NGUYÊN]
            // MaPX
            this.Controls.Add(new Label() { Text = "Mã Phiếu Xuất:", Location = new Point(xLabel, y) });
            txtMaPX = new TextBox() { Location = new Point(xText, y - 3), Width = widthText, Name = "txtMaPX" };
            this.Controls.Add(txtMaPX);

            y += 35;
            // NhanVienXuat (Mã Nhân Viên)
            this.Controls.Add(new Label() { Text = "Mã Nhân Viên:", Location = new Point(xLabel, y) });
            txtNV = new TextBox() { Location = new Point(xText, y - 3), Width = widthText, Name = "txtNV" };
            this.Controls.Add(txtNV);

            y += 35;
            // MaNM (Mã Người Mua)
            this.Controls.Add(new Label() { Text = "Mã Người Mua:", Location = new Point(xLabel, y) });
            txtMaNM = new TextBox() { Location = new Point(xText, y - 3), Width = widthText, Name = "txtMaNM" };
            this.Controls.Add(txtMaNM);

            y += 35;
            // NgayXuat (Ngày Xuất)
            this.Controls.Add(new Label() { Text = "Ngày Xuất:", Location = new Point(xLabel, y) });
            dtpNgayXuat = new DateTimePicker() { Location = new Point(xText, y - 3), Width = widthText, Format = DateTimePickerFormat.Short };
            this.Controls.Add(dtpNgayXuat);

            y += 35;
            this.Controls.Add(new Label() { Text = "--- Chi Tiết Sản Phẩm ---", Location = new Point(xLabel, y), Font = new Font(this.Font, FontStyle.Bold) });

            y += 35;
            // MaSP
            this.Controls.Add(new Label() { Text = "Mã Sản Phẩm:", Location = new Point(xLabel, y) });
            txtMaSP = new TextBox() { Location = new Point(xText, y - 3), Width = widthText, Name = "txtMaSP" };
            this.Controls.Add(txtMaSP);

            y += 35;
            // SoLuong (Số Lượng Xuất)
            this.Controls.Add(new Label() { Text = "Số Lượng Xuất:", Location = new Point(xLabel, y) });
            txtSoLuong = new TextBox() { Location = new Point(xText, y - 3), Width = widthText, Name = "txtSoLuong" };
            this.Controls.Add(txtSoLuong);

            y += 35;
            // DonGia (Đơn Giá Bán)
            this.Controls.Add(new Label() { Text = "Đơn Giá Bán:", Location = new Point(xLabel, y) });
            txtDonGia = new TextBox() { Location = new Point(xText, y - 3), Width = widthText, Name = "txtDonGia" };
            this.Controls.Add(txtDonGia);

            // Nút OK/Xuất
            btnOk = new Button() { Text = "Tạo Phiếu Xuất", Location = new Point(xText, y + 50), Size = new Size(120, 30) };
            btnOk.Click += BtnOk_Click;
            this.Controls.Add(btnOk);
        }

        private bool ValidateInput()
        {
            // [PHẦN VALIDATION - GIỮ NGUYÊN LOGIC]
            if (string.IsNullOrWhiteSpace(txtMaPX.Text) ||
                string.IsNullOrWhiteSpace(txtNV.Text) ||
                string.IsNullOrWhiteSpace(txtMaNM.Text) ||
                string.IsNullOrWhiteSpace(txtMaSP.Text) ||
                string.IsNullOrWhiteSpace(txtSoLuong.Text) ||
                string.IsNullOrWhiteSpace(txtDonGia.Text))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ Mã Phiếu, Nhân Viên, Mã Người Mua, Mã SP, Số Lượng và Đơn Giá.", "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!int.TryParse(txtSoLuong.Text, out int soLuong) || soLuong <= 0)
            {
                MessageBox.Show("Số Lượng Xuất phải là một số nguyên dương.", "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtSoLuong.Focus();
                return false;
            }

            if (!decimal.TryParse(txtDonGia.Text, out decimal donGia) || donGia <= 0)
            {
                MessageBox.Show("Đơn Giá Bán phải là một số dương.", "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtDonGia.Focus();
                return false;
            }

            return true;
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            if (!ValidateInput()) return;

            // Sử dụng Utilities để tạo chuỗi kết nối bảo mật
            string connString = Quan_ly_kho.Utilities.GetConnectionString(AppUsername, AppPassword);

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
                    string maPX = txtMaPX.Text.Trim();
                    string nvXuat = txtNV.Text.Trim();
                    string maNM = txtMaNM.Text.Trim();
                    DateTime ngayXuat = dtpNgayXuat.Value;
                    string maSP = txtMaSP.Text.Trim();
                    int soLuong = int.Parse(txtSoLuong.Text);
                    decimal donGia = decimal.Parse(txtDonGia.Text);

                    // 1. CHÈN VÀO PHIEUXUAT (Phần Header)
                    string insertPXQuery = $@"
                        INSERT INTO {Schema}.PHIEUXUAT (MaPX, NgayXuat, NhanVienXuat, MaNM)
                        VALUES (:MaPX, :NgayXuat, :NhanVienXuat, :MaNM)";

                    using (OracleCommand cmdPX = new OracleCommand(insertPXQuery, conn))
                    {
                        cmdPX.Transaction = transaction;
                        cmdPX.Parameters.Add(new OracleParameter("MaPX", maPX));
                        cmdPX.Parameters.Add(new OracleParameter("NgayXuat", ngayXuat));
                        cmdPX.Parameters.Add(new OracleParameter("NhanVienXuat", nvXuat));
                        cmdPX.Parameters.Add(new OracleParameter("MaNM", maNM));
                        cmdPX.ExecuteNonQuery();
                    }

                    // 2. CHÈN VÀO CT_PHIEUXUAT (Phần Chi Tiết)
                    // Thao tác INSERT này sẽ kích hoạt TRIGGER trg_ctpx_check_update_tonkho
                    string insertCTPXQuery = $@"
                        INSERT INTO {Schema}.CT_PHIEUXUAT (MaPX, MaSP, SoLuong, DonGia)
                        VALUES (:MaPX_CT, :MaSP, :SoLuong, :DonGia)";

                    using (OracleCommand cmdCTPX = new OracleCommand(insertCTPXQuery, conn))
                    {
                        cmdCTPX.Transaction = transaction;
                        cmdCTPX.Parameters.Add(new OracleParameter("MaPX_CT", maPX));
                        cmdCTPX.Parameters.Add(new OracleParameter("MaSP", maSP));
                        cmdCTPX.Parameters.Add(new OracleParameter("SoLuong", soLuong));
                        cmdCTPX.Parameters.Add(new OracleParameter("DonGia", donGia));
                        cmdCTPX.ExecuteNonQuery();
                    }

                    // 3. Commit Transaction
                    transaction.Commit();

                    MessageBox.Show("Tạo Phiếu Xuất thành công. Tồn kho đã được cập nhật.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                catch (OracleException ex)
                {
                    if (transaction != null) transaction.Rollback();

                    // Xử lý lỗi nghiệp vụ (trigger ném ra)
                    if (ex.Number == 20002)
                    {
                        MessageBox.Show($"Lỗi nghiệp vụ (Trigger): {ex.Message.Replace("-20002:", "")}", "Lỗi Tồn Kho", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    // Xử lý lỗi bảo mật
                    else if (ex.Number == 1031)
                    {
                        MessageBox.Show("LỖI BẢO MẬT: Bạn không có đủ quyền (ORA-01031). Hãy đảm bảo vai trò RL_THUKHO được cấp quyền INSERT.", "Lỗi Phân quyền", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        // Lỗi Khóa ngoại, trùng PK, v.v.
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
