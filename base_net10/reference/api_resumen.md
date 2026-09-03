# api_resumen — __RootNamespace__

Contrato de la API. Actualizar esta sección cada vez que se añada o modifique un Controller.

Convenciones:
- Prefijo `/api`, rutas en minúsculas.
- Lecturas → `Response<T>` (`idResponse`, `type`, `message`, `totalRows`, `data`).
- Escrituras → `Transaction` (`type`, `message`, `key`, `output`).
- `type = "ERR"` se devuelve con **HTTP 200** — el cliente debe inspeccionar `type`, no solo el status code. Solo `NOT_FOUND` produce 404 real.
- CRUD: `Id{Entidad} = 0` crea, `Id{Entidad} > 0` actualiza (mismo endpoint POST).

## Índice

- [Identity](#identity)
- [Producto](#producto)

## Identity

Login y renovación de token JWT. Sin `[Authorize]` (son los únicos endpoints que deben quedar siempre abiertos).

| Método | Ruta | Descripción |
|---|---|---|
| POST | `/api/identity/login` | Login — recibe `{ userName, password }`, devuelve `accessToken`/`refresh_token`. |
| PUT | `/api/identity/refresh` | Renueva el par de tokens — recibe `{ token, refreshToken }`. |

Ejemplo `POST /api/identity/login`:
```json
{ "userName": "admin", "password": "Admin123!" }
```

## Producto

Módulo de ejemplo (CRUD completo), plantilla de referencia para módulos nuevos.

| Método | Ruta | Descripción |
|---|---|---|
| GET | `/api/productos/{id}` | Obtiene un producto por Id. |
| GET | `/api/productos?idEmpresa=&estado=ACT` | Lista productos filtrados. |
| POST | `/api/productos` | Crea o actualiza (`IdProducto = 0` crea, `> 0` actualiza). |
| PUT | `/api/productos/{id}/anular?idUsuario=` | Anula (soft-delete, `IdEstado = 'ANU'`). |

Ejemplo `POST /api/productos` (crear):
```json
{ "idProducto": 0, "idEmpresa": 1, "codigo": "P-001", "nombre": "Producto de ejemplo", "descripcion": "Opcional", "precio": 19.90, "idUsuario": 1 }
```
