# Categories Feature Guide

This file documents the command-based structure used for the Categories feature so future handlers follow the same architecture.

## Folder Pattern

```text
Features/Categories/
  Dtos/
  Mappers/
  Commands/
    CreateCategory/
    UpdateCategory/
    DeleteCategory/
```

## Command Flow

Each command follows the same path:

1. Validate the request with `FluentValidation`.
2. Load the aggregate from `IApplicationDbContext`.
3. Check domain rules such as duplicates or missing records.
4. Call the domain method on `Category`.
5. Save changes.
6. Return a domain result.

## Create Category

- Input: `CreateCategoryCommand`
- Result: `Result<CategoryDto>`
- Handler responsibility:
  - reject duplicate names
  - call `Category.Create`
  - persist the new category
  - map the domain object to `CategoryDto`

## Update Category

- Input: `UpdateCategoryCommand`
- Result: `Result<Updated>`
- Handler responsibility:
  - load the category by id
  - reject missing categories
  - reject duplicate names from other rows
  - call `Category.Update`
  - save changes

## Delete Category

- Input: `RemoveCategoryCommand`
- Result: `Result<Deleted>`
- Handler responsibility:
  - load the category by id
  - reject missing categories
  - prevent deletion when products still reference the category
  - remove the category and save changes

## Reuse Rule

When adding a new feature in this style, keep the same structure:

- `Command` for input
- `Validator` for request validation
- `CommandHandler` for orchestration
- `Dto` for output
- `Mapper` for conversion

