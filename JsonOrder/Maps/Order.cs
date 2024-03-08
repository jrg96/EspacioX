
namespace JsonOrder.Maps;

/*
 * Por conveniencia, veremos la orden como algo super simple donde
 * NO se realizara un mapeo completo, solamente mapeo de las propiedades
 * que nos interesan, en este encabezado se describe la manera en la que una
 * orden puede verse:
 * 
 * Order
 *     -> GeneralData: la data general de la orden (source system, order number, version)
 *     -> OrderLineItem: Todas las lineas que tenia el usuario/orden en cuestion
 */

public class Order
{
    public string OrderNumber { get; set; }
    public int Version { get; set; }
    public Dictionary<string, string> GeneralData { get; set; }
    public List<OrderLineItem> LineItems { get; set; }
}
