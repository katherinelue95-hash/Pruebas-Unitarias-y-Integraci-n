-- ============================================================
-- StyleBookBarberBD · Tabla Resenas (reseñas de clientes)
-- Ejecutar una sola vez en la base StyleBookBarberBD
-- SQL Server: Server=KATHYARANA\SQLEXPRESSKATHY;Database=StyleBookBarberBD
-- ============================================================

USE StyleBookBarberBD;
GO

IF OBJECT_ID('dbo.Resenas', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Resenas
    (
        ResenaId   INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Resenas PRIMARY KEY,
        BarberosId INT NOT NULL,
        UsuariosId INT NOT NULL,
        Estrellas  INT NOT NULL CONSTRAINT CK_Resenas_Estrellas CHECK (Estrellas BETWEEN 1 AND 5),
        Comentario NVARCHAR(500) NOT NULL,
        Fecha      DATETIME2(7) NOT NULL CONSTRAINT DF_Resenas_Fecha DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_Resenas_Barberos FOREIGN KEY (BarberosId) REFERENCES dbo.Barberos (BarberosId) ON DELETE NO ACTION,
        CONSTRAINT FK_Resenas_Usuarios FOREIGN KEY (UsuariosId) REFERENCES dbo.Usuarios (UsuariosId) ON DELETE NO ACTION
    );

    CREATE NONCLUSTERED INDEX IX_Resenas_Barberos ON dbo.Resenas (BarberosId);
    CREATE NONCLUSTERED INDEX IX_Resenas_Usuarios ON dbo.Resenas (UsuariosId);
END
GO

-- Verificación: 
-- SELECT * FROM dbo.Resenas;