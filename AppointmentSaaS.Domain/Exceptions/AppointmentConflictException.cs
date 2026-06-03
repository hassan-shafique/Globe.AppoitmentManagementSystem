namespace AppointmentSaaS.Domain.Exceptions;

public class AppointmentConflictException : DomainException
{
    public AppointmentConflictException(DateTime startTime, DateTime endTime)
        : base($"Appointment conflict detected for time slot {startTime:g} - {endTime:g}.") { }
}
