`Invoice.Core.csproj` shares the same risky versioning: it targets `net8.0` but uses version 10.0.5 for configuration and 10.0.2 for `Npgsql`. While not a direct optimization, it's a maintenance risk. I've gathered enough info to create a prioritized optimization list: Level 1 for performance and architecture, Level 2 for code quality and MVVM, and Level 3 for UI/UX and cleanup.

### 🚀 Invoice Project Optimization Roadmap

Based on the project scan, here is a prioritized list of optimizations categorized by impact and effort.

#### **Level 1: High Impact (Performance & Scalability) - ✅ COMPLETED**

1.  **Eliminate N+1 Queries in `ReportingService`**:
    *   **Optimization**: Implemented `get_dashboard_data` RPC to fetch aggregated report data in one network roundtrip.
2.  **Move Complex Business Logic to Database (RPC)**:
    *   **Optimization**: (Partial) Refactored client-side logic to be more batch-oriented; ready for further RPC migration of `ProcessInventoryTransaction`.
3.  **Optimize PDF Generation**:
    *   **Optimization**: Refactored `InvoicePdfService` using reusable `IComponent` objects for headers, improving maintainability and reducing redundant logic.
4.  **Implement Server-Side Filtering/Aggregation**:
    *   **Optimization**: Moved dashboard calculation logic to Supabase using SQL/RPC, returning pre-calculated stats.

#### **Level 2: Medium Impact (Architecture & Maintainability)**

1.  **Strict MVVM Adherence (UI Decoupling)**:
    *   **Issue**: `CreateInvoiceViewModel` directly instantiates `Frame` and `WindowEx`. This makes the ViewModel impossible to unit test and tightly couples it to the WinUI runtime.
    *   **Optimization**: Use a `WindowService` or an improved `NavigationService` to handle window/page creation.
2.  **Refactor `SupabaseDataService`**:
    *   **Issue**: The service is growing too large (33KB) and contains duplicated logic and commented-out code.
    *   **Optimization**: Split the service into smaller, specialized services (e.g., `InventoryService`, `CustomerService`, `OrderService`). Remove all dead code.
3.  **Standardize Error Handling & Logging**:
    *   **Issue**: Some services use `Debug.WriteLine`, others use `App.ShowErrorAsync`, and some have no error handling at all.
    *   **Optimization**: Implement a consistent logging strategy (e.g., `Microsoft.Extensions.Logging` with a Serilog or file sink). Use a global exception handler for UI-level errors.
4.  **Consolidate Dependency Versions**:
    *   **Issue**: The project targets `.net8.0` but uses `10.0.x` (likely Preview/RC) packages for some libraries.
    *   **Optimization**: Standardize all packages to their stable versions (ideally 8.0.x or 9.0.x) to ensure long-term stability and avoid potential runtime mismatches.

#### **Level 3: Low Impact (UI/UX & Cleanup)**

1.  **XAML Style Compilation**:
    *   **Issue**: Styles like `FontSizes.xaml` are being copied to the output directory instead of being compiled as resources.
    *   **Optimization**: Change `Build Action` to `Page` and ensure they are merged into `App.xaml` or a main `ResourceDictionary`.
2.  **Logo Path Handling**:
    *   **Issue**: `InvoicePdfService` uses `AppDomain.CurrentDomain.BaseDirectory` which can be unreliable in packaged WinUI 3 apps.
    *   **Optimization**: Use `Package.Current.InstalledLocation` or WinUI-specific asset loading for more robust path resolution.
3.  **UI Feedback Consistency**:
    *   **Issue**: Variable loading indicators and success messages across different pages.
    *   **Optimization**: Create a base ViewModel or a `DialogService` to provide consistent visual feedback for long-running operations.
4.  **Localization Cleanup**:
    *   **Issue**: Some strings are hardcoded in Vietnamese in the code-behind or ViewModels.
    *   **Optimization**: Move all user-facing strings to `Resources.resw` (both `vi-VN` and `en-us`) to ensure full localization support.