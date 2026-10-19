-- =====================================================================
--  Punto de Venta - Esquema de base de datos
--  Motor: MySQL 8.0  |  Charset: utf8mb4
--  Basado en el diagrama ER del enunciado + tablas de seguridad,
--  menú dinámico, configuración y reservas del POS.
--
--  Ejecutar en orden:
--    01_esquema.sql  ->  02_datos_semilla.sql  ->  03_usuario_aplicacion.sql
-- =====================================================================

DROP DATABASE IF EXISTS db_punto_venta;
CREATE DATABASE db_punto_venta
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_0900_ai_ci;

USE db_punto_venta;

-- ---------------------------------------------------------------------
--  CATÁLOGOS
-- ---------------------------------------------------------------------

CREATE TABLE puestos (
    id_puesto   SMALLINT     NOT NULL AUTO_INCREMENT,
    puesto      VARCHAR(50)  NOT NULL,
    CONSTRAINT pk_puestos PRIMARY KEY (id_puesto),
    CONSTRAINT uq_puestos_puesto UNIQUE (puesto)
) ENGINE = InnoDB;

CREATE TABLE empleados (
    id_empleado         INT          NOT NULL AUTO_INCREMENT,
    nombres             VARCHAR(60)  NOT NULL,
    apellidos           VARCHAR(60)  NOT NULL,
    direccion           VARCHAR(80)  NOT NULL,
    telefono            VARCHAR(25)  NOT NULL,
    dpi                 VARCHAR(15)  NOT NULL,
    genero              BIT(1)       NOT NULL COMMENT '1 = Masculino, 0 = Femenino',
    fecha_nacimiento    DATE         NOT NULL,
    id_puesto           SMALLINT     NOT NULL,
    fecha_inicio_labores DATE        NOT NULL,
    fecha_ingreso       DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT pk_empleados PRIMARY KEY (id_empleado),
    CONSTRAINT uq_empleados_dpi UNIQUE (dpi),
    CONSTRAINT fk_empleados_puestos FOREIGN KEY (id_puesto)
        REFERENCES puestos (id_puesto) ON UPDATE CASCADE ON DELETE RESTRICT
) ENGINE = InnoDB;

CREATE TABLE clientes (
    id_cliente          INT          NOT NULL AUTO_INCREMENT,
    nombres             VARCHAR(60)  NOT NULL,
    apellidos           VARCHAR(60)  NOT NULL,
    nit                 VARCHAR(12)  NOT NULL COMMENT 'CF = Consumidor Final (puede repetirse)',
    genero              BIT(1)       NOT NULL COMMENT '1 = Masculino, 0 = Femenino',
    telefono            VARCHAR(25)  NULL,
    correo_electronico  VARCHAR(45)  NULL,
    fecha_ingreso       DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT pk_clientes PRIMARY KEY (id_cliente),
    INDEX ix_clientes_nit (nit)
    -- El NIT no es UNIQUE porque "CF" se repite. La API valida duplicados
    -- de NIT distintos de "CF".
) ENGINE = InnoDB;

CREATE TABLE proveedores (
    id_proveedor    INT          NOT NULL AUTO_INCREMENT,
    proveedor       VARCHAR(60)  NOT NULL,
    nit             VARCHAR(12)  NOT NULL,
    direccion       VARCHAR(80)  NOT NULL,
    telefono        VARCHAR(25)  NOT NULL,
    CONSTRAINT pk_proveedores PRIMARY KEY (id_proveedor),
    CONSTRAINT uq_proveedores_nit UNIQUE (nit)
) ENGINE = InnoDB;

CREATE TABLE marcas (
    id_marca    SMALLINT     NOT NULL AUTO_INCREMENT,
    marca       VARCHAR(50)  NOT NULL,
    CONSTRAINT pk_marcas PRIMARY KEY (id_marca),
    CONSTRAINT uq_marcas_marca UNIQUE (marca)
) ENGINE = InnoDB;

CREATE TABLE productos (
    id_producto     INT            NOT NULL AUTO_INCREMENT,
    producto        VARCHAR(50)    NOT NULL,
    id_marca        SMALLINT       NOT NULL,
    descripcion     VARCHAR(100)   NOT NULL,
    imagen          VARCHAR(255)   NULL COMMENT 'Ruta relativa, ej. /images/productos/xxx.jpg (ampliado desde VARCHAR(30) del ER)',
    precio_costo    DECIMAL(8,2)   NOT NULL DEFAULT 0.00,
    precio_venta    DECIMAL(8,2)   NOT NULL DEFAULT 0.00,
    existencia      INT            NOT NULL DEFAULT 0,
    fecha_ingreso   DATETIME       NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT pk_productos PRIMARY KEY (id_producto),
    CONSTRAINT uq_productos_producto_marca UNIQUE (producto, id_marca),
    CONSTRAINT ck_productos_existencia CHECK (existencia >= 0),
    CONSTRAINT ck_productos_precios CHECK (precio_costo >= 0 AND precio_venta >= 0),
    CONSTRAINT fk_productos_marcas FOREIGN KEY (id_marca)
        REFERENCES marcas (id_marca) ON UPDATE CASCADE ON DELETE RESTRICT
) ENGINE = InnoDB;

-- ---------------------------------------------------------------------
--  COMPRAS (maestro / detalle)
-- ---------------------------------------------------------------------

CREATE TABLE compras (
    id_compra        INT       NOT NULL AUTO_INCREMENT,
    no_orden_compra  INT       NOT NULL,
    id_proveedor     INT       NOT NULL,
    fecha_orden      DATE      NOT NULL,
    fecha_ingreso    DATETIME  NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT pk_compras PRIMARY KEY (id_compra),
    CONSTRAINT uq_compras_proveedor_orden UNIQUE (id_proveedor, no_orden_compra),
    CONSTRAINT fk_compras_proveedores FOREIGN KEY (id_proveedor)
        REFERENCES proveedores (id_proveedor) ON UPDATE CASCADE ON DELETE RESTRICT
) ENGINE = InnoDB;

CREATE TABLE compras_detalle (
    id_compra_detalle      BIGINT        NOT NULL AUTO_INCREMENT,
    id_compra              INT           NOT NULL,
    id_producto            INT           NOT NULL,
    cantidad               INT           NOT NULL,
    precio_costo_unitario  DECIMAL(8,2)  NOT NULL,
    CONSTRAINT pk_compras_detalle PRIMARY KEY (id_compra_detalle),
    CONSTRAINT ck_compras_detalle_cantidad CHECK (cantidad > 0),
    CONSTRAINT ck_compras_detalle_precio CHECK (precio_costo_unitario > 0),
    CONSTRAINT fk_compras_detalle_compras FOREIGN KEY (id_compra)
        REFERENCES compras (id_compra) ON UPDATE CASCADE ON DELETE CASCADE,
    CONSTRAINT fk_compras_detalle_productos FOREIGN KEY (id_producto)
        REFERENCES productos (id_producto) ON UPDATE CASCADE ON DELETE RESTRICT
) ENGINE = InnoDB;

-- ---------------------------------------------------------------------
--  VENTAS (maestro / detalle)
-- ---------------------------------------------------------------------

CREATE TABLE ventas (
    id_venta       INT       NOT NULL AUTO_INCREMENT,
    no_factura     INT       NOT NULL,
    serie          CHAR(1)   NOT NULL,
    fecha_factura  DATE      NOT NULL,
    id_cliente     INT       NOT NULL,
    id_empleado    INT       NOT NULL,
    fecha_ingreso  DATETIME  NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT pk_ventas PRIMARY KEY (id_venta),
    CONSTRAINT uq_ventas_serie_factura UNIQUE (serie, no_factura),
    CONSTRAINT fk_ventas_clientes FOREIGN KEY (id_cliente)
        REFERENCES clientes (id_cliente) ON UPDATE CASCADE ON DELETE RESTRICT,
    CONSTRAINT fk_ventas_empleados FOREIGN KEY (id_empleado)
        REFERENCES empleados (id_empleado) ON UPDATE CASCADE ON DELETE RESTRICT
) ENGINE = InnoDB;

CREATE TABLE ventas_detalle (
    id_venta_detalle  BIGINT        NOT NULL AUTO_INCREMENT,
    id_venta          INT           NOT NULL,
    id_producto       INT           NOT NULL,
    cantidad          INT           NOT NULL,
    precio_unitario   DECIMAL(8,2)  NOT NULL,
    CONSTRAINT pk_ventas_detalle PRIMARY KEY (id_venta_detalle),
    CONSTRAINT ck_ventas_detalle_cantidad CHECK (cantidad > 0),
    CONSTRAINT ck_ventas_detalle_precio CHECK (precio_unitario > 0),
    CONSTRAINT fk_ventas_detalle_ventas FOREIGN KEY (id_venta)
        REFERENCES ventas (id_venta) ON UPDATE CASCADE ON DELETE CASCADE,
    CONSTRAINT fk_ventas_detalle_productos FOREIGN KEY (id_producto)
        REFERENCES productos (id_producto) ON UPDATE CASCADE ON DELETE RESTRICT
) ENGINE = InnoDB;

-- ---------------------------------------------------------------------
--  SEGURIDAD: roles, usuarios y menú dinámico
-- ---------------------------------------------------------------------

CREATE TABLE roles (
    id_rol  SMALLINT     NOT NULL AUTO_INCREMENT,
    rol     VARCHAR(30)  NOT NULL,
    CONSTRAINT pk_roles PRIMARY KEY (id_rol),
    CONSTRAINT uq_roles_rol UNIQUE (rol)
) ENGINE = InnoDB;

CREATE TABLE usuarios (
    id_usuario      INT           NOT NULL AUTO_INCREMENT,
    usuario         VARCHAR(30)   NOT NULL,
    password_hash   VARCHAR(100)  NOT NULL COMMENT 'Hash BCrypt, nunca texto plano',
    id_empleado     INT           NULL,
    id_rol          SMALLINT      NOT NULL,
    activo          BIT(1)        NOT NULL DEFAULT b'1',
    fecha_ingreso   DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT pk_usuarios PRIMARY KEY (id_usuario),
    CONSTRAINT uq_usuarios_usuario UNIQUE (usuario),
    CONSTRAINT fk_usuarios_empleados FOREIGN KEY (id_empleado)
        REFERENCES empleados (id_empleado) ON UPDATE CASCADE ON DELETE SET NULL,
    CONSTRAINT fk_usuarios_roles FOREIGN KEY (id_rol)
        REFERENCES roles (id_rol) ON UPDATE CASCADE ON DELETE RESTRICT
) ENGINE = InnoDB;

-- Menú dinámico: estructura padre-hijo (id_padre NULL = opción principal)
CREATE TABLE menu (
    id_menu   INT           NOT NULL AUTO_INCREMENT,
    nombre    VARCHAR(50)   NOT NULL,
    url       VARCHAR(100)  NULL COMMENT 'Ruta de Blazor; NULL si solo agrupa subopciones',
    icono     VARCHAR(50)   NULL COMMENT 'Clase de Bootstrap Icons, ej. bi-box-seam',
    id_padre  INT           NULL,
    orden     INT           NOT NULL DEFAULT 0,
    activo    BIT(1)        NOT NULL DEFAULT b'1',
    CONSTRAINT pk_menu PRIMARY KEY (id_menu),
    CONSTRAINT fk_menu_padre FOREIGN KEY (id_padre)
        REFERENCES menu (id_menu) ON UPDATE CASCADE ON DELETE RESTRICT
) ENGINE = InnoDB;

-- Qué opciones del menú ve cada rol
CREATE TABLE roles_menu (
    id_rol   SMALLINT  NOT NULL,
    id_menu  INT       NOT NULL,
    CONSTRAINT pk_roles_menu PRIMARY KEY (id_rol, id_menu),
    CONSTRAINT fk_roles_menu_roles FOREIGN KEY (id_rol)
        REFERENCES roles (id_rol) ON UPDATE CASCADE ON DELETE CASCADE,
    CONSTRAINT fk_roles_menu_menu FOREIGN KEY (id_menu)
        REFERENCES menu (id_menu) ON UPDATE CASCADE ON DELETE CASCADE
) ENGINE = InnoDB;

-- ---------------------------------------------------------------------
--  CONFIGURACIÓN GENERAL (ej. porcentaje de ganancia del precio de venta)
-- ---------------------------------------------------------------------

CREATE TABLE configuracion (
    clave        VARCHAR(50)   NOT NULL,
    valor        VARCHAR(100)  NOT NULL,
    descripcion  VARCHAR(150)  NULL,
    CONSTRAINT pk_configuracion PRIMARY KEY (clave)
) ENGINE = InnoDB;

-- ---------------------------------------------------------------------
--  POS: reservas temporales de producto por sesión de caja
--  Disponible = existencia - SUM(reservas vigentes de OTRAS sesiones)
-- ---------------------------------------------------------------------

CREATE TABLE reservas_pos (
    id_reserva      BIGINT    NOT NULL AUTO_INCREMENT,
    id_producto     INT       NOT NULL,
    cantidad        INT       NOT NULL,
    sesion_pos      CHAR(36)  NOT NULL COMMENT 'GUID de la pantalla POS abierta',
    id_usuario      INT       NOT NULL,
    fecha_creacion  DATETIME  NOT NULL DEFAULT CURRENT_TIMESTAMP,
    expira          DATETIME  NOT NULL,
    CONSTRAINT pk_reservas_pos PRIMARY KEY (id_reserva),
    CONSTRAINT uq_reservas_pos_sesion_producto UNIQUE (sesion_pos, id_producto),
    CONSTRAINT ck_reservas_pos_cantidad CHECK (cantidad > 0),
    INDEX ix_reservas_pos_producto_expira (id_producto, expira),
    CONSTRAINT fk_reservas_pos_productos FOREIGN KEY (id_producto)
        REFERENCES productos (id_producto) ON UPDATE CASCADE ON DELETE CASCADE,
    CONSTRAINT fk_reservas_pos_usuarios FOREIGN KEY (id_usuario)
        REFERENCES usuarios (id_usuario) ON UPDATE CASCADE ON DELETE CASCADE
) ENGINE = InnoDB;
