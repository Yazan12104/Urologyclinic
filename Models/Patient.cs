namespace UrologyClinic.Models
{

	public enum Gender
	{
		Male = 0,
		Female = 1,
		Other = 2
	}

	public enum MaritalStatus
	{
		Married = 0,
		Single = 1,
		Widowed = 2,
		Divorced = 3
	}

	public class Patient
	{
		public int Id { get; set; }
		public string FullName { get; set; } = string.Empty;
		public int Age { get; set; }
		public Gender Gender { get; set; }
		public string Phone { get; set; } = string.Empty;
		public string Job { get; set; } = string.Empty;
		public MaritalStatus MaritalStatus { get; set; }
		public string Address { get; set; } = string.Empty;

		// معلومات طبية
		public string MainComplaint { get; set; } = string.Empty;
		public string MedicalHistory { get; set; } = string.Empty;
		public string Habits { get; set; } = string.Empty;
		public string FileNumber { get; set; } = string.Empty;

	}
}
