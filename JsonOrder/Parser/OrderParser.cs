using JsonOrder.Maps;
using System.Text.Json.Nodes;

namespace JsonOrder.Parser;


/*
 * Parser: aqui lo pueden hacer a preferencia de cada uno, en este ejemplo
 * No se complica y recorreremos manualmente el string convirtiendo las cosas
 * a JsonElement, haciendo un par de comparaciones y creando los objetos 
 * manualmente
 * 
 * Aqui es donde uno puede ponerse creativo y crear su propia interpretacion
 * de parser a como gusten (ocupando esta tactica, NewtonSoft, etc)
 */


public class OrderParser
{
    public static Order ParseOrder(string content)
    {
        Order result = new Order();
        JsonNode json = JsonObject.Parse(content);

        // Parte 1: Parseamos la data general (sin molestarnos lo que se tenga
        // adentro porque eso el LINQ lo resolvera)
        result.GeneralData = new Dictionary<string, string>();
        foreach (var characteristic in (JsonArray)json["characteristic"])
        {
            string name = (string)characteristic["name"];
            string value = (string)((JsonArray)characteristic["value"]).FirstOrDefault();
            result.GeneralData.Add(name, value);
        }

        result.OrderNumber = result.GeneralData["BSSOrderNumber"];
        result.Version = int.Parse(result.GeneralData["orderVersion"]);

        /*
         * Parte 2: Extraccion de OrderItem cuyo contenido es data de productos
         * 
         * Para esta parte vamos a ocupar la siguiente regla general:
         *     Un Nodo OrderItem sera considerado como un nodo que representa
         *     una linea (Internet, Video, Voice), si su:
         *     OrderItem.product.productSpecification.id contiene cualquiera de los valores (Internet, Video, Voice)
         *     
         * Nota: Se puede hacer una manera mas elegante pero gusto del cliente
         * 
         */
        string[] serviceTypes = new string[] { "Internet", "Video", "Voice" };
        result.LineItems = new List<OrderLineItem>();
        foreach(var orderItem in (JsonArray)json["orderItem"])
        {
            if (serviceTypes.Any(type => type == (string)orderItem["product"]["productSpecification"]["id"]))
            {
                OrderLineItem orderLineItem = new OrderLineItem();
                orderLineItem.Type = (string)orderItem["product"]["productSpecification"]["id"];
                orderLineItem.Action = (string)orderItem["action"];
                orderLineItem.Components = ParseProductData((JsonArray)orderItem["product"]["characteristic"]);
                orderLineItem.Promotions = ParsePromoData((JsonArray)orderItem["product"]["characteristic"]);

                result.LineItems.Add(orderLineItem);
            }
        }

        return result;
    }


    /*
     * Funcion para Obtener solamente los nodos internos de OrderItem
     * que sean productos
     * 
     * Regla: Un nodo characteristic (dentro de OrderItem) se considerara que
     *        es un nodo de producto, si:
     *        
     *        Tiene una propiedad llamada ProductCode (id producto)
     */
    public static List<Component> ParseProductData(JsonArray orderLineItemCharacteristics)
    {
        List<Component> result = new List<Component>();

         var condition = orderLineItemCharacteristics
            .SelectMany(characteristic => (JsonArray)characteristic["value"])
            .Where(characteristic => characteristic is JsonObject)
            .Where(characteristic => characteristic["productCode"] != null);

        foreach(var node in condition)
        {
            Component component = new Component
            {
                ExternalId = (string)((JsonArray)node["componentExternalId"]).FirstOrDefault(),
                Action = (string)((JsonArray)node["action"]).FirstOrDefault(),
                ComponentId = (string)((JsonArray)node["productCode"]).FirstOrDefault(),
                Name = (string)((JsonArray)node["productName"]).FirstOrDefault()
            };

            result.Add(component);
        }
            
        return result;
    }

    /*
     * Funcion para obtener solamente los OrderItem que representan Promos
     * 
     * Esta regla es un poco arbitraria basadan en los JSOn (puede estar equivocada
     * cambia a como convenga)
     * 
     * Regla: Un nodo characteristic (dentro de OrderItem) se considerara que
     *        es una Promo si:
     *        
     *        characteristic.value.promoSeq existe (usted puede cambiarla a
     *        productOverrideRate o promoDate)
     * 
     * 
     */
    public static List<Promotion> ParsePromoData (JsonArray orderLineItemCharacteristics) 
    {
        List<Promotion> result = new List<Promotion>();

        var condition = orderLineItemCharacteristics
            .SelectMany(characteristic => (JsonArray)characteristic["value"])
            .Where(characteristic => characteristic is JsonObject)
            .Where(characteristic => characteristic["promoSeq"] != null);

        foreach(var node in condition)
        {
            Promotion p = new Promotion()
            {
                ExternalId = (string)((JsonArray)node["componentExternalId"]).FirstOrDefault(),
                Action = (string)((JsonArray)node["action"]).FirstOrDefault(),
                ProductId = (string)((JsonArray)node["elementId"]).FirstOrDefault(),
                OverrideRate = ((JsonArray)node["productOverrideRate"]).FirstOrDefault().ToString(),
                PromoDate = node["promoDate"] != null ?
                        DateTime.Parse((string)((JsonArray)node["promoDate"]).FirstOrDefault())
                        : null
            };

            result.Add(p);
        }

        return result;
    }
}
