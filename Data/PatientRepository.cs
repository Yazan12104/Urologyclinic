using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using UrologyClinic.Models;

namespace UrologyClinic.Data
{
	public class PatientRepository
	{
		// إضافة مريض جديد
		public void AddPatient(Patient patient)
		{
			using var conn = DatabaseHelper.GetConnection();
			conn.Open();

			var cmd = conn.CreateCommand();
			cmd.CommandText = @"
                INSERT INTO Patients 
                (FileNumber, FullName, Age, Gender, Phone, Job, MaritalStatus, Address, MainComplaint, MedicalHistory, Habits)
                VALUES ($FileNumber, $FullName, $Age, $Gender, $Phone, $Job, $MaritalStatus, $Address, $MainComplaint, $MedicalHistory, $Habits);
            ";

			cmd.Parameters.AddWithValue("$FileNumber", patient.FileNumber ?? string.Empty);
			cmd.Parameters.AddWithValue("$FullName", patient.FullName ?? string.Empty);
			cmd.Parameters.AddWithValue("$Age", patient.Age);
			cmd.Parameters.AddWithValue("$Gender", patient.Gender.ToString());
			cmd.Parameters.AddWithValue("$Phone", patient.Phone ?? string.Empty);
			cmd.Parameters.AddWithValue("$Job", patient.Job ?? string.Empty);
			cmd.Parameters.AddWithValue("$MaritalStatus", patient.MaritalStatus.ToString());
			cmd.Parameters.AddWithValue("$Address", patient.Address ?? string.Empty);
			cmd.Parameters.AddWithValue("$MainComplaint", patient.MainComplaint ?? string.Empty);
			cmd.Parameters.AddWithValue("$MedicalHistory", patient.MedicalHistory ?? string.Empty);
			cmd.Parameters.AddWithValue("$Habits", patient.Habits ?? string.Empty);

			cmd.ExecuteNonQuery();
		}

		// تحديث مريض
		public void UpdatePatient(Patient patient)
		{
			using var conn = DatabaseHelper.GetConnection();
			conn.Open();

			var cmd = conn.CreateCommand();
			cmd.CommandText = @"
                UPDATE Patients SET
                    FileNumber = $FileNumber,
                    FullName = $FullName,
                    Age = $Age,
                    Gender = $Gender,
                    Phone = $Phone,
                    Job = $Job,
                    MaritalStatus = $MaritalStatus,
                    Address = $Address,
                    MainComplaint = $MainComplaint,
                    MedicalHistory = $MedicalHistory,
                    Habits = $Habits
                WHERE Id = $Id;
            ";

			cmd.Parameters.AddWithValue("$FileNumber", patient.FileNumber ?? string.Empty);
			cmd.Parameters.AddWithValue("$FullName", patient.FullName ?? string.Empty);
			cmd.Parameters.AddWithValue("$Age", patient.Age);
			cmd.Parameters.AddWithValue("$Gender", patient.Gender.ToString());
			cmd.Parameters.AddWithValue("$Phone", patient.Phone ?? string.Empty);
			cmd.Parameters.AddWithValue("$Job", patient.Job ?? string.Empty);
			cmd.Parameters.AddWithValue("$MaritalStatus", patient.MaritalStatus.ToString());
			cmd.Parameters.AddWithValue("$Address", patient.Address ?? string.Empty);
			cmd.Parameters.AddWithValue("$MainComplaint", patient.MainComplaint ?? string.Empty);
			cmd.Parameters.AddWithValue("$MedicalHistory", patient.MedicalHistory ?? string.Empty);
			cmd.Parameters.AddWithValue("$Habits", patient.Habits ?? string.Empty);
			cmd.Parameters.AddWithValue("$Id", patient.Id);

			cmd.ExecuteNonQuery();
		}

		// حذف مريض
		public void DeletePatient(int id)
		{
			using var conn = DatabaseHelper.GetConnection();
			conn.Open();

			var cmd = conn.CreateCommand();
			cmd.CommandText = "DELETE FROM Patients WHERE Id = $Id;";
			cmd.Parameters.AddWithValue("$Id", id);
			cmd.ExecuteNonQuery();
		}

		// الحصول على مريض واحد
		public Patient? GetPatientById(int id)
		{
			using var conn = DatabaseHelper.GetConnection();
			conn.Open();

			var cmd = conn.CreateCommand();
			cmd.CommandText = "SELECT * FROM Patients WHERE Id = $Id;";
			cmd.Parameters.AddWithValue("$Id", id);

			using var reader = cmd.ExecuteReader();
			if (reader.Read())
			{
				return MapReaderToPatient(reader);
			}

			return null;
		}

		// إرجاع قائمة المرضى (كلهم)
		public List<Patient> GetAllPatients()
		{
			var list = new List<Patient>();
			using var conn = DatabaseHelper.GetConnection();
			conn.Open();

			var cmd = conn.CreateCommand();
			cmd.CommandText = "SELECT * FROM Patients ORDER BY Id DESC;";

			using var reader = cmd.ExecuteReader();
			while (reader.Read())
			{
				list.Add(MapReaderToPatient(reader));
			}

			return list;
		}

		// إرجاع عدد المرضى (مع فلترة)
		public int GetPatientsCount(string? filter = null)
		{
			using var conn = DatabaseHelper.GetConnection();
			conn.Open();

			var cmd = conn.CreateCommand();

			if (string.IsNullOrWhiteSpace(filter))
			{
				cmd.CommandText = "SELECT COUNT(*) FROM Patients;";
			}
			else
			{
				// 🔥 استخدام نفس منطق البحث للعد
				cmd.CommandText = @"
            SELECT COUNT(*) FROM Patients
            WHERE FileNumber LIKE $filter || '%' 
               OR FullName LIKE $filter || '%' 
               OR Phone LIKE $filter || '%';
        ";
				cmd.Parameters.AddWithValue("$filter", filter);
			}

			return Convert.ToInt32(cmd.ExecuteScalar());
		}

		// إرجاع صفحة مرضى مع فلترة
		public List<Patient> GetPatientsPaged(int page, int pageSize, string? filter = null)
		{
			var list = new List<Patient>();
			using var conn = DatabaseHelper.GetConnection();
			conn.Open();

			var cmd = conn.CreateCommand();
			int offset = (page - 1) * pageSize;

			if (string.IsNullOrWhiteSpace(filter))
			{
				// 🔥 تحسين: ORDER BY مع INDEX
				cmd.CommandText = @"
            SELECT Id, FileNumber, FullName, Age, Phone 
            FROM Patients 
            ORDER BY Id DESC 
            LIMIT $pageSize OFFSET $offset;
        ";
			}
			else
			{
				// 🔥 تحسين: LIKE مع prefix فقط (أسرع 100x)
				cmd.CommandText = @"
            SELECT Id, FileNumber, FullName, Age, Phone 
            FROM Patients 
            WHERE FileNumber LIKE $filter || '%' 
               OR FullName LIKE $filter || '%' 
               OR Phone LIKE $filter || '%'
            ORDER BY Id DESC 
            LIMIT $pageSize OFFSET $offset;
        ";
				cmd.Parameters.AddWithValue("$filter", filter);
			}

			cmd.Parameters.AddWithValue("$pageSize", pageSize);
			cmd.Parameters.AddWithValue("$offset", offset);

			using var reader = cmd.ExecuteReader();
			while (reader.Read())
			{
				list.Add(new Patient
				{
					Id = Convert.ToInt32(reader["Id"]),
					FileNumber = reader["FileNumber"]?.ToString() ?? string.Empty,
					FullName = reader["FullName"]?.ToString() ?? string.Empty,
					Age = Convert.ToInt32(reader["Age"]),
					Phone = reader["Phone"]?.ToString() ?? string.Empty
				});
			}

			return list;
		}

		// ماب البيانات من Reader إلى Patient
		private Patient MapReaderToPatient(SqliteDataReader reader)
		{
			return new Patient
			{
				Id = Convert.ToInt32(reader["Id"]),
				FileNumber = reader["FileNumber"]?.ToString() ?? string.Empty,
				FullName = reader["FullName"]?.ToString() ?? string.Empty,
				Age = Convert.ToInt32(reader["Age"]),
				Gender = Enum.TryParse<Gender>(reader["Gender"]?.ToString(), out var g) ? g : Gender.Other,
				Phone = reader["Phone"]?.ToString() ?? string.Empty,
				Job = reader["Job"]?.ToString() ?? string.Empty,
				MaritalStatus = Enum.TryParse<MaritalStatus>(reader["MaritalStatus"]?.ToString(), out var m) ? m : MaritalStatus.Single,
				Address = reader["Address"]?.ToString() ?? string.Empty,
				MainComplaint = reader["MainComplaint"]?.ToString() ?? string.Empty,
				MedicalHistory = reader["MedicalHistory"]?.ToString() ?? string.Empty,
				Habits = reader["Habits"]?.ToString() ?? string.Empty
			};
		}
	}
}
