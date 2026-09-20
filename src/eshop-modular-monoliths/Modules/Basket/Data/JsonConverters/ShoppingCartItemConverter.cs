using System.Text.Json;
using System.Text.Json.Serialization;
using Basket.Models;

namespace Basket.Data.JsonConverters;

public class ShoppingCartItemConverter : JsonConverter<ShoppingCartItem>
{
    private static bool TryGetPropertyIgnoreCase(JsonElement element, string propertyName, out JsonElement value)
    {
        if (element.TryGetProperty(propertyName, out value)) return true;
        foreach (var p in element.EnumerateObject())
        {
            if (string.Equals(p.Name, propertyName, StringComparison.OrdinalIgnoreCase))
            {
                value = p.Value;
                return true;
            }
        }
        value = default;
        return false;
    }

    public override ShoppingCartItem? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var jsonDocument = JsonDocument.ParseValue(ref reader);
        var rootElement = jsonDocument.RootElement;

        Guid id = Guid.NewGuid();
        if (TryGetPropertyIgnoreCase(rootElement, "id", out var idProp) && idProp.ValueKind == JsonValueKind.String)
        {
            _ = idProp.TryGetGuid(out id);
        }

        Guid shoppingCartId = Guid.Empty;
        if (TryGetPropertyIgnoreCase(rootElement, "shoppingCartId", out var cartProp) && cartProp.ValueKind == JsonValueKind.String)
        {
            _ = cartProp.TryGetGuid(out shoppingCartId);
        }

        Guid productId = Guid.Empty;
        if (TryGetPropertyIgnoreCase(rootElement, "productId", out var prodProp) && prodProp.ValueKind == JsonValueKind.String)
        {
            _ = prodProp.TryGetGuid(out productId);
        }

        int quantity = 1;
        if (TryGetPropertyIgnoreCase(rootElement, "quantity", out var qtyProp) && qtyProp.ValueKind == JsonValueKind.Number)
        {
            quantity = qtyProp.GetInt32();
        }

        string color = string.Empty;
        if (TryGetPropertyIgnoreCase(rootElement, "color", out var colorProp) && colorProp.ValueKind == JsonValueKind.String)
        {
            color = colorProp.GetString() ?? string.Empty;
        }

        decimal price = 0;
        if (TryGetPropertyIgnoreCase(rootElement, "price", out var priceProp) && priceProp.ValueKind == JsonValueKind.Number)
        {
            price = priceProp.GetDecimal();
        }

        string productName = string.Empty;
        if (TryGetPropertyIgnoreCase(rootElement, "productName", out var nameProp) && nameProp.ValueKind == JsonValueKind.String)
        {
            productName = nameProp.GetString() ?? string.Empty;
        }

        return new ShoppingCartItem(id, shoppingCartId, productId, quantity, color, price, productName);
    }

    public override void Write(Utf8JsonWriter writer, ShoppingCartItem value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteString("id", value.Id.ToString());
        writer.WriteString("shoppingCartId", value.ShoppingCartId.ToString());
        writer.WriteString("productId", value.ProductId.ToString());
        writer.WriteNumber("quantity", value.Quantity);
        writer.WriteString("color", value.Color);
        writer.WriteNumber("price", value.Price);
        writer.WriteString("productName", value.ProductName);

        writer.WriteEndObject();
    }
}
