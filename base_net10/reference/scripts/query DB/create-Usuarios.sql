USE [__Database__];
GO

IF OBJECT_ID(N'dbo.Usuarios', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Usuarios
    (
        IdUsuario           INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Usuarios PRIMARY KEY,
        Username            VARCHAR(50) NOT NULL,
        Nombre              VARCHAR(50) NOT NULL DEFAULT (''),
        Apellidos           VARCHAR(50) NOT NULL DEFAULT (''),
        Email               VARCHAR(150) NOT NULL,
        PasswordHash        VARCHAR(200) NOT NULL,
        ResetPassword       BIT NOT NULL CONSTRAINT DF_Usuarios_ResetPassword DEFAULT (0),
        Intentos            INT NOT NULL CONSTRAINT DF_Usuarios_Intentos DEFAULT (0),
        IdEstado            VARCHAR(5) NOT NULL CONSTRAINT DF_Usuarios_IdEstado DEFAULT ('ACT'),
        UsuarioCreacion     INT NOT NULL,
        FechaCreacion       DATETIME NOT NULL CONSTRAINT DF_Usuarios_FechaCreacion DEFAULT (GETDATE()),
        UsuarioModificacion INT NULL,
        FechaModificacion   DATETIME NULL,
        CONSTRAINT UQ_Usuarios_Username UNIQUE (Username)
    );

    CREATE INDEX IX_Usuarios_IdEstado ON dbo.Usuarios (IdEstado);
    PRINT 'OK: tabla Usuarios creada';
END
ELSE
    PRINT 'INFO: Usuarios ya existe';
GO

-- Usuario de arranque (username: admin / password: Admin123!) para poder loguear tras crear el esquema.
-- El hash de abajo corresponde a PasswordHasher.HashPassword("Admin123!") — cámbialo en cuanto puedas loguear.
IF NOT EXISTS (SELECT 1 FROM dbo.Usuarios WHERE Username = 'admin')
BEGIN
    INSERT INTO dbo.Usuarios (Username, Nombre, Apellidos, Email, PasswordHash, UsuarioCreacion)
    VALUES ('admin', 'Administrador', '', 'admin@__Database__.local', '6v+EcUv1oQ8h/Fwp37z+hA==:hxVh9ae7ZcZzHmRardYA8iBvj3TH6/xt7DWlG3xBQuY=', 1);
    PRINT 'OK: usuario admin sembrado';
END
GO
