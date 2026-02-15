using AutoMapper;
using DB.Models;
using DTO.Models;

namespace DTO
{
    // AutoMapper profile
    public class PatientProfile : Profile
    {
        public PatientProfile()
        {
            // Entity -> DTO
            CreateMap<Patient, PatientDTO>()
                .ForMember(d => d.Gender, opt => opt.MapFrom<GenderIdToCodeResolver>())
                .ForMember(d => d.Active, opt => opt.MapFrom<ActiveIdToCodeResolver>())
                .ForMember(d => d.Name, opt => opt.MapFrom(src => src.Name));

            // DTO -> Entity
            CreateMap<PatientDTO, Patient>()
                .ForMember(d => d.GenderId, opt => opt.MapFrom<GenderCodeToIdResolver>())
                .ForMember(d => d.ActiveId, opt => opt.MapFrom<ActiveCodeToIdResolver>())
                .ForMember(d => d.Name, opt => opt.MapFrom(src => src.Name));


            CreateMap<NameDTO, Name>()
                    .ForMember(dest => dest.Patient,
                               opt => opt.Ignore())
                    .ReverseMap()
                    .ForMember(dest => dest.PatientId,
                               opt => opt.MapFrom(src => src.Patient != null ? (Guid?)src.Patient.Id : null));
        }

    }
}
