using EnumDictDemo.Models.Dto;

namespace EnumDictDemo.Services;

public interface IEnumInfoService
{
    List<EnumOptionResponse> GetEnumOptions(string enumName);
    Dictionary<string, List<EnumOptionResponse>> GetAllEnums();
}