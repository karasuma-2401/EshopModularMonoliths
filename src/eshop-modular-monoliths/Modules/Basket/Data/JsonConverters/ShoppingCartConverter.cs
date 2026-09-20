using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Basket.Models;

namespace Basket.Data.JsonConverters;

public class ShoppingCartConverter : JsonConverter<ShoppingCart>
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

    public override ShoppingCart? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var jsonDocument = JsonDocument.ParseValue(ref reader);
        var rootElement = jsonDocument.RootElement;

        Guid id = Guid.NewGuid();
        if (TryGetPropertyIgnoreCase(rootElement, "id", out var idProp) && idProp.ValueKind == JsonValueKind.String)
        {
            _ = idProp.TryGetGuid(out id);
        }

        string userName = string.Empty;
        if (TryGetPropertyIgnoreCase(rootElement, "userName", out var userProp) && userProp.ValueKind == JsonValueKind.String)
        {
            userName = userProp.GetString() ?? string.Empty;
        }

        var shoppingCart = ShoppingCart.Create(id, userName);

        if (TryGetPropertyIgnoreCase(rootElement, "items", out var itemsProp) && itemsProp.ValueKind == JsonValueKind.Array)
        {
            var items = itemsProp.Deserialize<List<ShoppingCartItem>>(options);
            if (items != null)
            {
                var itemsField = typeof(ShoppingCart).GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance);
                itemsField?.SetValue(shoppingCart, items);
            }
        }

        return shoppingCart;
    }

    public override void Write(Utf8JsonWriter writer, ShoppingCart value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteString("id", value.Id.ToString());
        writer.WriteString("userName", value.UserName);

        writer.WritePropertyName("items");
        JsonSerializer.Serialize(writer, value.Items, options);

        writer.WriteEndObject();
    }
}
