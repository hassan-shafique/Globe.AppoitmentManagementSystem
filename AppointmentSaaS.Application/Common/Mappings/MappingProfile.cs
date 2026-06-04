using AppointmentSaaS.Application.DTOs.Appointments;
using AppointmentSaaS.Application.DTOs.Businesses;
using AppointmentSaaS.Application.DTOs.Services;
using AppointmentSaaS.Application.DTOs.Staff;
using AppointmentSaaS.Application.DTOs.Tenants;
using AppointmentSaaS.Domain.Entities;
using AutoMapper;

namespace AppointmentSaaS.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Tenant, TenantDto>();
        CreateMap<Business, BusinessDto>();
        CreateMap<Service, ServiceDto>();
        CreateMap<Staff, StaffDto>();
        CreateMap<Appointment, AppointmentDto>()
            .ForMember(d => d.ServiceName, o => o.MapFrom(s => s.Service != null ? s.Service.Name : string.Empty))
            .ForMember(d => d.StaffName, o => o.MapFrom(s => s.Staff != null ? s.Staff.FullName : string.Empty))
            .ForMember(d => d.ClientName, o => o.MapFrom(s => s.Client != null ? s.Client.FullName : string.Empty));
    }
}
