# GitHub Copilot Instructions for Alpine Needs E-commerce Project

## Project Overview
Alpine Needs is an e-commerce web application for an outdoor equipment store. The application allows customers to browse products, add items to cart, complete purchases, and manage their orders. It includes an admin dashboard for store management.

## Technology Stack
- **Frontend**: Razor Pages, Bootstrap 5, jQuery
- **Backend**: ASP.NET Core (.NET 9)
- **Database**: SQL Server
- **ORM**: Entity Framework Core
- **Authentication**: ASP.NET Core Identity

## Coding Conventions

### General
- Use primary constructor without private fields
- Use PascalCase for class names, method names, and public properties
- Use camelCase for local variables and private fields
- Use meaningful and descriptive names
- Keep methods small and focused on a single responsibility
- Comment complex code sections, but prefer self-documenting code

### Razor Pages
- Name page models with suffix "Model" (e.g., `IndexModel`)
- Keep code-behind files focused on handling requests, move business logic to services
- Use view components for reusable UI elements
- Use partial views for repeated page sections
- Localize razor pages using `@inject IViewLocalizer Localizer`

### C# Guidelines
- Use async/await for all I/O operations
- Use LINQ for data transformations where appropriate
- Prefer explicit types over var except for obvious types
- Use dependency injection for services
- Implement proper exception handling
- Use nullable reference types
- Localize models and controllers using `IStringLocalizer`

## Architecture Patterns
- **Clean Architecture** with separation of concerns:
  - Models: Domain entities and DTOs
  - Services: Business logic implementation
  - Pages: Presentation layer using Razor Pages
  - Data: Repository pattern for data access

## Database Structure
Follow these entity relationships when writing queries or modifying data models:

- **Product**: Core product entity with price, description, and inventory
- **Category**: Hierarchical structure for product categorization
- **Order**: Contains order metadata and status
- **OrderItem**: Links products to orders with quantity and price
- **User**: Extended from ASP.NET Identity with additional profile data


## Security Considerations
- Always validate user input on server-side
- Use anti-forgery tokens for all POST requests
- Implement proper authorization checks before any sensitive operations
- Use parameterized queries to prevent SQL injection
- Don't store sensitive data in client-side storage
- Follow least privilege principle for admin functions
