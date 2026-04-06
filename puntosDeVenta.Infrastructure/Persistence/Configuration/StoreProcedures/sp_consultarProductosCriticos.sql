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