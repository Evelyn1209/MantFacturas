/* =========================================
   CREAR BASE DE DATOS
========================================= */

CREATE DATABASE MantFacturasBD
GO

USE MantFacturasBD
GO


/* =========================================
   TABLA FACTURAS
========================================= */

CREATE TABLE Facturas
(
    IDFactura INT IDENTITY(1,1) PRIMARY KEY,
    Cliente NVARCHAR(255) NOT NULL,
    FechaEmision DATETIME NOT NULL DEFAULT GETDATE(),
    MontoTotal DECIMAL(18,2) NOT NULL DEFAULT 0,
    Estado CHAR(1) NOT NULL DEFAULT 'A',

    -- Auditoría
    FechaAdicion DATETIME DEFAULT GETDATE(),
    UsuarioAgrego NVARCHAR(100),
    FechaModificacion DATETIME,
    UsuarioModifico NVARCHAR(100)
)
GO


/* =========================================
   TABLA DETALLE FACTURA
========================================= */

CREATE TABLE DetalleFactura
(
    IDDetalle INT IDENTITY(1,1) PRIMARY KEY,
    IDFactura INT NOT NULL,
    Descripcion NVARCHAR(255) NOT NULL,
    Cantidad INT NOT NULL,
    PrecioUnitario DECIMAL(18,2) NOT NULL,
    Total DECIMAL(18,2) NOT NULL,
    Estado CHAR(1) NOT NULL DEFAULT 'A',

    -- Auditoría
    FechaAdicion DATETIME DEFAULT GETDATE(),
    UsuarioAgrego NVARCHAR(100),
    FechaModificacion DATETIME,
    UsuarioModifico NVARCHAR(100),

    CONSTRAINT FK_DetalleFactura_Factura
    FOREIGN KEY (IDFactura)
    REFERENCES Facturas(IDFactura)
)
GO


/* =========================================
   STORED PROCEDURES
========================================= */


------------------------------------------------
-- INSERTAR FACTURA
------------------------------------------------

CREATE PROCEDURE sp_InsertarFactura
(
    @Cliente NVARCHAR(255),
    @UsuarioAgrego NVARCHAR(100)
)
AS
BEGIN

INSERT INTO Facturas
(
    Cliente,
    FechaEmision,
    MontoTotal,
    Estado,
    FechaAdicion,
    UsuarioAgrego
)
VALUES
(
    @Cliente,
    GETDATE(),
    0,
    'A',
    GETDATE(),
    @UsuarioAgrego
)

SELECT SCOPE_IDENTITY() AS IDFactura

END
GO


------------------------------------------------
-- INSERTAR DETALLE FACTURA
------------------------------------------------

CREATE PROCEDURE sp_InsertarDetalleFactura
(
    @IDFactura INT,
    @Descripcion NVARCHAR(255),
    @Cantidad INT,
    @PrecioUnitario DECIMAL(18,2),
    @UsuarioAgrego NVARCHAR(100)
)
AS
BEGIN

DECLARE @Total DECIMAL(18,2)

SET @Total = @Cantidad * @PrecioUnitario

INSERT INTO DetalleFactura
(
    IDFactura,
    Descripcion,
    Cantidad,
    PrecioUnitario,
    Total,
    Estado,
    FechaAdicion,
    UsuarioAgrego
)
VALUES
(
    @IDFactura,
    @Descripcion,
    @Cantidad,
    @PrecioUnitario,
    @Total,
    'A',
    GETDATE(),
    @UsuarioAgrego
)

END
GO


------------------------------------------------
-- LISTAR FACTURAS
------------------------------------------------

CREATE PROCEDURE sp_ObtenerFacturas
AS
BEGIN

SELECT
    IDFactura,
    Cliente,
    FechaEmision,
    MontoTotal,
    Estado,
    FechaAdicion,
    UsuarioAgrego,
    FechaModificacion,
    UsuarioModifico
FROM Facturas

END
GO


------------------------------------------------
-- BUSCAR FACTURAS
------------------------------------------------

CREATE PROCEDURE sp_BuscarFacturas
(
    @Busqueda NVARCHAR(255)
)
AS
BEGIN

SELECT
    IDFactura,
    Cliente,
    FechaEmision,
    MontoTotal,
    Estado,
    FechaAdicion,
    UsuarioAgrego,
    FechaModificacion,
    UsuarioModifico
FROM Facturas
WHERE Cliente LIKE '%' + @Busqueda + '%'
OR CAST(IDFactura AS NVARCHAR) LIKE '%' + @Busqueda + '%'

END
GO


------------------------------------------------
-- OBTENER DETALLE DE FACTURA
------------------------------------------------

CREATE PROCEDURE sp_ObtenerDetalleFactura
(
    @IDFactura INT
)
AS
BEGIN

SELECT
    IDDetalle,
    IDFactura,
    Descripcion,
    Cantidad,
    PrecioUnitario,
    Total,
    Estado,

    CASE
        WHEN Estado = 'A' THEN 'Activo'
        WHEN Estado = 'I' THEN 'Inactivo'
    END AS EstadoDescripcion,

    FechaAdicion,
    UsuarioAgrego,
    FechaModificacion,
    UsuarioModifico

FROM DetalleFactura
WHERE IDFactura = @IDFactura

END
GO


------------------------------------------------
-- ACTUALIZAR FACTURA
------------------------------------------------

CREATE PROCEDURE sp_ActualizarFactura
(
    @IDFactura INT,
    @Cliente NVARCHAR(255),
    @UsuarioModifico NVARCHAR(100)
)
AS
BEGIN

UPDATE Facturas
SET 
    Cliente = @Cliente,
    UsuarioModifico = @UsuarioModifico,
    FechaModificacion = GETDATE()
WHERE IDFactura = @IDFactura

END
GO


------------------------------------------------
-- ELIMINAR / INACTIVAR FACTURA
------------------------------------------------

CREATE PROCEDURE sp_EliminarFactura
(
    @IDFactura INT,
    @UsuarioModifico NVARCHAR(100)
)
AS
BEGIN

UPDATE Facturas
SET 
    Estado = 'I',
    UsuarioModifico = @UsuarioModifico,
    FechaModificacion = GETDATE()
WHERE IDFactura = @IDFactura

END
GO

------------------------------------------------
-- ACTUALIZAR DETALLE FACTURA
------------------------------------------------

CREATE PROCEDURE sp_ActualizarDetalleFactura
(
    @IDDetalle INT,
    @Descripcion NVARCHAR(255),
    @Cantidad INT,
    @PrecioUnitario DECIMAL(18,2),
    @UsuarioModifico NVARCHAR(100)
)
AS
BEGIN

DECLARE @Total DECIMAL(18,2)

SET @Total = @Cantidad * @PrecioUnitario

UPDATE DetalleFactura
SET 
    Descripcion = @Descripcion,
    Cantidad = @Cantidad,
    PrecioUnitario = @PrecioUnitario,
    Total = @Total,
    FechaModificacion = GETDATE(),
    UsuarioModifico = @UsuarioModifico
WHERE IDDetalle = @IDDetalle

END
GO


------------------------------------------------
-- INACTIVAR DETALLE FACTURA
------------------------------------------------

CREATE PROCEDURE sp_InactivarDetalleFactura
(
    @IDDetalle INT,
    @UsuarioModifico NVARCHAR(100)
)
AS
BEGIN

UPDATE DetalleFactura
SET
    Estado = 'I',
    FechaModificacion = GETDATE(),
    UsuarioModifico = @UsuarioModifico
WHERE IDDetalle = @IDDetalle

END
GO


------------------------------------------------
-- ACTIVAR DETALLE FACTURA
------------------------------------------------

CREATE PROCEDURE sp_ActivarDetalleFactura
(
    @IDDetalle INT,
    @UsuarioModifico NVARCHAR(100)
)
AS
BEGIN

UPDATE DetalleFactura
SET
    Estado = 'A',
    FechaModificacion = GETDATE(),
    UsuarioModifico = @UsuarioModifico
WHERE IDDetalle = @IDDetalle

END
GO


/* =========================================
   TRIGGER PARA RECALCULAR TOTAL
========================================= */

CREATE TRIGGER TR_RecalcularTotalFactura
ON DetalleFactura
AFTER INSERT, UPDATE
AS
BEGIN

UPDATE f
SET MontoTotal =
(
    SELECT ISNULL(SUM(Total),0)
    FROM DetalleFactura d
    WHERE d.IDFactura = f.IDFactura
    AND d.Estado = 'A'
)
FROM Facturas f
INNER JOIN inserted i
ON f.IDFactura = i.IDFactura

END
GO