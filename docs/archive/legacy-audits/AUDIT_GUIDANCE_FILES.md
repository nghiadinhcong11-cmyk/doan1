# AUDIT REPORT: AGENTS.md & AI_RULES.md vs. PROJECT_CONTEXT.md

**Audit Date**: 2026-09-13
**Comparison**: Guidance files vs. verified project reality
**Status**: ISSUES FOUND - 14 total issues identified
**Severity Breakdown**: 2 CRITICAL, 2 HIGH, 3 MEDIUM, 7 LOW

---

## EXECUTIVE SUMMARY

AGENTS.md and AI_RULES.md contain several discrepancies with actual implementation verified in PROJECT_CONTEXT.md and source code:

1. **CRITICAL**: Manager role documented as implemented but NOT in code
2. **CRITICAL**: AI branch isolation not enforced in AiPermissionService despite documentation
3. **HIGH**: Order merge logic and SentQuantity protection missing from AGENTS.md
4. **HIGH**: Payment state machine not documented for AI agents

These issues could lead to AI agents making unsafe decisions or attempting operations that don't exist.

---

## SECTION 1: ROLE DEFINITIONS - CRITICAL DISCREPANCY

### Current Documentation (AGENTS.md Section 13, AI_RULES.md Section 10)

Both state manager role is currently authorized and active:
- admin: Super Admin / Global scope
- manager: Quản lý chi nhánh (Branch Manager) 
- employee: Nhân viên
- cashier: Thu ngân
- kitchen: Nhân viên bếp
- customer: Khách hàng

AGENTS.md states: "Role manager is an ACTIVE role, primarily used for branch-level management"

### Verified Implementation (PROJECT_CONTEXT.md + Employee.cs)

Implemented roles only:
- admin ✓
- cashier ✓
- kitchen ✓
- employee ✓
- customer ✓

NOT implemented:
- manager ✗

**Evidence**:
- Employee.cs Role field contains no "manager" value option
- AuthController.cs never generates JWT with Role="manager"
- No controller authorization checks test for "manager" role
- KitchenHub.cs authorize attribute defensively includes "manager" but no user can be assigned it

**SEVERITY**: CRITICAL (Security/Authorization Model)

**IMPACT ON AI AGENTS**: 
- AI_RULES.md Rule 10 instructs agents to respect "manager" role authorization
- If agent checks "can manager execute tool X?", there is no actual manager to test against
- Creates false authorization model that doesn't match reality
- AI could believe manager has permissions that don't actually exist

**RECOMMENDATION**: 
`
Option A (Recommended): Update both documents to state manager is NOT implemented
- AGENTS.md Section 13: Remove "manager" from roles list, add note: "Manager role (defensive code only, not implemented)"
- AI_RULES.md Section 10: Same change

Option B: Implement manager role fully in code (requires separate task)
- Add manager to Employee.Role enum
- Add manager JWT claim generation in AuthController
- Add manager authorization checks in all relevant controllers
- Document manager permissions in both AGENTS.md and AI_RULES.md
`

**Recommended**: Choose Option A (update docs to match reality)

---

## SECTION 2: CUSTOMER ROLE DEFINITION - MODERATE DISCREPANCY

### Current Documentation

Both AGENTS.md and AI_RULES.md list "customer" in the role table alongside admin, manager, employee, cashier, kitchen

### Verified Implementation (Customer.cs + AuthController.cs)

Customer is NOT an Employee role:
- Customers are separate entity (Customer.cs, not Employee role field)
- Customer authentication via POST /api/auth/customer-token (phone-based)
- No password required for customer tokens
- JWT Role="customer" generated separately from Employee role

**SEVERITY**: MEDIUM (Documentation clarity)

**IMPACT ON AI AGENTS**:
- Misleading to list customer alongside employee roles
- AI agents might assume customer is stored in Employee.Role field
- Creates confusion about authorization model

**RECOMMENDATION**:
- AGENTS.md Section 13: Restructure to show:
  * "Employee Roles: admin, cashier, kitchen, employee"
  * "Separate Authentication: Customer (phone-based, separate Customer entity)"
- AI_RULES.md Section 10: Same restructuring
- Add note: "Customer is NOT an Employee role"

---

## SECTION 3: ORDER FINANCIAL CALCULATION - INCOMPLETE DOCUMENTATION

### Current Documentation (AGENTS.md Section 8)

Correctly states:
- Calculation order: Item subtotal → Service fee → VAT → Discount
- VAT calculated on (Subtotal + ServiceFee)
- Rounding: MidpointRounding.AwayFromZero
- All prices from database, not client

### Verified Implementation (OrderService.cs)

Implements correctly BUT documentation missing:

1. **Size & Topping Price Extraction** (OrderService line 116-140)
   - Parses Product.SizesJson (JSON array) for size premiums
   - Parses Product.ToppingsJson (JSON array) for topping prices
   - Not documented in AGENTS.md

2. **Order Merge Logic** (OrderService.CreateOrUpdateOrderAsync line 78-92)
   - If order exists for same table/branch with Status="Đang xử lý", updates it instead of creating new
   - Critical for POS workflow (don't create duplicate orders)
   - Not documented in AGENTS.md

3. **SentQuantity Protection** (OrderService line 106-112)
   - Cannot reduce SentQuantity (tracks items already sent to kitchen)
   - Only items with SentQuantity=0 can be removed
   - If SentQuantity > 0, quantity is reset to SentQuantity
   - Not documented in AGENTS.md

4. **Client Discount Bypass** (OrderService line 34)
   - Client-supplied discount in payload is ignored: order.Discount = 0;
   - Not explicitly stated in AGENTS.md

5. **No Promotion Flow** (OrderService comment line 32-33)
   - States "until server-side promotion flow is introduced"
   - Currently NO path for AI to apply discounts
   - Matters if AI task: "Apply 10% discount to order"

**SEVERITY**: HIGH (Missing business logic documentation)

**IMPACT ON AI AGENTS**:
- Cannot understand POS workflow (orders merge, don't create duplicates)
- Cannot understand kitchen constraints (SentQuantity immutable)
- Might attempt discount operations that don't exist
- Might not understand size/topping pricing

**RECOMMENDATION**:
- Add to AGENTS.md Section 8:
  * "Size and topping prices extracted from Product.SizesJson and Product.ToppingsJson (JSON arrays)"
  * "When CreateOrUpdateOrderAsync receives order for existing 'Đang xử lý' table/branch, it merges (does not create duplicate)"
  * "OrderDetail.SentQuantity tracks kitchen delivery. Cannot reduce SentQuantity."
  * "Items with SentQuantity > 0 quantity reset to SentQuantity (already sent)"
  * "Client-supplied Discount values ignored (set to 0 on creation)"
  * "Promotion/discount application flow NOT YET IMPLEMENTED"

---

## SECTION 4: AI PERMISSION SERVICE - BRANCH ISOLATION NOT ENFORCED

### Current Documentation (AI_RULES.md Sections 11-12)

States:
- "For branch-sensitive requests: Authenticated User → authorized branch → tool → branch-scoped service → database"
- "Before using branchId: validate format, verify branch exists, verify user can access"

### Verified Implementation (AiPermissionService.cs)

**Current Code**:
`csharp
public async Task<bool> CanExecuteAsync(IAiTool tool, AiUserContext context)
{
    if (!_authorization.IsRoleAllowed(tool, context)) return false;
    if (!_authorization.IsRiskLevelAllowed(tool, context)) return false;
    await Task.CompletedTask;
    return true;
}
`

**Checks performed**:
- ✓ Role is in tool.AllowedRoles
- ✓ Risk level allowed
- ✗ Branch isolation NOT checked

**Available but NOT called**: _authorization.IsInAuthorizedBranch(context, dataBranchId)

**Current Enforcement**: Relies on individual tools to filter by AiUserContext.BranchId

**SEVERITY**: CRITICAL (Security/Data isolation)

**IMPACT ON AI AGENTS**:
- Documentation promises branch validation, code doesn't enforce it
- Non-admin read tools could return cross-branch data if tool doesn't filter
- AI agents think they're protected by permission service when they're not
- Relies on tool implementation correctness (decentralized, error-prone)

**RECOMMENDATION**:
`
URGENT FIX in code:
- Modify AiPermissionService.CanExecuteAsync() to call:
  await _authorization.IsInAuthorizedBranch(context, requestedBranchId);
- Verify branch ID is passed through tool execution context

Update AI_RULES.md Section 11:
- Add: "CURRENT IMPLEMENTATION: Branch isolation enforced at tool level, not permission service"
- Add: "HIGH-PRIORITY FIX: Permission service should validate branch before tool execution"
- Add: "Workaround: Each tool MUST filter by AiUserContext.BranchId"
`

---

## SECTION 5: CRITICAL MISSING RULES FOR AI AGENTS

### Not Documented Anywhere

1. **Order Status State Machine** (None)
   - Valid statuses: "Đang xử lý", "Hoàn thành", "Đã hủy"
   - Valid transitions: "Đang xử lý" → "Hoàn thành" (on payment), "Đang xử lý" → "Đã hủy" (cancel)
   - Terminal states: "Hoàn thành", "Đã hủy" have no exits
   - Missing from both documents

2. **Payment State Machine** (None)
   - Initial: PaidAmount=0, PaymentMethod=null, PaymentAt=null
   - After payment: Immutable
   - Cannot re-pay already paid order
   - Missing from both documents

3. **Immutable Order Fields** (None)
   - InvoiceCode: Generated once, immutable forever
   - SentQuantity: Can only increase, never decrease
   - PaymentAt: Immutable once set
   - PaidAmount: Immutable once set
   - Missing from both documents

4. **SentQuantity Protection Constraint** (Partial)
   - Cannot reduce SentQuantity (already sent to kitchen)
   - Mentioned in AI_RULES.md but not emphasized as critical constraint
   - Not in AGENTS.md

5. **Loyalty Points Immutability** (None)
   - LoyaltyTransaction creates audit trail
   - Cannot modify point balances retroactively
   - Missing from both documents

6. **MAX_QUANTITY Constraint** (Partial)
   - const int MAX_QUANTITY = 500 in OrderService
   - AI_RULES.md Section 22 mentions quantity validation but not this constant
   - Missing explicit constant documentation

7. **Invoice Code Uniqueness** (None)
   - Generated as "HD" + DateTime.Now.ToString("yyyyMMddHHmmss")
   - Collision risk if multiple orders in same second
   - Missing from both documents

**SEVERITY**: HIGH (Cannot operate safely without these rules)

**IMPACT ON AI AGENTS**:
- Cannot understand order lifecycle
- Cannot understand payment mechanics
- Cannot understand immutability constraints
- Might attempt modifications that are forbidden

**RECOMMENDATION**:
- Add new sections to AGENTS.md:
  * Section 54.5: "ORDER STATUS STATE MACHINE" (document valid states and transitions)
  * Section 29.5: "PAYMENT STATE MACHINE" (document payment immutability)

- Add new sections to AI_RULES.md:
  * Section 13.5: "IMMUTABLE ORDER FIELDS"
  * Section 23.5: "QUANTITY CONSTRAINTS" (include MAX_QUANTITY=500)
  * Section 80.5: "PAYMENT IMMUTABILITY"

---

## SECTION 6: SIGNALR CONSISTENCY - MISSING DOCUMENTATION

### Current Documentation (AGENTS.md Section 19)

States OrderService → KitchenHub → Kitchen clients flow but does NOT specify:
- Exact event names triggered
- Event payloads
- Delivery guarantees
- Ordering guarantees (before/after DB commit?)

### Verified Implementation (KitchenHub.cs, OrderController.cs)

Groups defined:
- kitchen-admin (admin users)
- role:{role} (by role)
- kitchen-branch:{branchId} (by branch)
- branch:{branchId}:role:{role} (by branch + role)

But signal flow not documented.

**SEVERITY**: MEDIUM (Kitchen workflow)

**IMPACT ON AI AGENTS**:
- If AI modifies order, doesn't know if/when kitchen gets notified
- Cannot verify operation success (does broadcast failure mean operation failed?)
- Might assume real-time delivery isn't required

**RECOMMENDATION**:
- Add to AGENTS.md Section 19:
  * "SignalR broadcast is SECONDARY to database operation"
  * "If database commits but broadcast fails, order is still valid"
  * "Kitchen client MUST refresh on reconnect (do not use broadcast as delivery guarantee)"
  * "Source of truth: Database state, not SignalR event"

---

## SECTION 7: REDUNDANT/UNNECESSARILY VERBOSE SECTIONS

### Identified

**AGENTS.md Sections 91-106** (Process safety, workspace safety)
- Excellent content but exceeds "agent coding rules" scope
- Better suited for developer handbook
- ~800 lines on debugging, git safety, process management
- Recommendation: Keep but move to appendix or separate document

**AI_RULES.md Sections 57-60** (UI/UX guidance)
- Loading states, confirmation UI not AI rules
- Frontend implementation details
- Recommendation: Remove or relocate to frontend guidelines

**Repeated "No Secrets" Rules**
- Section 47: Sensitive information
- Section 48: Passwords
- Section 85: No secret in context
- Recommendation: Consolidate to single authoritative section

**SEVERITY**: LOW (Wasted context/tokens, not incorrect)

**RECOMMENDATION**:
- Consolidate redundant sections in AI_RULES.md
- Move process safety guidance to appendix
- Remove frontend UX details from AI_RULES.md

---

## SECTION 8: TESTING REQUIREMENTS - INCOMPLETE

### Current Documentation (AGENTS.md Section 52-53)

Mandates test matrices for order and branch changes:
- Section 52: "Required Test Matrix for Order Changes" (12 test cases)
- Section 53: "Required Test Matrix for Branch Changes" (9 test cases)

### Verified Implementation (PROJECT_CONTEXT.md)

No automated test files found in tests/ directory.

**SEVERITY**: MEDIUM (Guidance exists but infrastructure missing)

**ISSUE**: AGENTS.md mandates tests but project has no test framework documented.

**RECOMMENDATION**:
- Update AGENTS.md Section 51: "Manual testing matrix required (automated tests not yet available)"
- Document: "Test results must be manually verified and reported"
- Add: "Do not claim feature works without manual verification steps"
- Clarify expectations for test infrastructure

---

## SECTION 9: ARCHITECTURE BOUNDARIES - MISSING CLARITY

### Current Documentation (AGENTS.md Section 3)

Describes layers correctly but doesn't state:
- What if an Application Service is missing?
- Can Controller call Infrastructure directly? (should be no)
- Can Domain entities reference external frameworks? (should be no)
- What if you need to bypass a layer?

### AI_RULES.md

Doesn't address AI layer boundary rules.

**SEVERITY**: LOW (Implementation mostly correct)

**RECOMMENDATION**:
- Add to AI_RULES.md new section: "AI ARCHITECTURE BOUNDARIES"
  * "AI must call Application Services, never DbContext"
  * "AI must never bypass authorization layer"
  * "AI tools are independent of controller layer"
  * "AI layer sits alongside WebAPI layer, not above it"

---

## SUMMARY TABLE: Issues Categorized

| Severity | Issue | Location | Impact |
|----------|-------|----------|--------|
| CRITICAL | Manager role not implemented | AGENTS.md 13, AI_RULES.md 10 | Authorization model mismatch |
| CRITICAL | Branch isolation not enforced | AI_RULES.md 11-12 | Data isolation gap |
| HIGH | Order merge logic missing | AGENTS.md 8 | POS workflow misunderstood |
| HIGH | SentQuantity protection missing | AGENTS.md 8 | Kitchen constraints ignored |
| HIGH | Order status machine missing | (None) | Cannot understand lifecycle |
| HIGH | Payment immutability missing | (None) | Cannot understand payment |
| MEDIUM | Customer role presentation | AGENTS.md 13, AI_RULES.md 10 | Auth model clarity |
| MEDIUM | SignalR delivery guarantees missing | AGENTS.md 19 | Kitchen workflow unclear |
| MEDIUM | Test framework missing but mandated | AGENTS.md 52-53 | Misleading test requirements |
| LOW | Defensive manager code | KitchenHub.cs | Architecture debt |
| LOW | Redundant sections | AI_RULES.md 57-60, 47-48, 85 | Context bloat |
| LOW | Overly verbose process guidance | AGENTS.md 91-106 | Scope creep |

---

## RECOMMENDED REVISION PLAN

### CRITICAL (Do immediately before AI implementation)

**1. Fix Manager Role References**
- [ ] AGENTS.md Section 13: Remove manager from active roles, add note about defensive code
- [ ] AI_RULES.md Section 10: Same change
- [ ] Decision: Implement manager fully OR document as NOT implemented

**2. Update Branch Isolation Documentation**
- [ ] AI_RULES.md Section 11: Clarify branch check is at tool level, not permission service
- [ ] Add HIGH-PRIORITY FIX note about AiPermissionService
- [ ] Document workaround: "Each tool must filter by AiUserContext.BranchId"

**3. Document Missing Order Rules**
- [ ] AGENTS.md: Add Section 54.5 (ORDER STATUS STATE MACHINE)
- [ ] AGENTS.md: Add Section 29.5 (PAYMENT STATE MACHINE)
- [ ] AGENTS.md Section 8: Add order merge logic, SentQuantity, discount bypass rules
- [ ] AI_RULES.md: Add new sections for immutable fields, payment immutability

### HIGH (Do before AI task implementation)

**4. Fix Customer Role Documentation**
- [ ] Restructure role definitions to separate Employee roles from Customer auth path
- [ ] Clarify Customer is separate entity, not Employee.Role

**5. Document SignalR Behavior**
- [ ] AGENTS.md Section 19: Add delivery guarantees and ordering
- [ ] Document: Broadcast is secondary to DB state

**6. Clarify Test Requirements**
- [ ] AGENTS.md Sections 52-53: Change "Required Test Matrix" to "Recommended" or specify manual testing

### MEDIUM (Do for documentation quality)

**7. Consolidate Redundancy in AI_RULES.md**
- [ ] Merge sections 47, 48, 85 into single "SECRETS & SENSITIVE DATA" section
- [ ] Remove sections 57-60 (frontend UX guidance) or relocate

**8. Add Architecture Boundary Rules**
- [ ] AI_RULES.md: New section "AI ARCHITECTURE BOUNDARIES"

### LOW (Do when refactoring documentation)

**9. Reorganize Verbose Sections**
- [ ] Move AGENTS.md sections 91-106 to appendix
- [ ] Create separate "Developer Process Guide" if keeping

---

## APPROVED CHANGES (Ready to implement)

These changes are safe to make now:

1. **AGENTS.md Section 13**: Remove "manager" from roles list
2. **AI_RULES.md Section 10**: Remove "manager" from roles list
3. **AGENTS.md Section 8**: Add 5 bullet points about order merge, SentQuantity, discounts
4. **AI_RULES.md Section 11**: Add clarification note about branch check location
5. **AGENTS.md**: Add new Sections 29.5 and 54.5 (ORDER and PAYMENT state machines)
6. **AI_RULES.md**: Restructure role definitions to show "Employee Roles" vs "Customer (separate)"

---

**END AUDIT REPORT**

**Next Steps**:
1. Share this audit with team
2. Prioritize CRITICAL fixes
3. Create implementation task for each fix
4. Update docs per recommendations
5. Verify fixes against PROJECT_CONTEXT.md
6. Re-audit before full AI agent deployment
