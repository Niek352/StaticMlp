# Phase 2 Plan 3: Inventory To Stockpile Transfer Summary

**Carried raw inventory can now be deposited into completed stockpiles through a server-authoritative typed request path**

## Accomplishments
- Added a stockpile deposit request/result contract, custom network codec, and request registration in the Settlement shared resources feature.
- Implemented server-side transfer from `ResourcesInventoryMinimal` carried inventory into Settlement shared storage through owner-provided domain accessors.
- Routed the stockpile `StoreItems` interaction and stockpile panel action to the new request path.
- Added editor test coverage for accepted deposits with capacity clamping and rejected non-stockpile targets.

## Files Created/Modified
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Contracts/Events/DepositCarriedResourcesToStockpileRequestEvent.cs` - Client-to-server stockpile deposit request contract.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Contracts/Events/DepositCarriedResourcesToStockpileResultEvent.cs` - Server result contract with transferred amount.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Contracts/Events/DepositCarriedResourcesToStockpileEventCodec.cs` - Hand-written registration codec for the typed request/result events.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Logic/Requests/DepositCarriedResourcesToStockpileHandler.cs` - Server trust-boundary validation and inventory-to-storage transfer.
- `Assets/Scripts/StaticMlp/Features/ResourcesInventoryMinimal/Runtime/Logic/Domain/ResourcesInventoryAccess.cs` - Added read-only carried amount copy API for owner-provided inventory access.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Logic/SettlementSharedResourcesGameplayFeature.cs` - Registered the deposit event codec and request handler.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Logic/StaticMlp.Features.Settlement.Logic.asmdef` - Added the ResourcesInventoryMinimal dependency for the owner accessor.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Presentation/Actions/DepositCarriedResourcesToStockpileInteractionHandler.cs` - Sends the stockpile store interaction request.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Presentation/Actions/BuildingInteractionHandlerRegistry.cs` - Registered the StoreItems handler.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Presentation/Actions/OpenOperationIntentInteractionHandler.cs` - Removed StoreItems from generic open-intent handling.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Presentation/BuildingPanelPresentation.cs` - Added stockpile panel Store Items action state.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Presentation/BuildingManagementPanelController.cs` - Sends deposit requests from the stockpile panel.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Presentation/BuildingManagementPanelView.cs` - Displays stockpile action status.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Presentation/Definitions/BuildingActionPresentationCatalog.cs` - Updated StoreItems effect description.
- `Assets/Scripts/StaticMlp/Features/Settlement/Runtime/Presentation/Definitions/BuildingPanelActionKind.cs` - Added stockpile deposit action kind.
- `Assets/Scripts/StaticMlp/Features/BuildingCatalog/Runtime/Catalogs/BuildingCatalogData.cs` - Renamed the stockpile interaction label to Store Items.
- `Assets/Tests/Editor/Settlement/StockpileDepositRequestTests.cs` - Added handler behavior tests.
- `Assets/Tests/Editor/Settlement/SettlementOperationTestWorldScope.cs` - Added test helpers for players and stockpile targets.
- `Assets/Tests/Editor/Settlement/StaticMlp.Tests.Settlement.asmdef` - Added the ResourcesInventoryMinimal test dependency.
- `docs/Roadmap по GDD.md` - Updated Phase 2 status for stockpile deposit flow.

## Decisions Made
- The request deposits all carried raw resources rather than requiring the Settlement UI to inspect another feature's inventory internals.
- The server computes accepted amounts from current carried inventory and remaining stockpile capacity, then spends and adds the same accepted amount through the owning domain accessors.
- Used a hand-written request/result event codec so no `.Generated.cs` file had to be manually edited.

## Deviations from Plan
None - plan executed as written.

## Issues Encountered
None

## Next Step
Ready for `02-04-PLAN.md`.
