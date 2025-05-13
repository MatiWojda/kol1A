namespace kol1A.Models.DTOs;
public class AppointmentServiceDto
{
    public string Name { get; set; } = string.Empty;
    public decimal ServiceFee { get; set; }
}

public class AppointmentDto
{
    public DateTime Date { get; set; }
    public PatientDto Patient { get; set; } = new PatientDto();
    public DoctorDto Doctor { get; set; } = new DoctorDto();
    public List<AppointmentServiceDto> Services { get; set; } = [];
}

public class PatientDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
}

public class DoctorDto
{
    public int DoctorId { get; set; }
    public string pwz { get; set; } = string.Empty;
}