---
name: arnes-trabajo
description: Arnés de trabajo instalado con la skill arnes_trabajo; reglas clave del proyecto
metadata:
  type: feedback
---

Arnés instalado el {{FECHA}} con la skill `arnes_trabajo`: `AGENTS.md`, `CLAUDE.md`, `progress.md`, `Resumen.md`, `init.sh` (hook SessionStart), agentes implementador/revisor. Modo del arnés: {{MODO}}. Prefijos de ficha: {{PREFIJOS_FICHA}}.
Reglas acordadas con el usuario: {{BD_TEXTO}}; commit/push solo si lo pide; {{ENTORNO_TEXTO}}; planes grandes solo se implementan en estado Aprobado.

**Why:** el usuario quiere la misma forma de trabajar en todos sus proyectos, estable entre sesiones y herramientas.
**How to apply:** seguir `AGENTS.md`; al cerrar cada tarea actualizar `progress.md` y `Resumen.md` sin que lo pida.
