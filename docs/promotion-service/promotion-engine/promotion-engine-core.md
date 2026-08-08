# Promotion Engine — Core (Phase 5.1)

**Scope:** Business capabilities only — architecture patterns (CQRS/Persistence/Repository/DI/Mapster/exceptions/tenant) are already documented elsewhere and not repeated here.

```text
Promotion
    ↓
Lifecycle (Draft → Active → Paused/Expired/Cancelled, gated by Approval)
    ↓
Target (Product / Variant(Sku) / Category / Cart / Order / Customer)
    ↓
Condition (Constraint: Minimum/Maximum Order Amount, Minimum/Maximum Quantity, Category, Maximum Discount Amount)
    ↓
Discount (Benefit: PercentageOff / FixedAmountOff / FreeShipping / FreeGift)
    ↓
Priority / Stacking (NotStackable / StackWithAny / StackWithSameType / StackWithSpecific, PromotionExclusion)
    ↓
Evaluation (POST /promotions/evaluate)
```

## Lifecycle and approval

`PromotionStatus` is `Draft → Active → Paused/Expired/Cancelled` — no separate `PendingApproval`/`Approved`/`Rejected`/`Scheduled` states exist on the Promotion itself. Approval is modeled by linking a Promotion to the platform's general-purpose `ApprovalWorkflow` aggregate (`Promotion.ApprovalWorkflowId`, `Promotion.IsApproved`):

- **Submit** creates and starts a new `ApprovalWorkflow`, links it to the Promotion (still Draft).
- **Approve** approves the linked workflow and, in the same operation, activates the Promotion (`Status → Active`) — there is no separate "Activate" endpoint, since `Activate()` itself requires `IsApproved` and nothing else can satisfy that gate.
- **Reject** rejects the linked workflow, clears the link, and leaves the Promotion in Draft so it can be resubmitted with a fresh workflow.

`Enable`/`Disable` (`IsEnabled`) is independent of `Status` — a disabled Promotion is excluded from evaluation regardless of lifecycle state. "Delete" maps to `Promotion.Cancel()` — no physical delete.

## Targeting

A Promotion applies to the whole order when it has no item-level targets (or only `Cart`/`Order`/`Customer` targets). `Product`/`Sku`(Variant)/`Category` targets narrow it to matching order items. A `Customer` target is an order-level gate against the evaluating user.

## Conditions and discount cap

Only the constraint types the Domain represents are enforced, and only where the evaluation request carries the data for them: `MinimumOrderAmount`, `MaximumOrderAmount`, `MinimumQuantity`, `MaximumQuantity`, `ProductCategory`, `MaximumDiscountAmount` (caps the computed discount, not an eligibility gate). `CustomerSegment`/`PaymentMethod` constraints are configurable but **not evaluated this phase** — the evaluation request (Section 14 of the issuing prompt) deliberately carries neither field. This is a documented gap, not an oversight.

`PercentageOff`/`FixedAmountOff` are the two implemented discount types; `FreeShipping`/`FreeGift` report a zero monetary `DiscountAmount` (no shipping-cost or gift-value data is modeled in this evaluation context) for the caller to interpret. All monetary math uses `decimal`, never `float`/`double`.

## Priority, stacking, evaluation

`Promotion.Priority` (higher wins) orders candidates. A deterministic greedy pass then walks that order: a `NotStackable` Promotion (or one arriving after a `NotStackable` Promotion is already applied) stops there; `StackWithSameType` additionally requires every already-applied Promotion to share its `PromotionType`; an explicit `PromotionExclusion` pairing always blocks, regardless of `StackingMode`. This is a baseline pass, not the full stacking optimization algorithm — that is explicit future-phase scope.

`POST /promotions/evaluate` answers "which Promotions apply, and what do they discount?" only — it never returns a final Order total (OrderService's responsibility) and never queries OrderService/ProductService/InventoryService; the caller supplies the order snapshot it needs. `PromotionRule`/`PromotionRuleGroup`/`PromotionCondition` (the generic Field/Operator/Value mechanism) remain configurable via the Domain but are **not interpreted by the evaluation engine this phase** — evaluating them generically would be the "generic expression engine" the issuing prompt explicitly forbade; only the fixed, small `PromotionConstraint`/`PromotionTarget` enums are evaluated.

## Not implemented this phase

Coupon-triggered and Campaign-triggered evaluation (only `PromotionExecutionMode.Automatic` Promotions are evaluated); Product Gift detail configuration (no structural Gift entity exists under the Promotion aggregate to configure); child-collection updates after creation (Benefits/Targets/Constraints/StackingMode are set once, at `CreatePromotion`); Promotion Rule/RuleGroup/Condition CRUD endpoints; the full stacking optimization algorithm. All explicitly out of scope per the issuing prompt.
