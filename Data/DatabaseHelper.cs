using System.IO;
using Microsoft.Data.Sqlite;

namespace UrologyClinic.Data
{
    public static class DatabaseHelper
    {
        private static string dbFile = "clinic.db";
        private static string connectionString = $"Data Source={dbFile}";

        public static void InitializeDatabase()
        {
            // ينشئ الملف إذا لم يكن موجودًا
            if (!File.Exists(dbFile))
            {
                using var conn = new SqliteConnection(connectionString);
                conn.Open();
            }

            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();

            cmd.CommandText = @"
    CREATE TABLE IF NOT EXISTS Patients (
        Id INTEGER PRIMARY KEY AUTOINCREMENT,
        FileNumber TEXT UNIQUE,
        FullName TEXT NOT NULL,
        Age INTEGER,
        Gender INTEGER,
        Phone TEXT,
        Job TEXT,
        MaritalStatus INTEGER,
        Address TEXT,
        MainComplaint TEXT,
        MedicalHistory TEXT,
        Habits TEXT
    );
";
            cmd.ExecuteNonQuery();
            BackupManager.InitializeBackupSystem();
            try
            {
                using var indexCmd = connection.CreateCommand();
                indexCmd.CommandText = @"
            -- 🔹 Indexes للبحث السريع
            CREATE INDEX IF NOT EXISTS idx_patients_filenumber ON Patients(FileNumber);
            CREATE INDEX IF NOT EXISTS idx_patients_fullname ON Patients(FullName);
            CREATE INDEX IF NOT EXISTS idx_patients_phone ON Patients(Phone);
            CREATE INDEX IF NOT EXISTS idx_patients_id_desc ON Patients(Id DESC);
            
            -- 🔹 Index للترتيب السريع
            CREATE INDEX IF NOT EXISTS idx_patients_created_date ON Patients(Id DESC);
        ";
                indexCmd.ExecuteNonQuery();
                Console.WriteLine("✅ تم إنشاء Indexes بنجاح");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ تحذير في إنشاء Indexes: {ex.Message}");
            }
        }

        public static SqliteConnection GetConnection() => new SqliteConnection(connectionString);
    }
}
