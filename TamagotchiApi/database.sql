-- Script para crear la base tamagotchi.db a mano con sqlite3
-- (NO hace falta si se ejecuta la API: Program.cs la crea sola con
--  EnsureCreated la primera vez). Mismo esquema que AppDbContext.
--
-- Uso:  sqlite3 tamagotchi.db < database.sql

PRAGMA foreign_keys = ON;

CREATE TABLE usuario (
    id              INTEGER NOT NULL CONSTRAINT PK_usuario PRIMARY KEY AUTOINCREMENT,
    nombre          TEXT    NOT NULL COLLATE NOCASE,
    fecha_creacion  TEXT    NOT NULL
);
CREATE UNIQUE INDEX IX_usuario_nombre ON usuario (nombre);

CREATE TABLE ropa (
    id      INTEGER NOT NULL CONSTRAINT PK_ropa PRIMARY KEY AUTOINCREMENT,
    nombre  TEXT    NOT NULL,
    clave   TEXT    NOT NULL,
    etapa   TEXT    NOT NULL,
    orden   INTEGER NOT NULL
);
CREATE UNIQUE INDEX IX_ropa_clave ON ropa (clave);

CREATE TABLE pinguino (
    id               INTEGER NOT NULL CONSTRAINT PK_pinguino PRIMARY KEY AUTOINCREMENT,
    nombre           TEXT    NOT NULL,
    usuario_id       INTEGER NOT NULL,
    etapa            TEXT    NOT NULL,
    ticks_etapa      INTEGER NOT NULL,
    es_de_dia        INTEGER NOT NULL,
    ropa_puesta_id   INTEGER NULL,
    resultado        TEXT    NULL,
    fecha_creacion   TEXT    NOT NULL,
    ultimo_guardado  TEXT    NOT NULL,
    CONSTRAINT FK_pinguino_usuario_usuario_id FOREIGN KEY (usuario_id) REFERENCES usuario (id) ON DELETE CASCADE,
    CONSTRAINT FK_pinguino_ropa_ropa_puesta_id FOREIGN KEY (ropa_puesta_id) REFERENCES ropa (id) ON DELETE SET NULL
);
CREATE INDEX IX_pinguino_usuario_id ON pinguino (usuario_id);
CREATE INDEX IX_pinguino_ropa_puesta_id ON pinguino (ropa_puesta_id);

CREATE TABLE estado_pinguino (
    id           INTEGER NOT NULL CONSTRAINT PK_estado_pinguino PRIMARY KEY AUTOINCREMENT,
    pinguino_id  INTEGER NOT NULL,
    hambre       REAL    NOT NULL,
    energia      REAL    NOT NULL,
    higiene      REAL    NOT NULL,
    diversion    REAL    NOT NULL,
    vejiga       REAL    NOT NULL,
    enfermo      INTEGER NOT NULL,
    CONSTRAINT FK_estado_pinguino_pinguino_pinguino_id FOREIGN KEY (pinguino_id) REFERENCES pinguino (id) ON DELETE CASCADE
);
CREATE UNIQUE INDEX IX_estado_pinguino_pinguino_id ON estado_pinguino (pinguino_id);

CREATE TABLE accion_registrada (
    id           INTEGER NOT NULL CONSTRAINT PK_accion_registrada PRIMARY KEY AUTOINCREMENT,
    pinguino_id  INTEGER NOT NULL,
    accion       TEXT    NOT NULL,
    fecha        TEXT    NOT NULL,
    CONSTRAINT FK_accion_registrada_pinguino_pinguino_id FOREIGN KEY (pinguino_id) REFERENCES pinguino (id) ON DELETE CASCADE
);
CREATE INDEX IX_accion_registrada_pinguino_id ON accion_registrada (pinguino_id);

-- Ropa inicial (la Clave es el prefijo de los sprites en Assets/Ropa/{Etapa}/)
INSERT INTO ropa (id, nombre, clave, etapa, orden) VALUES
    (1, 'Traje de oso',    'Oso',     'Bebe',        1),
    (2, 'Traje de araña',  'Arana',   'Nino',        2),
    (3, 'Remera de Mario', 'Remera1', 'Adolescente', 3);
