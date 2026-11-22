# CLAUDE.md — Reglas de trabajo de Claude en {{PROYECTO}}

> **Forma de trabajar (multi-agente, init, progress, memoria):** ver `AGENTS.md`. Al empezar, ejecutar `bash init.sh` y leer
> `progress.md`. Las decisiones y preferencias del usuario están en `doc/agentes/memoria/MEMORY.md`.
>
> **No tocar `{{NO_TOCAR}}`**: son reglas de otra herramienta. Si el usuario cambia una, se sincroniza aquí.

Si una regla de aquí choca con otro documento, **manda este archivo** y se pregunta al usuario.

---

## 0. Reglas críticas (no negociables)

### 0.1 Base de datos: SOLO LECTURA
- Contra la base de datos **solo se ejecutan `SELECT`** de consulta.
- **Prohibido:**
  - `INSERT`, `UPDATE`, `DELETE`, `MERGE` y `TRUNCATE`;
  - `CREATE`, `ALTER` y `DROP`;
  - `GRANT` y `REVOKE`;
  - `EXEC` de procedimientos o funciones que escriban;
  - `SELECT ... INTO`;
  - transacciones;
  - cualquier cosa que cambie datos, esquema, permisos o configuración.
- Si no se sabe con certeza que un procedimiento sea de solo lectura, **no se ejecuta**: se trata como escritura.
- Las credenciales **no se escriben** en ningún archivo del proyecto ni en la bitácora.

### 0.2 Cambios de base de datos → carpeta `{{SCRIPTS_DIR}}/`
- Todo lo que requiera modificar la BD se **escribe como script en `{{SCRIPTS_DIR}}/`** y **lo ejecuta el usuario**.
- **Nombre:** `{{SCRIPT_PATRON}}`, con la numeración existente (siguiente = mayor + 1; `init.sh` lo indica).
- **Nunca editar un script ya ejecutado**: crear uno nuevo numerado.
- **Formato de cada script:**
  - cabecera en comentario: tema, número, qué hace y cuándo ejecutarlo;
  - **idempotente**: comprobar existencia antes de crear y `CREATE OR ALTER` para procedimientos y vistas;
  - orden de ejecución y dependencias (p. ej. "ejecutar antes de publicar la API").
- Un script **no se da por ejecutado** hasta que el usuario lo confirme.

### 0.3 Control de versiones
- {{REPOS_TEXTO}}. **Commit y push solo si el usuario lo pide.** Sin ramas automáticas.
- Si la raíz no es repo, antes de modificar o borrar un archivo de la raíz se lee y se explica el cambio.
  Nada de borrados ni reescrituras masivas sin confirmación.

---

## 1. Contexto del proyecto

| Capa | Stack | Carpeta |
|------|-------|---------|
{{STACK_TABLA}}

- Ignorar: `bin/`, `obj/`, `node_modules/`, `.nuxt/`, `.output/`, `dist/` y copias de trabajo de otras herramientas.

---

## 2. Convenciones de código
{{REGLAS_PROPIAS}}

---

## 3. Documentación viva del avance — `Resumen.md` (OBLIGATORIO)

Tras **cada tarea o conjunto de cambios** que avance el producto, se actualiza `Resumen.md`. **No hace falta que el usuario lo pida.**

### Qué actualizar
- Fecha de "Última actualización".
- Sección del módulo tocado.
- Scripts nuevos: qué hacen y si el usuario ya los ejecutó.
- Decisiones de diseño y reglas de negocio nuevas o cambiadas. Las anteriores no se borran: se marcan como sustituidas y se enlaza la nueva.
- API y pantallas nuevas o cableadas.
- Pendientes: quitar lo hecho y añadir lo que quede explícito.
- Entrada en **Bitácora**: fecha `AAAA-MM-DD`, qué se hizo o decidió, archivos tocados y pendientes.

### Qué NO hace falta documentar
Typos, formato, renombres triviales sin impacto funcional, experimentos revertidos y preguntas del usuario que no cambian nada.

### Cómo escribir
- En {{IDIOMA}}, claro y concreto (bullets). Resumir y enlazar; no copiar manuales.
- **Parchear** las secciones afectadas; no reescribir el archivo entero.
- No dar por hecho algo que solo está descrito: un script no ejecutado sigue pendiente.

### Orden al terminar una tarea
1. Completar el cambio en código o SQL.
2. Actualizar `Resumen.md`, los otros documentos afectados y `progress.md`.
3. Responder al usuario indicando qué documentos se actualizaron.

---

## 4. Forma de trabajar
- Idioma: **{{IDIOMA}}**.
- No inventar tablas, columnas ni endpoints: verificar en el código o con `SELECT` antes de afirmar.
- Antes de cambios no triviales, explicar el plan. Lo que afecte a la BD se entrega como script.
- Al terminar, decir qué se verificó y qué no (compilación, consulta, prueba).
