# agente_revisor

Agente de **solo lectura**: no tiene `Edit` ni `Write`. Revisa el diff contra el encargo y `CLAUDE.md` y busca bugs, seguridad y reglas rotas.
Devuelve los hallazgos ordenados por gravedad, con `archivo:línea`, un escenario que falla y la corrección. Modelo: Opus.

Guía del componente: [docs/02-agentes.md](../docs/02-agentes.md).

## Instalar
**Plugin:**
```bash
claude plugin marketplace add hmedinas/IA-Skills-claude   # una vez
claude plugin install agente_revisor@ia-skills-claude
```
**A mano:** copiar `agents/revisor.md` a `~/.claude/agents/` o a `<proyecto>/.claude/agents/`.

Comprobar con `/agents`.

## Usar
> Usa el agente revisor sobre el diff actual. El encargo era: …

Úsalo en cambios grandes o de riesgo: permisos, SQL de negocio, scripts de BD, seguridad. Para cambios pequeños no compensa.
