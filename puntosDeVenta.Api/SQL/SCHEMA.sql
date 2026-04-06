-- ============================================================
-- INVENTORY SCHEMA + SAMPLE DATA (SQL Server)
-- CREATE TABLES, INSERT SAMPLE DATA and SELECT for critical items
-- ============================================================

-- DROP (si es necesario para pruebas)
IF OBJECT_ID('dbo.Ventas_Detalle', 'U') IS NOT NULL DROP TABLE dbo.Ventas_Detalle;
IF OBJECT_ID('dbo.Ventas_Cabecera', 'U') IS NOT NULL DROP TABLE dbo.Ventas_Cabecera;
IF OBJECT_ID('dbo.Inventario_POS', 'U') IS NOT NULL DROP TABLE dbo.Inventario_POS;
IF OBJECT_ID('dbo.Productos', 'U') IS NOT NULL DROP TABLE dbo.Productos;
GO

-- TABLA: Productos
CREATE TABLE Productos (
    producto_id INT IDENTITY(1,1) PRIMARY KEY,
    codigo_sku VARCHAR(50) NOT NULL UNIQUE,
    nombre VARCHAR(150) NOT NULL
);
GO

-- TABLA: Inventario_POS
CREATE TABLE Inventario_POS (
    pos_id INT NOT NULL,
    producto_id INT NOT NULL,
    stock_actual DECIMAL(18,2) NOT NULL,
    stock_minimo_permitido DECIMAL(18,2) NOT NULL,
    CONSTRAINT PK_Inventario_POS PRIMARY KEY (pos_id, producto_id),
    CONSTRAINT FK_Inventario_Producto FOREIGN KEY (producto_id) REFERENCES Productos(producto_id)
);
GO

-- Datos de ejemplo: Productos
INSERT INTO Productos (codigo_sku, nombre) VALUES
('SKU-BREAD-01',  'Pan Francés Congelado'),
('SKU-BREAD-02',  'Pan Aliñado'),
('SKU-BREAD-03',  'Mogolla Integral'),
('SKU-BREAD-04',  'Pan de Queso'),
('SKU-BREAD-05',  'Pandebono'),
('SKU-CAKE-01',   'Ponqué de Vainilla'),
('SKU-CAKE-02',   'Brownie Doble Chocolate'),
('SKU-CAKE-03',   'Torta de Zanahoria'),
('SKU-CAKE-04',   'Galleta de Avena'),
('SKU-CAKE-05',   'Torta de Chocolate'),
('SKU-BEV-01',    'Café Americano'),
('SKU-BEV-02',    'Cappuccino'),
('SKU-COMBO-01',  'Combo Desayuno Básico'),
('SKU-COMBO-02',  'Combo Café + Pastel'),
('SKU-SNACK-01',  'Empanada de Carne');
GO

-- Datos de ejemplo: Inventario por POS (usando los mismos datos de la descripción)
INSERT INTO Inventario_POS (pos_id, producto_id, stock_actual, stock_minimo_permitido) VALUES
(105, 1,  15.0,  50.0),
(105, 2,  200.0, 30.0),
(105, 5,  80.0,  25.0),
(105, 7,  45.0,  10.0),
(105, 10, 1.0,   5.0),
(105, 11, 100.0, 20.0),
(105, 13, 60.0,  15.0),
(105, 14, 3.0,   10.0),

(210, 1,  120.0, 50.0),
(210, 3,  5.0,   20.0),
(210, 4,  75.0,  15.0),
(210, 6,  2.0,   8.0),
(210, 8,  30.0,  10.0),
(210, 9,  10.0,  10.0),
(210, 12, 50.0,  20.0),
(210, 15, 90.0,  25.0),

(330, 1,  100.0, 50.0),
(330, 2,  80.0,  30.0),
(330, 5,  60.0,  25.0),
(330, 10, 20.0,  5.0),
(330, 11, 45.0,  20.0),

(450, 5,  0.0,   25.0);
GO

-- TABLA: Ventas_Cabecera
IF OBJECT_ID('dbo.Ventas_Cabecera', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Ventas_Cabecera (
        venta_id INT IDENTITY(1,1) PRIMARY KEY,
        pos_id VARCHAR(20) NOT NULL,
        cashier_id VARCHAR(20) NOT NULL,
        sale_date DATETIME2 NOT NULL,
        total_amount DECIMAL(18,2) NOT NULL,
        created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
    );
END
GO

-- TABLA: Ventas_Detalle
IF OBJECT_ID('dbo.Ventas_Detalle', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Ventas_Detalle (
        detalle_id INT IDENTITY(1,1) PRIMARY KEY,
        venta_id INT NOT NULL,
        producto_id INT NOT NULL,
        quantity INT NOT NULL CHECK (quantity > 0),
        unit_price DECIMAL(18,2) NOT NULL CHECK (unit_price >= 0),
        subtotal DECIMAL(18,2) NOT NULL,
        CONSTRAINT FK_VentasDetalle_VentasCabecera FOREIGN KEY (venta_id) REFERENCES dbo.Ventas_Cabecera(venta_id)
    );
END
GO

-- STORED PROCEDURE: sp_RegistrarVentaPos
-- Parámetros:
-- @PosId NVARCHAR(20), @CashierId NVARCHAR(20), @SaleDate DATETIME2,
-- @TotalAmount DECIMAL(18,2), @ItemsJson NVARCHAR(MAX)
-- Retorna: @VentaId INT OUTPUT
IF OBJECT_ID('dbo.sp_RegistrarVentaPos', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_RegistrarVentaPos;
GO

CREATE PROCEDURE dbo.sp_RegistrarVentaPos
    @PosId NVARCHAR(20),
    @CashierId NVARCHAR(20),
    @SaleDate DATETIME2,
    @TotalAmount DECIMAL(18,2),
    @ItemsJson NVARCHAR(MAX),
    @VentaId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRAN;

        -- Insertar cabecera
        INSERT INTO dbo.Ventas_Cabecera (pos_id, cashier_id, sale_date, total_amount)
        VALUES (@PosId, @CashierId, @SaleDate, @TotalAmount);

        -- Obtener venta_id generado
        SET @VentaId = SCOPE_IDENTITY();

        -- Insertar detalle desde JSON
        -- items JSON expected: [{ "producto_id": 12, "quantity": 2, "unit_price": 10.00 }, ...]
        INSERT INTO dbo.Ventas_Detalle (venta_id, producto_id, quantity, unit_price, subtotal)
        SELECT
            @VentaId AS venta_id,
            CAST([producto_id] AS INT) AS producto_id,
            CAST([quantity] AS INT) AS quantity,
            CAST([unit_price] AS DECIMAL(18,2)) AS unit_price,
            CAST([quantity] AS INT) * CAST([unit_price] AS DECIMAL(18,2)) AS subtotal
        FROM OPENJSON(@ItemsJson)
        WITH (
            producto_id NVARCHAR(50) '$.producto_id',
            quantity INT '$.quantity',
            unit_price DECIMAL(18,2) '$.unit_price'
        );

        COMMIT TRAN;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRAN;

        DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@ErrMsg, 16, 1);
        RETURN;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_consultarProductosCriticos
    @Point_Id INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        p.producto_id,
        p.codigo_sku,
        p.nombre,
        inv.stock_actual AS stock,
        inv.stock_minimo_permitido
    FROM  Inventario_POS inv
	inner join Productos p on inv.producto_id = p.producto_id
	where inv.pos_id= @Point_Id and inv.stock_actual <= inv.stock_minimo_permitido;
END
-- ============================================================
-- CONSULTA: productos en estado CRÍTICO para un pos_id
-- Regla de negocio: stock_actual <= stock_minimo_permitido -> crítico
-- Uso: reemplazar @posId por el valor deseado o parametrizar en un SP
-- ============================================================
/*
SELECT
    p.producto_id AS producto_id,
    p.codigo_sku AS codigo_sku,
    p.nombre AS nombre,
    ip.stock_actual AS stock_actual,
    ip.stock_minimo_permitido AS stock_minimo_permitido
FROM Inventario_POS ip
INNER JOIN Productos p ON p.producto_id = ip.producto_id
WHERE ip.pos_id = @posId
  AND ip.stock_actual <= ip.stock_minimo_permitido
ORDER BY p.producto_id;
*/

-- Ejemplo de ejecución (pos_id = 105):
-- DECLARE @posId INT = 105;
-- EXEC sp_executesql N'SELECT ... above ...', N'@posId INT', @posId = @posId;



GO