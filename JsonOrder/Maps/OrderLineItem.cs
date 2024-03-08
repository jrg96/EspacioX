namespace JsonOrder.Maps;

/*
 * Un OrderLineItem representa una linea.
 * 
 * Una linea puede ser de 3 tipos: Voice (POTS), Internet, Video
 * Una linea puede tener acciones:
 *     add = Cuando la linea no existia en el sitio, pero en esta orden se agrego
 *     rmv = Cuando la linea YA existia en el sitio, pero en esta orden se removio
 *     noChange = Linea que no sufrio cambios en esta orden
 *     modify = Cuando la linea ya existia en el sitio, pero en esta orden se le agrego
 *              o elimino productos
 * 
 * Nota: para una move order mirarla como si fueran 2 ordenes en una:
 *       una Establish (sitio de destino), donde todas las lineas apareceran como add
 *       una Disconnect (sitio de origen), donde todas las lineas apareceran como rmv
 * 
 * Un par de ejemplos especificos a move order:
 * 
 * Ejemplo 1: Origen tenia internet, pero a la hora de hacer la move se decide que
 *            ya no se quiere tener internet
 * Respuesta: Solo habra 1 linea con action = rmv, porque en el destino se elegio
 *            ya no tener internet
 * 
 * 
 * Ejemplo 2: Origen tenia internet, se decide mantener internet en el destino
 * Respuesta: Habran 2 lineas de internet, una con action = add, otra con action = rmv
 * 
 * 
 * Ejemplo 3: Origen no tenia internet, destino se decidio tener internet
 * Respuesta: Habra 1 linea de internet, con action = add (representando el destino)
 */

public class OrderLineItem
{
    public string Type { get; set; }
    public string Action { get; set; }
    public List<Component> Components { get; set; }
    public List<Promotion> Promotions { get; set; }
}
