
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'CatalogoDB')
BEGIN
    CREATE DATABASE CatalogoDB;
END
GO

USE CatalogoDB;
GO

IF OBJECT_ID('dbo.Categoria', 'U') IS NOT NULL DROP TABLE dbo.Categoria;
CREATE TABLE dbo.Categoria (
    IdCategoria INT IDENTITY(1,1) NOT NULL,
    Nombre NVARCHAR(100) NOT NULL,
    Descripcion NVARCHAR(250) NULL,
    CONSTRAINT PK_Categoria PRIMARY KEY (IdCategoria)
);
GO

IF OBJECT_ID('dbo.Marca', 'U') IS NOT NULL DROP TABLE dbo.Marca;
CREATE TABLE dbo.Marca (
    IdMarca INT IDENTITY(1,1) NOT NULL,
    Nombre NVARCHAR(100) NOT NULL,
    PaisOrigen NVARCHAR(100) NULL,
    CONSTRAINT PK_Marca PRIMARY KEY (IdMarca)
);
GO

IF OBJECT_ID('dbo.Producto', 'U') IS NOT NULL DROP TABLE dbo.Producto;
CREATE TABLE dbo.Producto (
    IdProducto INT IDENTITY(1,1) NOT NULL,
    IdCategoria INT NOT NULL,
    IdMarca INT NOT NULL,
    Nombre NVARCHAR(150) NOT NULL,
    SKU NVARCHAR(50) NOT NULL UNIQUE,
    Precio DECIMAL(18,2) NOT NULL,
    Estado BIT NOT NULL DEFAULT 1, -- 1: Activo, 0: Inactivo
    CONSTRAINT PK_Producto PRIMARY KEY (IdProducto),
    CONSTRAINT FK_Producto_Categoria FOREIGN KEY (IdCategoria) REFERENCES dbo.Categoria(IdCategoria),
    CONSTRAINT FK_Producto_Marca FOREIGN KEY (IdMarca) REFERENCES dbo.Marca(IdMarca)
);
GO

INSERT INTO dbo.Categoria (Nombre, Descripcion) VALUES 
('Laptops', 'Equipos portátiles para trabajo y gaming'),
('Accesorios', 'Periféricos y complementos tecnológicos');

INSERT INTO dbo.Marca (Nombre, PaisOrigen) VALUES 
('Dell', 'Estados Unidos'),
('Logitech', 'Suiza');

INSERT INTO dbo.Producto (IdCategoria, IdMarca, Nombre, SKU, Precio, Estado) VALUES 
(1, 1, 'Laptop Dell Inspiron 15', 'DELL-INSP-15', 750.00, 1),
(2, 2, 'Mouse Inalámbrico Logitech M170', 'LOGI-M170', 15.50, 1);
GO



IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'InventarioDB')
BEGIN
    CREATE DATABASE InventarioDB;
END
GO

USE InventarioDB;
GO

IF OBJECT_ID('dbo.Bodega', 'U') IS NOT NULL DROP TABLE dbo.Bodega;
CREATE TABLE dbo.Bodega (
    IdBodega INT IDENTITY(1,1) NOT NULL,
    Nombre NVARCHAR(100) NOT NULL,
    Ciudad NVARCHAR(100) NOT NULL,
    Direccion NVARCHAR(200) NOT NULL,
    CONSTRAINT PK_Bodega PRIMARY KEY (IdBodega)
);
GO


IF OBJECT_ID('dbo.Existencia', 'U') IS NOT NULL DROP TABLE dbo.Existencia;
CREATE TABLE dbo.Existencia (
    IdExistencia INT IDENTITY(1,1) NOT NULL,
    IdProducto INT NOT NULL, 
    IdBodega INT NOT NULL,
    Stock INT NOT NULL DEFAULT 0,
    CONSTRAINT PK_Existencia PRIMARY KEY (IdExistencia),
    CONSTRAINT FK_Existencia_Bodega FOREIGN KEY (IdBodega) REFERENCES dbo.Bodega(IdBodega),
    CONSTRAINT UQ_Producto_Bodega UNIQUE (IdProducto, IdBodega)
);
GO


IF OBJECT_ID('dbo.MovimientoInventario', 'U') IS NOT NULL DROP TABLE dbo.MovimientoInventario;
CREATE TABLE dbo.MovimientoInventario (
    IdMovimiento INT IDENTITY(1,1) NOT NULL,
    IdProducto INT NOT NULL,
    IdBodega INT NOT NULL,
    Tipo NVARCHAR(50) NOT NULL, -- 'ENTRADA', 'SALIDA', 'AJUSTE'
    Cantidad INT NOT NULL,
    Fecha DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT PK_MovimientoInventario PRIMARY KEY (IdMovimiento),
    CONSTRAINT FK_MovimientoInventario_Bodega FOREIGN KEY (IdBodega) REFERENCES dbo.Bodega(IdBodega)
);
GO


IF OBJECT_ID('dbo.ReservaStock', 'U') IS NOT NULL DROP TABLE dbo.ReservaStock;
CREATE TABLE dbo.ReservaStock (
    IdReserva INT IDENTITY(1,1) NOT NULL,
    IdProducto INT NOT NULL,
    IdBodega INT NOT NULL,
    Cantidad INT NOT NULL,
    Estado NVARCHAR(50) NOT NULL DEFAULT 'PENDIENTE', -- 'PENDIENTE', 'CONFIRMADA', 'CANCELADA'
    CONSTRAINT PK_ReservaStock PRIMARY KEY (IdReserva),
    CONSTRAINT FK_ReservaStock_Bodega FOREIGN KEY (IdBodega) REFERENCES dbo.Bodega(IdBodega)
);
GO

INSERT INTO dbo.Bodega (Nombre, Ciudad, Direccion) VALUES 
('Bodega Central Quito', 'Quito', 'Av. 10 de Agosto y Colón'),
('Bodega Guayaquil Norte', 'Guayaquil', 'Av. Juan Tanca Marengo');

INSERT INTO dbo.Existencia (IdProducto, IdBodega, Stock) VALUES 
(1, 1, 20),
(1, 2, 10),
(2, 1, 50);
GO



IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'PedidosDB')
BEGIN
    CREATE DATABASE PedidosDB;
END
GO

USE PedidosDB;
GO

IF OBJECT_ID('dbo.Cliente', 'U') IS NOT NULL DROP TABLE dbo.Cliente;
CREATE TABLE dbo.Cliente (
    IdCliente INT IDENTITY(1,1) NOT NULL,
    Nombres NVARCHAR(150) NOT NULL,
    Cedula NVARCHAR(20) NOT NULL UNIQUE,
    Correo NVARCHAR(150) NOT NULL,
    CONSTRAINT PK_Cliente PRIMARY KEY (IdCliente)
);
GO

IF OBJECT_ID('dbo.Pedido', 'U') IS NOT NULL DROP TABLE dbo.Pedido;
CREATE TABLE dbo.Pedido (
    IdPedido INT IDENTITY(1,1) NOT NULL,
    IdCliente INT NOT NULL,
    Fecha DATETIME2 NOT NULL DEFAULT GETDATE(),
    Total DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    Estado NVARCHAR(50) NOT NULL DEFAULT 'PENDIENTE', -- 'PENDIENTE', 'PAGADO', 'CANCELADO', 'PROCESADO'
    CONSTRAINT PK_Pedido PRIMARY KEY (IdPedido),
    CONSTRAINT FK_Pedido_Cliente FOREIGN KEY (IdCliente) REFERENCES dbo.Cliente(IdCliente)
);
GO

IF OBJECT_ID('dbo.DetallePedido', 'U') IS NOT NULL DROP TABLE dbo.DetallePedido;
CREATE TABLE dbo.DetallePedido (
    IdDetalle INT IDENTITY(1,1) NOT NULL,
    IdPedido INT NOT NULL,
    IdProducto INT NOT NULL,
    Cantidad INT NOT NULL,
    PrecioUnitario DECIMAL(18,2) NOT NULL,
    CONSTRAINT PK_DetallePedido PRIMARY KEY (IdDetalle),
    CONSTRAINT FK_DetallePedido_Pedido FOREIGN KEY (IdPedido) REFERENCES dbo.Pedido(IdPedido) ON DELETE CASCADE
);
GO

INSERT INTO dbo.Cliente (Nombres, Cedula, Correo) VALUES 
('Juan Pérez', '1712345678', 'juan.perez@example.com'),
('María López', '0912345678', 'maria.lopez@example.com');
GO

