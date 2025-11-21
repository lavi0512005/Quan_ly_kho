-- 1. TẠO SCHEMA ỨNG DỤNG (USER chứa tất cả các bảng)
CREATE USER C##Quan_Ly_Kho IDENTIFIED BY "123" DEFAULT TABLESPACE USERS TEMPORARY TABLESPACE TEMP;

-- 2. Cấp quyền cơ bản
GRANT CREATE SESSION TO C##Quan_Ly_Kho;
ALTER USER C##QUAN_LY_KHO QUOTA 100M ON USERS;
GRANT RESOURCE TO C##QUAN_LY_KHO;

-- 3. BẬT TÍNH NĂNG AUDIT (Giám sát) - Chỉ cần chạy 1 lần
ALTER SYSTEM SET AUDIT_TRAIL = DB, EXTENDED SCOPE=SPFILE;
-- Cần khởi động lại database instance để thay đổi có hiệu lực.

-- 4. TẠO CÁC ROLE BẢO MẬT VÀ GÁN MẬT KHẨU (Khuyến nghị dùng trong môi trường sản phẩm)
CREATE ROLE RL_ADMIN IDENTIFIED BY AdminPass2025;
CREATE ROLE RL_THUKHO IDENTIFIED BY KhoPass2025;
CREATE ROLE RL_KETOAN IDENTIFIED BY KetoanPass2025;

-- 5. GÁN TẤT CẢ CÁC ROLE CHO SCHEMA ỨNG DỤNG (Để nó có thể kích hoạt các quyền này)
GRANT RL_ADMIN, RL_THUKHO, RL_KETOAN TO C##Quan_Ly_Kho;



-- Tài khoản
CREATE TABLE TAIKHOAN 
(
    UserID             VARCHAR2(10) PRIMARY KEY,
    Username           VARCHAR2(50) UNIQUE NOT NULL,
    PasswordHash       VARCHAR2(100) NOT NULL, 
    Salt               VARCHAR2(50) NOT NULL,  
    TrangThai          VARCHAR2(10) DEFAULT 'CHO_DUYET' NOT NULL
);

-- Vai trò
CREATE TABLE VAITRO
(
    VAITRO VARCHAR2(15) NOT NULL, 
    UserID VARCHAR2(10) NOT NULL,
    CONSTRAINT pk_vaitro PRIMARY KEY (VAITRO, UserID),
    CONSTRAINT fk_vt_user FOREIGN KEY (UserID) REFERENCES TAIKHOAN(UserID)
);

-- Sản phẩm
CREATE TABLE SANPHAM
(
    MaSP VARCHAR2(30) PRIMARY KEY,
    TenSP NVARCHAR2(100) NOT NULL,
    NhomSP NVARCHAR2(50),
    DonViTinh NVARCHAR2(10),
    GiaNhap NUMBER(12,2),
    GiaBan NUMBER(12,2),
    SoLuongTon NUMBER DEFAULT 0 NOT NULL
);


-- Nhà cung cấp
CREATE TABLE NHACUNGCAP
(
    MaNCC VARCHAR2(20) PRIMARY KEY,
    TenNCC NVARCHAR2(50) NOT NULL,
    DiaChi NVARCHAR2(200),
    SDT VARCHAR2(20)
);

-- Người mua (Có cột mã hóa)
CREATE TABLE NGUOIMUA
(
    MaNM VARCHAR2(20) PRIMARY KEY,
    TenNM NVARCHAR2(100) NOT NULL,
    DiaChi NVARCHAR2(200),
    SDT VARCHAR2(20),
    DiaChi_Enc RAW(2000),
    SDT_Enc RAW(2000)
);

-- Phiếu nhập (Header)
CREATE TABLE PHIEUNHAP
(
    MaPN VARCHAR2(20) PRIMARY KEY,
    NgayNhap DATE DEFAULT SYSDATE,
    NhanVienNhap VARCHAR2(10) NOT NULL,
    MaNCC VARCHAR2(20) NOT NULL,
    CONSTRAINT fk_pn_nv FOREIGN KEY (NhanVienNhap) REFERENCES TAIKHOAN(UserID),
    CONSTRAINT fk_pn_ncc FOREIGN KEY (MaNCC) REFERENCES NHACUNGCAP(MaNCC)
);

-- Chi tiết Phiếu nhập
CREATE TABLE CT_PHIEUNHAP
(
    MaPN VARCHAR2(20),
    MaSP VARCHAR2(30),
    SoLuong NUMBER NOT NULL,
    DonGia NUMBER(12,2),
    PRIMARY KEY (MaPN, MaSP),
    CONSTRAINT fk_ctpn_pn FOREIGN KEY (MaPN) REFERENCES PHIEUNHAP(MaPN) ON DELETE CASCADE,
    CONSTRAINT fk_ctpn_sp FOREIGN KEY (MaSP) REFERENCES SANPHAM(MaSP)
);

-- Phiếu xuất (Header)
CREATE TABLE PHIEUXUAT
(
    MaPX VARCHAR2(20) PRIMARY KEY,
    NgayXuat DATE DEFAULT SYSDATE,
    NhanVienXuat VARCHAR2(10) NOT NULL,
    MaNM VARCHAR2(20) NOT NULL,
    CONSTRAINT fk_px_nv FOREIGN KEY (NhanVienXuat) REFERENCES TAIKHOAN(UserID),
    CONSTRAINT fk_px_nm FOREIGN KEY (MaNM) REFERENCES NGUOIMUA(MaNM)
);

-- Chi tiết Phiếu xuất
CREATE TABLE CT_PHIEUXUAT
(
    MaPX VARCHAR2(20),
    MaSP VARCHAR2(30),
    SoLuong NUMBER NOT NULL,
    DonGia NUMBER(12,2) NOT NULL,
    PRIMARY KEY (MaPX, MaSP),
    CONSTRAINT fk_ctpx_px FOREIGN KEY (MaPX) REFERENCES PHIEUXUAT(MaPX) ON DELETE CASCADE,
    CONSTRAINT fk_ctpx_sp FOREIGN KEY (MaSP) REFERENCES SANPHAM(MaSP)
);

-- HÀM GIẢI MÃ DỮ LIỆU
CREATE OR REPLACE FUNCTION decrypt_value (p_encrypted_raw IN RAW)
RETURN NVARCHAR2
IS
    -- KHÓA BẢO MẬT: CẦN LƯU KHÓA NÀY AN TOÀN TRONG ỨNG DỤNG HOẶC KHO KHÓA
    encryption_key RAW(32) := UTL_RAW.CAST_TO_RAW('THIS_IS_A_VERY_STRONG_KEY_2025!'); 
    encryption_type PLS_INTEGER := DBMS_CRYPTO.ENCRYPT_AES256 + DBMS_CRYPTO.CHAIN_CBC + DBMS_CRYPTO.PAD_PKCS5;
    v_decrypted_raw RAW(2000);
BEGIN
    IF p_encrypted_raw IS NULL THEN RETURN NULL; END IF;

    v_decrypted_raw := DBMS_CRYPTO.DECRYPT(
        src => p_encrypted_raw,
        typ => encryption_type,
        key => encryption_key
    );

    RETURN UTL_I18N.RAW_TO_STRING(v_decrypted_raw, 'AL32UTF8');
EXCEPTION
    WHEN OTHERS THEN
        RETURN '***GIÁ TRỊ BỊ MÃ HÓA***';
END;
/

-- VIEW GIẢI MÃ NGƯỜI MUA
CREATE OR REPLACE VIEW view_NGUOIMUA_DECRYPT AS
SELECT
    MaNM, TenNM,
    decrypt_value(DiaChi_Enc) AS DiaChi,
    decrypt_value(SDT_Enc) AS SDT
FROM NGUOIMUA;
/

-- PROCEDURE TẠO PHIẾU NHẬP (Để cô lập quyền DML)
CREATE OR REPLACE PROCEDURE PROC_TAO_PHIEUNHAP (
    p_MaPN IN VARCHAR2, 
    p_NhanVienNhap IN VARCHAR2, 
    p_MaNCC IN VARCHAR2,
    p_ChiTietNhap VARCHAR2, -- Tham số đơn giản hóa cho Chi tiết
    p_KetQua OUT VARCHAR2
)
IS
BEGIN
    INSERT INTO PHIEUNHAP(MaPN, NhanVienNhap, MaNCC)
    VALUES (p_MaPN, p_NhanVienNhap, p_MaNCC);

    -- Logic thêm chi tiết phiếu nhập (cần phát triển thêm trong thực tế)

    COMMIT;
    p_KetQua := 'OK';

EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        p_KetQua := 'ERROR: ' || SQLERRM;
END;
/

-- DỮ LIỆU TÀI KHOẢN VÀ VAI TRÒ
INSERT INTO TAIKHOAN(UserID, Username, PasswordHash, Salt, TrangThai) VALUES ('U01', 'ADMIN', '0', 'SALT1', 'DUYET');
INSERT INTO TAIKHOAN(UserID, Username, PasswordHash, Salt, TrangThai) VALUES ('U02', 'NV_KHO', '0', 'SALT2', 'DUYET');
INSERT INTO TAIKHOAN(UserID, Username, PasswordHash, Salt, TrangThai) VALUES ('U03', 'NV_KT', '0', 'SALT3', 'DUYET');

-- Cập nhật mật khẩu băm SHA256 (Mật khẩu: 123456)
UPDATE TAIKHOAN SET Salt = 'SALT1', PasswordHash = LOWER(RAWTOHEX(STANDARD_HASH('123456' || 'SALT1', 'SHA256'))) WHERE Username = 'ADMIN';
UPDATE TAIKHOAN SET Salt = 'SALT2', PasswordHash = LOWER(RAWTOHEX(STANDARD_HASH('123456' || 'SALT2', 'SHA256'))) WHERE Username = 'NV_KHO';
UPDATE TAIKHOAN SET Salt = 'SALT3', PasswordHash = LOWER(RAWTOHEX(STANDARD_HASH('123456' || 'SALT3', 'SHA256'))) WHERE Username = 'NV_KT';

-- GÁN VAI TRÒ CHO TÀI KHOẢN
INSERT INTO VAITRO(VAITRO, UserID) VALUES ('RL_ADMIN', 'U01');
INSERT INTO VAITRO(VAITRO, UserID) VALUES ('RL_THUKHO', 'U02');
INSERT INTO VAITRO(VAITRO, UserID) VALUES ('RL_KETOAN', 'U03');

-- CHÈN DỮ LIỆU GỐC NGUOIMUA (Tạm thời có plaintext)
INSERT INTO NGUOIMUA(MaNM, TenNM, DiaChi, SDT) VALUES ('NM01', N'Nguyễn Văn A', N'Đà Nẵng', '0987654321');
INSERT INTO NGUOIMUA(MaNM, TenNM, DiaChi, SDT) VALUES('NM02', N'Trần Thị B', N'Hải Phòng', '0978123456');

-- THỰC THI MÃ HÓA DỮ LIỆU (Chuyển dữ liệu từ cột plaintext sang cột Encrypted)
DECLARE
    encryption_key RAW(32) := UTL_RAW.CAST_TO_RAW('THIS_IS_A_VERY_STRONG_KEY_2025!'); 
    encryption_type PLS_INTEGER := DBMS_CRYPTO.ENCRYPT_AES256 + DBMS_CRYPTO.CHAIN_CBC + DBMS_CRYPTO.PAD_PKCS5;
    v_input_string NVARCHAR2(200);
    v_encrypted_raw RAW(2000);
BEGIN
    FOR rec IN (SELECT MaNM, DiaChi, SDT FROM NGUOIMUA WHERE DiaChi IS NOT NULL OR SDT IS NOT NULL)
    LOOP
        -- Mã hóa Địa chỉ
        v_input_string := rec.DiaChi;
        v_encrypted_raw := DBMS_CRYPTO.ENCRYPT(src => UTL_I18N.STRING_TO_RAW(v_input_string, 'AL32UTF8'), typ => encryption_type, key => encryption_key);
        UPDATE NGUOIMUA SET DiaChi_Enc = v_encrypted_raw, DiaChi = NULL WHERE MaNM = rec.MaNM; -- Xóa plaintext

        -- Mã hóa SĐT
        v_input_string := rec.SDT;
        v_encrypted_raw := DBMS_CRYPTO.ENCRYPT(src => UTL_I18N.STRING_TO_RAW(v_input_string, 'AL32UTF8'), typ => encryption_type, key => encryption_key);
        UPDATE NGUOIMUA SET SDT_Enc = v_encrypted_raw, SDT = NULL WHERE MaNM = rec.MaNM; -- Xóa plaintext
    END LOOP;
    COMMIT;
EXCEPTION WHEN OTHERS THEN ROLLBACK;
END;
/

-- KẾT NỐI BẰNG SYS AS SYSDBA

-- 1. Thu hồi các quyền CREATE/RESOURCE (Chỉ để lại CREATE SESSION)
REVOKE CREATE TABLE, CREATE VIEW, CREATE TRIGGER, CREATE PROCEDURE, RESOURCE FROM C##Quan_Ly_Kho;

-- 2. Cấp quyền DML/Execute cho các ROLE nghiệp vụ (Dùng tên Schema C##Quan_Ly_Kho.Tên_Bảng)

-- Role ADMIN: Quản lý người dùng và duyệt tài khoản
GRANT SELECT, INSERT, UPDATE, DELETE ON C##Quan_Ly_Kho.TAIKHOAN TO RL_ADMIN;
GRANT SELECT, INSERT, DELETE ON C##Quan_Ly_Kho.VAITRO TO RL_ADMIN;

-- Role KETOAN: Xem báo cáo và thông tin nhà cung cấp
GRANT SELECT ON C##Quan_Ly_Kho.view_BC_NHAPXUAT_TON TO RL_KETOAN;
GRANT SELECT ON C##Quan_Ly_Kho.NHACUNGCAP TO RL_KETOAN; 
GRANT SELECT ON C##Quan_Ly_Kho.PHIEUNHAP TO RL_KETOAN;

-- Role THUKHO: Nhập xuất và xem thông tin sản phẩm/khách hàng
GRANT SELECT, INSERT, UPDATE ON C##Quan_Ly_Kho.SANPHAM TO RL_THUKHO;
GRANT SELECT ON C##Quan_Ly_Kho.view_NGUOIMUA_DECRYPT TO RL_THUKHO;
GRANT EXECUTE ON C##Quan_Ly_Kho.decrypt_value TO RL_THUKHO; 

-- Cấp quyền EXECUTE cho Stored Procedure (Thay thế DML trực tiếp)
GRANT EXECUTE ON C##Quan_Ly_Kho.PROC_TAO_PHIEUNHAP TO RL_THUKHO;
-- Cần cấp quyền INSERT/UPDATE/DELETE trên các bảng liên quan nếu không dùng PROC/TRIGGER
GRANT INSERT, UPDATE ON C##Quan_Ly_Kho.PHIEUNHAP TO RL_THUKHO;
GRANT INSERT, UPDATE ON C##Quan_Ly_Kho.CT_PHIEUNHAP TO RL_THUKHO;
GRANT INSERT, UPDATE, DELETE ON C##Quan_Ly_Kho.PHIEUXUAT TO RL_THUKHO;
GRANT INSERT, UPDATE, DELETE ON C##Quan_Ly_Kho.CT_PHIEUXUAT TO RL_THUKHO;

-- 3. Cài đặt AUDIT trên các đối tượng nhạy cảm
-- Giám sát các thao tác DELETE trên bảng Sản phẩm
AUDIT DELETE ON C##Quan_Ly_Kho.SANPHAM BY ACCESS; 

-- Giám sát các thao tác UPDATE trên bảng TAIKHOAN (thay đổi trạng thái/password)
AUDIT UPDATE ON C##Quan_Ly_Kho.TAIKHOAN BY ACCESS;