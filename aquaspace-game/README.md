# Overview

This is a aquascape game for test, that implement 
- file watcher system, 
- realtime config, 
- and hot-reload assets updater

## Game
- config.json handle gameplay setting
- input are left-mouse click

## Core Architecture
- Use modular, decoupled systems
- Prefer event-driven over Update polling
- Separate systems:
  - FileWatcherSystem
  - SpawnSystem
  - BehaviorSystem (FishController / TrashController)
  - InputSystem 
  - ConfigSystem

## File-Based Spawning
- Only accept .png files
- Parse filename:
  PREFIX_TYPE_TIMESTAMP
- Prefix:
  - FISH → Fish entity
  - TRASH → Trash entity
- File loading must be async
- Do NOT block main thread
- folder location (C:\Users\%userprofile%\AppData\LocalLow\DefaultCompany\aquaspace-game) 

## Spawning Rules
- Spawn inside defined bounds
- Prevent overlap (use spatial checks or simple radius)

## Fish Behavior
- by default fish facing left, can flip sprite to face right
- Avoid Overlap with other fish and trash
- Movement:
  - Random roaming within bounds
  - Speed range (min/max)

- Hunger system:
  - Hunger decreases over time
  - At 0 → seek food
  - At 100 → idle
  - Use cooldown after eating

- Food targeting:
  - Nearest food within detection radius

- can't move out of bounds, should turn around or choose new direction

## Trash Behavior
- Floating random movement
- Speed range (min/max)
- Avoid overlap with fish (simple separation)
- can't move out of bounds, should turn around or choose new direction

## Food Behavior
- spawn at empty location on left click
- can overlapp with fish and trash
- constant moving downwards slightly (simulate sinking)
- destroyed on contact with fish (eaten)

## Input System
- Left click:
  - Empty → spawn food
  - Trash → destroy
  - Fish → trigger flee behavior

## Config System
- Use external JSON file
- Configurable:
  - max spawn 
  - speed ranges
  - hunger values
  - detection radius
