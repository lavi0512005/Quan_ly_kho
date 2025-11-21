using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Quan_ly_kho.SanPhamService;

namespace Quan_ly_kho
{
    // Form này dùng để quản lý Danh mục Sản phẩm (Master Data)
    public partial class QuanLySanPhamForm : Form
    {
        private DataGridView dgvSanPham;
        private SanPhamService _sanPhamService;

        private TextBox txtMaSP, txtTenSP, txtNhomSP, txtDonViTinh, txtGiaNhap, txtGiaBan, txtSoLuongTon;
        private Button btnThem, btnSua, btnXoa, btnLuu;
        private Panel pnlInput;

        // Trạng thái đang Thêm (true) hay Sửa (false)
        private bool isAddingNew = false;

        public QuanLySanPhamForm()
        {
            this.Text = "Quản Lý Danh mục Sản phẩm";
            this.Size = new Size(950, 650);
            this.StartPosition = FormStartPosition.CenterScreen;

            _sanPhamService = new SanPhamService();

            InitializeUI();
            LoadData();
            SetInputState(false); // Khởi tạo ở trạng thái xem
            CheckRolePermissions();
        }

        private void CheckRolePermissions()
        {
            // Chỉ RL_ADMIN và RL_THUKHO mới có quyền quản lý sản phẩm
            if (SessionManager.CurrentRole != "RL_ADMIN" && SessionManager.CurrentRole != "RL_THUKHO")
            {
                MessageBox.Show("Truy cập bị từ chối. Form này chỉ dành cho Quản trị viên và Thủ kho.", "Lỗi Bảo mật", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                this.Close();
            }
        }

        private void InitializeUI()
        {
            // Thiết lập DataGridView
            dgvSanPham = new DataGridView();
            dgvSanPham.Name = "dgvSanPham";
            dgvSanPham.Location = new Point(10, 10);
            dgvSanPham.Size = new Size(580, 580);
            dgvSanPham.ReadOnly = true;
            dgvSanPham.AutoGenerateColumns = true;
            dgvSanPham.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvSanPham.Click += DgvSanPham_Click;
            this.Controls.Add(dgvSanPham);


            // Thiết lập Panel Input
            pnlInput = new Panel();
            pnlInput.Location = new Point(600, 10);
            pnlInput.Size = new Size(320, 450);
            pnlInput.BorderStyle = BorderStyle.FixedSingle;
            this.Controls.Add(pnlInput);

            int y = 20;

            void AddInputControl(string labelText, ref TextBox textBox, string name)
            {
                pnlInput.Controls.Add(new Label() { Text = labelText, Location = new Point(10, y), AutoSize = true });
                textBox = new TextBox() { Location = new Point(100, y - 3), Width = 200, Name = name };
                pnlInput.Controls.Add(textBox);
                y += 30;
            }

            // Thêm các controls nhập liệu
            AddInputControl("Mã SP:", ref txtMaSP, "txtMaSP");
            AddInputControl("Tên SP:", ref txtTenSP, "txtTenSP");
            AddInputControl("Nhóm SP:", ref txtNhomSP, "txtNhomSP");
            AddInputControl("Đơn vị tính:", ref txtDonViTinh, "txtDonViTinh");
            AddInputControl("Giá Nhập:", ref txtGiaNhap, "txtGiaNhap");
            AddInputControl("Giá Bán:", ref txtGiaBan, "txtGiaBan");
            AddInputControl("SL Tồn:", ref txtSoLuongTon, "txtSoLuongTon");
            txtSoLuongTon.ReadOnly = true; // KHÔNG CHO SỬA TỒN KHO TRỰC TIẾP

            // Nút chức năng
            int buttonY = y + 20;

            btnThem = new Button() { Text = "Thêm mới", Location = new Point(10, buttonY), Size = new Size(90, 30) };
            btnThem.Click += BtnThem_Click;
            pnlInput.Controls.Add(btnThem);

            btnSua = new Button() { Text = "Cập nhật", Location = new Point(110, buttonY), Size = new Size(90, 30) };
            btnSua.Click += BtnSua_Click;
            pnlInput.Controls.Add(btnSua);

            btnXoa = new Button() { Text = "Xóa", Location = new Point(210, buttonY), Size = new Size(90, 30) };
            btnXoa.Click += BtnXoa_Click;
            pnlInput.Controls.Add(btnXoa);

            // Nút LƯU
            buttonY += 40;
            btnLuu = new Button() { Text = "LƯU", Location = new Point(100, buttonY), Size = new Size(120, 40), BackColor = Color.LightGreen };
            btnLuu.Click += BtnLuu_Click;
            pnlInput.Controls.Add(btnLuu);
        }

        private void SetInputState(bool isEditMode)
        {
            // true cho phép chỉnh sửa, false chỉ cho phép xem/chọn
            txtMaSP.ReadOnly = !isEditMode || !isAddingNew;
            txtTenSP.ReadOnly = !isEditMode;
            txtNhomSP.ReadOnly = !isEditMode;
            txtDonViTinh.ReadOnly = !isEditMode;
            txtGiaNhap.ReadOnly = !isEditMode;
            txtGiaBan.ReadOnly = !isEditMode;
            // txtSoLuongTon luôn ReadOnly = true

            btnLuu.Enabled = isEditMode;
            btnThem.Enabled = !isEditMode;
            btnSua.Enabled = !isEditMode;
            btnXoa.Enabled = !isEditMode;
        }


        private void DgvSanPham_Click(object sender, EventArgs e)
        {
            if (dgvSanPham.SelectedRows.Count > 0)
            {
                DataGridViewRow row = dgvSanPham.SelectedRows[0];
                // Ánh xạ dữ liệu vào input fields
                txtMaSP.Text = row.Cells["MaSP"].Value?.ToString();
                txtTenSP.Text = row.Cells["TenSP"].Value?.ToString();
                txtNhomSP.Text = row.Cells["NhomSP"].Value?.ToString();
                txtDonViTinh.Text = row.Cells["DonViTinh"].Value?.ToString();
                txtGiaNhap.Text = row.Cells["GiaNhap"].Value?.ToString();
                txtGiaBan.Text = row.Cells["GiaBan"].Value?.ToString();
                txtSoLuongTon.Text = row.Cells["SoLuongTon"].Value?.ToString();

                isAddingNew = false;
                SetInputState(false); // Chuyển về trạng thái xem
            }
        }

        private void LoadData()
        {
            try
            {
                dgvSanPham.DataSource = _sanPhamService.GetAll();
                // Định dạng lại header text
                dgvSanPham.Columns["MaSP"].HeaderText = "Mã SP";
                dgvSanPham.Columns["TenSP"].HeaderText = "Tên Sản Phẩm";
                if (dgvSanPham.Columns.Contains("DonViTinh"))
                {
                    dgvSanPham.Columns["DonViTinh"].HeaderText = "ĐVT";
                }
                dgvSanPham.Columns["SoLuongTon"].HeaderText = "SL Tồn";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu sản phẩm: " + ex.Message, "Lỗi DB", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearInput()
        {
            txtMaSP.Clear();
            txtTenSP.Clear();
            txtNhomSP.Clear();
            txtDonViTinh.Clear();
            txtGiaNhap.Clear();
            txtGiaBan.Clear();
            txtSoLuongTon.Clear();
        }

        private SanPham MapToSanPham()
        {
            if (string.IsNullOrWhiteSpace(txtMaSP.Text) || string.IsNullOrWhiteSpace(txtTenSP.Text))
            {
                throw new Exception("Mã SP và Tên SP không được để trống.");
            }
            if (!decimal.TryParse(txtGiaNhap.Text, out decimal giaNhap) || giaNhap < 0) throw new Exception("Giá Nhập không hợp lệ.");
            if (!decimal.TryParse(txtGiaBan.Text, out decimal giaBan) || giaBan < 0) throw new Exception("Giá Bán không hợp lệ.");

            int soLuongTon = 0;
            // Chỉ đọc giá trị tồn kho hiện tại (nếu có)
            if (int.TryParse(txtSoLuongTon.Text, out int sl))
            {
                soLuongTon = sl;
            }

            return new SanPham
            {
                MaSP = txtMaSP.Text.Trim(),
                TenSP = txtTenSP.Text.Trim(),
                NhomSP = txtNhomSP.Text.Trim(),
                DonViTinh = txtDonViTinh.Text.Trim(),
                GiaNhap = giaNhap,
                GiaBan = giaBan,
                SoLuongTon = soLuongTon
            };
        }

        // --- Sự kiện Thêm ---
        private void BtnThem_Click(object sender, EventArgs e)
        {
            ClearInput();
            txtSoLuongTon.Text = "0";
            isAddingNew = true;
            SetInputState(true); // Bật chế độ chỉnh sửa
            txtMaSP.Focus();
        }

        private void BtnSua_Click(object sender, EventArgs e)
        {
            if (dgvSanPham.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn một sản phẩm để cập nhật.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            isAddingNew = false;
            SetInputState(true); // Bật chế độ chỉnh sửa
            txtMaSP.ReadOnly = true; // KHÓA Mã SP khi sửa
        }

        private void BtnXoa_Click(object sender, EventArgs e)
        {
            if (dgvSanPham.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn một sản phẩm để xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string maSP = dgvSanPham.SelectedRows[0].Cells["MaSP"].Value.ToString();
            string tenSP = dgvSanPham.SelectedRows[0].Cells["TenSP"].Value.ToString();

            DialogResult confirm = MessageBox.Show($"Bạn có chắc chắn muốn xóa sản phẩm '{tenSP}' ({maSP}) không? \nThao tác này sẽ thất bại nếu sản phẩm đã phát sinh giao dịch nhập/xuất.", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                try
                {
                    if (_sanPhamService.Delete(maSP))
                    {
                        MessageBox.Show("Xóa sản phẩm thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ClearInput();
                        LoadData();
                    }
                    else
                    {
                        MessageBox.Show("Xóa thất bại. Vui lòng kiểm tra lại Mã SP.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Oracle.ManagedDataAccess.Client.OracleException ex) when (ex.Number == 2292)
                {
                    // ORA-02292: Lỗi khóa ngoại (sản phẩm đã được sử dụng trong phiếu)
                    MessageBox.Show("Lỗi: Không thể xóa sản phẩm này vì nó đã được sử dụng trong các Phiếu Nhập hoặc Phiếu Xuất. Vui lòng xóa các giao dịch liên quan trước.", "Lỗi Khóa Ngoại", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Oracle.ManagedDataAccess.Client.OracleException ex) when (ex.Number == 1031)
                {
                    // ORA-01031: Lỗi phân quyền
                    MessageBox.Show("LỖI BẢO MẬT: Bạn không có quyền XÓA sản phẩm (ORA-01031). Hãy kiểm tra lại phân quyền của vai trò.", "Lỗi Phân quyền", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi xóa sản phẩm: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnLuu_Click(object sender, EventArgs e)
        {
            try
            {
                SanPham sp = MapToSanPham();
                bool success = false;
                string action = "";

                if (!isAddingNew) // Chế độ Sửa (Cập nhật)
                {
                    action = "cập nhật";
                    success = _sanPhamService.Update(sp);
                }
                else // Chế độ Thêm mới
                {
                    action = "thêm mới";
                    success = _sanPhamService.Add(sp);
                }

                if (success)
                {
                    MessageBox.Show($"Đã {action} sản phẩm thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearInput();
                    LoadData();
                    SetInputState(false); // Trở lại trạng thái xem
                }
                else
                {
                    MessageBox.Show($"Thao tác {action} thất bại. Vui lòng kiểm tra Mã SP đã tồn tại hoặc lỗi dữ liệu.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Oracle.ManagedDataAccess.Client.OracleException ex) when (ex.Number == 1)
            {
                // ORA-00001: Lỗi khóa chính (thêm trùng Mã SP)
                MessageBox.Show($"Lỗi: Mã sản phẩm '{txtMaSP.Text}' đã tồn tại.", "Lỗi Khóa chính", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Oracle.ManagedDataAccess.Client.OracleException ex) when (ex.Number == 1031)
            {
                // ORA-01031: Lỗi phân quyền
                MessageBox.Show("LỖI BẢO MẬT: Bạn không có quyền THÊM/SỬA sản phẩm (ORA-01031).", "Lỗi Phân quyền", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}