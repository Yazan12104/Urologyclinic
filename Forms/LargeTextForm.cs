using System;
using System.Drawing;
using System.Windows.Forms;

namespace UrologyClinic.Forms
{
	public class LargeTextForm : Form
	{
		private TextBox txtContent;
		private Button btnSave, btnCancel;
		public string ResultText { get; private set; } = string.Empty;

		public LargeTextForm(string title, string initialText)
		{
			Text = title;
			Width = 900;
			Height = 650;
			StartPosition = FormStartPosition.CenterParent;
			BackColor = ColorTranslator.FromHtml("#F9F7F7");

			txtContent = new TextBox
			{
				Multiline = true,
				ScrollBars = ScrollBars.Vertical,
				Dock = DockStyle.Fill,
				Font = new Font("Segoe UI", 11),
				Text = initialText ?? string.Empty
			};

			btnSave = new Button { Text = "💾 حفظ", Width = 110, Height = 36, Dock = DockStyle.Right };
			btnCancel = new Button { Text = "❌ إلغاء", Width = 110, Height = 36, Dock = DockStyle.Right };

			btnSave.Click += (s, e) => { ResultText = txtContent.Text; DialogResult = DialogResult.OK; Close(); };
			btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

			var pnlButtons = new Panel { Dock = DockStyle.Bottom, Height = 50, Padding = new Padding(8) };
			pnlButtons.Controls.Add(btnCancel);
			pnlButtons.Controls.Add(btnSave);

			Controls.Add(txtContent);
			Controls.Add(pnlButtons);
		}
	}
}
