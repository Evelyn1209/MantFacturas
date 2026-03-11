using System;
using System.Collections.Generic;

namespace MantFacturas.Classes
{
    /// <summary>
    /// Clase que representa una Factura en el sistema.
    /// </summary>
    public class FacturaClass
    {
        // PROPIEDADES (GET Y SET)
        public int IDFactura { get; set; }
        public string Cliente { get; set; }
        public DateTime FechaEmision { get; set; }
        public decimal MontoTotal { get; set; }
        public string Estado { get; set; } // 'A' para Activo, 'I' para Inactivo
        
        // Campos de auditoría
        public DateTime? FechaAdicion { get; set; }
        public string UsuarioAgrego { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public string UsuarioModifico { get; set; }

        // Relación con detalles
        public List<DetalleFacturaClass> Detalles { get; set; }

        // CONSTRUCTOR VACÍO
        public FacturaClass()
        {
            Detalles = new List<DetalleFacturaClass>();
        }
    }

    /// <summary>
    /// Clase que representa el detalle de una Factura.
    /// </summary>
    public class DetalleFacturaClass
    {
        public int IDDetalle { get; set; }
        public int IDFactura { get; set; }
        public string Descripcion { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Total { get; set; } // Total = Cantidad * PrecioUnitario
        public string Estado { get; set; }
        
        // Campos de auditoría
        public DateTime? FechaAdicion { get; set; }
        public string UsuarioAgrego { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public string UsuarioModifico { get; set; }
        public DateTime? FechaEmision { get; set; } // Fecha de la factura cabecera para auditoría

        public DetalleFacturaClass() { }
    }
}