using System;
using System.Drawing;
using System.Windows.Forms;
using UrologyClinic.Data;
using UrologyClinic.Models;

namespace UrologyClinic.Forms
{
	public class PatientDetailsForm : Form
	{
		private readonly PatientRepository _repo = new PatientRepository();
		private Patient _patient;
		private bool isEditMode = false;

		private TextBox txtFileNumber, txtFullName, txtPhone, txtJob, txtAddress, txtMainComplaint, txtMedicalHistory, txtHabits;
		private NumericUpDown numAge;
		private ComboBox cbGender, cbMarital;
		private Button btnEditSave, btnClose;

		public PatientDetailsForm(Patient patient)
		{
			_patient = patient;
			Text = "📄 تفاصيل المريض";
			Width = 850;
			Height = 700;
			StartPosition = FormStartPosition.CenterParent;
			BackColor = ColorTranslator.FromHtml("#F9F7F7");

			InitializeComponent();
			LoadPatientData();
			SetReadOnlyMode(true);
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

			// ========= Personal Info =========
			var personalGroup = new GroupBox { Text = "🧑‍🤝‍🧑 المعلومات الشخصية", Dock = DockStyle.Top, AutoSize = true };
			var tlPersonal = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, AutoSize = true };
			tlPersonal.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
			tlPersonal.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));

			txtFileNumber = new TextBox();
			txtFullName = new TextBox();
			numAge = new NumericUpDown { Minimum = 0, Maximum = 150 };
			cbGender = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
			cbMarital = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
			txtPhone = new TextBox();
			txtJob = new TextBox();
			txtAddress = new TextBox { Multiline = true, Height = 60, ScrollBars = ScrollBars.Vertical };
			txtHabits = new TextBox { Multiline = true, Height = 50, ScrollBars = ScrollBars.Vertical };

			cbGender.Items.AddRange(Enum.GetNames(typeof(Gender)));
			cbMarital.Items.AddRange(Enum.GetNames(typeof(MaritalStatus)));

			AddRow(tlPersonal, "رقم الأضبارة:", txtFileNumber, 0);
			AddRow(tlPersonal, "الاسم:", txtFullName, 1);
			AddRow(tlPersonal, "العمر:", numAge, 2);
			AddRow(tlPersonal, "الجنس:", cbGender, 3);
			AddRow(tlPersonal, "الهاتف:", txtPhone, 4);
			AddRow(tlPersonal, "الوظيفة:", txtJob, 5);
			AddRow(tlPersonal, "الحالة الاجتماعية:", cbMarital, 6);
			AddRow(tlPersonal, "العنوان:", txtAddress, 7);
			AddRow(tlPersonal, "العادات:", txtHabits, 8); // ✅ نقلناها هنا

			personalGroup.Controls.Add(tlPersonal);

			// ========= Medical Info =========
			var medicalGroup = new GroupBox { Text = "🩺 المعلومات الطبية", Dock = DockStyle.Top, AutoSize = true };
			var tlMedical = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, AutoSize = true };
			tlMedical.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
			tlMedical.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));

			txtMainComplaint = new TextBox { Multiline = true, Height = 60, ScrollBars = ScrollBars.Vertical };
			txtMedicalHistory = new TextBox
			{
				Multiline = true,
				Height = 200,
				Width = 400,  // 🔹 تحديد عرض ثابت
				ScrollBars = ScrollBars.Vertical,
				Dock = DockStyle.None  // 🔹 تعطيل الـ Dock
			};

			// زر "تكبير القصة المرضية"
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

			// Panel يحتوي على TextBox + زر تكبير
			var pnlHistory = new TableLayoutPanel { ColumnCount = 2, Dock = DockStyle.Fill, AutoSize = true };
			pnlHistory.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
			pnlHistory.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
			pnlHistory.Controls.Add(txtMedicalHistory, 0, 0);
			pnlHistory.Controls.Add(btnExpandHistory, 1, 0);

			AddRow(tlMedical, "الشكوى:", txtMainComplaint, 0);
			AddRow(tlMedical, "القصة المرضية:", pnlHistory, 1);

			medicalGroup.Controls.Add(tlMedical);

			// ========= Buttons =========
			var pnlButtons = new FlowLayoutPanel { Dock = DockStyle.Bottom, FlowDirection = FlowDirection.RightToLeft };
			btnEditSave = CreateStyledButton("✏ تعديل", "#3F72AF");
			btnClose = CreateStyledButton("❌ إغلاق", "#DBE2EF");

			btnEditSave.Click += BtnEditSave_Click;
			btnClose.Click += (s, e) => Close();

			pnlButtons.Controls.Add(btnClose);
			pnlButtons.Controls.Add(btnEditSave);

			// Add to main layout
			mainLayout.Controls.Add(personalGroup, 0, 0);
			mainLayout.Controls.Add(medicalGroup, 0, 1);
			mainLayout.Controls.Add(pnlButtons, 0, 2);

			Controls.Add(mainLayout);
		}

		private void AddRow(TableLayoutPanel tl, string label, Control ctrl, int row)
		{
			var lbl = new Label { Text = label, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
			ctrl.Dock = DockStyle.Fill;

			if (tl.RowCount <= row)
			{
				for (int i = tl.RowCount; i <= row; i++)
					tl.RowStyles.Add(new RowStyle(SizeType.AutoSize));
				tl.RowCount = row + 1;
			}

			tl.Controls.Add(lbl, 0, row);
			tl.Controls.Add(ctrl, 1, row);
		}

		private void LoadPatientData()
		{
			txtFileNumber.Text = _patient.FileNumber;
			txtFullName.Text = _patient.FullName;
			numAge.Value = _patient.Age;
			cbGender.SelectedItem = _patient.Gender.ToString();
			cbMarital.SelectedItem = _patient.MaritalStatus.ToString();
			txtPhone.Text = _patient.Phone;
			txtJob.Text = _patient.Job;
			txtAddress.Text = _patient.Address;
			txtMainComplaint.Text = _patient.MainComplaint;
			txtMedicalHistory.Text = _patient.MedicalHistory;
			txtHabits.Text = _patient.Habits;
		}

		private void BtnEditSave_Click(object? sender, EventArgs e)
		{
			if (!isEditMode)
			{
				isEditMode = true;
				SetReadOnlyMode(false);
				btnEditSave.Text = "💾 حفظ";
			}
			else
			{
				SaveChanges();
				isEditMode = false;
				SetReadOnlyMode(true);
				btnEditSave.Text = "✏ تعديل";
				DialogResult = DialogResult.OK;
			}
		}

		private void SaveChanges()
		{
			_patient.FileNumber = txtFileNumber.Text.Trim();
			_patient.FullName = txtFullName.Text.Trim();
			_patient.Age = (int)numAge.Value;
			_patient.Gender = Enum.Parse<Gender>(cbGender.SelectedItem!.ToString()!);
			_patient.MaritalStatus = Enum.Parse<MaritalStatus>(cbMarital.SelectedItem!.ToString()!);
			_patient.Phone = txtPhone.Text.Trim();
			_patient.Job = txtJob.Text.Trim();
			_patient.Address = txtAddress.Text.Trim();
			_patient.MainComplaint = txtMainComplaint.Text.Trim();
			_patient.MedicalHistory = txtMedicalHistory.Text.Trim();
			_patient.Habits = txtHabits.Text.Trim();

			_repo.UpdatePatient(_patient);
			MessageBox.Show("✅ تم حفظ التعديلات بنجاح", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		private void SetReadOnlyMode(bool readOnly)
		{
			txtFileNumber.ReadOnly = readOnly;
			txtFullName.ReadOnly = readOnly;
			numAge.Enabled = !readOnly;
			cbGender.Enabled = !readOnly;
			cbMarital.Enabled = !readOnly;
			txtPhone.ReadOnly = readOnly;
			txtJob.ReadOnly = readOnly;
			txtAddress.ReadOnly = readOnly;
			txtMainComplaint.ReadOnly = readOnly;
			txtMedicalHistory.ReadOnly = readOnly;
			txtHabits.ReadOnly = readOnly;
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
