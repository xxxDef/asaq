namespace Asaq.Samples.WebApi.Model;

public enum Developer
{
    Apple, Google, Samsung, Xiaomi, Nokia, Sony,  Huawei,
}

public record Phone(string Name, int Year, Developer Company, double Weight) { }

public class FakeDatabase
{
    readonly private Phone[] phones = [
        new Phone("iPhone 15", 2024, Developer.Apple, Weight: 220.10),
        new Phone("Galaxy S22", 2024, Developer.Samsung, Weight: 200.5),
        new Phone("Pixel 8", 2023, Developer.Google, Weight: 198.8),
        new Phone("Redmi Note 12", 2024, Developer.Xiaomi, Weight: 203.9),
        new Phone("Xperia 1 V", 2023, Developer.Sony, Weight: 193.6),
        new Phone("Nokia X30", 2022, Developer.Nokia, Weight: 185.4),
        new Phone("Huawei P50 Pro", 2022, Developer.Huawei, Weight: 209.3),
        new Phone("iPhone SE 3", 2022, Developer.Apple, Weight: 144.0),
        new Phone("Galaxy A53", 2023, Developer.Samsung, Weight: 189.9),
        new Phone("Pixel 7a", 2023, Developer.Google, Weight: 193.5),
        new Phone("Mi 12 Pro", 2023, Developer.Xiaomi, Weight: 204.2),
        new Phone("Xperia 5 IV", 2023, Developer.Sony, Weight: 172.0),
        new Phone("Nokia G50", 2022, Developer.Nokia, Weight: 220.2),
        new Phone("Huawei Mate 50", 2023, Developer.Huawei, Weight: 206.5),
        new Phone("iPhone 14 Pro", 2023, Developer.Apple, Weight: 206.0),
        new Phone("Galaxy Z Fold4", 2023, Developer.Samsung, Weight: 263.0),
        new Phone("Pixel 6 Pro", 2022, Developer.Google, Weight: 210.1),
        new Phone("iPhone 6s", 2015, Developer.Apple, Weight: 143.0),
        new Phone("Galaxy S7", 2016, Developer.Samsung, Weight: 152.0),
        new Phone("Pixel XL", 2016, Developer.Google, Weight: 168.0),
        new Phone("Redmi Note 5", 2017, Developer.Xiaomi, Weight: 180.0),
        new Phone("Xperia XZ Premium", 2017, Developer.Sony, Weight: 195.0),
        new Phone("Nokia 8", 2017, Developer.Nokia, Weight: 160.0),
        new Phone("Huawei P10", 2017, Developer.Huawei, Weight: 145.0),
        new Phone("iPhone X", 2017, Developer.Apple, Weight: 174.0),
        new Phone("Galaxy S9", 2018, Developer.Samsung, Weight: 163.0),
        new Phone("Pixel 3", 2018, Developer.Google, Weight: 148.0)
    ];

    public IQueryable<Phone> Phones => phones.AsQueryable();
}