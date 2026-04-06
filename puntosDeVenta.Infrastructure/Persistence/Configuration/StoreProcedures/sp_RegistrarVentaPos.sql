USE [db_puntosDeVenta_dev]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [sales].[sp_RegistrarVentaPos]
    @PosId NVARCHAR(20),
    @CashierId NVARCHAR(20),
    @SaleDate DATETIME2(7),
    @TotalAmount DECIMAL(12, 2),
    @ItemsJson NVARCHAR(MAX),
    @SaleId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- 1. Validaciones b�sicas
        IF @PosId IS NULL OR @PosId = ''
            THROW 50001, 'El PosId es requerido', 1;

        IF @CashierId IS NULL OR @CashierId = ''
            THROW 50002, 'El CashierId es requerido', 1;

        IF @TotalAmount <= 0
            THROW 50003, 'El TotalAmount debe ser mayor a 0', 1;

        IF @ItemsJson IS NULL OR @ItemsJson = ''
            THROW 50004, 'Los items no pueden estar vac�os', 1;

        -- 2. Validar que la suma de items coincida con el total
        -- IMPORTANTE: Usamos $.Quantity y $.UnitPrice (May�sculas) para coincidir con el DTO
        DECLARE @CalculatedTotal DECIMAL(12, 2);

        SELECT @CalculatedTotal = ISNULL(SUM(
            CAST(JSON_VALUE(value, '$.Quantity') AS INT) * CAST(JSON_VALUE(value, '$.UnitPrice') AS DECIMAL(12, 2))
        ), 0)
        FROM OPENJSON(@ItemsJson) AS items;

        -- Depuraci�n: Si la suma falla, lanzamos el error con los valores para ver qu� pas�
        IF ABS(@CalculatedTotal - @TotalAmount) > 0.01
        BEGIN
            DECLARE @Msg NVARCHAR(200) = FORMATMESSAGE('La suma de items (%s) no coincide con el total enviado (%s)', 
                                         CAST(@CalculatedTotal AS NVARCHAR), CAST(@TotalAmount AS NVARCHAR));
            THROW 50005, @Msg, 1;
        END

        -- 3. Insertar cabecera de venta
        INSERT INTO [sales].[sale_headers] 
            ([pos_id], [cashier_id], [sale_date], [total_amount], [status], [created_at], [updated_at])
        VALUES 
            (@PosId, @CashierId, @SaleDate, @TotalAmount, 'Registered', GETUTCDATE(), GETUTCDATE());

        SET @SaleId = SCOPE_IDENTITY();

        -- 4. Insertar detalles de venta
        -- Ajustamos las llaves del JSON aqu� tambi�n: $.ProductId, $.Quantity, $.UnitPrice
        INSERT INTO [sales].[sale_details] 
            ([sale_header_id], [product_id], [quantity], [unit_price], [subtotal], [created_at])
        SELECT 
            @SaleId,
            CAST(JSON_VALUE(value, '$.ProductId') AS INT),
            CAST(JSON_VALUE(value, '$.Quantity') AS INT),
            CAST(JSON_VALUE(value, '$.UnitPrice') AS DECIMAL(12, 2)),
            CAST(JSON_VALUE(value, '$.Quantity') AS INT) * CAST(JSON_VALUE(value, '$.UnitPrice') AS DECIMAL(12, 2)),
            GETUTCDATE()
        FROM OPENJSON(@ItemsJson) AS items;

        -- 5. Confirmar transacci�n
        COMMIT TRANSACTION;

    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        DECLARE @ErrorMessage NVARCHAR(MAX) = ERROR_MESSAGE();
        DECLARE @ErrorNumber INT = ERROR_NUMBER();

        THROW @ErrorNumber, @ErrorMessage, 1;
    END CATCH
END;