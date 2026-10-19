-- =====================================================================
--  Punto de Venta - Datos semilla (generados de forma consistente)
--  * existencia   = total comprado - total vendido
--  * precio_costo = costo de la última compra
--  * precio_venta = precio_costo + 25 %
--
--  Usuarios de prueba (cambiar contraseñas después de la demo):
--    admin    / Admin123*     (Administrador)
--    vendedor / Vendedor123*  (Vendedor)
--    compras  / Compras123*   (Compras)
-- =====================================================================

USE db_punto_venta;
SET NAMES utf8mb4;
START TRANSACTION;

INSERT INTO puestos (id_puesto, puesto) VALUES
  (1,'Gerente General'),
  (2,'Cajero'),
  (3,'Vendedor'),
  (4,'Bodeguero'),
  (5,'Encargado de Compras'),
  (6,'Contador');

INSERT INTO empleados (id_empleado, nombres, apellidos, direccion, telefono, dpi, genero, fecha_nacimiento, id_puesto, fecha_inicio_labores) VALUES
  (1,'Abraham Isaac','López Cifuentes','Zona 1, Ciudad de Guatemala','5512-3401','2456789010101',b'1','2000-03-14',1,'2023-01-09'),
  (2,'Ángel Andre','Barrientos García','Zona 7, Mixco','5512-3402','2456789020101',b'1','2001-07-22',2,'2023-02-01'),
  (3,'Sharon Georgette','Calancho Toledo','Zona 18, Ciudad de Guatemala','5512-3403','2456789030101',b'0','2002-11-05',3,'2023-03-15'),
  (4,'Elder David','Orellana Caal','Zona 6, Villa Nueva','5512-3404','2456789040101',b'1','1999-01-30',4,'2023-04-10'),
  (5,'Jhostin Estuardo','Quiñonez Aguilar','Zona 12, Ciudad de Guatemala','5512-3405','2456789050101',b'1','2001-09-18',5,'2023-05-02'),
  (6,'Eutimio Rafael','Zamora Monterroso','Zona 3, San Miguel Petapa','5512-3406','2456789060101',b'1','1998-05-27',6,'2023-06-12'),
  (7,'María José','Hernández Pérez','Zona 11, Ciudad de Guatemala','5512-3407','2456789070101',b'0','1997-12-03',2,'2024-01-08'),
  (8,'Carlos Alberto','Méndez Rivas','Zona 5, Ciudad de Guatemala','5512-3408','2456789080101',b'1','1995-08-19',3,'2024-02-19'),
  (9,'Ana Lucía','Morales Juárez','Zona 10, Ciudad de Guatemala','5512-3409','2456789090101',b'0','1996-04-11',2,'2024-06-03'),
  (10,'Luis Fernando','García Ramírez','Zona 2, Mixco','5512-3410','2456789100101',b'1','1994-10-25',4,'2025-01-13');

INSERT INTO clientes (id_cliente, nombres, apellidos, nit, genero, telefono, correo_electronico) VALUES
  (1,'Consumidor','Final','CF',b'1',NULL,NULL),
  (2,'José Manuel','Castillo Lemus','4567891-2',b'1','4410-2201','jose.castillo@correo.com'),
  (3,'Karla Patricia','Ruiz Ordóñez','5678912-3',b'0','4410-2202','karla.ruiz@correo.com'),
  (4,'Diego Alejandro','Paz Monzón','6789123-4',b'1','4410-2203','diego.paz@correo.com'),
  (5,'Andrea Sofía','Cabrera Lima','7891234-5',b'0','4410-2204','andrea.cabrera@correo.com'),
  (6,'Roberto Carlos','Estrada Soto','8912345-6',b'1','4410-2205','roberto.estrada@correo.com'),
  (7,'Gabriela María','Fuentes Barrios','9123456-7',b'0','4410-2206','gabriela.fuentes@correo.com'),
  (8,'Jorge Luis','Sandoval Toj','1234567-8',b'1','4410-2207','jorge.sandoval@correo.com'),
  (9,'Daniela Alejandra','Velásquez Chew','2345678-9',b'0','4410-2208','daniela.velasquez@correo.com'),
  (10,'Kevin Josué','Ajanel Coc','3456789-K',b'1','4410-2209','kevin.ajanel@correo.com');

INSERT INTO proveedores (id_proveedor, proveedor, nit, direccion, telefono) VALUES
  (1,'Distribuidora Tecnológica, S.A.','1122334-5','Calzada Roosevelt 22-43, Zona 11','2201-1001'),
  (2,'Importadora Centroamericana, S.A.','2233445-6','Avenida Petapa 45-10, Zona 12','2201-1002'),
  (3,'Electrónica del Valle, S.A.','3344556-7','Boulevard Los Próceres 18-50, Zona 10','2201-1003'),
  (4,'Computadoras y Accesorios GT','4455667-8','6a. Avenida 9-20, Zona 1','2201-1004'),
  (5,'Mayoreo Digital, S.A.','5566778-9','Calzada San Juan 30-15, Zona 7','2201-1005'),
  (6,'Suministros de Oficina Maya','6677889-1','Anillo Periférico 17-36, Zona 11','2201-1006'),
  (7,'TecnoPartes Guatemala','7788991-2','Avenida Las Américas 7-20, Zona 13','2201-1007'),
  (8,'Global Store Distribuciones','8899112-3','Carretera a El Salvador Km 14','2201-1008');

INSERT INTO marcas (id_marca, marca) VALUES
  (1,'HP'),
  (2,'Dell'),
  (3,'Lenovo'),
  (4,'Logitech'),
  (5,'Samsung'),
  (6,'Kingston'),
  (7,'Epson'),
  (8,'TP-Link'),
  (9,'Xiaomi'),
  (10,'Genérica');

INSERT INTO productos (id_producto, producto, id_marca, descripcion, imagen, precio_costo, precio_venta, existencia) VALUES
  (1,'Laptop 15 pulgadas',1,'Laptop Core i5, 8 GB RAM, SSD 512 GB',NULL,4560.00,5700.00,19),
  (2,'Monitor 24 pulgadas',2,'Monitor Full HD IPS 75 Hz',NULL,978.50,1223.13,36),
  (3,'Laptop ThinkPad E14',3,'Laptop Ryzen 5, 16 GB RAM, SSD 512 GB',NULL,6200.00,7750.00,26),
  (4,'Mouse inalámbrico M185',4,'Mouse inalámbrico USB, 1000 DPI',NULL,87.55,109.44,59),
  (5,'Teclado K120',4,'Teclado USB en español',NULL,95.00,118.75,13),
  (6,'Monitor 27 pulgadas',5,'Monitor curvo Full HD 75 Hz',NULL,1377.50,1721.88,26),
  (7,'Memoria USB 64 GB',6,'Memoria USB 3.2 DataTraveler',NULL,55.00,68.75,48),
  (8,'Memoria RAM 8 GB DDR4',6,'Memoria RAM 3200 MHz para laptop',NULL,185.40,231.75,5),
  (9,'Impresora EcoTank L3250',7,'Multifuncional de tinta continua Wi-Fi',NULL,1350.00,1687.50,22),
  (10,'Router Archer C6',8,'Router Wi-Fi doble banda AC1200',NULL,320.00,400.00,33),
  (11,'Audífonos Redmi Buds',9,'Audífonos Bluetooth con estuche de carga',NULL,160.00,200.00,20),
  (12,'Cable HDMI 2 metros',10,'Cable HDMI 2.0 4K',NULL,25.75,32.19,36),
  (13,'Disco SSD 480 GB',6,'Unidad de estado sólido SATA',NULL,267.80,334.75,58),
  (14,'Webcam C270',4,'Cámara web HD 720p con micrófono',NULL,216.30,270.38,52),
  (15,'Tinta Epson 544 negra',7,'Botella de tinta original 65 ml',NULL,70.00,87.50,68),
  (16,'Smartphone Galaxy A15',5,'Teléfono 128 GB, 4 GB RAM',NULL,1545.00,1931.25,106),
  (17,'Mouse pad gamer',10,'Mouse pad 80 x 30 cm',NULL,42.75,53.44,32),
  (18,'Adaptador USB Wi-Fi',8,'Adaptador inalámbrico USB N300',NULL,65.00,81.25,62),
  (19,'Monitor 22 pulgadas',1,'Monitor Full HD 60 Hz',NULL,780.00,975.00,31),
  (20,'Teclado y mouse combo',2,'Combo inalámbrico teclado y mouse',NULL,228.00,285.00,20);

INSERT INTO compras (id_compra, no_orden_compra, id_proveedor, fecha_orden, fecha_ingreso) VALUES
  (1,1001,1,'2026-08-03','2026-08-03 09:30:00'),
  (2,1002,2,'2026-08-09','2026-08-09 09:30:00'),
  (3,1003,3,'2026-08-15','2026-08-15 09:30:00'),
  (4,1004,4,'2026-08-21','2026-08-21 09:30:00'),
  (5,1005,5,'2026-08-27','2026-08-27 09:30:00'),
  (6,1006,6,'2026-09-02','2026-09-02 09:30:00'),
  (7,1007,7,'2026-09-08','2026-09-08 09:30:00'),
  (8,1008,8,'2026-09-14','2026-09-14 09:30:00');

INSERT INTO compras_detalle (id_compra_detalle, id_compra, id_producto, cantidad, precio_costo_unitario) VALUES
  (1,1,1,11,4800.00),
  (2,1,2,24,902.50),
  (3,1,3,15,6386.00),
  (4,1,4,25,87.55),
  (5,1,5,22,95.00),
  (6,1,6,8,1377.50),
  (7,1,7,11,55.00),
  (8,1,8,11,185.40),
  (9,1,9,8,1390.50),
  (10,1,10,18,320.00),
  (11,2,11,20,160.00),
  (12,2,12,19,25.00),
  (13,2,13,20,247.00),
  (14,2,14,18,199.50),
  (15,2,15,25,70.00),
  (16,2,16,17,1545.00),
  (17,2,17,12,45.00),
  (18,2,18,8,65.00),
  (19,2,19,19,803.40),
  (20,2,20,21,228.00),
  (21,3,13,21,267.80),
  (22,3,19,20,780.00),
  (23,3,18,21,66.95),
  (24,3,16,24,1425.00),
  (25,3,4,15,85.00),
  (26,4,2,12,978.50),
  (27,4,17,15,42.75),
  (28,4,4,22,87.55),
  (29,4,14,25,210.00),
  (30,4,16,25,1545.00),
  (31,5,10,21,320.00),
  (32,5,18,17,65.00),
  (33,5,17,8,42.75),
  (34,5,16,12,1545.00),
  (35,5,13,24,267.80),
  (36,6,12,24,25.75),
  (37,6,7,21,52.25),
  (38,6,9,17,1350.00),
  (39,6,16,22,1425.00),
  (40,6,15,22,70.00),
  (41,7,15,16,72.10),
  (42,7,6,19,1377.50),
  (43,7,14,14,216.30),
  (44,7,16,12,1545.00),
  (45,7,3,8,5890.00),
  (46,8,1,15,4560.00),
  (47,8,15,9,70.00),
  (48,8,18,19,65.00),
  (49,8,3,8,6200.00),
  (50,8,7,23,55.00);

INSERT INTO ventas (id_venta, no_factura, serie, fecha_factura, id_cliente, id_empleado, fecha_ingreso) VALUES
  (1,1,'A','2026-09-01',9,3,'2026-09-01 15:10:00'),
  (2,2,'A','2026-09-04',10,7,'2026-09-04 15:10:00'),
  (3,3,'A','2026-09-07',2,2,'2026-09-07 15:10:00'),
  (4,4,'A','2026-09-10',8,7,'2026-09-10 15:10:00'),
  (5,5,'A','2026-09-13',10,9,'2026-09-13 15:10:00'),
  (6,6,'A','2026-09-16',10,7,'2026-09-16 15:10:00'),
  (7,7,'A','2026-09-19',9,2,'2026-09-19 15:10:00'),
  (8,8,'A','2026-09-22',7,7,'2026-09-22 15:10:00'),
  (9,9,'A','2026-09-25',5,8,'2026-09-25 15:10:00'),
  (10,10,'A','2026-09-28',9,9,'2026-09-28 15:10:00'),
  (11,11,'A','2026-10-01',6,8,'2026-10-01 15:10:00'),
  (12,12,'A','2026-10-04',9,9,'2026-10-04 15:10:00');

INSERT INTO ventas_detalle (id_venta_detalle, id_venta, id_producto, cantidad, precio_unitario) VALUES
  (1,1,19,1,975.00),
  (2,1,5,4,118.75),
  (3,1,17,2,53.44),
  (4,1,10,2,400.00),
  (5,2,13,4,334.75),
  (6,2,5,1,118.75),
  (7,2,17,1,53.44),
  (8,2,7,4,68.75),
  (9,3,12,4,32.19),
  (10,3,1,3,5700.00),
  (11,3,19,3,975.00),
  (12,4,16,4,1931.25),
  (13,5,5,2,118.75),
  (14,5,19,4,975.00),
  (15,6,20,1,285.00),
  (16,6,8,2,231.75),
  (17,6,3,2,7750.00),
  (18,6,13,1,334.75),
  (19,7,7,2,68.75),
  (20,7,16,2,1931.25),
  (21,7,1,2,5700.00),
  (22,7,5,2,118.75),
  (23,8,18,3,81.25),
  (24,8,14,2,270.38),
  (25,8,3,3,7750.00),
  (26,8,13,2,334.75),
  (27,9,14,3,270.38),
  (28,9,10,4,400.00),
  (29,9,15,4,87.50),
  (30,9,6,1,1721.88),
  (31,10,4,1,109.44),
  (32,10,7,1,68.75),
  (33,10,8,4,231.75),
  (34,11,1,2,5700.00),
  (35,11,12,3,32.19),
  (36,12,4,2,109.44),
  (37,12,9,3,1687.50);

INSERT INTO roles (id_rol, rol) VALUES
  (1,'Administrador'),
  (2,'Vendedor'),
  (3,'Compras');

INSERT INTO usuarios (id_usuario, usuario, password_hash, id_empleado, id_rol, activo) VALUES
  (1,'admin','$2b$11$vnUleIvP3JNSHU2C73jRXeef9xKVzdDLmiQ5MDE4rxR//SBmvOxLq',1,1,b'1'),
  (2,'vendedor','$2b$11$aNCBvjfWNktmo5pxhspWSuvyeeMX49H.dqPFi/VUv5IeyJoHT2jYa',2,2,b'1'),
  (3,'compras','$2b$11$KsNIyQfY.5RxeeDshbFPse3eqpoI9Gdu0dmbpaE8k7sIgPSN0YDAC',5,3,b'1');

INSERT INTO menu (id_menu, nombre, url, icono, id_padre, orden, activo) VALUES
  (1,'Inicio','/','bi-house-door',NULL,0,b'1'),
  (2,'Productos','/productos','bi-box-seam',NULL,1,b'1'),
  (3,'Marcas','/marcas','bi-tags',2,1,b'1'),
  (4,'Ventas','/ventas','bi-receipt',NULL,2,b'1'),
  (5,'Clientes','/clientes','bi-people',4,1,b'1'),
  (6,'Empleados','/empleados','bi-person-badge',4,2,b'1'),
  (7,'Puestos','/puestos','bi-diagram-3',6,1,b'1'),
  (8,'Punto de Venta','/pos','bi-shop',4,3,b'1'),
  (9,'Compras','/compras','bi-cart-plus',NULL,3,b'1'),
  (10,'Proveedores','/proveedores','bi-truck',9,1,b'1'),
  (11,'Reportes','/reportes','bi-bar-chart-line',NULL,4,b'1'),
  (12,'Reporte de productos','/reportes/productos','bi-file-earmark-text',11,1,b'1'),
  (13,'Reporte de existencias','/reportes/existencias','bi-boxes',11,2,b'1'),
  (14,'Reporte de compras','/reportes/compras','bi-file-earmark-arrow-down',11,3,b'1'),
  (15,'Reporte de ventas','/reportes/ventas','bi-file-earmark-arrow-up',11,4,b'1'),
  (16,'Administración',NULL,'bi-gear',NULL,5,b'1'),
  (17,'Usuarios','/usuarios','bi-person-lock',16,1,b'1'),
  (18,'Configuración','/configuracion','bi-sliders',16,2,b'1');

INSERT INTO roles_menu (id_rol, id_menu) VALUES
  (1,1),
  (1,2),
  (1,3),
  (1,4),
  (1,5),
  (1,6),
  (1,7),
  (1,8),
  (1,9),
  (1,10),
  (1,11),
  (1,12),
  (1,13),
  (1,14),
  (1,15),
  (1,16),
  (1,17),
  (1,18),
  (2,1),
  (2,2),
  (2,3),
  (2,4),
  (2,5),
  (2,8),
  (2,11),
  (2,12),
  (2,13),
  (2,15),
  (3,1),
  (3,2),
  (3,3),
  (3,9),
  (3,10),
  (3,11),
  (3,12),
  (3,13),
  (3,14);

INSERT INTO configuracion (clave, valor, descripcion) VALUES
  ('porcentaje_ganancia','25','Porcentaje que se suma al precio de costo para calcular el precio de venta'),
  ('serie_factura','A','Serie por defecto de las facturas de venta'),
  ('minutos_reserva_pos','15','Minutos que dura la reserva de un producto en el POS'),
  ('existencia_minima','5','Existencia a partir de la cual se considera producto por agotarse');

INSERT INTO reservas_pos (id_producto, cantidad, sesion_pos, id_usuario, expira) VALUES
  (4, 1, '00000000-0000-0000-0000-000000000001', 2, DATE_ADD(NOW(), INTERVAL 15 MINUTE));

COMMIT;
