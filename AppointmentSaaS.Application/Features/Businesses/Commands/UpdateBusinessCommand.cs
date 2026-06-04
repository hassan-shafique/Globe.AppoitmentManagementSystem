using AppointmentSaaS.Application.Common.Exceptions;
using AppointmentSaaS.Application.DTOs.Businesses;
using AppointmentSaaS.Domain.Enums;
using AppointmentSaaS.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace AppointmentSaaS.Application.Features.Businesses.Commands;

public record UpdateBusinessCommand(
    Guid Id,
    string Name,
    BusinessType Type,
    string? Address,
    string? City,
    string? Phone,
    string? Email) : IRequest<BusinessDto>;

public class UpdateBusinessCommandHandler(
    IBusinessRepository businessRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper)
    : IRequestHandler<UpdateBusinessCommand, BusinessDto>
{
    public async Task<BusinessDto> Handle(UpdateBusinessCommand request, CancellationToken ct)
    {
        var business = await businessRepository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.Business), request.Id);

        business.Update(request.Name, request.Type, request.Address, request.City, request.Phone, request.Email);
        await businessRepository.UpdateAsync(business, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return mapper.Map<BusinessDto>(business);
    }
}
