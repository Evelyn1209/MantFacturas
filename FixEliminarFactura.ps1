$connectionString = "Data Source=EVELYN\PROGRAV;Initial Catalog=MantFacturasBD;Integrated Security=True"
$sql = @"
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
        FechaModificacion = GETDATE(),
        UsuarioModifico = @UsuarioModifico
    WHERE IDFactura = @IDFactura
END
"@

$conn = New-Object System.Data.SqlClient.SqlConnection($connectionString)
try {
    $conn.Open()
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = $sql
    $cmd.ExecuteNonQuery()
    Write-Host "Stored procedure created successfully."
}
catch {
    Write-Error $_.Exception.Message
}
finally {
    $conn.Close()
}
