# {{FICHA_ID}} — {{TITULO}}
> **Estado:** Borrador
> Estados posibles: Borrador → **Aprobada** (solo el usuario) → Obsoleta.
> Aprobación: (vacío hasta que el usuario apruebe. Si lo hace en el chat: `Aprobada por el usuario en chat AAAA-MM-DD: "<sus palabras>"`)
> Tipo: {{FICHA_TIPO}} · Creada: {{FECHA}} · Última actualización: {{FECHA}}
> Plan: {{PLAN}} · Referencia externa: {{REFERENCIA}}

<!--
Ficha de requerimiento: lo que queda de la tarea cuando se cierra, para quien llegue después (otra persona u otro agente).
Antes de guardarla, pasar el filtro:
  - sin credenciales, tokens, cadenas de conexión ni claves;
  - sin datos de producción (filas reales, nombres o correos de personas, importes de clientes…);
  - sin nombres de servidor, IPs internas ni rutas de máquinas concretas;
  - sin conversación copiada tal cual: se resume.
Lo que no aplique se borra; no se dejan secciones vacías ni marcadores.
-->

## 1. Objetivo y contexto
- **Objetivo:** (una frase: qué problema resuelve y para quién)
- **Contexto:** (por qué hacía falta, de dónde viene la petición, qué había antes)

## 2. Decisiones
| Decisión | Alternativas descartadas y por qué | Fecha |
|---|---|---|
| … | … | … |

**ADR propuestos:** (decisiones que perduran más allá de esta tarea; enlace al ADR si el usuario lo aceptó, o "ninguno")

## 3. Qué se cambió
| Repo | Rama | Commits | Archivos o áreas |
|---|---|---|---|
| … | … | `abc1234` mensaje corto (o "sin commit todavía") | … |

Scripts de base de datos: (nombre, qué hacen y si el usuario ya los ejecutó; o "ninguno")

## 4. Qué se ejecutó y dónde
| Qué | Entorno | Resultado |
|---|---|---|
| Build del back / front, script, migración… | local / desarrollo / pruebas (nunca el nombre del servidor) | ✔ / ✘ y error exacto |

## 5. Pruebas
| Prueba | Cómo se hizo | Resultado |
|---|---|---|
| … | automática / manual del usuario / no hecha | ✔ / ✘ / pendiente |

**No verificado:** (lo que no se probó, dicho claramente)

## 6. Hallazgos
- (lo descubierto por el camino que conviene saber: deuda, riesgos, comportamientos raros)

## 7. Pendientes
- [ ] (qué queda y quién lo tiene: usuario / agente / otro equipo)

## 8. Historial de la ficha
| Fecha | Qué |
|---|---|
| {{FECHA}} | Ficha creada en Borrador |
