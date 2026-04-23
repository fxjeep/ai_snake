# Project Rules: MVVM & Data Architecture

This project follows the **MVVM (Model-View-ViewModel)** pattern and uses a **Service-based Data Access Layer** to maintain clean separation of concerns.

## Directory Structure

- `Views/`: XAML UserControls and Windows. Code-behind should only contain UI-related logic (e.g., event handlers that can't be bound).
- `ViewModels/`: Classes that hold the state and logic for the Views. Inherit from `ObservableObject`.
- `Models/`: Plain Data Objects (POCOs) or Entity classes.
- `Data/`: Interfaces and Implementations for data access (e.g., SQLite, Supabase).
- `Services/`: Business logic and cross-cutting concerns.

## MVVM Rules (using CommunityToolkit.Mvvm)

1. **Property Change Notification**: Use `[ObservableProperty]` on private fields to generate public observable properties.
   ```csharp
   [ObservableProperty]
   private string _name;
   ```
2. **Commands**: Use `[RelayCommand]` on methods to generate `ICommand` implementations.
   ```csharp
   [RelayCommand]
   private async Task SaveAsync() { ... }
   ```
3. **Data Binding**: Views must bind to ViewModel properties. Avoid named controls in code-behind whenever possible.
4. **Dependency Injection**: Services and Data Providers should be injected into ViewModels via constructors to facilitate testing and swapping implementations (e.g., SQLite to Supabase).

## Data Access Rules

1. **Interface Segregation**: Always define an interface (e.g., `IDataService`) for data operations.
2. **Asynchronous Operations**: All data access and I/O operations must be `async` to keep the UI responsive.
3. **Database Initialization**: Ensure the database and tables are initialized (created if not exists) before any operations.
4. Keep all data related files in PlaqueData project
5. All operations on database are defined in PlaqueData/Data/IDataService.cs. 
6. UI project only reference to IDataService for data operations.
7. We could define different data source implementation and it must be switchable to current SQLite data source.

## Localization   

1. For every label in UI element, use the resource key to display the text.
2. Translate the label from English to Chinese, 
3. Save English labels in `Strings.en.xaml` (default) 
4. Save Chinese labels in `Strings.zh-Hans.xaml`.
5. Add language switch function in UI, user can switch between English and Chinese.

