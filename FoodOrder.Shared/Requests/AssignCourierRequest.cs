namespace FoodOrder.Shared.Requests;

public class AssignCourierRequest
{
    public int CourierId { get; set; }
    public int ExpectedVersion { get; set; }
}