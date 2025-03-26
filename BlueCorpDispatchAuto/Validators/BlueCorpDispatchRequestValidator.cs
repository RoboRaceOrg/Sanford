using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueCorpDispatchAuto
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;

    public class BlueCorpDispatchRequestValidator
    {
        public static List<string> ValidateJsonData(string jsonString)
        {
            List<string> missingFields = new List<string>();

            try
            {
                var jsonDocument = JsonDocument.Parse(jsonString);
                var root = jsonDocument.RootElement;

                if (!root.TryGetProperty("controlNumber", out _))
                    missingFields.Add("controlNumber");
                if (!root.TryGetProperty("salesOrder", out _))
                    missingFields.Add("salesOrder");
                if (!root.TryGetProperty("containers", out var containers))
                    missingFields.Add("containers");
                if (!root.TryGetProperty("deliveryAddress", out var deliveryAddress))
                    missingFields.Add("deliveryAddress");

                if (deliveryAddress.ValueKind == JsonValueKind.Object)
                {
                    foreach (var field in new[] { "street", "city", "state", "postalCode", "country" })
                    {
                        if (!deliveryAddress.TryGetProperty(field, out _))
                            missingFields.Add($"deliveryAddress.{field}");
                    }
                }

                if (containers.ValueKind == JsonValueKind.Array)
                {
                    foreach (var container in containers.EnumerateArray())
                    {
                        if (!container.TryGetProperty("loadId", out _))
                            missingFields.Add("containers[].loadId");
                        if (!container.TryGetProperty("containerType", out _))
                            missingFields.Add("containers[].containerType");
                        if (!container.TryGetProperty("items", out var items) || items.ValueKind != JsonValueKind.Array)
                            missingFields.Add("containers[].items");
                        else
                        {
                            foreach (var item in items.EnumerateArray())
                            {
                                foreach (var field in new[] { "itemCode", "quantity", "cartonWeight" })
                                {
                                    if (!item.TryGetProperty(field, out _))
                                        missingFields.Add($"containers[].items[].{field}");
                                }
                            }
                        }
                    }
                }
            }
            catch (JsonException ex)
            {
                missingFields.Add("Invalid JSON format: " + ex.Message);
            }

            return missingFields;
        }
    }

}
