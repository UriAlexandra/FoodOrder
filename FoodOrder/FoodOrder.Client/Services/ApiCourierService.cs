using FoodOrder.Shared.Models;
using FoodOrder.Shared.Requests;

namespace FoodOrder.Client.Services;

public class ApiCourierService(ApiClient api) : ICourierService
{
    public Task<List<CourierDto>> GetCouriersAsync() =>
        api.GetAsync<List<CourierDto>>("api/couriers");

    public Task<CourierDto> AddCourierAsync(CreateCourierRequest request) =>
        api.PostAsync<CreateCourierRequest, CourierDto>("api/couriers", request);

    public Task<CourierDto> SetActiveAsync(int courierId, bool isActive) =>
        api.PatchAsync<SetCourierActiveRequest, CourierDto>(
            $"api/couriers/{courierId}/active", new SetCourierActiveRequest { IsActive = isActive });
}
