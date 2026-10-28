using System.ComponentModel.DataAnnotations;

namespace FoodOrder.Shared.Requests;

public class ChangeOrderStatusRequest
{
    [Required]
    public string NewStatus { get; set; } = string.Empty;

    public int ExpectedVersion { get; set; }
}