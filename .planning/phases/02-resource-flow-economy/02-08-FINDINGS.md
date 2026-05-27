# Phase 2 Research 8: Barter Boundary Findings

**Defer barter runtime and offer contracts until merchant/faction and item ownership exist; Phase 2 should only preserve Settlement resource contracts as the future resource leg.**

## Recommendation

Defer full barter runtime until Phase 3/4 systems define merchant identity, faction availability, and item ownership. Phase 2 should not add a static `BarterOfferDefinition` yet because the expected barter shape crosses systems that are not stable in the current source tree:

- `Settlement` owns `ResourceId`, `ResourceAmount`, resource catalog validation, and server-authoritative settlement storage.
- `Npc` owns product NPC identity, acquisition paths, roster records, and future NPC-owned validation, but it does not yet model merchants or factions.
- `Progression` owns flags, reward packages, and boss preparation tokens, not merchant inventory or trade lifecycle.
- `Loadout` currently owns build modules and preparation state, not the Phase 4 item drops, item tiers, item pools, or tradable inventory implied by barter.

Creating Phase 2 barter definitions now would either encode placeholder merchant/faction/item ids or collapse barter into a resource-only settlement exchange board. Both would make the future architecture more fragile than deferring.

## Ownership Boundary

Recommended future owner: a dedicated commerce/barter feature once Phase 3/4 contracts exist.

The dedicated feature should own:

- `BarterOfferId`.
- Offer definitions and availability rules.
- Merchant or faction offer lifecycle state.
- Client-to-server `AcceptBarterOfferRequestEvent` and result events.

The dedicated feature should read, not own:

- `Settlement.Contracts` for `ResourceId`, `ResourceAmount`, `ResourceDefinition`, `ResourceFamily`, and `ResourceUsageFlags`.
- `Npc.Contracts` for merchant NPC identity or roster facts once merchant roles are explicit.
- Future faction contracts for faction reputation/availability.
- Future item/build-system contracts for item ids, item stacks, tiers, and pool membership.
- `Progression.Contracts` only for stable progress flags that gate offer visibility.

Write path:

- Settlement storage mutation must remain owner-applied by `Settlement` through typed request/event handlers.
- Item inventory/loadout mutation must remain owner-applied by the future item/build feature.
- Barter acceptance must be a server-authoritative request/result flow, using reliable delivery because accepted exchanges are durable economy changes.
- If an offer touches multiple owners, do not directly mutate both owners from a barter system. Introduce an owner-approved transaction boundary only after the item inventory owner exists; otherwise the exchange can partially apply across features.

## Safe Phase 2 Scope

Safe now:

- Document that barter is deferred.
- Reuse `Settlement.Contracts.ResourceId` and `ResourceAmount` for future resource costs/rewards.
- Validate that any future barter resource leg uses `ResourceCatalog.Get` and existing usage flags instead of creating a parallel resource catalog.
- Keep current settlement production, storage, construction, and unlock gate systems as the Phase 2 economy surface.

Unsafe now:

- Adding `BarterOfferDefinition` with merchant, faction, item, or item-pool fields before those ids/contracts exist.
- Adding a settlement-owned resource-for-resource board and calling it merchant barter.
- Adding placeholder currency, because the roadmap calls for item-for-resource exchange rather than currency.
- Mutating settlement storage, NPC roster, progression, or loadout state directly from a foreign barter system.

## Deferred Scope

Deferred to Phase 3:

- Merchant identity if merchants are NPCs.
- Merchant availability facts such as rescued/recruited specialist, faction contact, or NPC role.
- NPC-owned read models/events that expose merchant facts without making Settlement or Commerce depend on NPC logic internals.

Deferred to Phase 4:

- Tradable item ids, item stacks, tiers, pool membership, drop ownership, and item inventory authority.
- Item-for-resource barter validation.
- Unlocking item pools through buildings, NPCs, or progression facts.

Deferred until both sides exist:

- Atomic exchange handling that spends one owner-owned asset and grants another.
- Offer lifecycle persistence, refresh rules, stock limits, and multiplayer contention handling.

## Follow-up Plan

Do not create a Phase 2 implementation plan for barter. Create a later research/plan pair after Phase 3 NPC/faction contracts and Phase 4 item contracts exist, for example:

- `03-merchant-identity-boundary-RESEARCH.md`
- `04-barter-runtime-PLAN.md`

Confidence: high. The recommendation follows current project ownership rules and the source tree has no stable merchant, faction, or item-pool contracts to bind a Phase 2 barter implementation to.
