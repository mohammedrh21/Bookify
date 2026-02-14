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
            CreateMap<Service, ServiceResponse>().ReverseMap();
            CreateMap<CreateServiceRequest, Service>();
            CreateMap<UpdateServiceRequest, Service>();

            CreateMap<Category, CategoryResponse>().ReverseMap();
            CreateMap<CreateCategoryRequest, Category>();
            CreateMap<UpdateCategoryRequest, Category>();


            CreateMap<RegisterClientRequest, Client>();
            CreateMap<RegisterStaffRequest, Staff>();

            CreateMap<CreateBookingRequest, Booking>()
                .ForMember(x => x.Status, opt => opt.MapFrom(_ => BookingStatus.Pending));
            CreateMap<Booking, BookingResponse>()
            .ForMember(
                dest => dest.Status,
                opt => opt.MapFrom(src => src.Status.ToString())
            );
        }
    }
}
