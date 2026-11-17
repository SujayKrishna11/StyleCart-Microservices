using Microsoft.EntityFrameworkCore;
using StyleCart.ProductService.Models;

namespace StyleCart.ProductService.Data;

public static class CatalogSeedData
{
    public static async Task InitializeAsync(ProductDbContext dbContext)
    {
        var products = new[]
        {
            Product("Men", "Container Demo Shirt", "Product created through the API Gateway.", "StyleCart", 899m, "https://images.unsplash.com/photo-1603252109303-2751441dd157?auto=format&fit=crop&w=900&q=80",
                Variant("M", "Blue", "DEMO-SHIRT-BLUE-M", 899m, 20)),
            Product("Men", "Oxford Cotton Shirt", "Classic button-down cotton shirt for everyday wear.", "StyleCart Men", 1299m, "https://images.unsplash.com/photo-1603252109303-2751441dd157?auto=format&fit=crop&w=900&q=80",
                Variant("M", "Sky Blue", "MEN-OXFORD-SKY-M", 1299m, 18),
                Variant("L", "White", "MEN-OXFORD-WHITE-L", 1299m, 15)),
            Product("Men", "Slim Fit Chinos", "Comfort-stretch chinos with a modern slim fit.", "StyleCart Men", 1799m, "https://images.unsplash.com/photo-1624378439575-d8705ad7ae80?auto=format&fit=crop&w=900&q=80",
                Variant("32", "Beige", "MEN-CHINO-BEIGE-32", 1799m, 12),
                Variant("34", "Navy", "MEN-CHINO-NAVY-34", 1799m, 10)),
            Product("Men", "Everyday Hoodie", "Soft fleece hoodie for casual comfort.", "StyleCart Men", 1599m, "https://images.unsplash.com/photo-1556821840-3a63f95609a7?auto=format&fit=crop&w=900&q=80",
                Variant("M", "Charcoal", "MEN-HOODIE-CHARCOAL-M", 1599m, 20),
                Variant("L", "Olive", "MEN-HOODIE-OLIVE-L", 1599m, 16)),
            Product("Women", "Floral Summer Dress", "Lightweight floral dress for warm days.", "StyleCart Women", 1499m, "https://images.unsplash.com/photo-1595777457583-95e059d581b8?auto=format&fit=crop&w=900&q=80",
                Variant("M", "Rose", "WOMEN-FLORAL-ROSE-M", 1499m, 14),
                Variant("L", "Ivory", "WOMEN-FLORAL-IVORY-L", 1499m, 11)),
            Product("Women", "Satin Midi Dress", "Sleeveless satin midi dress with an elegant drape.", "StyleCart Women", 1599m, "https://images.unsplash.com/photo-1566174053879-31528523f8ae?auto=format&fit=crop&w=900&q=80",
                Variant("M", "Ruby", "WOMEN-SATIN-RUBY-M", 1599m, 9),
                Variant("L", "Emerald", "WOMEN-SATIN-EMERALD-L", 1599m, 8)),
            Product("Women", "Ribbed Knit Top", "Versatile ribbed top for everyday styling.", "StyleCart Women", 899m, "https://images.unsplash.com/photo-1485968579580-b6d095142e6e?auto=format&fit=crop&w=900&q=80",
                Variant("S", "Lilac", "WOMEN-KNIT-LILAC-S", 899m, 17),
                Variant("M", "Black", "WOMEN-KNIT-BLACK-M", 899m, 19)),
            Product("Accessories", "Leather Belt", "Genuine leather belt with a polished buckle.", "StyleCart Accessories", 799m, "https://images.unsplash.com/photo-1624222247344-550fb60583dc?auto=format&fit=crop&w=900&q=80",
                Variant("One Size", "Brown", "ACC-BELT-BROWN", 799m, 25)),
            Product("Accessories", "Canvas Tote Bag", "Reusable canvas tote for daily essentials.", "StyleCart Accessories", 699m, "https://images.unsplash.com/photo-1591561954557-26941169b49e?auto=format&fit=crop&w=900&q=80",
                Variant("One Size", "Natural", "ACC-TOTE-NATURAL", 699m, 30)),
            Product("Accessories", "Classic Sunglasses", "UV-protected sunglasses with a timeless frame.", "StyleCart Accessories", 1199m, "https://images.unsplash.com/photo-1511499767150-a48a237f0083?auto=format&fit=crop&w=900&q=80",
                Variant("One Size", "Black", "ACC-SUNGLASSES-BLACK", 1199m, 13))
        };

        foreach (var product in products)
        {
            var existingProduct = await dbContext.Products
                .FirstOrDefaultAsync(item => item.Name == product.Name);

            if (existingProduct is not null)
            {
                if (string.IsNullOrWhiteSpace(existingProduct.ImageUrl))
                {
                    existingProduct.ImageUrl = product.ImageUrl;
                    existingProduct.Category = product.Category;
                }

                continue;
            }

            dbContext.Products.Add(product);
        }

        await dbContext.SaveChangesAsync();
    }

    private static Product Product(string category, string name, string description, string brand, decimal price, string imageUrl, params ProductVariant[] variants)
    {
        var product = new Product
        {
            Category = category,
            Name = name,
            Description = description,
            Brand = brand,
            ImageUrl = imageUrl,
            BasePrice = price
        };

        foreach (var variant in variants)
        {
            product.Variants.Add(variant);
        }

        return product;
    }

    private static ProductVariant Variant(string size, string color, string sku, decimal price, int stock) => new()
    {
        Size = size,
        Color = color,
        Sku = sku,
        Price = price,
        Inventory = new Inventory { QuantityInStock = stock }
    };
}
