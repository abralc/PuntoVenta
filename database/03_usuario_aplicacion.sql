-- =====================================================================
--  Usuario de MySQL exclusivo para la aplicación (no usar root en la API)
--  CAMBIAR la contraseña antes de ejecutar y usar la misma en
--  src/PuntoVenta.Api/appsettings.json -> ConnectionStrings:PuntoVenta
-- =====================================================================

CREATE USER IF NOT EXISTS 'pv_app'@'localhost' IDENTIFIED BY 'PvApp2026*';
GRANT SELECT, INSERT, UPDATE, DELETE, EXECUTE ON db_punto_venta.* TO 'pv_app'@'localhost';
FLUSH PRIVILEGES;
