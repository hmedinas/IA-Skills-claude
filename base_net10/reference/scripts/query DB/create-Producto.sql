USE [__Database__];
GO

IF OBJECT_ID(N'dbo.Producto', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Producto
    (
        IdProducto          INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Producto PRIMARY KEY,
        IdEmpresa            INT NOT NULL,
        Codigo               VARCHAR(30) NOT NULL,
        Nombre               VARCHAR(150) NOT NULL,
        Descripcion          VARCHAR(500) NULL,
        Precio               DECIMAL(18,2) NOT NULL,
        IdEstado             VARCHAR(5) NOT NULL CONSTRAINT DF_Producto_IdEstado DEFAULT ('ACT'),
        UsuarioCreacion      INT NOT NULL,
        FechaCreacion        DATETIME NOT NULL CONSTRAINT DF_Producto_FechaCreacion DEFAULT (GETDATE()),
        UsuarioModificacion  INT NULL,
        FechaModificacion    DATETIME NULL
    );

    CREATE INDEX IX_Producto_IdEmpresa_Estado ON dbo.Producto (IdEmpresa, IdEstado);
    PRINT 'OK: tabla Producto creada';
END
ELSE
    PRINT 'INFO: Producto ya existe';
GO
