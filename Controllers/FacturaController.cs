using MantFacturas.Classes;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace MantFacturas.Controladores
{
    /// <summary>
    /// Controlador para gestionar las operaciones CRUD de Facturas y sus Detalles.
    /// </summary>
    public class FacturaController
    {
        private string connectionString;

        public FacturaController()
        {
            connectionString = ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString; // obtener la cadena de conexión desde el archivo de configuración

        }

        #region Métodos de Factura

        public List<FacturaClass> ObtenerFacturas(string busqueda = "")
        {
            List<FacturaClass> facturas = new List<FacturaClass>(); // INSTANCIA DE LA LISTA DE FACTURAS
            try // SE ENVUELVE TODO EL BLOQUE DE CÓDIGOS EN UN TRY-CATCH PARA CAPTURAR CUALQUIER EXCEPCIÓN QUE PUEDA OCURRIR DURANTE LA EJECUCIÓN DE LAS CONSULTAS A LA BASE DE DATOS
            {
                using (SqlConnection conn = new SqlConnection(connectionString)) // SE CREA CONEXION A LA DB
                {
                    SqlCommand cmd = new SqlCommand(string.IsNullOrEmpty(busqueda) ? "sp_ObtenerFacturas" : "sp_BuscarFacturas", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    
                    if (!string.IsNullOrEmpty(busqueda))
                    {
                        cmd.Parameters.AddWithValue("@Busqueda", busqueda); // SE AGREGA EL PARÁMETRO DE BÚSQUEDA SOLO SI NO ESTÁ VACÍO, PERMITIENDO ASÍ UTILIZAR EL MISMO MÉTODO PARA LISTAR TODAS LAS FACTURAS O BUSCAR POR CRITERIO
                    }

                    conn.Open(); // SE ABRE LA CONEXIÓN ANTES DE EJECUTAR EL READER PARA OBTENER LOS NOMBRES DE LAS COLUMNAS CORRECTAMENTE
                    SqlDataReader reader = cmd.ExecuteReader(); // SE EJECUTA EL READER DESPUÉS DE ABRIR LA CONEXIÓN PARA OBTENER LOS NOMBRES DE LAS COLUMNAS CORRECTAMENTE

                    while (reader.Read()) // SE LEE CADA REGISTRO DEVUELTO POR EL READER Y SE CREA UNA INSTANCIA DE FACTURAClass CON LOS DATOS OBTENIDOS, INCLUYENDO LOS CAMPOS DE AUDITORÍA SI ESTÁN PRESENTES EN EL RESULTADO
                    {
                        var fac = new FacturaClass
                        {
                            IDFactura = Convert.ToInt32(reader["IDFactura"]),
                            Cliente = reader["Cliente"].ToString(),
                            FechaEmision = Convert.ToDateTime(reader["FechaEmision"]),
                            MontoTotal = Convert.ToDecimal(reader["MontoTotal"]),
                            Estado = reader["Estado"].ToString().Trim(),
                            FechaAdicion = HasColumn(reader, "FechaAdicion") && !reader.IsDBNull(reader.GetOrdinal("FechaAdicion")) ? (DateTime?)Convert.ToDateTime(reader["FechaAdicion"]) : null,
                            UsuarioAgrego = HasColumn(reader, "UsuarioAgrego") && !reader.IsDBNull(reader.GetOrdinal("UsuarioAgrego")) ? reader["UsuarioAgrego"].ToString() : "Sistema",
                            FechaModificacion = HasColumn(reader, "FechaModificacion") && !reader.IsDBNull(reader.GetOrdinal("FechaModificacion")) ? (DateTime?)Convert.ToDateTime(reader["FechaModificacion"]) : null,
                            UsuarioModifico = HasColumn(reader, "UsuarioModifico") && !reader.IsDBNull(reader.GetOrdinal("UsuarioModifico")) ? reader["UsuarioModifico"].ToString() : "-"
                        };
                        facturas.Add(fac);
                    }
                    reader.Close();

                    // Cargar detalles para cada factura (Necesario para el resumen en el Listar)
                    foreach (var f in facturas)
                    {
                        f.Detalles = ObtenerDetallesFactura(f.IDFactura);
                    }
                }
            }
          
            catch (Exception ex)
            {
                throw new Exception("Error al obtener facturas: " + ex.Message);
            }

            return facturas;
        }

        private bool HasColumn(SqlDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++) 
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }
            return false;
        }

        public int AgregarFactura(string cliente, string usuarioAgrego)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand("sp_InsertarFactura", conn);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Cliente", cliente);
                    cmd.Parameters.AddWithValue("@UsuarioAgrego", usuarioAgrego);

                    conn.Open();
                    object result = cmd.ExecuteScalar();
                    return Convert.ToInt32(result);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al agregar factura: " + ex.Message);
            }
        }

        public FacturaClass ObtenerFacturaPorId(int id)
        {
            FacturaClass factura = null;
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand("sp_BuscarFacturas", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Busqueda", id.ToString());

                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        factura = new FacturaClass
                        {
                            IDFactura = Convert.ToInt32(reader["IDFactura"]),
                            Cliente = reader["Cliente"].ToString(),
                            FechaEmision = Convert.ToDateTime(reader["FechaEmision"]),
                            MontoTotal = Convert.ToDecimal(reader["MontoTotal"]),
                            Estado = reader["Estado"].ToString().Trim(),
                            FechaAdicion = HasColumn(reader, "FechaAdicion") && !reader.IsDBNull(reader.GetOrdinal("FechaAdicion")) ? (DateTime?)Convert.ToDateTime(reader["FechaAdicion"]) : null,
                            UsuarioAgrego = HasColumn(reader, "UsuarioAgrego") && !reader.IsDBNull(reader.GetOrdinal("UsuarioAgrego")) ? reader["UsuarioAgrego"].ToString() : "Sistema",
                            FechaModificacion = HasColumn(reader, "FechaModificacion") && !reader.IsDBNull(reader.GetOrdinal("FechaModificacion")) ? (DateTime?)Convert.ToDateTime(reader["FechaModificacion"]) : null,
                            UsuarioModifico = HasColumn(reader, "UsuarioModifico") && !reader.IsDBNull(reader.GetOrdinal("UsuarioModifico")) ? reader["UsuarioModifico"].ToString() : "-"
                        };
                    }
                    reader.Close();

                    if (factura != null)
                    {
                        factura.Detalles = ObtenerDetallesFactura(id);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener factura por ID: " + ex.Message);
            }

            return factura;
        }

        public void ActualizarFactura(int idFactura, string cliente, string usuarioModifico)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand("sp_ActualizarFactura", conn);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@IDFactura", idFactura);
                    cmd.Parameters.AddWithValue("@Cliente", cliente);
                    cmd.Parameters.AddWithValue("@UsuarioModifico", usuarioModifico);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al actualizar factura: " + ex.Message);
            }
        }

        public void EliminarFactura(int idFactura, string usuarioModifico)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand("sp_EliminarFactura", conn);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@IDFactura", idFactura);
                    cmd.Parameters.AddWithValue("@UsuarioModifico", usuarioModifico);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al inactivar factura: " + ex.Message);
            }
        }

        #endregion

        #region Métodos de Detalle Factura

        public List<DetalleFacturaClass> ObtenerDetallesFactura(int idFactura)
        {
            List<DetalleFacturaClass> detalles = new List<DetalleFacturaClass>();
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand("sp_ObtenerDetalleFactura", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@IDFactura", idFactura);

                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        detalles.Add(new DetalleFacturaClass
                        {
                            IDDetalle = Convert.ToInt32(reader["IDDetalle"]),
                            IDFactura = Convert.ToInt32(reader["IDFactura"]),
                            Descripcion = reader["Descripcion"].ToString(),
                            Cantidad = Convert.ToInt32(reader["Cantidad"]),
                            PrecioUnitario = Convert.ToDecimal(reader["PrecioUnitario"]),
                            Total = Convert.ToDecimal(reader["Total"]),
                            Estado = reader["Estado"].ToString().Trim(),
                            FechaAdicion = reader.IsDBNull(reader.GetOrdinal("FechaAdicion")) ? (DateTime?)null : Convert.ToDateTime(reader["FechaAdicion"]),
                            UsuarioAgrego = reader["UsuarioAgrego"].ToString(),
                            FechaModificacion = reader.IsDBNull(reader.GetOrdinal("FechaModificacion")) ? (DateTime?)null : Convert.ToDateTime(reader["FechaModificacion"]),
                            UsuarioModifico = reader["UsuarioModifico"].ToString(),
                            FechaEmision = HasColumn(reader, "FechaEmisionHeader") ? (reader.IsDBNull(reader.GetOrdinal("FechaEmisionHeader")) ? (DateTime?)null : Convert.ToDateTime(reader["FechaEmisionHeader"])) : (DateTime?)null
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener detalles: " + ex.Message);
            }
            return detalles;
        }

        public void AgregarDetalleFactura(int idFactura, string descripcion, int cantidad, decimal precioUnitario, string usuarioAgrego)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand("sp_InsertarDetalleFactura", conn);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@IDFactura", idFactura);
                    cmd.Parameters.AddWithValue("@Descripcion", descripcion);
                    cmd.Parameters.AddWithValue("@Cantidad", cantidad);
                    cmd.Parameters.AddWithValue("@PrecioUnitario", precioUnitario);
                    cmd.Parameters.AddWithValue("@UsuarioAgrego", usuarioAgrego);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al agregar detalle: " + ex.Message);
            }
        }

        public void ActualizarDetalleFactura(int idDetalle, string descripcion, int cantidad, decimal precioUnitario, string usuarioModifico)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand("sp_ActualizarDetalleFactura", conn);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@IDDetalle", idDetalle);
                    cmd.Parameters.AddWithValue("@Descripcion", descripcion);
                    cmd.Parameters.AddWithValue("@Cantidad", cantidad);
                    cmd.Parameters.AddWithValue("@PrecioUnitario", precioUnitario);
                    cmd.Parameters.AddWithValue("@UsuarioModifico", usuarioModifico);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al actualizar detalle: " + ex.Message);
            }
        }

        public void InactivarDetalleFactura(int idDetalle, string usuarioModifico)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand("sp_InactivarDetalleFactura", conn);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@IDDetalle", idDetalle);
                    cmd.Parameters.AddWithValue("@UsuarioModifico", usuarioModifico);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al inactivar detalle: " + ex.Message);
            }
        }

        public void ActivarDetalleFactura(int idDetalle, string usuarioModifico)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand("sp_ActivarDetalleFactura", conn);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@IDDetalle", idDetalle);
                    cmd.Parameters.AddWithValue("@UsuarioModifico", usuarioModifico);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al activar detalle: " + ex.Message);
            }
        }

        #endregion
    }
}