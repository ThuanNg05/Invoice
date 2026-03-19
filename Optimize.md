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

#### **Level 2: Medium Impact (Architecture & Maintainability) - 🔄 IN PROGRESS**

1.  **Strict MVVM Adherence (UI Decoupling)**: ✅ **COMPLETED**
    *   **Optimization**: Use `IWindowService` and `INavigationService` to handle window/page creation, removing direct dependencies on `Frame` and `WindowEx` from ViewModels.
2.  **Refactor `SupabaseDataService`**: ✅ **COMPLETED**
    *   **Optimization**: Split the service into partial classes (`SupabaseDataService.Customers.cs`, `SupabaseDataService.Inventory.cs`, `SupabaseDataService.Invoices.cs`) to improve maintainability.
3.  **Standardize Error Handling & Logging**:
    *   **Issue**: Some services use `Debug.WriteLine`, others use `App.ShowErrorAsync`, and some have no error handling at all.
    *   **Optimization**: Implement a consistent logging strategy (e.g., `Microsoft.Extensions.Logging` with a Serilog or file sink). Use a global exception handler for UI-level errors.
4.  **Consolidate Dependency Versions**:
    *   **Issue**: The project targets `.net8.0` but uses `10.0.x` (likely Preview/RC) packages for some libraries.
    *   **Optimization**: Standardize all packages to their stable versions (ideally 8.0.x or 9.0.x) to ensure long-term stability and avoid potential runtime mismatches.

#### **Level 3: Low Impact (UI/UX & Cleanup) - ✅ COMPLETED**

1.  **XAML Style Compilation**: ✅ **COMPLETED**
    *   **Optimization**: Changed `Build Action` to `Page` for `FontSizes.xaml`, `Thickness.xaml`, and `TextBlock.xaml` and ensured they are merged into `App.xaml`.
2.  **Logo Path Handling**: ✅ **COMPLETED**
    *   **Optimization**: Refactored `InvoicePdfService` to use `Windows.ApplicationModel.Package.Current.InstalledLocation.Path` when running as an MSIX package for robust path resolution.
3.  **UI Feedback Consistency**: ✅ **COMPLETED**
    *   **Optimization**: Implemented `ViewModelBase` and `IDialogService` to provide consistent visual feedback (`IsBusy` state) and decoupled dialog management across all pages.
4.  **Localization Cleanup**: ✅ **COMPLETED**
    *   **Optimization**: Moved all user-facing strings from C# and XAML to `Resources.resw` using `x:Uid` and `GetLocalized()`, ensuring full localization support for the entire application.