using Cloudland.APM.Mapper.Profiles;
using Cloudland.APM.Mapper.Profiles.CustomConverter;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.DTO.ViewDTO;
using Cloudland.SSSB.Models;
using Cloudland.SSSB.Models.View;

namespace Cloudland.SSSB.Mapper.Profiles;

public sealed class SSSBProfiles : BaseProfiles
{
    public SSSBProfiles()
    {
        CreateMap<DateTime?, string?>().ConvertUsing(new DateTimeToPersianDateTimeConverter());
        CreateMap<string?, DateTime?>().ConvertUsing(new PersianDateToDateTimeConverter());
        CreateMap<DateTime, string>().ConvertUsing(new DateTimeToPersianDateTimeNonNullableConverter());
        CreateMap<string, DateTime>().ConvertUsing(new PersianDateToDateTimeNonNullableConverter());
        
        CreateMap<OrganizationalChart, OrganizationalChartDTO>().ReverseMap();
        CreateMap<AccountNumber, AccountNumberDTO>().ReverseMap();
        CreateMap<Vehicle, VehicleDTO>().ReverseMap();
        CreateMap<Contractor, ContractorDTO>().ReverseMap();
        CreateMap<Meal, MealDTO>().ReverseMap();
        CreateMap<Food, FoodDTO>().ReverseMap();
        CreateMap<Location, LocationDTO>().ReverseMap();
        CreateMap<ContractFoodCalendar, ContractFoodCalendarDTO>().ReverseMap();
        CreateMap<WeekDay, WeekDayDTO>().ReverseMap();
        CreateMap<Personnel, PersonnelDTO>().ReverseMap();
        CreateMap<VehicleDriver, VehicleDriverDTO>().ReverseMap();
        CreateMap<Room, RoomDTO>().ReverseMap();
        CreateMap<Bed, BedDTO>().ReverseMap();
        CreateMap<OrganizationalPosition, OrganizationalPositionDTO>().ReverseMap();
        CreateMap<ContractLocation, ContractLocationDTO>().ReverseMap();
        CreateMap<Contract, ContractDTO>().ReverseMap();
        CreateMap<ContractRecursive, ContractRecursiveDTO>()
            .ForMember(dest => dest.level, opt => opt.MapFrom(src => src.Level))
            .ReverseMap()
            .ForMember(dest => dest.Level, opt => opt.MapFrom(src => src.level));
        CreateMap<ContractFood, ContractFoodDTO>().ReverseMap();
        CreateMap<ContractFoodLocation, ContractFoodLocationDTO>().ReverseMap();
        CreateMap<ReservationRequestHeader, ReservationRequestHeaderDTO>().ReverseMap();
        CreateMap<ReservationRequestLetter, ReservationRequestLetterDTO>().ReverseMap();
        CreateMap<ReservationFoodCalendar, ReservationFoodCalendarDTO>().ReverseMap();
        CreateMap<ReservationRequestBed, ReservationRequestBedDTO>().ReverseMap();
        CreateMap<ReservationRequestPerson, ReservationRequestPersonDTO>().ReverseMap();
        
        // View mappings
        CreateMap<LocationView, LocationDTO>();
        CreateMap<ContractFoodCalendarView, ContractFoodCalendarDTO>();
        CreateMap<WeekDayView, WeekDayDTO>();
        CreateMap<OrganizationalChartView, OrganizationalChartDTO>();
        CreateMap<PersonnelView, PersonnelDTO>();
        CreateMap<VehicleView, VehicleDTO>();
        CreateMap<FoodView, FoodDTO>();
        CreateMap<OrganizationalPositionView, OrganizationalPositionDTO>();
        CreateMap<MealView, MealDTO>();
        CreateMap<ContractView, ContractDTO>();
        CreateMap<ContractLocationView, ContractLocationDTO>();
        CreateMap<ContractFoodView, ContractFoodDTO>();
        CreateMap<ContractorView, ContractorDTO>();
        CreateMap<ReservationRequestHeaderView, ReservationRequestHeaderDTO>();
        
        // View and ViewDTO mappings
        CreateMap<LocationView, LocationViewDTO>();
        CreateMap<ContractFoodCalendarView, ContractFoodCalendarViewDTO>();
        CreateMap<WeekDayView, WeekDayViewDTO>();
        CreateMap<OrganizationalChartView, OrganizationalChartViewDTO>();
        CreateMap<PersonnelView, PersonnelViewDTO>();
        CreateMap<VehicleView, VehicleViewDTO>();
        CreateMap<FoodView, FoodViewDTO>();
        CreateMap<OrganizationalPositionView, OrganizationalPositionViewDTO>();
        CreateMap<MealView, MealViewDTO>();
        CreateMap<ContractView, ContractViewDTO>();
        CreateMap<ContractLocationView, ContractLocationViewDTO>();
        CreateMap<ContractFoodView, ContractFoodViewDTO>();
        CreateMap<ContractFoodLocationView, ContractFoodLocationViewDTO>();
        CreateMap<ContractorView, ContractorViewDTO>();
        CreateMap<ReservationRequestHeaderView, ReservationRequestHeaderViewDTO>();
        CreateMap<ReservationRequestBedView, ReservationRequestBedViewDTO>();
        CreateMap<ReservationRequestLetterView, ReservationRequestLetterViewDTO>();
        CreateMap<ReservationRequestPersonView, ReservationRequestPersonViewDTO>();
        CreateMap<RoomView, RoomViewDTO>();
        CreateMap<BedView, BedViewDTO>();
        CreateMap<VehicleDriverView, VehicleDriverViewDTO>();
        CreateMap<AccountNumberView, AccountNumberViewDTO>();
        CreateMap<ReservationFoodCalendarView, ReservationFoodCalendarViewDTO>();
    }
}