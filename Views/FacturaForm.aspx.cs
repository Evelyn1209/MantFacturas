using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using MantFacturas.Classes;
using MantFacturas.Controladores;

namespace MantFacturas.Views
{
    public partial class FacturaForm : System.Web.UI.Page //Define la página web de facturas.
    {
        private FacturaController controller = new FacturaController(); // crea un objeto del controlador.
        protected string CurrentUser
        {
            get
            {
                // Obtener el nombre de usuario del sistema (Windows)
                string user = System.Environment.UserName;
                return !string.IsNullOrEmpty(user) ? user : "Sistema";
            }
        }

        private List<DetalleFacturaClass> DetalleTemp // Es una lista temporal de los detalles de la factura.
        {
            get
            {
                if (Session["DetalleTemp"] == null) // Session guarda datos mientras el usuario usa la página. mantener los detalles antes de guardar la factura.
                    Session["DetalleTemp"] = new List<DetalleFacturaClass>();
                return (List<DetalleFacturaClass>)Session["DetalleTemp"];
            }
            set { Session["DetalleTemp"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e) // Se ejecuta cada vez que la página se carga.
        {
            if (!IsPostBack) // Evita que se cargue la pagina  en cada evento. solo cuando se carga por primera vez
            {
                CargarFacturas();
                LimpiarTodo();
            }
        }

        private void CargarFacturas() // Carga las facturas desde el controlador y las muestra en un GridView.
        {
            try // Manejo de errores para mostrar mensajes amigables al usuario. Try-Catch para capturar cualquier error que ocurra al cargar las facturas.
            {
                gvFacturas.DataSource = controller.ObtenerFacturas();
                gvFacturas.DataBind();
            }
            catch (Exception ex)
            {
                MostrarError("Error: " + ex.Message); // Muestra el error en la interfaz 
            }
        }

        protected void btnAddDetalle_Click(object sender, EventArgs e) // Evento que se ejecuta al hacer clic en el botón para agregar un detalle a la factura.
        {
            if (!Page.IsValid) return;// Verifica que los controles de validación en la página sean válidos antes de continuar.
            try // Validaciones de detalle
            {
                // Validaciones de detalle
                if (string.IsNullOrWhiteSpace(txtDetalleDescripcion.Text)) throw new Exception("La descripción es obligatoria."); // Verifica que la descripción del detalle no esté vacía o solo con espacios
                if (!int.TryParse(txtDetalleCantidad.Text, out int cant) || cant <= 0) throw new Exception("Cantidad debe ser > 0.");// Verifica que la cantidad sea un número entero válido y mayor que cero
                if (!decimal.TryParse(txtDetallePrecio.Text, out decimal precio) || precio <= 0) throw new Exception("Precio debe ser > 0."); 

                if (EsFacturaConNotaCredito(DetalleTemp)) throw new Exception("Esta factura cuenta con una Nota de Crédito y no puede ser modificada.");

                // VERIFICAR SI ESTAMOS EDITANDO UN ITEM EXISTENTE EN LA LISTA
                int editIndex = int.Parse(hfIndexDetalleEditing.Value);

                if (editIndex >= 0 && editIndex < DetalleTemp.Count)
                {
                    // MODIFICAR ITEM EN MEMORIA
                    var det = DetalleTemp[editIndex];
                    det.Descripcion = txtDetalleDescripcion.Text.Trim();
                    det.Cantidad = cant;
                    det.PrecioUnitario = precio;
                    det.Total = cant * precio;
                    det.UsuarioModifico = CurrentUser; // Auditoría
                    det.FechaModificacion = DateTime.Now;

                    // SI LA FACTURA YA EXISTE EN BD, SINCRONIZAR CAMBIO AL INSTANTE
                    if (!string.IsNullOrEmpty(hfIDFactura.Value) && det.IDDetalle > 0)
                    {
                        controller.ActualizarDetalleFactura(det.IDDetalle, det.Descripcion, det.Cantidad, det.PrecioUnitario, CurrentUser);
                        
                        // Refrescar reporte consolidado para ver totales reales
                        var facActualizada = controller.ObtenerFacturaPorId(int.Parse(hfIDFactura.Value));
                        FillReportLabels(facActualizada);
                    }
                }
                else
                {
                    // LÓGICA ORIGINAL: AGREGAR NUEVO ITEM
                    if (!string.IsNullOrEmpty(hfIDFactura.Value))
                    {
                        int idFac = int.Parse(hfIDFactura.Value);
                        controller.AgregarDetalleFactura(idFac, txtDetalleDescripcion.Text.Trim(), cant, precio, CurrentUser);
                        
                        var facActualizada = controller.ObtenerFacturaPorId(idFac); 
                        DetalleTemp = facActualizada.Detalles;
                    }
                    else
                    {
                        DetalleTemp.Add(new DetalleFacturaClass
                        {
                            IDDetalle = 0,
                            Descripcion = txtDetalleDescripcion.Text.Trim(),
                            Cantidad = cant,
                            PrecioUnitario = precio,
                            Total = cant * precio,
                            Estado = "A",
                            FechaAdicion = DateTime.Now,
                            UsuarioAgrego = CurrentUser
                        });
                    }
                }

                BindDetalles(); 
                LimpiarDetalleCampos();
            }
            catch (Exception ex) // Manejo de errores para mostrar mensajes amigables al usuario en caso de que las validaciones fallen o haya algún error al agregar el detalle
            {
                MostrarError(ex.Message); // Muestra el error en la interfa
            }
        }

        private void BindDetalles()// Enlaza la lista temporal de detalles al GridView y actualiza el monto total mostrad
        {
            gvDetallesTemp.DataSource = DetalleTemp;
            gvDetallesTemp.DataBind();
            lblMontoTotal.Text = "Monto Total: " + DetalleTemp.Where(d => d.Estado == "A").Sum(d => d.Total).ToString("C"); // Solo suma los detalles activos para mostrar el monto total correcto
        }

        private void LimpiarDetalleCampos() 
        {
            txtDetalleDescripcion.Text = "";
            txtDetalleCantidad.Text = "";
            txtDetallePrecio.Text = "";
            hfIndexDetalleEditing.Value = "-1"; // Resetear índice de edición
            btnAddDetalle.Text = "Agregar Item"; // Restaurar texto del botón
        }

        protected void btnGuardar_Click(object sender, EventArgs e)// Evento que se ejecuta al hacer clic en el botón para guardar la factura
        {
            if (!Page.IsValid) return; 
            try// Manejo de errores para mostrar mensajes amigables al usuario en caso de que las validaciones fallen o haya algún error al guardar la facturas
            {
                // Validaciones de factura
                if (string.IsNullOrWhiteSpace(txtCliente.Text)) throw new Exception("El nombre del cliente es obligatorio.");

                if (!string.IsNullOrEmpty(hfIDFactura.Value) && EsFacturaConNotaCredito(DetalleTemp)) 
                    throw new Exception("No se puede guardar cambios en una factura que ya tiene una Nota de Crédito.");

                string usuario = CurrentUser; 
                int idFactura; // Variable para almacenar el ID de la factura creada o actualizada, que se usará para recargar los datos y mostrar el reporte consolidado después de guardar
                bool esNotaCreditoGenerada = false;

                // LÓGICA DE NOTA DE CRÉDITO
                bool hayActivos = DetalleTemp.Any(d => d.Estado == "A");
                bool hayInactivos = DetalleTemp.Any(d => d.Estado == "I");

                if (!hayActivos)
                {
                    // Si no hay activos pero tampoco hay inactivos → no hay items
                    if (!hayInactivos)
                        throw new Exception("La factura debe tener al menos un item.");

                    // Si hay inactivos → generar Nota de Crédito
                    esNotaCreditoGenerada = true;

                    string idRef = string.IsNullOrEmpty(hfIDFactura.Value) ? "(Nueva)" : hfIDFactura.Value;

                    DetalleTemp.Add(new DetalleFacturaClass
                    {
                        IDDetalle = 0,
                        Descripcion = "Nota de Crédito de la Factura #" + idRef,
                        Cantidad = 0,
                        PrecioUnitario = 0,
                        Total = 0,
                        Estado = "A",
                        FechaAdicion = DateTime.Now,
                        UsuarioAgrego = CurrentUser
                    });
                }

                if (string.IsNullOrEmpty(hfIDFactura.Value)) // Si no hay ID en el campo oculto, significa que estamos creando una nueva factura, de lo contrario, estamos actualizando una existente
                {
                    // CREAR
                    idFactura = controller.AgregarFactura(txtCliente.Text.Trim(), usuario);
                    // Solo insertamos los que estén activos en memoria
                    foreach (var det in DetalleTemp.Where(d => d.Estado == "A")) // Recorre solo los detalles que están activos en la lista temporal para agregarlos a la base de datos, evitando agregar los que el usuario haya marcado como inactivos antes de guardar
                    {
                        controller.AgregarDetalleFactura(idFactura, det.Descripcion, det.Cantidad, det.PrecioUnitario, usuario);
                    }

                    if(esNotaCreditoGenerada) {
                         controller.EliminarFactura(idFactura, usuario); // EliminarFactura inactiva la factura
                    }

                    MostrarExito("Factura #" + idFactura + " creada exitosamente."); // Muestra un mensaje de éxito al usuario indicando que la factura se creó correctamente, incluyendo el ID de la factura recién creada para referencia
                }
                else
                {
                    // ACTUALIZAR
                    idFactura = int.Parse(hfIDFactura.Value);
                    controller.ActualizarFactura(idFactura, txtCliente.Text.Trim(), usuario);
                    
                    foreach (var det in DetalleTemp) // Recorre todos los detalles en la lista temporal para determinar si se deben actualizar, agregar o inactivar en la base de datos según su estado y si ya tienen un ID asignado (lo que indica que ya existe en la base de datos)
                    {
                        if (det.IDDetalle > 0)
                        {
                            // Sincronizar cambios básicos
                            controller.ActualizarDetalleFactura(det.IDDetalle, det.Descripcion, det.Cantidad, det.PrecioUnitario, usuario);
                        }
                        else if (det.Estado == "A")
                        {
                            // Nuevos agregados durante la edición
                            controller.AgregarDetalleFactura(idFactura, det.Descripcion, det.Cantidad, det.PrecioUnitario, usuario);
                        }
                    }

                    if(esNotaCreditoGenerada) {
                         controller.EliminarFactura(idFactura, usuario); // EliminarFactura inactiva la factura
                    }

                    MostrarExito("Factura #" + idFactura + " actualizada exitosamente."); // Muestra un mensaje de éxito al usuario indicando que la factura se actualizó correctamente, incluyendo el ID de la factura para referencia
                }

                // After save success, ensure UI sync
                CargarFacturas();
                BindDetalles();

                // REFRESCAR TODO desde BD para ver los cambios reales (incluyendo Estados)
                var facFinal = controller.ObtenerFacturaPorId(idFactura);
                CargarFacturaParaEditar(idFactura); // Esto sincroniza DetalleTemp y campos arriba
                FillReportLabels(facFinal); // Esto sincroniza el reporte consolidado
                gvDetallesNested.DataSource = facFinal.Detalles?
    .Where(d => d.Estado?.Trim() == "A")
    .ToList();

                gvDetallesNested.DataBind();
            }
            catch (Exception ex) // Manejo de errores para mostrar mensajes amigables al usuario en caso de que las validaciones fallen o haya algún error al guardar la factura
            {
                MostrarError(ex.Message);// Muestra el error en la interfaz. El mensaje de error se muestra al usuario para informarle sobre lo que salió mal durante el proceso de guardado, como validaciones fallidas o errores inesperados.
            }
        }

        protected void gvFacturas_RowCommand(object sender, GridViewCommandEventArgs e) // Evento que se ejecuta cuando se hace clic en un botón dentro de una fila del GridView de facturas, como "Ver Detalle" o "Editar". Permite mostrar los detalles de la factura seleccionada o cargarla para edición según el comando
        {
            int id = Convert.ToInt32(e.CommandArgument); // Obtiene el ID de la factura desde el CommandArgument del botón que se hizo clic, lo que permite identificar qué factura se está seleccionando para mostrar detalles o editar
            
            if (e.CommandName == "VerDetalle")
            {   // Cargar para solo ver detalles (sin editar). 
                var fac = controller.ObtenerFacturaPorId(id);
                FillReportLabels(fac);
                gvDetallesNested.DataSource = fac.Detalles?
    .Where(d => d.Estado?.Trim() == "A")
    .ToList();

                gvDetallesNested.DataBind();

                ScriptManager.RegisterStartupScript(this, GetType(), "ScrollToAudit", "setTimeout(function() { document.getElementById('auditItems').scrollIntoView({ behavior: 'smooth' }); }, 100);", true); // Desplaza suavemente la vista hacia
                // la sección de auditoría después de mostrar los detalles para que el usuario pueda ver toda la información relevante sin tener que desplazarse manualmente, mejorando la experiencia de usuario al revisar los detalles de la factura

            }
            else if (e.CommandName == "EditFactura") 
            {
                // Cargar para editar (Actualizar). Aquí se carga toda la información de la factura en los campos de edición, incluyendo los detalles, para que el usuario pueda modificarla.
                // También se actualiza el reporte consolidado para reflejar la información de la factura que se está editando, asegurando que el usuario vea los datos correctos en el reporte mientras realiza cambios.
                var fac = controller.ObtenerFacturaPorId(id);
                CargarFacturaParaEditar(id);
                FillReportLabels(fac);
                // Mostrar solo los items activos en el detalle
                var detallesActivos = fac.Detalles?
    .Where(d => d.Estado?.Trim() == "A")
    .ToList();

                gvDetallesNested.DataSource = detallesActivos;
                gvDetallesNested.DataBind();

                // Subir al formulario de edición automáticamente con un pequeño delay para asegurar efectividad en UpdatePanel
                ScriptManager.RegisterStartupScript(upnlMain, upnlMain.GetType(), "ScrollToHeader", "setTimeout(function() { window.scrollTo({ top: 0, behavior: 'smooth' }); }, 100);", true);
            }
        }

        protected void btnVolverListado_Click(object sender, EventArgs e)
        {
            string script = $@"
                // Increased timeout to ensure grid is fully rendered before scrolling and highlighting
                setTimeout(function() {{
                    var span = document.getElementById('facturaRow_{hfIDFactura.Value}');
                    var searchBar = document.getElementById('txtBusqueda');
                    if (span) {{
                        var row = span.closest('tr'); // Find parent row
                        if (row) {{
                            row.scrollIntoView({{ behavior: 'smooth', block: 'center' }});
                            // Ensure highlight animation restarts reliably
                            row.classList.remove('highlight-row');
                            // Force reflow
                            void row.offsetWidth;
                            // Add class after a short delay to allow scroll to finish
                            setTimeout(function() {{
                                row.classList.add('highlight-row');
                            }}, 100);
                        }}
                    }} else if (searchBar) {{
                        searchBar.scrollIntoView({{ behavior: 'smooth', block: 'center' }});
                        searchBar.focus();
                    }}
                }}, 300);";
                ScriptManager.RegisterStartupScript(upnlMain, upnlMain.GetType(), "ScrollToList", script, true);
        }

        protected void gvDetallesNested_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                // Obtener el estado del item actual para decidir si se oculta en impresión
                var estado = DataBinder.Eval(e.Row.DataItem, "Estado")?.ToString()?.Trim();
                if (estado != "A")
                {
                    // Añadir clase CSS para que el @media print lo oculte (definido en FacturaStyle.css)
                    e.Row.CssClass += " tr-inactivo-print";
                }
            }
        }

        // Método para cargar los datos de una factura específica en los campos de edición, incluyendo la información de auditoría y los detalles, para que el usuario pueda editarla.
        // También actualiza el reporte consolidado con la información de la factura que se está editando. 
        private void CargarFacturaParaEditar(int id) 
        {
            var fac = controller.ObtenerFacturaPorId(id);
            if (fac != null) // Verifica que la factura exista antes de intentar cargar sus datos en los campos de edición. Si la factura no se encuentra, no se realiza ninguna acción para evitar errores
            {
                hfIDFactura.Value = fac.IDFactura.ToString(); // Almacena el ID de la factura en un campo oculto para referencia futura, como al guardar los cambios, para saber si estamos editando una factura existente o creando una nueva
                lblFacturaIDDisplay.Text = fac.IDFactura.ToString(); // Muestra el ID de la factura en un label para que el usuario lo vea claramente mientras edita, proporcionando contexto sobre qué factura está modificando
                txtCliente.Text = fac.Cliente; // Carga el nombre del cliente en el campo de texto para que el usuario pueda verlo y editarlo si es necesario
                txtFechaEmision.Text = fac.FechaEmision.ToString("dd/MM/yyyy HH:mm"); 
                
                // Auditoría de Cabecera
                lblAgregadoPor.Text = !string.IsNullOrEmpty(fac.UsuarioAgrego) ? fac.UsuarioAgrego : CurrentUser;
                lblFechaAgrego.Text = fac.FechaAdicion?.ToString("dd/MM/yyyy HH:mm") ?? DateTime.Now.ToString("dd/MM/yyyy HH:mm");
                lblModificadoPor.Text = (!string.IsNullOrEmpty(fac.UsuarioModifico) && fac.UsuarioModifico != "-") ? fac.UsuarioModifico : "No modificado"; // Si no hay información de modificación, muestra "No modificado" para que el Usuario tenga claridad sobre el estado de la factura en cuanto a cambios posteriores a su creación
                lblFechaModifico.Text = fac.FechaModificacion?.ToString("dd/MM/yyyy HH:mm") ?? "-";

                DetalleTemp = fac.Detalles;
                BindDetalles();
                
                litAccion.Text = "Editando Factura #" + id; // Actualiza el título de la sección para reflejar que se está editando una factura específica, proporcionando contexto claro al usuario sobre la acción que está realizando
                btnGuardar.Text = "ACTUALIZAR CAMBIOS";
                btnLimpiar.Text = "CANCELAR EDICION";
                lblMensajeError.Text = ""; // Limpia cualquier mensaje de error previo para que el usuario no se confunda con mensajes que no son relevantes para la factura que acaba de cargar
                FillReportLabels(fac);

                // BLOQUEO POR NOTA DE CRÉDITO
                if (EsFacturaConNotaCredito(DetalleTemp))
                {
                    btnGuardar.Enabled = false;
                    btnAddDetalle.Enabled = false;
                    MostrarError("ESTA FACTURA CUENTA CON UNA NOTA DE CRÉDITO Y YA NO PUEDE SER MODIFICADA.");
                    litAccion.Text = "Consultando Factura (Solo Lectura) #" + id;
                }
                else
                {
                    btnGuardar.Enabled = true;
                    btnAddDetalle.Enabled = true;
                }
            }
        }

        private bool EsFacturaConNotaCredito(List<DetalleFacturaClass> detalles)
        {
            return detalles != null && detalles.Any(d => d.Descripcion.StartsWith("Nota de Crédito"));
        }

        private void FillReportLabels(FacturaClass fac) // Método para llenar los labels del reporte consolidado con la información de la factura proporcionada.
           //Esto se utiliza tanto al cargar una factura para edición como al mostrar los detalles para que el usuario tenga toda la información relevante visible en el reporte mientras interactúa con la factura
        {
            if (fac == null) return;
            lblRepID.Text = fac.IDFactura.ToString();
            lblRepFechaEmision.Text = fac.FechaEmision.ToString("dd/MM/yyyy HH:mm");
            lblRepCliente.Text = fac.Cliente;
            lblRepMontoTotal.Text = fac.MontoTotal.ToString("C");
            lblRepEstado.Text = fac.Estado == "A" ? "Activo" : "Inactivo";
            lblRepFechaAgrego.Text = fac.FechaAdicion?.ToString("dd/MM/yyyy HH:mm") ?? "N/A";
            lblRepAgregadoPor.Text = !string.IsNullOrEmpty(fac.UsuarioAgrego) ? fac.UsuarioAgrego : CurrentUser;
            lblRepFechaModifico.Text = fac.FechaModificacion?.ToString("dd/MM/yyyy HH:mm") ?? "-";
            lblRepModificadoPor.Text = (!string.IsNullOrEmpty(fac.UsuarioModifico) && fac.UsuarioModifico != "-") ? fac.UsuarioModifico : "No modificado";
        }

        protected void gvDetallesTemp_RowCommand(object sender, GridViewCommandEventArgs e) // Evento para procesar comandos en la lista de items
        {
            GridViewRow row = (GridViewRow)((Control)e.CommandSource).NamingContainer; 
            int index = row.RowIndex;

            if (index >= 0 && index < DetalleTemp.Count)
            {
                var det = DetalleTemp[index];

                if (e.CommandName == "ToggleDet") 
                {
                    // SEGURIDAD: No permitir inactivar la Nota de Crédito
                    if (det.Descripcion.StartsWith("Nota de Crédito"))
                    {
                        MostrarError("No se puede inactivar ni modificar el estado de la Nota de Crédito.");
                        return;
                    }

                    det.Estado = (det.Estado.Trim() == "A") ? "I" : "A";

                    if (!string.IsNullOrEmpty(hfIDFactura.Value) && det.IDDetalle > 0) 
                    {
                        string usuario = CurrentUser;
                        if (det.Estado.Trim() == "I")
                            controller.InactivarDetalleFactura(det.IDDetalle, usuario);
                        else
                            controller.ActivarDetalleFactura(det.IDDetalle, usuario); 

                        // Sincronizar reporte
                        var fac = controller.ObtenerFacturaPorId(int.Parse(hfIDFactura.Value));
                        FillReportLabels(fac);
                    }
                    BindDetalles();
                }
                else if (e.CommandName == "EditDet") // CARGAR DATOS PARA EDICIÓN
                {
                    if (det.Descripcion.StartsWith("Nota de Crédito"))
                    {
                        MostrarError("No se puede editar la Nota de Crédito de sistema.");
                        return;
                    }

                    txtDetalleDescripcion.Text = det.Descripcion;
                    txtDetalleCantidad.Text = det.Cantidad.ToString();
                    txtDetallePrecio.Text = det.PrecioUnitario.ToString("F2");
                    
                    hfIndexDetalleEditing.Value = index.ToString(); 
                    btnAddDetalle.Text = "Actualizar Item"; 
                    
                    txtDetalleDescripcion.Focus();
                }
            }
        }

        protected void btnLimpiar_Click(object sender, EventArgs e)
        {
            LimpiarTodo();
        }

        private void LimpiarTodo()
        {
            hfIDFactura.Value = "";
            lblFacturaIDDisplay.Text = "Nuevo";
            txtCliente.Text = "";
            txtFechaEmision.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            
            lblAgregadoPor.Text = CurrentUser;
            lblFechaAgrego.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            lblModificadoPor.Text = "No modificado";
            lblFechaModifico.Text = "-";

            DetalleTemp = new List<DetalleFacturaClass>();
            BindDetalles();
            LimpiarDetalleCampos();
            gvDetallesNested.DataSource = null;
            gvDetallesNested.DataBind();

            // Limpiar reporte
            lblRepID.Text = "-";
            lblRepFechaEmision.Text = "-";
            lblRepCliente.Text = "-";
            lblRepMontoTotal.Text = "$0.00";
            lblRepEstado.Text = "-";
            lblRepFechaAgrego.Text = "-";
            lblRepAgregadoPor.Text = "-";
            lblRepFechaModifico.Text = "-";
            lblRepModificadoPor.Text = "-";

            litAccion.Text = "Crear Nueva Factura";
            btnGuardar.Text = "GUARDAR FACTURA";
            btnGuardar.Enabled = true;
            btnAddDetalle.Enabled = true;
            btnLimpiar.Text = "LIMPIAR"; // En creación se limpia, en edición es cancelar
            lblMensajeError.Text = "";
            lblMensajeExito.Text = "";
        }

        private void MostrarError(string msg) 
        { 
            lblMensajeError.Text = msg; 
            lblMensajeExito.Text = "";
            // Usamos setTimeout y el UpdatePanel específico para asegurar que el scroll ocurra tras la actualización del DOM
            ScriptManager.RegisterStartupScript(upnlMain, upnlMain.GetType(), "ScrollTopErr", "setTimeout(function() { window.scrollTo({ top: 0, behavior: 'smooth' }); }, 100);", true);
        } 

        private void MostrarExito(string msg) 
        { 
            lblMensajeExito.Text = msg; 
            lblMensajeError.Text = "";
            ScriptManager.RegisterStartupScript(upnlMain, upnlMain.GetType(), "ScrollTopOk", "setTimeout(function() { window.scrollTo({ top: 0, behavior: 'smooth' }); }, 100);", true);
        }
    }
}