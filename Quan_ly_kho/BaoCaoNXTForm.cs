using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Quan_ly_kho
{
    public partial class BaoCaoNXTForm : Form
    {
        private DataGridView dgvReport;
        private Button btnLoadReport;
        private ComboBox cbxReportType;
        private Label lblTitle;

        // Định nghĩa hằng số CSDL
        private const string Schema = "C##QUAN_LY_KHO";
        private const string AppUsername = "C##QUAN_LY_KHO";
        private const string AppPassword = "123";

        public BaoCaoNXTForm()
        {
            this.Text = "Báo cáo Nhập - Xuất - Tồn";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            InitializeUI();
            CheckRolePermissions();
        }

        private void CheckRolePermissions()
        {
            // Chỉ RL_ADMIN và RL_KETOAN mới có quyền xem báo cáo
            if (SessionManager.CurrentRole != "RL_ADMIN" && SessionManager.CurrentRole != "RL_KETOAN")
            {
                MessageBox.Show("Truy cập bị từ chối. Form này chỉ dành cho Quản trị viên và Kế toán.", "Lỗi Bảo mật", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                this.Close();
            }
        }

        private void InitializeUI()
        {
            // Tiêu đề
            lblTitle = new Label()
            {
                Text = "BÁO CÁO TỔNG HỢP NHẬP - XUẤT - TỒN",
                Location = new Point(20, 20),
                Font = new Font("Arial", 16, FontStyle.Bold),
                AutoSize = true
            };
            this.Controls.Add(lblTitle);

            // Chọn loại báo cáo
            this.Controls.Add(new Label() { Text = "Chọn loại báo cáo:", Location = new Point(20, 70), AutoSize = true });
            cbxReportType = new ComboBox()
            {
                Location = new Point(150, 68),
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cbxReportType.Items.AddRange(new object[] { "Tồn kho hiện tại", "Tổng hợp Nhập/Xuất theo Sản phẩm", "Chi tiết các Phiếu Nhập" });
            cbxReportType.SelectedIndex = 0;
            this.Controls.Add(cbxReportType);

            // Nút Tải báo cáo
            btnLoadReport = new Button()
            {
                Text = "Xem Báo cáo",
                Location = new Point(370, 65),
                Size = new Size(120, 30),
                BackColor = Color.LightYellow
            };
            btnLoadReport.Click += BtnLoadReport_Click;
            this.Controls.Add(btnLoadReport);

            // DataGridView hiển thị kết quả
            dgvReport = new DataGridView()
            {
                Location = new Point(20, 120),
                Size = new Size(950, 520),
                ReadOnly = true,
                AutoGenerateColumns = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            this.Controls.Add(dgvReport);
        }

        private void BtnLoadReport_Click(object sender, EventArgs e)
        {
            string reportType = cbxReportType.SelectedItem.ToString();
            lblTitle.Text = $"BÁO CÁO: {reportType.ToUpper()}";

            try
            {
                DataTable dt = new DataTable();

                switch (reportType)
                {
                    case "Tồn kho hiện tại":
                        dt = GetTonKhoHienTaiReport();
                        break;
                    case "Tổng hợp Nhập/Xuất theo Sản phẩm":
                        dt = GetNXTTheoSanPhamReport();
                        break;
                    case "Chi tiết các Phiếu Nhập":
                        dt = GetChiTietPhieuNhapReport();
                        break;
                    default:
                        throw new Exception("Loại báo cáo không hợp lệ.");
                }

                dgvReport.DataSource = dt;
                FormatReportGrid(reportType);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải báo cáo: {ex.Message}", "Lỗi CSDL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                dgvReport.DataSource = null; // Xóa dữ liệu cũ
            }
        }

        private void FormatReportGrid(string reportType)
        {
            // Thiết lập tên cột thân thiện
            switch (reportType)
            {
                case "Tồn kho hiện tại":
                    dgvReport.Columns["MaSP"].HeaderText = "Mã SP";
                    dgvReport.Columns["TenSP"].HeaderText = "Tên Sản phẩm";
                    dgvReport.Columns["SoLuongTon"].HeaderText = "Tồn kho";
                    dgvReport.Columns["GiaNhap"].HeaderText = "Giá Nhập";
                    dgvReport.Columns["GiaBan"].HeaderText = "Giá Bán";
                    break;
                case "Tổng hợp Nhập/Xuất theo Sản phẩm":
                    dgvReport.Columns["MaSP"].HeaderText = "Mã SP";
                    dgvReport.Columns["TONG_NHAP"].HeaderText = "Tổng SL Nhập";
                    dgvReport.Columns["TONG_XUAT"].HeaderText = "Tổng SL Xuất";
                    dgvReport.Columns["TONG_TON"].HeaderText = "Tổng Tồn (Cuối kỳ)";
                    break;
                case "Chi tiết các Phiếu Nhập":
                    dgvReport.Columns["MaPN"].HeaderText = "Mã Phiếu Nhập";
                    dgvReport.Columns["NgayNhap"].HeaderText = "Ngày Nhập";
                    dgvReport.Columns["NhanVienNhap"].HeaderText = "Mã NV";
                    dgvReport.Columns["MaSP"].HeaderText = "Mã SP";
                    dgvReport.Columns["SoLuong"].HeaderText = "SL Nhập";
                    dgvReport.Columns["DonGia"].HeaderText = "Đơn Giá";
                    break;
            }
            dgvReport.AutoResizeColumns();
        }

        // --- Các Phương thức Truy vấn CSDL ---

        private OracleConnection GetSecureConnection()
        {
            // Sử dụng tài khoản ứng dụng để kết nối và kích hoạt ROLE
            string connString = Quan_ly_kho.Utilities.GetConnectionString(AppUsername, AppPassword);
            OracleConnection conn = new OracleConnection(connString);

            try
            {
                conn.Open();

                string currentRole = Quan_ly_kho.SessionManager.CurrentRole;
                string rolePassword = (currentRole == "RL_KETOAN") ? "KetoanPass2025" : "AdminPass2025";
                string setRoleCommand = $"SET ROLE {currentRole} IDENTIFIED BY {rolePassword}";

                using (OracleCommand cmd = new OracleCommand(setRoleCommand, conn))
                {
                    cmd.ExecuteNonQuery();
                }
                return conn;
            }
            catch (OracleException ex)
            {
                // Xử lý lỗi phân quyền ngay tại đây
                MessageBox.Show($"Lỗi kết nối bảo mật (SET ROLE): {ex.Message}", "Lỗi CSDL");
                conn.Close();
                throw;
            }
        }

        private DataTable GetTonKhoHienTaiReport()
        {
            // Lấy trực tiếp từ bảng SANPHAM (tồn kho hiện tại)
            // RL_KETOAN cần quyền SELECT trên SANPHAM
            string query = $"SELECT MaSP, TenSP, SoLuongTon, GiaNhap, GiaBan FROM {Schema}.SANPHAM ORDER BY MaSP";
            DataTable dt = new DataTable();

            using (OracleConnection conn = GetSecureConnection())
            using (OracleDataAdapter adapter = new OracleDataAdapter(query, conn))
            {
                adapter.Fill(dt);
            }
            return dt;
        }

        private DataTable GetNXTTheoSanPhamReport()
        {
            // Truy vấn tổng hợp Nhập/Xuất/Tồn (Sử dụng VIEW hoặc Truy vấn phức tạp)
            // Giả định có view tổng hợp: view_BC_NHAPXUAT_TON
            string query = $"SELECT * FROM {Schema}.view_BC_NHAPXUAT_TON ORDER BY MaSP";
            DataTable dt = new DataTable();

            using (OracleConnection conn = GetSecureConnection())
            using (OracleDataAdapter adapter = new OracleDataAdapter(query, conn))
            {
                adapter.Fill(dt);
            }
            return dt;
        }

        private DataTable GetChiTietPhieuNhapReport()
        {
            // Chi tiết phiếu nhập (Kế toán cần xem)
            // RL_KETOAN cần quyền SELECT trên PHIEUNHAP và CT_PHIEUNHAP
            string query = $@"
                SELECT P.MaPN, P.NgayNhap, P.NhanVienNhap, C.MaSP, C.SoLuong, C.DonGia
                FROM {Schema}.PHIEUNHAP P
                JOIN {Schema}.CT_PHIEUNHAP C ON P.MaPN = C.MaPN
                ORDER BY P.NgayNhap DESC, P.MaPN, C.MaSP";
            DataTable dt = new DataTable();

            using (OracleConnection conn = GetSecureConnection())
            using (OracleDataAdapter adapter = new OracleDataAdapter(query, conn))
            {
                adapter.Fill(dt);
            }
            return dt;
        }
    }
}