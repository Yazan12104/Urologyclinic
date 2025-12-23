using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace UrologyClinic.Data
{
	public static class BackupManager
	{
		private static string backupFolder = Path.Combine(Application.StartupPath, "Backups");
		private static string sourceDbPath = "clinic.db";

		public static void InitializeBackupSystem()
		{
			// إنشاء مجلد النسخ الاحتياطي إذا لم يكن موجوداً
			if (!Directory.Exists(backupFolder))
			{
				Directory.CreateDirectory(backupFolder);
			}
		}

		public static bool CreateBackup(string? backupName = null)
		{
			try
			{
				// التأكد من وجود قاعدة البيانات الأصلية
				if (!File.Exists(sourceDbPath))
				{
					MessageBox.Show("لم يتم العثور على قاعدة البيانات الأصلية.", "تحذير", MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return false;
				}

				string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
				string fileName = backupName ?? $"Backup_{timestamp}.db";
				string backupPath = Path.Combine(backupFolder, fileName);

				// نسخ الملف
				File.Copy(sourceDbPath, backupPath, true);

				MessageBox.Show($"تم إنشاء النسخ الاحتياطي بنجاح:\n{fileName}", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return true;
			}
			catch (Exception ex)
			{
				MessageBox.Show($"فشل في إنشاء النسخ الاحتياطي: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}
		}

		public static bool RestoreBackup(string backupFilePath)
		{
			try
			{
				if (!File.Exists(backupFilePath))
				{
					MessageBox.Show("لم يتم العثور على ملف النسخ الاحتياطي المحدد.", "تحذير", MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return false;
				}

				string currentDb = sourceDbPath;
				string tempBackup = Path.Combine(backupFolder, $"temp_restore_{DateTime.Now:yyyyMMdd_HHmmss}.db");

				// حفظ نسخة احتياطية من قاعدة البيانات الحالية قبل الاستعادة
				if (File.Exists(currentDb))
				{
					File.Copy(currentDb, tempBackup, true);
				}

				// استعادة النسخة الاحتياطية
				File.Copy(backupFilePath, currentDb, true);

				MessageBox.Show("تم استعادة النسخ الاحتياطي بنجاح.\nسيتم إعادة تشغيل التطبيق لتطبيق التغييرات.",
					"نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);

				// إعادة تشغيل التطبيق
				Application.Restart();

				return true;
			}
			catch (Exception ex)
			{
				MessageBox.Show($"فشل في استعادة النسخ الاحتياطي: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}
		}

		public static string[] GetAvailableBackups()
		{
			if (!Directory.Exists(backupFolder))
				return new string[0];

			return Directory.GetFiles(backupFolder, "Backup_*.db")
						   .OrderByDescending(f => File.GetCreationTime(f))
						   .ToArray();
		}

		public static bool DeleteBackup(string backupFilePath)
		{
			try
			{
				if (File.Exists(backupFilePath))
				{
					File.Delete(backupFilePath);
					return true;
				}
				return false;
			}
			catch (Exception ex)
			{
				MessageBox.Show($"فشل في حذف النسخ الاحتياطي: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}
		}

		public static string GetBackupInfo(string backupFilePath)
		{
			if (!File.Exists(backupFilePath))
				return "غير متوفر";

			var fileInfo = new FileInfo(backupFilePath);
			return $"الاسم: {fileInfo.Name}\nالحجم: {fileInfo.Length / 1024} KB\nالتاريخ: {fileInfo.CreationTime:yyyy-MM-dd HH:mm:ss}";
		}
	}
}