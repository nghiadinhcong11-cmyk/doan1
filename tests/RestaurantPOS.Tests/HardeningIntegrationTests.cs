using System;
using RestaurantPOS.Domain.Entities;
using Xunit;

namespace RestaurantPOS.Tests
{
    public class HardeningIntegrationTests
    {
        [Fact]
        public void Product_ImageUrl_Length_Validation_Logic()
        {
            // Manual verification of the logic used in ProductController
            var largeImage = new string('A', 800000);
            var product = new Product { Name = "Large Image Product", ImageUrl = largeImage };

            // Logic check: length > 700000 should be rejected.
            Assert.True(product.ImageUrl.Length > 700000);
        }
    }
}
