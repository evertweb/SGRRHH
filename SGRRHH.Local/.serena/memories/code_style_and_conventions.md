# Code Style and Conventions - SGRRHH.Local

## C# Conventions
- **Naming**: PascalCase for classes, methods, properties; camelCase for private fields
- **Namespaces**: Follow folder structure
- **Async methods**: Suffix with `Async`
- **Entity classes**: Inherit from `EntidadBase` (common base with Id, audit fields)

## Blazor/Razor Conventions
- **Components**: PascalCase, stored in Components/Pages or Components/Shared
- **Parameters**: Use `[Parameter]` attribute
- **Event handlers**: Prefix with `On` (e.g., `OnSaveClicked`)
- **Code-behind**: Use `@code { }` blocks at the end of .razor files

## UI/Design System - Hospital/ForestechOil
- **NO border-radius**: Everything is rectangular
- **NO animations/transitions**: `animation: none !important`
- **Font**: Monospaced 'Courier New'
- **Colors**: Use CSS variables from `hospital.css` (--color-bg, --color-text, etc.)
- **Text**: UPPERCASE for labels, headers, buttons
- **Buttons**: Use classes `.btn`, `.btn-primary`, `.btn-danger`, `.btn-success`
- **Forms**: Use `.form-group`, `.form-row` classes
- **Tables**: Headers in UPPERCASE, use `.selected` for active rows
- **Modals**: Use `FormModal` component
- **States**: Handle loading, empty, error states consistently

## File Organization
- Pages in `Components/Pages/`
- Shared components in `Components/Shared/`
- CSS in `wwwroot/css/hospital.css` (single global stylesheet)
- Documentation in `wwwroot/docs/`

## Design Patterns
- Clean Architecture separation
- Repository pattern for data access
- Service layer for business logic
- DTOs for data transfer
- Result pattern for operation outcomes
