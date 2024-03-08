using JsonOrder.Maps;
using System.Text.Json.Nodes;

namespace JsonOrder.Parser;

public class OrderParserMinified
{
    public static Order ParseOrder(string content)
    {
        Order result = new Order();
        JsonNode json = JsonObject.Parse(content);
        string[] serviceTypes = new string[] { "Internet", "Video", "Voice" };

        result.GeneralData = new Dictionary<string, string>();
        foreach (var characteristic in (JsonArray)json["characteristic"])
        {
            string name = (string)characteristic["name"];
            string value = (string)((JsonArray)characteristic["value"]).FirstOrDefault();
            result.GeneralData.Add(name, value);
        }
        result.OrderNumber = result.GeneralData["BSSOrderNumber"];
        result.Version = int.Parse(result.GeneralData["orderVersion"]);


        result.LineItems =
            ((JsonArray)json["orderItem"])
            .Where(orderItem =>
                serviceTypes.Any(type => type == (string)orderItem["product"]["productSpecification"]["id"])
            )
            .Select(service =>
            {
                OrderLineItem orderLineItem = new OrderLineItem();
                orderLineItem.Type = (string)service["product"]["productSpecification"]["id"];
                orderLineItem.Action = (string)service["action"];

                orderLineItem.Components = ((JsonArray)service["product"]["characteristic"])
                    .SelectMany(characteristic => (JsonArray)characteristic["value"])
                    .Where(characteristic => characteristic is JsonObject)
                    .Where(characteristic => characteristic["productCode"] != null)
                    .Select(characteristic =>
                    {
                        Component component = new Component
                        {
                            ExternalId = (string)((JsonArray)characteristic["componentExternalId"]).FirstOrDefault(),
                            Action = (string)((JsonArray)characteristic["action"]).FirstOrDefault(),
                            ComponentId = (string)((JsonArray)characteristic["productCode"]).FirstOrDefault(),
                            Name = (string)((JsonArray)characteristic["productName"]).FirstOrDefault()
                        };

                        return component;
                    }).ToList(); ;

                orderLineItem.Promotions = ((JsonArray)service["product"]["characteristic"])
                    .SelectMany(characteristic => (JsonArray)characteristic["value"])
                    .Where(characteristic => characteristic is JsonObject)
                    .Where(characteristic => characteristic["promoSeq"] != null)
                    .Select(characteristic =>
                    {
                        return new Promotion()
                        {
                            ExternalId = (string)((JsonArray)characteristic["componentExternalId"]).FirstOrDefault(),
                            Action = (string)((JsonArray)characteristic["action"]).FirstOrDefault(),
                            ProductId = (string)((JsonArray)characteristic["elementId"]).FirstOrDefault(),
                            OverrideRate = ((JsonArray)characteristic["productOverrideRate"]).FirstOrDefault().ToString(),
                            PromoDate = characteristic["promoDate"] != null ?
                                DateTime.Parse((string)((JsonArray)characteristic["promoDate"]).FirstOrDefault())
                                : null
                        };
                    }).ToList();

                return orderLineItem;
            }).ToList();

        return result;
    }
}
