/*
 * Dividamos el problema en 3 partes:
 * 1) Carga de string Json en memoria
 * 2) Convertir un STR JSON a una clase Order completa
 * 3) Ejercicios meramente relacionados a LINQ despues que tenemos la clase Order
 * 
 */


/*
 * Parte 1: Carga de striong JSON en memoria
 * Esto puede user cambiarlo a una clase donde encapsule toda la logica
 * Por comodidad, lo dejare todo aca.
 */

using JsonOrder.Maps;
using JsonOrder.Parser;

List<string> orderData = new List<string>();
string path = "C:\\Users\\ADMIN\\source\\repos\\JsonOrder\\JsonOrder\\ExampleJson";
string[] files = Directory.GetFiles(path, "*.json");


foreach (var file in files)
{
    string content = await File.ReadAllTextAsync(file);
    orderData.Add(content);
}


/*
 * Parte 2: parseo de la clase
 */
List<Order> orderObj = new List<Order>();

foreach (var order in orderData)
{
    Order data = OrderParser.ParseOrder(order);
    orderObj.Add(data);
}


/*
 * Ejercicios
 */

/*
 * Ejercicio 1: Facilito
 */

//-------------------------------------------------------------------------------------------
/*
 * Ejercicio 2: Obtener todos los componentes removidos por orden
 */
var e2 = orderObj
    .Select(order =>
    {
        return order.LineItems
            .SelectMany(line => line.Components)
            .Where(component => component.Action == "rmv")
            .Select(component => new
            {
                Order = order.GeneralData.Where(x => x.Key == "BSSOrderNumber").FirstOrDefault().Value,
                OrderVersion = order.GeneralData.Where(x => x.Key == "orderVersion").FirstOrDefault().Value,
                ComponentId = component.ComponentId,
                Action = component.Action
            });
    })
    .ToList();

//-----------------------------------------------------------------------------------------
/*
 * Ejercicio 3: Obtener similitudes de productos agregados y removidos entre 2 ordenes
 * 
 * Entiendase por similitudes aquellas ordenes que tanto en la base como en la sup
 * tienen el mismo producto a agregarse o remover
 */
var similarComponents = orderObj
    .Where(order => order.GeneralData.Any(data => data.Value == "M800S03"))
    .SelectMany(order => order.LineItems)
    .SelectMany(line => line.Components)
    .GroupBy(component => new { component.Action, component.ComponentId })
    .Where(group => group.Count() > 1);

//-------------------------------------------------------------------------------------------------------
/*
 * Ejercicio 4: Obtener la lista de ordenes que hayan conservado una o mas promociones al ser supeada
 */

/*
 * Ordenes que aparecen mas de una vez en la lista de ordenes (osea que tienen version)
 */
var testOrders = orderObj
    .GroupBy(order => order.OrderNumber)
    .Where(group => group.Count() > 1);


/*
 * Verificar que una promo haya sobrevivido
 */
var ordersWithPromo = orderObj
    .GroupBy(order => order.OrderNumber)
    .Where(group => group.Count() > 1)
    .Where(group =>
    {
        // version base y ultima version
        Order baseOrder = group.OrderBy(order => order.Version).First();
        Order latestOrder = group.OrderBy(order => order.Version).Last();

        /*
         * Regla: Se considera que una promo se preservo si la promo:
         *      En la orden base aparece  (noChange / modify / add) (osea != rmv)
         *      ultima orden aparece aun (noChange / modify / add) (osea != rmv)
         */
        return baseOrder.LineItems
            .SelectMany(line => line.Promotions)
            .Where(promo => promo.Action != "rmv")
            .Select(promo => promo.ProductId)
            .Distinct()
            .Intersect(
                 latestOrder.LineItems
                    .SelectMany(line => line.Promotions)
                    .Where(promo => promo.Action != "rmv")
                    .Select(promo => promo.ProductId)
                    .Distinct()
            ).Count() > 0;
    }).ToList();


//---------------------------------------------------------------------------------
/*
 * Ejercicio 5: Obtener todas las ordenes que hayan consrvado todas las promociones al ser supeadas
 */

/*
 * Ordenes que aparecen mas de una vez en la lista de ordenes (osea que tienen version)
 */
var orderTestList5 = orderObj
    .GroupBy(order => order.OrderNumber)
    .Where(group => group.Count() > 1);

/*
 * Verificar que todas las promos hayan sobrevivido
 */
var ordersWithPromo5 = orderObj
    .GroupBy(order => order.OrderNumber)
    .Where(group => group.Count() > 1)
    .Where(group =>
    {
        // version base y ultima version
        Order baseOrder = group.OrderBy(order => order.Version).First();
        Order latestOrder = group.OrderBy(order => order.Version).Last();

        /*
         * Regla: Se considera que una promo se preservo si la promo:
         *      En la orden base aparece  (noChange / modify / add) (osea != rmv)
         *      ultima orden aparece aun (noChange / modify / add) (osea != rmv)
         *      
         * Para esta en vez de un intercept hacemos un Except: verificamos que
         * elementos no se encuentran en ambas listas y si alguno no esta (Any) 
         * reportamos que no cumple la condicion (operador !)
         */

        IEnumerable<string> baseGroup = baseOrder.LineItems
            .SelectMany(line => line.Promotions)
            .Where(promo => promo.Action != "rmv")
            .Select(promo => promo.ProductId)
            .Distinct();

        IEnumerable<string> supGroup = latestOrder.LineItems
                    .SelectMany(line => line.Promotions)
                    .Where(promo => promo.Action != "rmv")
                    .Select(promo => promo.ProductId)
                    .Distinct();

        return !baseGroup.Except(supGroup).Any() && baseGroup.Count() > 0;
    }).ToList();


// ----------------------------------------------------------------------------
/*
 * Ejercicio 6: Obtener las ordenes que remuevan dry loop
 * 
 * Product = 132804
 */
var ej6 = orderObj
    .Where(order =>
        order.LineItems
            .SelectMany(line => line.Components)
            .Any(component => component.Action == "rmv" && component.ComponentId == "132804")
    ).ToList();


// ----------------------------------------------------------------------------
/*
 * Ejercicio 7: Obtener todas las ordenes que tengan additional STB
 * 
 * Como solo piden que tenga alguno, se acepta que su action sea noChange, add, modify
 * osease != rmv
 */
var ej7 = orderObj
    .Where(order =>
        order.LineItems
            .SelectMany(line => line.Components)
            .Any(component => component.Action != "rmv" 
                && component.ComponentId == "920022")
    ).ToList();

// ----------------------------------------------------------------------------
/*
 * Ejercicio 8: Obtenga las ordenes que no tengan additional STB pero que si tenga STB principal
 */
var ej8 = orderObj
    .Where(order =>
        order.LineItems
            .SelectMany(line => line.Components)
            .All(component => // para este any verificamos que no exista el 920022 o que se este removiendo para considerar que no existe
                component.ComponentId != "920022"
                || (component.ComponentId == "920022" && component.Action == "rmv"))

            && order.LineItems // en esta condicion verificamos que tenga el first STB
                .SelectMany(line => line.Components)
                .Any(component => component.ComponentId == "920086")
    ).ToList();

// -----------------------------------------------------------------------------
/*
 * Ejercicio 9: Obtener las ordenes que no tengan promociones
 */
var ej9 = orderObj
    .Where(order =>
        order.LineItems
            .SelectMany(line => line.Promotions)
            .Count() == 0
    ).ToList();

var test = "holi";