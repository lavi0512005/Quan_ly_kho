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
    public partial class MainForm : Form
    {
        private MenuStrip mainMenu; private ToolStripMenuItem menuQuanLyNguoiDung; private ToolStripMenuItem menuNhapXuat; private ToolStripMenuItem menuBaoCao;
        private ToolStripMenuItem subMenuQLSanPham; private ToolStripMenuItem subMenuTaoPhieuNhap; private ToolStripMenuItem subMenuTaoPhieuXuat;
        private ToolStripMenuItem subMenuBaoCaoNXT;

        public MainForm()
        {
            this.Load += MainForm_Load;
            this.IsMdiContainer = true;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.Text = $"Quản Lý Kho - Xin chào {SessionManager.CurrentUsername} ({SessionManager.CurrentRole})";
            ApplyRolePermissions(SessionManager.CurrentRole);
        }

        private void ApplyRolePermissions(string role)
        {
            menuQuanLyNguoiDung.Visible = false;
            menuNhapXuat.Visible = false;
            menuBaoCao.Visible = false;

            switch (role)
            {
                case "RL_ADMIN":
                    menuQuanLyNguoiDung.Visible = true;
                    menuNhapXuat.Visible = true;
                    menuBaoCao.Visible = true;
                    break;
                case "RL_THUKHO":
                    menuNhapXuat.Visible = true;
                    break;
                case "RL_KETOAN":
                    menuBaoCao.Visible = true;
                    break;
            }
        }

        // --- Sự kiện mở Form con ---
        private void SubMenuQLSanPham_Click(object sender, EventArgs e) => ShowMdiChild(new QuanLySanPhamForm());
        private void SubMenuTaoPhieuNhap_Click(object sender, EventArgs e) => ShowMdiChild(new TaoPhieuNhapForm());
        private void SubMenuTaoPhieuXuat_Click(object sender, EventArgs e) => ShowMdiChild(new TaoPhieuXuatForm());
        private void SubMenuBaoCaoNXT_Click(object sender, EventArgs e) => ShowMdiChild(new BaoCaoNXTForm());
        private void MenuQuanLyNguoiDung_Click(object sender, EventArgs e) => ShowMdiChild(new QuanLyTaiKhoanForm());
        private void MenuLogout_Click(object sender, EventArgs e) => Application.Restart();

        private void ShowMdiChild(Form form) { form.MdiParent = this; form.Show(); }

        private void InitializeComponent()
        {
            this.SuspendLayout(); this.Size = new System.Drawing.Size(1200, 800); this.StartPosition = FormStartPosition.CenterScreen;
            mainMenu = new MenuStrip();
            
            menuQuanLyNguoiDung = new ToolStripMenuItem("Quản lý người dùng"); menuQuanLyNguoiDung.Click += MenuQuanLyNguoiDung_Click;
            ToolStripMenuItem menuLogout = new ToolStripMenuItem("Đăng xuất"); menuLogout.Click += MenuLogout_Click;
            ToolStripMenuItem menuHeThong = new ToolStripMenuItem("Hệ thống"); menuHeThong.DropDownItems.AddRange(new ToolStripItem[] { menuQuanLyNguoiDung, menuLogout });

            menuNhapXuat = new ToolStripMenuItem("Nhập/Xuất Kho");
            subMenuQLSanPham = new ToolStripMenuItem("Quản lý Sản phẩm & Khách hàng"); subMenuQLSanPham.Click += SubMenuQLSanPham_Click;
            subMenuTaoPhieuNhap = new ToolStripMenuItem("Tạo Phiếu Nhập"); subMenuTaoPhieuNhap.Click += SubMenuTaoPhieuNhap_Click;
            subMenuTaoPhieuXuat = new ToolStripMenuItem("Tạo Phiếu Xuất"); subMenuTaoPhieuXuat.Click += SubMenuTaoPhieuXuat_Click;
            menuNhapXuat.DropDownItems.AddRange(new ToolStripItem[] { subMenuQLSanPham, subMenuTaoPhieuNhap, subMenuTaoPhieuXuat });

            menuBaoCao = new ToolStripMenuItem("Báo cáo");
            subMenuBaoCaoNXT = new ToolStripMenuItem("Báo cáo Nhập/Xuất/Tồn"); subMenuBaoCaoNXT.Click += SubMenuBaoCaoNXT_Click;
            menuBaoCao.DropDownItems.Add(subMenuBaoCaoNXT);

            mainMenu.Items.AddRange(new ToolStripItem[] { menuHeThong, menuNhapXuat, menuBaoCao });
            this.Controls.Add(mainMenu); this.ResumeLayout(false); this.PerformLayout();
        }

    }
}
