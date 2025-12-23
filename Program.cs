using System;
using System.IO;
using System.Windows.Forms;
using UrologyClinic.Data;

namespace UrologyClinic
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            try
            {
                DatabaseHelper.InitializeDatabase();
            }
            catch (Exception ex)
            {
                File.WriteAllText("startup_error.txt", ex.ToString());
                MessageBox.Show("فشل تهيئة قاعدة البيانات:\n" + ex.Message + "\nتفاصيل محفوظة في startup_error.txt",
                                "خطأ عند بدء التشغيل", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var mainForm = new Forms.PatientsListForm();
            mainForm.Icon = new Icon("Resources/app.ico");
            Application.Run(mainForm);
        }
    }
}
