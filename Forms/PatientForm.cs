using System;
using System.Drawing;
using System.Windows.Forms;
using UrologyClinic.Data;
using UrologyClinic.Models;

namespace UrologyClinic.Forms
{
	public class PatientForm : Form
	{
		private readonly PatientRepository _repo = new PatientRepository();
		private Patient? _patient; // null = add, not null = edit

		// Controls
		private TextBox txtFileNumber, txtFullName, txtPhone, txtJob, txtAddress, txtMainComplaint, txtMedicalHistory, txtHabits;
		private NumericUpDown numAge;
		private ComboBox cbGender, cbMarital;
		private Button btnSave, btnCancel;

		public PatientForm(Patient? patient = null)
		{
			_patient = patient;
			Text = patient == null ? "➕ إضافة مريض" : "✏ تعديل بيانات المريض";
			Width = 750;
			Height = 650;
			StartPosition = FormStartPosition.CenterParent;
			BackColor = ColorTranslator.FromHtml("#F9F7F7");

			InitializeComponent();
			LoadEnums();

			if (_patient != null) PopulateFields();
		}

		private void InitializeComponent()
		{
			var mainLayout = new TableLayoutPanel
			{
				Dock = DockStyle.Fill,
				ColumnCount = 1,
				RowCount = 3,
				Padding = new Padding(15),
				AutoScroll = true
			};

			// ========== Personal group ==========
			var personalGroup = new GroupBox { Text = "🧑‍🤝‍🧑 المعلومات الشخصية", Dock = DockStyle.Top, AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
			var tlPersonal = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, AutoSize = true };
			tlPersonal.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
			tlPersonal.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));
			tlPersonal.RowCount = 12;

			// Controls
			txtFileNumber = new TextBox();
			txtFullName = new TextBox();
			numAge = new NumericUpDown { Minimum = 0, Maximum = 150, Value = 30 };
			cbGender = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
			txtPhone = new TextBox();
			txtJob = new TextBox();
			cbMarital = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
			txtAddress = new TextBox { Multiline = true, Height = 60, ScrollBars = ScrollBars.Vertical };
			txtHabits = new TextBox { Multiline = false }; // simple single-line or small multiline if you want

			// Helper usage: add rows in personal group
			AddRow(tlPersonal, "رقم الأضبارة:", txtFileNumber, 0);
			AddRow(tlPersonal, "الاسم الكامل:", txtFullName, 1);
			AddRow(tlPersonal, "العمر:", numAge, 2);
			AddRow(tlPersonal, "الجنس:", cbGender, 3);
			AddRow(tlPersonal, "الهاتف:", txtPhone, 4);
			AddRow(tlPersonal, "الوظيفة:", txtJob, 5);
			AddRow(tlPersonal, "الحالة الاجتماعية:", cbMarital, 6);
			AddRow(tlPersonal, "العنوان:", txtAddress, 7);
			AddRow(tlPersonal, "العادات:", txtHabits, 8); // نقل العادات إلى المعلومات الشخصية

			personalGroup.Controls.Add(tlPersonal);

			// ========== Medical group ==========
			var medicalGroup = new GroupBox { Text = "🩺 المعلومات الطبية", Dock = DockStyle.Top, AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
			var tlMedical = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, AutoSize = true };
			tlMedical.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
			tlMedical.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));
			tlMedical.RowCount = 6;

			txtMainComplaint = new TextBox { Multiline = true, Height = 80, ScrollBars = ScrollBars.Vertical };
			txtMedicalHistory = new TextBox
			{
				Multiline = true,
				Height = 200,
				Width = 400,  // 🔹 تحديد عرض ثابت
				ScrollBars = ScrollBars.Vertical,
				Dock = DockStyle.None  // 🔹 تعطيل الـ Dock
			}; // أكبر الحقل
			   // create the "expand" button and a small panel to host both
			   // زر "تكبير القصة المرضية" بشكل أيقونة صغيرة فقط
			var btnExpandHistory = new Button
			{
				Text = "📝",
				BackColor = ColorTranslator.FromHtml("#DBE2EF"),
				Font = new Font("Segoe UI Emoji", 12, FontStyle.Regular),
				Width = 40,
				Height = 40,
				Margin = new Padding(5),
				FlatStyle = FlatStyle.Flat,
				TextAlign = ContentAlignment.MiddleCenter
			};
			btnExpandHistory.Click += (s, e) =>
			{
				using var dlg = new LargeTextForm("📖 القصة المرضية", txtMedicalHistory.Text);
				if (dlg.ShowDialog(this) == DialogResult.OK)
				{
					txtMedicalHistory.Text = dlg.ResultText;
				}
			};

			// inner panel for medical history: text + expand button
			var pnlHistory = new TableLayoutPanel { ColumnCount = 2, Dock = DockStyle.Fill, AutoSize = true };
			pnlHistory.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
			pnlHistory.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
			pnlHistory.Controls.Add(txtMedicalHistory, 0, 0);
			pnlHistory.Controls.Add(btnExpandHistory, 1, 0);

			AddRow(tlMedical, "الشكوى:", txtMainComplaint, 0);
			AddRow(tlMedical, "القصة المرضية:", pnlHistory, 1);

			medicalGroup.Controls.Add(tlMedical);

			// ========== Buttons ==========
			var pnlButtons = new FlowLayoutPanel { Dock = DockStyle.Bottom, FlowDirection = FlowDirection.RightToLeft, AutoSize = true };
			btnSave = CreateStyledButton("💾 حفظ", "#3F72AF");
			btnCancel = CreateStyledButton("❌ إلغاء", "#DBE2EF");

			btnSave.Click += BtnSave_Click;
			btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

			pnlButtons.Controls.Add(btnSave);
			pnlButtons.Controls.Add(btnCancel);

			// add to main layout
			mainLayout.Controls.Add(personalGroup, 0, 0);
			mainLayout.Controls.Add(medicalGroup, 0, 1);
			mainLayout.Controls.Add(pnlButtons, 0, 2);

			Controls.Add(mainLayout);
		}

		// ---------------- helper: AddRow ----------------
		private void AddRow(TableLayoutPanel tl, string labelText, Control control, int row)
		{
			var lbl = new Label { Text = labelText, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight, AutoSize = true };
			control.Dock = DockStyle.Fill;
			// ensure RowCount and RowStyles are sufficient
			if (tl.RowCount <= row)
			{
				for (int i = tl.RowCount; i <= row; i++)
					tl.RowStyles.Add(new RowStyle(SizeType.AutoSize));
				tl.RowCount = row + 1;
			}
			tl.Controls.Add(lbl, 0, row);
			tl.Controls.Add(control, 1, row);
		}

		private void LoadEnums()
		{
			cbGender.Items.Clear();
			foreach (var g in Enum.GetValues(typeof(Gender)))
				cbGender.Items.Add(g);

			cbMarital.Items.Clear();
			foreach (var m in Enum.GetValues(typeof(MaritalStatus)))
				cbMarital.Items.Add(m);

			if (cbGender.Items.Count > 0) cbGender.SelectedIndex = 0;
			if (cbMarital.Items.Count > 0) cbMarital.SelectedIndex = 0;
		}

		private void PopulateFields()
		{
			if (_patient == null) return;

			txtFileNumber.Text = _patient.FileNumber;
			txtFullName.Text = _patient.FullName;
			numAge.Value = _patient.Age;
			cbGender.SelectedItem = _patient.Gender;
			txtPhone.Text = _patient.Phone;
			txtJob.Text = _patient.Job;
			cbMarital.SelectedItem = _patient.MaritalStatus;
			txtAddress.Text = _patient.Address;
			txtMainComplaint.Text = _patient.MainComplaint;
			txtMedicalHistory.Text = _patient.MedicalHistory;
			txtHabits.Text = _patient.Habits;
		}

		private void BtnSave_Click(object? sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(txtFileNumber.Text))
			{
				MessageBox.Show("⚠ رقم الأضبارة مطلوب", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				txtFileNumber.Focus();
				return;
			}

			if (string.IsNullOrWhiteSpace(txtFullName.Text))
			{
				MessageBox.Show("⚠ الاسم مطلوب", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				txtFullName.Focus();
				return;
			}

			try
			{
				if (_patient == null)
				{
					var p = new Patient
					{
						FileNumber = txtFileNumber.Text.Trim(),
						FullName = txtFullName.Text.Trim(),
						Age = (int)numAge.Value,
						Gender = (Gender)cbGender.SelectedItem,
						Phone = txtPhone.Text.Trim(),
						Job = txtJob.Text.Trim(),
						MaritalStatus = (MaritalStatus)cbMarital.SelectedItem,
						Address = txtAddress.Text.Trim(),
						MainComplaint = txtMainComplaint.Text.Trim(),
						MedicalHistory = txtMedicalHistory.Text.Trim(),
						Habits = txtHabits.Text.Trim()
					};
					_repo.AddPatient(p);
				}
				else
				{
					_patient.FileNumber = txtFileNumber.Text.Trim();
					_patient.FullName = txtFullName.Text.Trim();
					_patient.Age = (int)numAge.Value;
					_patient.Gender = (Gender)cbGender.SelectedItem;
					_patient.Phone = txtPhone.Text.Trim();
					_patient.Job = txtJob.Text.Trim();
					_patient.MaritalStatus = (MaritalStatus)cbMarital.SelectedItem;
					_patient.Address = txtAddress.Text.Trim();
					_patient.MainComplaint = txtMainComplaint.Text.Trim();
					_patient.MedicalHistory = txtMedicalHistory.Text.Trim();
					_patient.Habits = txtHabits.Text.Trim();

					_repo.UpdatePatient(_patient);
				}

				DialogResult = DialogResult.OK;
				Close();
			}
			catch (Exception ex)
			{
				MessageBox.Show("خطأ أثناء الحفظ:\n" + ex.Message, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
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
	}
}
