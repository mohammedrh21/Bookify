using AutoMapper;
using Bookify.Application.DTO;
using Bookify.Application.DTO.Booking;
using Bookify.Application.DTO.Category;
using Bookify.Application.DTO.Service;
using Bookify.Domain.Entities;
using Bookify.Domain.Enums;

namespace Bookify.Application.Mapping
{
    public class MappingConfig : Profile
    {
        public MappingConfig()
        {
            // ── Service ────────────────────────────────────────────
            CreateMap<Service, ServiceResponse>()
                .ForMember(d => d.CategoryName,
                           o => o.MapFrom(s => s.Category != null ? s.Category.Name : string.Empty))
                .ForMember(d => d.StaffName,
                           o => o.MapFrom(s => s.Staff != null ? s.Staff.FullName : string.Empty));

            CreateMap<CreateServiceRequest, Service>();
            CreateMap<UpdateServiceRequest, Service>();

            // ── Category ───────────────────────────────────────────
            CreateMap<Category, CategoryResponse>()
                .ForMember(d => d.ServiceCount,
                           o => o.MapFrom(s => s.Services != null
                               ? s.Services.Count(svc => !svc.IsDeleted)
                               : 0));

            CreateMap<CreateCategoryRequest, Category>();
            CreateMap<UpdateCategoryRequest, Category>();

            // ── Auth / Users ───────────────────────────────────────
            CreateMap<RegisterClientRequest, Client>();
            CreateMap<RegisterStaffRequest, Staff>();

            // ── Booking ────────────────────────────────────────────
            CreateMap<CreateBookingRequest, Booking>()
                .ForMember(x => x.Status, opt => opt.MapFrom(_ => BookingStatus.Pending));

            CreateMap<Booking, BookingResponse>()
                .ForMember(d => d.Status,
                           o => o.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.StaffId,
                           o => o.MapFrom(s => s.Service != null ? s.Service.StaffId : Guid.Empty))
                .ForMember(d => d.ServiceName,
                           o => o.MapFrom(s => s.Service != null ? s.Service.Name : string.Empty))
                .ForMember(d => d.StaffName,
                           o => o.MapFrom(s => s.Service != null && s.Service.Staff != null
                               ? s.Service.Staff.FullName
                               : string.Empty))
                .ForMember(d => d.ClientName,
                           o => o.MapFrom(s => s.Client != null ? s.Client.FullName : string.Empty))
                .ForMember(d => d.Price,
                           o => o.MapFrom(s => s.Service != null ? s.Service.Price : 0m))
                .ForMember(d => d.DurationMinutes,
                           o => o.MapFrom(s => s.Service != null ? s.Service.Duration : 0));
        }
    }
}
