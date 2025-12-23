using System;
using System.Drawing;
using System.Windows.Forms;
using UrologyClinic.Data;
using UrologyClinic.Models;

namespace UrologyClinic.Forms
{
	public class PatientsListForm : Form
	{
		private readonly PatientRepository _repo = new PatientRepository();
		private System.Windows.Forms.Timer searchTimer;
		private DataGridView dgv;
		private TextBox txtSearch;
		private Button btnSearch, btnAdd, btnPrev, btnNext, btnExport;
		private Label lblPageInfo;

		private int currentPage = 1;
		private int pageSize = 20;
		private string? currentFilter = null;

		public PatientsListForm()
		{
			Text = "دكتور طارق عدرة - V1.0.0";
			Width = 1000;
			Height = 650;
			StartPosition = FormStartPosition.CenterScreen;
			BackColor = ColorTranslator.FromHtml("#F9F7F7");

			InitializeComponent();
			searchTimer = new System.Windows.Forms.Timer();
			searchTimer.Interval = 500; // 500ms تأخير
			searchTimer.Tick += (s, e) =>
			{
				searchTimer.Stop();
				currentPage = 1;
				LoadPatients();
			};
			LoadPatients();
		}

		private void InitializeComponent()
		{
			var mainLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1 };
			mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
			mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
			mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

			// 🔍 شريط البحث
			var searchPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, Padding = new Padding(10) };
			txtSearch = new TextBox { Width = 300, PlaceholderText = "ابحث بالرقم، الاسم أو الهاتف..." };
			btnSearch = CreateStyledButton("🔍 بحث", "#3F72AF");
			btnSearch.Click += (s, e) =>
			{
				searchTimer.Stop(); // إيقاف Timer إذا كان يعمل
				currentFilter = string.IsNullOrWhiteSpace(txtSearch.Text) ? null : txtSearch.Text.Trim();
				currentPage = 1;
				LoadPatients();
			};
			// في جزء شريط البحث، أضف هذا الزر:
			var btnBackup = CreateStyledButton("💾 نسخ احتياطي", "#F6BD60");
			btnBackup.Click += (s, e) =>
			{
				var form = new BackupManagerForm();
				form.ShowDialog();
			};

			// أضفه إلى searchPanel:
			searchPanel.Controls.Add(btnBackup);
			btnAdd = CreateStyledButton("➕ إضافة مريض", "#84A59D");
			btnAdd.Click += (s, e) =>
			{
				var form = new PatientForm();
				if (form.ShowDialog() == DialogResult.OK) LoadPatients();
			};

			btnExport = CreateStyledButton("📤 تصدير إلى Excel", "#F6BD60");
			btnExport.Click += (s, e) => ExportToExcel();

			searchPanel.Controls.Add(txtSearch);
			searchPanel.Controls.Add(btnSearch);
			searchPanel.Controls.Add(btnAdd);
			searchPanel.Controls.Add(btnExport);

			// 📑 جدول المرضى
			dgv = new DataGridView
			{
				Dock = DockStyle.Fill,
				ReadOnly = true,
				AllowUserToAddRows = false,
				AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
				SelectionMode = DataGridViewSelectionMode.FullRowSelect,
				MultiSelect = false,
				RowTemplate = { Height = 40 }
			};

			dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "FileNumber", HeaderText = "رقم الأضبارة", Width = 120 });
			dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "FullName", HeaderText = "الاسم", Width = 200 });
			dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Age", HeaderText = "العمر", Width = 50 });
			dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Phone", HeaderText = "الهاتف", Width = 120 });

			var btnDetails = new DataGridViewButtonColumn
			{
				HeaderText = "التفاصيل",
				Text = "عرض",
				UseColumnTextForButtonValue = true,
				Width = 80
			};
			dgv.Columns.Add(btnDetails);

			var btnDelete = new DataGridViewButtonColumn
			{
				HeaderText = "حذف",
				Text = "🗑",
				UseColumnTextForButtonValue = true,
				Width = 60
			};
			dgv.Columns.Add(btnDelete);

			dgv.CellClick += Dgv_CellClick;

			// 📄 أزرار الصفحة
			var pagingPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(10) };
			btnNext = CreateStyledButton("➡ التالي", "#3F72AF");
			btnPrev = CreateStyledButton("⬅ السابق", "#3F72AF");
			lblPageInfo = new Label { Text = "صفحة 1", AutoSize = true, TextAlign = ContentAlignment.MiddleCenter, Padding = new Padding(10) };

			btnNext.Click += (s, e) => { currentPage++; LoadPatients(); };
			btnPrev.Click += (s, e) => { if (currentPage > 1) { currentPage--; LoadPatients(); } };

			pagingPanel.Controls.Add(btnNext);
			pagingPanel.Controls.Add(btnPrev);
			pagingPanel.Controls.Add(lblPageInfo);

			// تجميع
			mainLayout.Controls.Add(searchPanel, 0, 0);
			mainLayout.Controls.Add(dgv, 0, 1);
			mainLayout.Controls.Add(pagingPanel, 0, 2);

			Controls.Add(mainLayout);
		}

		private void LoadPatients()
		{
			dgv.Rows.Clear();
			var patients = _repo.GetPatientsPaged(currentPage, pageSize, currentFilter);
			var totalCount = _repo.GetPatientsCount(currentFilter);
			int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

			foreach (var p in patients)
			{
				dgv.Rows.Add(p.FileNumber, p.FullName, p.Age, p.Phone, "عرض", "🗑");
				dgv.Rows[dgv.Rows.Count - 1].Tag = p;
			}

			lblPageInfo.Text = $"صفحة {currentPage} من {totalPages}";
			btnPrev.Enabled = currentPage > 1;
			btnNext.Enabled = currentPage < totalPages;
		}

		private void Dgv_CellClick(object? sender, DataGridViewCellEventArgs e)
		{
			// 🔹 التحقق من أن النقر ليس على رأس العمود أو خارج النطاق
			if (e.RowIndex < 0 || e.RowIndex >= dgv.Rows.Count)
				return;

			// 🔹 التحقق من أن الصف يحتوي على بيانات (ليس صف رأس)
			if (dgv.Rows[e.RowIndex].Tag == null)
				return;

			var patient = dgv.Rows[e.RowIndex].Tag as Patient;
			if (patient == null) return;

			if (e.ColumnIndex >= 0 && dgv.Columns[e.ColumnIndex].HeaderText == "التفاصيل")
			{
				var form = new PatientDetailsForm(patient);
				if (form.ShowDialog() == DialogResult.OK) LoadPatients();
			}
			else if (e.ColumnIndex >= 0 && dgv.Columns[e.ColumnIndex].HeaderText == "حذف")
			{
				if (MessageBox.Show("⚠ هل أنت متأكد من حذف هذا المريض؟", "تأكيد",
					MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
				{
					_repo.DeletePatient(patient.Id);
					LoadPatients();
				}
			}
		}

		private void ExportToExcel()
		{
			try
			{
				using var sfd = new SaveFileDialog
				{
					Filter = "Excel Files|*.xlsx",
					FileName = $"مرضى_العيادة_{DateTime.Now:yyyyMMdd}.xlsx"
				};

				if (sfd.ShowDialog() == DialogResult.OK)
				{
					// 🔥 تحسين: تصدير مجزأ للبيانات الكبيرة
					SmartBatchExport(sfd.FileName);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("❌ خطأ في التصدير:\n" + ex.Message);
			}
		}

		private void SmartBatchExport(string filePath)
		{
			int totalPatients = _repo.GetPatientsCount();

			// 🔥 تحديد حجم الدفعة بناءً على عدد المرضى
			int batchSize = totalPatients > 10000 ? 2000 :
						   totalPatients > 5000 ? 3000 : 5000;

			int totalBatches = (int)Math.Ceiling((double)totalPatients / batchSize);

			using var package = new OfficeOpenXml.ExcelPackage();
			var ws = package.Workbook.Worksheets.Add("المرضى");

			// 🔥 كتابة الرأس مرة واحدة
			string[] headers = { "رقم الأضبارة", "الاسم", "العمر", "الهاتف" };
			for (int i = 0; i < headers.Length; i++)
			{
				ws.Cells[1, i + 1].Value = headers[i];
				ws.Cells[1, i + 1].Style.Font.Bold = true;
			}

			int currentRow = 2;

			// 🔥 التصدير على دفعات
			for (int batch = 1; batch <= totalBatches; batch++)
			{
				var patients = _repo.GetPatientsPaged(batch, batchSize, null);

				foreach (var p in patients)
				{
					ws.Cells[currentRow, 1].Value = p.FileNumber;
					ws.Cells[currentRow, 2].Value = p.FullName;
					ws.Cells[currentRow, 3].Value = p.Age;
					ws.Cells[currentRow, 4].Value = p.Phone;
					currentRow++;
				}

				// 🔥 تحديث الواجهة للإشارة للتقدم
				if (totalBatches > 1)
				{
					lblPageInfo.Text = $"جاري التصدير {batch} من {totalBatches}";
					Application.DoEvents(); // تحديث الواجهة
				}
			}

			// 🔥 ضبط الأعمدة تلقائياً
			ws.Cells[1, 1, currentRow - 1, headers.Length].AutoFitColumns();

			package.SaveAs(new FileInfo(filePath));

			MessageBox.Show($"✅ تم التصدير بنجاح\nالمرضى: {totalPatients}\nالملف: {Path.GetFileName(filePath)}",
				"نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		private Button CreateStyledButton(string text, string colorHex)
		{
			return new Button
			{
				Text = text,
				BackColor = ColorTranslator.FromHtml(colorHex),
				ForeColor = Color.Black,
				FlatStyle = FlatStyle.Flat,
				AutoSize = true,
				Font = new Font("Segoe UI", 10, FontStyle.Bold),
				Padding = new Padding(8),
				Margin = new Padding(5)
			};
		}
		private void TxtSearch_TextChanged(object? sender, EventArgs e)
		{
			// 🔥 إعادة تشغيل Timer عند كل كتابة
			searchTimer.Stop();
			searchTimer.Start();
		}
	}
}
