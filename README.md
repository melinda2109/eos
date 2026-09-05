# Eos

A 2D turn-based battler prototype built in Unity, exploring a day/night duality theme (Eos, goddess of dawn, versus her night form).

## Status

Early prototype — core loop is being built out. Currently in place:

- `GameManager` / `Player` core scripts
- Battle scene with attack / damage / heal / idle sprite states for both the day and night forms
- Basic UI (health bar, buttons)

## Stack

- Unity (Universal Render Pipeline)
- C#
- New Input System

## Structure

- `Assets/Scripts/Core` — gameplay logic (game manager, player)
- `Assets/Scripts/UI`, `Assets/Scripts/Audio`, `Assets/Scripts/Effects`
- `Assets/Sprites` — character and UI art (dawn/night variants)
- `Assets/Scenes` — `BattleScene`
