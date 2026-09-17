# Contract: Context-Sensitive Action Button

The single right-side action button (FR-007, FR-009, FR-010, FR-011) exposes exactly one active
state at a time. Scope for this phase only covers the interactables that exist in the test room.

| Available interaction | Button shows | On press | Notes |
|---|---|---|---|
| None available | Hidden / no label | — | FR-011 |
| A `worldBattery` (still `.world`) in range | "Pick up" | `worldBattery → .spare` if `spareBattery == nil`; otherwise rejected with feedback and the battery stays in the world (FR-008) | object is highlighted while in range (FR-018) |
| Player has a `spareBattery` **and** installed charge ≤ `GameConfig.lightStateCriticalStart` (10%) | "Install battery" | `spareBattery → .installed`, discard old `installedBattery`, `charge = batteryDuration` | not position-gated; NOT offered above 10% charge (GDD 4.2 / 5.2, FR-009) |
| `door` (`isOpen == false`) in range | "Open door" | `door.isOpen = true` | one-way; object highlighted while in range (FR-018) |

## Priority rule when multiple interactions are available

1. Position-gated interactions (pick up battery, open door): the nearer object by distance wins.
2. Install battery is offered only when no position-gated interaction is in range.

This is a UI/UX tie-break, not a new functional requirement — recorded here so it isn't decided
ad hoc during implementation. Later specs (003 hiding, 005 keys & locked doors) extend this table;
they MUST keep rule 1 (nearest position-gated object wins).

## Revision note (GDD alignment pass 2026-09-17)

- Install row: previously "available regardless of current installed charge". Now gated to ≤10%
  per GDD 4.2 and 5.2.
- Pick-up row: the room now has two loose batteries so the slot-full rejection is reachable in
  normal play (GDD 20.3 DoD).
- Highlight: added per GDD 4.2 / FR-018.
