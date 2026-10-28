using FoodOrder.Domain.Entities;
using FoodOrder.Domain.Enums;
using FoodOrder.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FoodOrder.Infrastructure.Persistence;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (!await db.Dishes.AnyAsync())
        {
            db.Dishes.AddRange(
                Dish.Create("Újházi tyúkhúsleves", "Gazdag, ízletes házi leves", DishCategory.Soup, Money.Huf(1890)),
                Dish.Create("Gulyásleves", "Hagyományos magyar gulyás", DishCategory.Soup, Money.Huf(2190)),
                Dish.Create("Rántott sertéskaraj", "Krumplipürével", DishCategory.MainCourse, Money.Huf(3490)),
                Dish.Create("Csirkepaprikás", "Házi nokedlivel", DishCategory.MainCourse, Money.Huf(3290)),
                Dish.Create("Somlói galuska", "Klasszikus magyar desszert", DishCategory.Dessert, Money.Huf(1490)),
                Dish.Create("Házi limonádé", "Frissítő citrusos ital", DishCategory.Drink, Money.Huf(990))
            );
        }

        if (!await db.Couriers.AnyAsync())
        {
            db.Couriers.AddRange(
                Courier.Create("Vincze Gábor", "+36 20 123 4567"),
                Courier.Create("Kovács Béla", "+36 30 987 6543")
            );
        }

        await db.SaveChangesAsync();
    }
}