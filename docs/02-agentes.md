# 02 · Agentes (subagentes)

## Qué es
Un **agente** es otro Claude que el principal lanza para un encargo concreto. Tiene:
- su **propio contexto**: no ve la conversación, solo el encargo que le pasan;
- su **modelo** (p. ej. Sonnet para lo mecánico, Opus para revisar);
- sus **herramientas**: puedes quitarle `Edit` para que no pueda tocar código;
- sus **instrucciones** (el cuerpo del archivo).

Cuando termina, devuelve un informe al principal, que es quien decide qué hacer con él.

## Cuándo usarlo
- **Repartir:** el principal diseña; un implementador ejecuta un encargo cerrado.
- **Aislar un rol:** un revisor que no puede editar, solo leer y opinar.
- **Ahorrar contexto:** búsquedas largas cuyo detalle no necesitas en la conversación principal.
- **No** para cambios pequeños (1–3 archivos): arranca sin contexto y cuesta más que hacerlo directamente.

## Dónde vive
| Ámbito | Ruta |
|---|---|
| Personal | `~/.claude/agents/<nombre>.md` |
| Proyecto | `<proyecto>/.claude/agents/<nombre>.md` |
| Plugin | `<plugin>/agents/<nombre>.md` (aparece como `<plugin>:<nombre>`) |

## Cómo se crea
**Opción A, con asistente:** escribir `/agents` en Claude Code → crear → elegir ámbito y herramientas.

**Opción B, a mano:** crear el `.md`:

```markdown
---
name: documentador
description: Actualiza la documentación (resumen de API, manual de usuario) a partir del diff actual. Usar al terminar un cambio que añade o modifica endpoints o pantallas.
model: sonnet
tools: Read, Grep, Glob, Edit, Bash
---
Eres el DOCUMENTADOR. Responde en español.

1. Lee `AGENTS.md` y `CLAUDE.md`.
2. Obtén el diff con `git -C <repo> diff`.
3. Actualiza solo los documentos afectados (resumen de API, manual). No toques código.
4. Entrega la lista de documentos cambiados y qué añadiste en cada uno.
```

### Campos de la cabecera
| Campo | Para qué |
|---|---|
| `name` | Identificador del agente |
| `description` | Cuándo debe usarlo el principal. Igual que en las skills, es lo que decide si se usa |
| `model` | `sonnet`, `opus`, `haiku`… (si no se pone, usa el de la sesión) |
| `tools` | Lista de herramientas permitidas. **Si no se pone, tiene todas** |

## Cómo se usa
- Claude lo lanza solo cuando la `description` encaja.
- O se lo pides: "usa el agente revisor sobre este diff".
- Un buen encargo lleva: **archivos**, **patrón a seguir** y **criterio de aceptación**.

## Ejemplo real
Los dos agentes del arnés, también como componentes sueltos en este repositorio:
- `agente_implementador/agents/implementador.md`: Sonnet, todas las herramientas, ejecuta el encargo y deja el build en verde.
- `agente_revisor/agents/revisor.md`: Opus, solo `Read, Grep, Glob, Bash`, revisa el diff y no puede editar.

## Cómo se instala
- **A mano:** copiar el `.md` a `~/.claude/agents/` o a `<proyecto>/.claude/agents/`.
- **Como plugin:** `claude plugin install agente_revisor@ia-skills-claude`.
- Comprobar con `/agents`: debe aparecer en la lista.

## Errores típicos
| Síntoma | Causa |
|---|---|
| El agente "no sabe nada" del tema | Normal: no ve la conversación. El encargo tiene que llevar todo el contexto necesario |
| El revisor edita archivos | Le faltó `tools:`. Sin esa línea tiene todas las herramientas |
| Nunca se usa solo | `description` sin el *cuándo* |
| Sale caro | Lo estás usando para cambios pequeños. Hazlos en la sesión principal |
