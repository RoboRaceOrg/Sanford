using System.Text;
using System.Text.Json;


namespace BlueCorpDispatchAuto
{
    public class JsonToCsvConverter
    {
        private static readonly Dictionary<string, string> ContainerMapping = new Dictionary<string, string>
        {
            { "20RF", "REF20" },
            { "40RF", "REF40" },
            { "20HC", "HC20" },
            { "40HC", "HC40" }
        };

        public static string ConvertJsonToCsv(string jsonContent)
        {
            using JsonDocument jsonDoc = JsonDocument.Parse(jsonContent);
            var root = jsonDoc.RootElement;

            if (!root.TryGetProperty("salesOrder", out var salesOrder) ||
                !root.TryGetProperty("containers", out var containers) ||
                !root.TryGetProperty("deliveryAddress", out var deliveryAddress))
            {
                throw new ArgumentException("Missing required fields in JSON");
            }

            var csvBuilder = new StringBuilder();
            csvBuilder.AppendLine("CustomerReference,LoadId,ContainerType,ItemCode,ItemQuantity,ItemWeight,Street,City,State,PostalCode,Country");

            foreach (var container in containers.EnumerateArray())
            {
                if (!container.TryGetProperty("loadId", out var loadId) ||
                    !container.TryGetProperty("containerType", out var containerType) ||
                    !container.TryGetProperty("items", out var itemsArray))
                {
                    continue; // Skip invalid container entries
                }

                string mappedContainerType = ContainerMapping.TryGetValue(containerType.GetString(), out string mappedType)
                    ? mappedType : containerType.GetString();

                foreach (var item in itemsArray.EnumerateArray())
                {
                    if (!item.TryGetProperty("itemCode", out var itemCode) ||
                        !item.TryGetProperty("quantity", out var quantity) ||
                        !item.TryGetProperty("cartonWeight", out var cartonWeight))
                    {
                        continue; // Skip invalid item entries
                    }

                    csvBuilder.AppendLine($"{salesOrder.GetString()},{loadId.GetString()},{mappedContainerType}," +
                                          $"{itemCode.GetString()},{quantity.GetInt32()},{cartonWeight.GetDouble()}," +
                                          $"{deliveryAddress.GetProperty("street").GetString()}," +
                                          $"{deliveryAddress.GetProperty("city").GetString()}," +
                                          $"{deliveryAddress.GetProperty("state").GetString()}," +
                                          $"{deliveryAddress.GetProperty("postalCode").GetString()}," +
                                          $"{deliveryAddress.GetProperty("country").GetString()}");
                }
            }

            return csvBuilder.ToString();
        }
    }
}
