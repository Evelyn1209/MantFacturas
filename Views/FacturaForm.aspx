
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="FacturaForm.aspx.cs" Inherits="MantFacturas.Views.FacturaForm" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title>Facturación</title>
    <link rel="icon" type="image/icon.png" href="../Images/icon.png"/> 
    <link href="../Styles/FacturaStyle.css?v=1.2" rel="stylesheet" type="text/css"/>

    <script> //JavaScript 
        function filterTable() { // Función que se ejecuta cada vez que el usuario escribe en el buscador
            var input, filter, table, tr, tdId, tdCliente, i, txtValueId, txtValueCliente; // se crean varias variables que se usarán en el proceso.
            input = document.getElementById("txtBusqueda");
            filter = input.value.toUpperCase();  // Toma el valor escrito por el usuario y lo convierte a mayúsculas
            table = document.getElementById("<%= gvFacturas.ClientID %>"); //Obtener el GridView
            tr = table.getElementsByTagName("tr"); //Obtener todas las filas del Gridview

            for (i = 1; i < tr.length; i++) { //Recorrer todas las filas (tr.lenght= Cantidad total de filas en tr )
                tdId = tr[i].getElementsByTagName("td")[0]; //obtener todas las celdas de esa fila (td= data)
                tdCliente = tr[i].getElementsByTagName("td")[1];
                //if se ejecuta en cada fila de la tabla: primero verifica que existan las celdas de ID y Cliente, luego compara su contenido con el texto buscado; si coincide muestra la fila y si no coincide la oculta.
                if (tdId && tdCliente) {
                    txtValueId = tdId.textContent || tdId.innerText;
                    txtValueCliente = tdCliente.textContent || tdCliente.innerText;
                    if (txtValueId.toUpperCase().indexOf(filter) > -1 || txtValueCliente.toUpperCase().indexOf(filter) > -1) {
                        tr[i].style.display = "";
                    } else {
                        tr[i].style.display = "none";
                    }
                }
            }
        }
    </script>
</head>
<body>
    <form id="form1" runat="server">  <%--  el formulario será procesado por el servidor ASP.NET.  permitiendo controles como: TextBox, Button, GridView, Label y maneja eventos en el codebehind (FacturaForm.aspx.cs)--%>
        <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager> <%-- Permite AJAX = Es una técnica que permite actualizar parte de una página web sin recargar toda la página. Esto mejora la experiencia del usuario al hacer que las interacciones sean más rápidas y fluidas. --%>
        <div class="container">
            <h1 id="tituloFactura">SISTEMA DE FACTURACION PREMIUM</h1> 
            <%-- actualizar solo una parte de la página sin recargar todo. --%>
            <asp:UpdatePanel ID="upnlMain" runat="server">
                <ContentTemplate>
                    
                    <%-- MENSAJES DE EXITO Y DE ERROR --%>
                    <div style="text-align:center; margin-bottom: 20px;">
                        <asp:Label ID="lblMensajeExito" runat="server" CssClass="texto-exito"></asp:Label>
                        <asp:Label ID="lblMensajeError" runat="server" CssClass="texto-error"></asp:Label>
                    </div>

                    <%-- SECCIÓN: CREAR / EDITAR --%>
                    <div class="formulario section-card">
                    <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 10px;"> 
                        <h3 id="subtituloAccion"><asp:Literal ID="litAccion" runat="server" Text="Crear Nueva Factura"></asp:Literal></h3>
                        <asp:Button ID="btnNuevaFactura" runat="server" Text="+ NUEVA FACTURA" OnClick="btnLimpiar_Click" CssClass="btn-nueva" CausesValidation="false" /> <%--  Botón que ejecuta un evento en C#. En el archivo FacturaForm.aspx.cs:--%>
                    </div>
                        <%--Campo oculto que guarda datos sin mostrarlos.--%>
                        <asp:HiddenField ID="hfIDFactura" runat="server" />  
                        <asp:HiddenField ID="hfIndexDetalleEditing" runat="server" Value="-1" /> <%-- Guarda el índice del item que se está editando (-1 = ninguno) --%>
                        
                        <%--ESPACIOS DEL CLIENTE--%>
                        <div class="flex-row">
                            <div class="flex-col" style="flex: 2;">
                                <label>Cliente:</label>
                                <%-- campo de texto donde el usuario escribe información.--%>
                                <asp:TextBox ID="txtCliente" runat="server" placeholder="Nombre del cliente"></asp:TextBox> 
                                <%-- Verifica que el campo no esté vacío. Si esta vacio envia mensaje de error --%>
                                <asp:RequiredFieldValidator ID="rfvCliente" runat="server" ControlToValidate="txtCliente" 
                                    ErrorMessage="El nombre del cliente es obligatorio" ForeColor="black" Display="Dynamic" ValidationGroup="Main" /> 
                            </div>
                            <div class="flex-col">
                                <label>Fecha Emision:</label>
                                <%-- se llena automáticamente desde C#.--%>
                                <asp:TextBox ID="txtFechaEmision" runat="server" ReadOnly="true" Enabled="false"></asp:TextBox>
                            </div>
                        </div>

                        <%-- AUDITORÍA DE FACTURA --%>
                        <div class="audit-section">
                            <div class="audit-item"><b>ID:</b> <asp:Label ID="lblFacturaIDDisplay" runat="server" Text="Nuevo"></asp:Label></div>
                            <div class="audit-item"><b>Creado por:</b> <asp:Label ID="lblAgregadoPor" runat="server" Text="-"></asp:Label></div>
                            <div class="audit-item"><b>F. Creacion:</b> <asp:Label ID="lblFechaAgrego" runat="server" Text="-"></asp:Label></div>
                            <div class="audit-item"><b>Modificado por:</b> <asp:Label ID="lblModificadoPor" runat="server" Text="-"></asp:Label></div>
                            <div class="audit-item"><b>F. Modificacion:</b> <asp:Label ID="lblFechaModifico" runat="server" Text="-"></asp:Label></div>
                        </div>

                        <div class="section-card" style="background:#fff;">
                            <h4>Gestion de items de la factura</h4>
                            <div class="flex-row">
                                <div style="flex: 2;">
                                    <label>Descripcion:</label>
                                    <asp:TextBox ID="txtDetalleDescripcion" runat="server" placeholder="Ej. Producto A"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="rfvDesc" runat="server" ControlToValidate="txtDetalleDescripcion" 
                                        ErrorMessage="La descripcion es obligatoria" ForeColor="Black" Display="Dynamic" ValidationGroup="AddDetalle" />
                                </div>
                                <div style="flex: 0.5;">
                                    <label>Cantidad:</label>
                                    <asp:TextBox ID="txtDetalleCantidad" runat="server" TextMode="Number" min="1"></asp:TextBox>
                                    <%-- Verifica que el campo no esté vacío. Si esta vacio envia mensaje de error --%>
                                    <asp:RequiredFieldValidator ID="rfvCant" runat="server" ControlToValidate="txtDetalleCantidad" 
                                        ErrorMessage="La cantidad es obligatoria" ForeColor="Red" Display="Dynamic" ValidationGroup="AddDetalle" />
                                    <%-- Se usa para validar valores numéricos.la cantidad debe ser mayor a 0.--%>
                                    <asp:CompareValidator ID="cvCant" runat="server" ControlToValidate="txtDetalleCantidad" Operator="GreaterThan" ValueToCompare="0" Type="Integer"
                                        ErrorMessage="Debe ser mayor a 0" ForeColor="Black" Display="Dynamic" ValidationGroup="AddDetalle" />
                                </div>
                                <div style="flex: 0.5;">
                                    <label>Precio Unit:</label>
                                    <asp:TextBox ID="txtDetallePrecio" runat="server" placeholder="0.00"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="rfvPrecio" runat="server" ControlToValidate="txtDetallePrecio" 
                                        ErrorMessage="El precio es obligatorio" ForeColor="Black" Display="Dynamic" ValidationGroup="AddDetalle" />
<asp:CompareValidator ID="cvPrecio" runat="server"
ControlToValidate="txtDetallePrecio"
Operator="GreaterThan"
ValueToCompare="0"
Type="Double"
ErrorMessage="El precio debe ser mayor a 0"
ForeColor="Black"
Display="Dynamic"
ValidationGroup="AddDetalle" />                                </div>
                                <div style="flex: 1; align-self: flex-end;">
                                    <%-- Boton, se jecuta en C#:--%>
                                    <asp:Button ID="btnAddDetalle" runat="server" Text="Agregar Item" OnClick="btnAddDetalle_Click" CssClass="asp-button" Style="width:100%; margin-bottom:10px;" ValidationGroup="AddDetalle" /> 
                                </div>
                            </div>

                            <%-- GRIDVIEW--%>
                            <%-- Control que muestra los detalles de la factura en forma de tabla.--%>
                            <asp:GridView ID="gvDetallesTemp" runat="server" AutoGenerateColumns="False" CssClass="gridview-factura" 
                                OnRowCommand="gvDetallesTemp_RowCommand" DataKeyNames="IDDetalle">
                                <Columns>
                                    <asp:TemplateField HeaderText="Descripcion"> 
                                        <ItemTemplate> 
       
                                            <span class='<%# Eval("Estado").ToString().Trim() == "I" ? "item-tachado" : "" %>'> 
                                                <asp:Literal runat="server" Text='<%# Eval("Descripcion") %>'></asp:Literal>
                                            </span>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:BoundField DataField="Cantidad" HeaderText="Cant" />
                                    <asp:BoundField DataField="PrecioUnitario" HeaderText="P. Unit" DataFormatString="{0:C}" />
                                    <asp:BoundField DataField="Total" HeaderText="Total" DataFormatString="{0:C}" />
                                    <asp:TemplateField HeaderText="Estado">
                                        <ItemTemplate>
                                            <span class='<%# Eval("Estado").ToString().Trim() == "A" ? "activo" : "inactivo" %>'>
                                                <%# Eval("Estado").ToString().Trim() == "A" ? "Activo" : "Inactivo" %>
                                            </span>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Auditor&iacute;a">
                                        <ItemTemplate>
                                            <div style="font-size:0.75em; color:#666;">
                                                <%# Eval("UsuarioAgrego") %> (<%# Eval("FechaAdicion", "{0:dd/MM/yy}") %>)
                                            </div>
                                        </ItemTemplate>
                                    </asp:TemplateField>


                                    <asp:TemplateField HeaderText="Acci&oacute;n"> <%--EDITAR DETALLE --%>
                                        <ItemTemplate>
                                            <div class="grid-actions">
                                                <asp:LinkButton ID="btnEditDet" runat="server" 
                                                    CommandName="EditDet" 
                                                    CommandArgument='<%# Container.DataItemIndex %>' 
                                                    CssClass="btn-actualizar"
                                                    Text="Editar"
                                                    Visible='<%# !Eval("Descripcion").ToString().StartsWith("Nota de Crédito") %>'
                                                    CausesValidation="false" />

                                                <%-- Botón dentro del GridView, activa o desactiva un detalle--%>
                                                <asp:LinkButton ID="btnToggleDet" runat="server" 
                                                    CommandName="ToggleDet" 
                                                    CommandArgument='<%# Container.DataItemIndex %>' 
                                                    CssClass='<%# Eval("Estado").ToString().Trim() == "A" ? "btn-eliminar" : "btn-nueva" %>'
                                                    Text='<%# Eval("Estado").ToString().Trim() == "A" ? "Inactivar" : "Activar" %>'
                                                    Visible='<%# !Eval("Descripcion").ToString().StartsWith("Nota de Crédito") %>'
                                                    CausesValidation="false" />
                                            </div>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                </Columns>
                            </asp:GridView>
                        </div>

                        <%-- Monto total --%>
                        <div style="margin-top: 25px; border-top: 1px solid #eee; padding-top: 15px;">
                            <div style="text-align: center; margin-bottom: 10px;">
                                <asp:Label ID="lblMontoTotal" runat="server" Text="Monto Total: $0.00" Font-Bold="true" Font-Size="X-Large" style="color:#1693A5;"></asp:Label>
                            </div>

                            <%-- Boton guardar la factura en la base de datos.--%>
                            <div class="centered-buttons">
                                <asp:Button ID="btnGuardar" runat="server" Text="GUARDAR FACTURA" OnClick="btnGuardar_Click" CssClass="btn-nueva main-action-btn" ValidationGroup="Main" />
                                <asp:Button ID="btnLimpiar" runat="server" Text="LIMPIAR" OnClick="btnLimpiar_Click" CssClass="btn-limpiar main-action-btn" CausesValidation="false" />
                            </div>
                        </div>
                    </div>

                    <%-- SECCIÓN: LISTADO DE FACTURAS --%>
                    <div class="section-card">
                        <h3>Listado de Facturas</h3>
                        
                        <%--Sirve para buscar facturas por ID o cliente.--%>
                        <div class="busqueda-realtime">
                            <label>Filtrar Facturas:</label>
                            <input type="text" id="txtBusqueda" onkeyup="filterTable()" placeholder="Escriba cliente o ID..." style="width: 300px;"/>
                        </div>
                        <%-- GridView es un control de ASP.NET que muestra datos en forma de tabla.--%>
                        <asp:GridView ID="gvFacturas" runat="server" AutoGenerateColumns="False" CssClass="gridview-factura"
                            OnRowCommand="gvFacturas_RowCommand" DataKeyNames="IDFactura">
                            <%--BoundField muestra datos directamente de la base o del objeto.--%>
                            <Columns>
                                <asp:TemplateField HeaderText="ID Factura">
                                    <ItemTemplate>
                                        <span id='facturaRow_<%# Eval("IDFactura") %>'></span>
                                        <%# Eval("IDFactura") %>
                                    </ItemTemplate>

                                </asp:TemplateField>
                                <asp:BoundField DataField="Cliente" HeaderText="Cliente" />
                                <asp:BoundField DataField="FechaEmision" HeaderText="Fecha Emisi&oacute;n" DataFormatString="{0:dd/MM/yyyy}" />
                                <asp:BoundField DataField="MontoTotal" HeaderText="Monto Total" DataFormatString="{0:C}" />
                                <%-- Templatefield= Aquí se usa para mostrar los detalles de la factura. En general, permite personalizar cómo se muestran los datos.--%>
                                <asp:TemplateField HeaderText="Detalles (Resumen)">
                                    <ItemTemplate>
                                        <div style="font-size:0.85em; max-height:60px; overflow-y:auto; border-left: 2px solid #1693A5; padding-left:5px;">
                                            <%-- Repeater recorre una lista de datos. Aquí muestra los productos o servicios de la factura.--%>
                                            <asp:Repeater ID="rptDetalles" runat="server" DataSource='<%# Eval("Detalles") %>'>
                                                <ItemTemplate>
                                                    - <%# Eval("Descripcion") %> (x<%# Eval("Cantidad") %>)<br />
                                                    - <%# Eval("Total", "{0:C}") %><br />
                                                </ItemTemplate>
                                            </asp:Repeater>
                                        </div>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Estado" Visible="false">
                                    <ItemTemplate>
                                        <span class='<%# Eval("Estado").ToString().Trim() == "A" ? "activo" : "inactivo" %>'>
                                            <%# Eval("Estado").ToString().Trim() == "A" ? "Activa" : "Inactiva" %>
                                        </span>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Acci&oacute;n">
                                    <HeaderStyle HorizontalAlign="Center" />
                                    <ItemTemplate>
                                        <%-- Estos botones ejecutan el evento en C#:--%>
                                        <div class="grid-actions">
                                            <asp:Button ID="btnVer" runat="server" Text="Ver" CommandName="VerDetalle" CommandArgument='<%# Eval("IDFactura") %>' CssClass="asp-button" CausesValidation="false" />
                                            <asp:Button ID="btnActualizar" runat="server" Text="Editar" CommandName="EditFactura" CommandArgument='<%# Eval("IDFactura") %>' CssClass="btn-actualizar" CausesValidation="false" Visible='<%# Eval("Estado").ToString().Trim() == "A" %>' />
                                            <asp:Button ID="btnInactivaPlaceholder" runat="server" Text="INACTIVA" CssClass="asp-button btn-inactivo-placeholder" Enabled="false" Visible='<%# Eval("Estado").ToString().Trim() != "A" %>' />
                                        </div>
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                    </div>

                    <%-- SECCIÓN: INFORME DE FACTURA (VISTA UNIFICADA MASTER-DETAIL) --%>
                    <div id="auditItems" class="section-card report-container" style="margin-top:30px;">
                        
                        <div class="report-header" style="display: flex !important; justify-content: space-between !important; align-items: center !important; border-bottom: 2px solid #1693A5 !important; padding-bottom: 10px !important; margin-bottom: 20px !important;">
                            <h3 style="margin: 0 !important; border: none !important;">Informe Consolidado de la Factura</h3>
                            <div class="report-header-actions" style="display: flex !important; gap: 15px !important;">
                                <asp:LinkButton ID="btnBackToList" runat="server" CssClass="btn-back" OnClick="btnVolverListado_Click" CausesValidation="false" ToolTip="Volver al Listado">
                                    <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3" stroke-linecap="round" stroke-linejoin="round">
                                        <polyline points="15 18 9 12 15 6"></polyline>
                                    </svg>
                                </asp:LinkButton>
                            </div>
                        </div>

                        <table class="header-report-table"> <%-- Muestra información de cabecera de la factura:--%>
                            <tr>
                                <td>IDFactura:</td>
                                <td colspan="3"><asp:Label ID="lblRepID" runat="server" Text="-"></asp:Label></td>
                            </tr>
                            <tr>
                                <td>Fecha Emisi&oacute;n:</td>
                                <td><asp:Label ID="lblRepFechaEmision" runat="server" Text="-"></asp:Label></td>
                                <td>Fecha Agrego:</td>
                                <td><asp:Label ID="lblRepFechaAgrego" runat="server" Text="-"></asp:Label></td>
                            </tr>
                            <tr>
                                <td>Cliente:</td>
                                <td><asp:Label ID="lblRepCliente" runat="server" Text="-"></asp:Label></td> <%-- Label muestra información cargada desde C#.--%>
                                <td>Agregado por:</td>
                                <td><asp:Label ID="lblRepAgregadoPor" runat="server" Text="-"></asp:Label></td>
                            </tr>
                            <tr>
                                <td>Monto Total:</td>
                                <td><b><asp:Label ID="lblRepMontoTotal" runat="server" Text="$0.00"></asp:Label></b></td>
                                <td>Fecha Modifico:</td>
                                <td><asp:Label ID="lblRepFechaModifico" runat="server" Text="-"></asp:Label></td>
                            </tr>
                            <tr>
                                <td>Estado:</td>
                                <td><asp:Label ID="lblRepEstado" runat="server" Text="-"></asp:Label></td>
                                <td>Modificado por:</td>
                                <td><asp:Label ID="lblRepModificadoPor" runat="server" Text="-"></asp:Label></td>
                            </tr>
                        </table>

                        <h4 style="margin-bottom:10px; color:#1693A5;">Detalles del Documento:</h4>

                        <%--GridView de detalles muestra los productos o servicios de la factura.--%>
                        
                        <asp:GridView ID="gvDetallesNested" runat="server" AutoGenerateColumns="False" CssClass="gridview-report"
                            OnRowDataBound="gvDetallesNested_RowDataBound">
                            <Columns>
                                <asp:BoundField DataField="Descripcion" HeaderText="Descripci&oacute;n" />
                                <asp:BoundField DataField="Cantidad" HeaderText="Cantidad" />
                                <asp:BoundField DataField="PrecioUnitario" HeaderText="Precio Unitario" DataFormatString="{0:C}" />
                                <asp:BoundField DataField="Total" HeaderText="Total" DataFormatString="{0:C}" />
                                <asp:TemplateField HeaderText="Estado">
                                    <ItemTemplate>
                                        <span class='<%# Eval("Estado").ToString() == "A" ? "activo" : "inactivo" %>'>
                                            <%# Eval("Estado").ToString() == "A" ? "Activo" : "Desactivado" %>
                                        </span>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="FechaAdicion" HeaderText="Fecha Agrego" DataFormatString="{0:dd/MM/yy HH:mm}" NullDisplayText="N/A" />
                                <asp:TemplateField HeaderText="Agregado por">
                                    <ItemTemplate><%# !string.IsNullOrEmpty(Eval("UsuarioAgrego")?.ToString()) ? Eval("UsuarioAgrego") : CurrentUser %></ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="FechaModificacion" HeaderText="Fecha Modifico" DataFormatString="{0:dd/MM/yy HH:mm}" NullDisplayText="---" />
                                <asp:TemplateField HeaderText="Modificado por">
                                    <ItemTemplate><%# (!string.IsNullOrEmpty(Eval("UsuarioModifico")?.ToString()) && Eval("UsuarioModifico").ToString() != "-") ? Eval("UsuarioModifico") : "No modificado" %></ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                    </div>

                </ContentTemplate>
            </asp:UpdatePanel> <%-- Permite usar AJAX en ASP.NET. para actualizar solo una parte --%>
        </div>
    </form>
</body>
</html>