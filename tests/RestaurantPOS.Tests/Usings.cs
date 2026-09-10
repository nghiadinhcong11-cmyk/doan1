global using Xunit;
global using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using System.Text;

namespace RestaurantPOS.Tests;

public static class TestInitializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        Console.OutputEncoding = Encoding.UTF8;
    }
}
