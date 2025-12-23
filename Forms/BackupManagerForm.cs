using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace UrologyClinic.Forms
{
	public class BackupManagerForm : Form
	{
		private ListBox? lstBackups;
		private Button? btnCreateBackup;
		private Button? btnRestoreBackup;
		private Button? btnDeleteBackup;
		private Label? lblBackupInfo;
		private Button? btnRefresh;

		public BackupManagerForm()
		{
			InitializeComponent();
			LoadBackupsList();
		}

		private void InitializeComponent()
		{
			// ألوان متناسقة مع التطبيق
			Color colorPrimary = ColorTranslator.FromHtml("#3F72AF");
			Color colorSecondary = ColorTranslator.FromHtml("#DBE2EF");
			Color colorBackground = ColorTranslator.FromHtml("#F9F7F7");
			Color colorAccent = ColorTranslator.FromHtml("#84A59D");

			this.Text = "💾 إدارة النسخ الاحتياطي";
			this.Size = new Size(600, 500);
			this.BackColor = colorBackground;
			this.Padding = new Padding(10);

			// عنوان النافذة
			var lblTitle = new Label
			{
				Text = "إدارة النسخ الاحتياطي لقاعدة البيانات",
				Font = new Font("Segoe UI", 14, FontStyle.Bold),
				ForeColor = colorPrimary,
				Location = new Point(10, 10),
				AutoSize = true
			};

			// قائمة النسخ الاحتياطية
			var lblList = new Label
			{
				Text = "النسخ الاحتياطية المتاحة:",
				Location = new Point(10, 50),
				AutoSize = true,
				Font = new Font("Segoe UI", 10, FontStyle.Bold)
			};

			lstBackups = new ListBox
			{
				Location = new Point(10, 75),
				Size = new Size(560, 200),
				BackColor = Color.White,
				Font = new Font("Segoe UI", 9)
			};
			lstBackups.SelectedIndexChanged += LstBackups_SelectedIndexChanged;

			// معلومات النسخة المحددة
			lblBackupInfo = new Label
			{
				Location = new Point(10, 285),
				Size = new Size(560, 60),
				BackColor = colorSecondary,
				ForeColor = Color.Black,
				BorderStyle = BorderStyle.FixedSingle,
				Padding = new Padding(5),
				Font = new Font("Segoe UI", 9)
			};

			// الأزرار
			btnCreateBackup = CreateStyledButton("إنشاء نسخة احتياطية جديدة", colorAccent);
			btnCreateBackup.Location = new Point(10, 360);
			btnCreateBackup.Size = new Size(180, 35);
			btnCreateBackup.Click += BtnCreateBackup_Click;

			btnRestoreBackup = CreateStyledButton("استعادة النسخة المحددة", colorPrimary);
			btnRestoreBackup.Location = new Point(200, 360);
			btnRestoreBackup.Size = new Size(180, 35);
			btnRestoreBackup.Enabled = false;
			btnRestoreBackup.Click += BtnRestoreBackup_Click;

			btnDeleteBackup = CreateStyledButton("حذف النسخة المحددة", ColorTranslator.FromHtml("#F6BD60"));
			btnDeleteBackup.Location = new Point(390, 360);
			btnDeleteBackup.Size = new Size(180, 35);
			btnDeleteBackup.Enabled = false;
			btnDeleteBackup.Click += BtnDeleteBackup_Click;

			btnRefresh = CreateStyledButton("تحديث القائمة", colorSecondary);
			btnRefresh.Location = new Point(10, 410);
			btnRefresh.Size = new Size(100, 30);
			btnRefresh.Click += BtnRefresh_Click;

			// إضافة العناصر إلى النموذج
			this.Controls.AddRange(new Control[] {
				lblTitle, lblList, lstBackups, lblBackupInfo,
				btnCreateBackup, btnRestoreBackup, btnDeleteBackup, btnRefresh
			});
		}

		private void LoadBackupsList()
		{
			if (lstBackups == null) return;

			lstBackups.Items.Clear();
			var backups = Data.BackupManager.GetAvailableBackups();

			foreach (var backup in backups)
			{
				var fileName = Path.GetFileName(backup);
				var creationTime = File.GetCreationTime(backup);
				lstBackups.Items.Add($"{fileName} ({creationTime:yyyy-MM-dd HH:mm})");
			}

			if (backups.Length == 0)
			{
				lstBackups.Items.Add("لا توجد نسخ احتياطية متاحة");
			}
		}

		private void LstBackups_SelectedIndexChanged(object? sender, EventArgs e)
		{
			if (lstBackups?.SelectedIndex < 0) return;

			var backups = Data.BackupManager.GetAvailableBackups();
			if (lstBackups.SelectedIndex < backups.Length)
			{
				var selectedBackup = backups[lstBackups.SelectedIndex];
				if (lblBackupInfo != null)
					lblBackupInfo.Text = Data.BackupManager.GetBackupInfo(selectedBackup);

				if (btnRestoreBackup != null) btnRestoreBackup.Enabled = true;
				if (btnDeleteBackup != null) btnDeleteBackup.Enabled = true;
			}
		}

		private void BtnCreateBackup_Click(object? sender, EventArgs e)
		{
			var result = MessageBox.Show("هل تريد إنشاء نسخة احتياطية جديدة لقاعدة البيانات؟",
				"تأكيد إنشاء نسخة احتياطية", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

			if (result == DialogResult.Yes)
			{
				Data.BackupManager.CreateBackup();
				LoadBackupsList();
			}
		}

		private void BtnRestoreBackup_Click(object? sender, EventArgs e)
		{
			if (lstBackups?.SelectedIndex < 0) return;

			var backups = Data.BackupManager.GetAvailableBackups();
			if (lstBackups.SelectedIndex < backups.Length)
			{
				var selectedBackup = backups[lstBackups.SelectedIndex];

				var result = MessageBox.Show(
					"⚠️ تحذير: سيتم استبدال قاعدة البيانات الحالية بالنسخة الاحتياطية.\n" +
					"سيتم إعادة تشغيل التطبيق بعد الاستعادة.\n\n" +
					"هل تريد متابعة عملية الاستعادة؟",
					"تأكيد استعادة النسخة الاحتياطية",
					MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

				if (result == DialogResult.Yes)
				{
					Data.BackupManager.RestoreBackup(selectedBackup);
				}
			}
		}

		private void BtnDeleteBackup_Click(object? sender, EventArgs e)
		{
			if (lstBackups?.SelectedIndex < 0) return;

			var backups = Data.BackupManager.GetAvailableBackups();
			if (lstBackups.SelectedIndex < backups.Length)
			{
				var selectedBackup = backups[lstBackups.SelectedIndex];

				var result = MessageBox.Show("هل تريد حذف النسخة الاحتياطية المحددة؟",
					"تأكيد الحذف", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

				if (result == DialogResult.Yes)
				{
					if (Data.BackupManager.DeleteBackup(selectedBackup))
					{
						MessageBox.Show("تم حذف النسخة الاحتياطية بنجاح", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
						LoadBackupsList();
						if (lblBackupInfo != null)
							lblBackupInfo.Text = "";
						if (btnRestoreBackup != null) btnRestoreBackup.Enabled = false;
						if (btnDeleteBackup != null) btnDeleteBackup.Enabled = false;
					}
				}
			}
		}

		private void BtnRefresh_Click(object? sender, EventArgs e)
		{
			LoadBackupsList();
		}

		private Button CreateStyledButton(string text, Color color)
		{
			return new Button
			{
				Text = text,
				BackColor = color,
				ForeColor = Color.Black,
				FlatStyle = FlatStyle.Flat,
				Font = new Font("Segoe UI", 9, FontStyle.Bold),
				Cursor = Cursors.Hand
			};
		}
	}
}