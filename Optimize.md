 Act as a senior software engineer and code reviewer.

Task:
Refactor and debug the business logic in file: @Invoice/Views/IOActionsPage.xaml.cs @Invoice/ViewModels/IOActionsViewModel.cs @Invoice.Core/Services/SupabaseDataService.cs

Objective:
Fix incorrect product detection and broken inventory updates while preserving the existing invoice save workflow.

Current problems:
- The current logic incorrectly detects different products as the same product.
- When adding a different product, the system behaves as if it is an existing product and increases the quantity instead of creating/separating the correct line item.
- Inventory is not being calculated or updated correctly.
- The inventory issue may come from either:
  - application-side business logic
  - Supabase stored procedure / RPC / database function
- The refactor must remain consistent with the invoice saving workflow, especially inventory updates for both Product and Plank.

What I need you to do:
1. Analyze the existing logic in [@Invoice.Core/Services/SupabaseDataService.cs].
2. Identify the exact root cause of why different products are being merged incorrectly.
3. Identify whether the bug comes from:
   - bad comparison logic
   - missing unique key validation
   - duplicate handling logic
   - wrong state mutation
   - wrong Supabase procedure call
   - incorrect order of operations
4. Refactor the logic with minimal unnecessary architecture changes.
5. Preserve the current business workflow:
   - save invoice
   - save invoice details
   - update Product inventory
   - update Plank inventory
6. Ensure inventory updates are accurate and consistent.
7. If Supabase RPC / procedure is faulty, propose the corrected implementation or corrected calling pattern.
8. Highlight any risky assumptions in the current code.

Strict rules:
- Different products must NEVER be merged unless they share the same stable unique identifier.
- Do not rely only on product name, display text, formatted string, or partial object equality.
- Prefer matching by unique product ID / SKU / database primary key.
- Quantity should only increase when the item is truly the same product.
- Refactor for correctness first, then maintainability.
- Do not break the existing invoice save workflow.

Expected response format:
1. Root cause analysis
2. Bug explanation for wrong product detection
3. Bug explanation for inventory not updating
4. Refactored code for [@Invoice.Core/Services/SupabaseDataService.cs]
5. If needed, corrected Supabase procedure / RPC logic
6. Final recommended workflow order for invoice save and inventory update
7. Notes on edge cases / failure scenarios

Extra note:
The function must stay fully consistent with the workflow of saving invoice, especially:
- update inventory of Product
- update inventory of Plank