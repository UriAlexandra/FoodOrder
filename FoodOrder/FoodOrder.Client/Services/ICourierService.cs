using FoodOrder.Shared.Models;
using FoodOrder.Shared.Requests;

namespace FoodOrder.Client.Services;

public interface ICourierService
{
    Task<List<CourierDto>> GetCouriersAsync();
    Task<CourierDto> AddCourierAsync(CreateCourierRequest request);
    Task<CourierDto> SetActiveAsync(int courierId, bool isActive);
}
