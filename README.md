# DynamoDB Query UI (ASP.NET 9.0)

## Overview
This project provides a web-based UI to query AWS DynamoDB using an SQL-like syntax.
It supports **multiple conditions** (`AND`, `OR`) and common operators (`=`, `>`, `<`, `>=`, `<=`, `CONTAINS`).

## Features
- List all DynamoDB tables
- Run queries like: `SELECT * FROM users WHERE status = 'active' AND age > '30'`
- Supports:
  - **Operators**: `=`, `>`, `<`, `>=`, `<=`, `CONTAINS`
  - **Multiple conditions** (`AND`, `OR`)
- Sortable and pageable results

## Setup
1. Ensure your **AWS credentials** are mounted at `./aws/credentials` in your OpenShift container.
2. Run the application in OpenShift.

## Example Queries
| SQL-like Query | DynamoDB Equivalent |
|---------------|--------------------|
| `SELECT * FROM users WHERE id = '12345'` | `Scan: id = '12345'` |
| `SELECT * FROM orders WHERE status = 'shipped' AND total > '100'` | `Scan: status = 'shipped' AND total > 100` |

---

## File Structure
```
DynamoDB_Query_UI/
│── Pages/
│   ├── Index.cshtml
│   ├── Index.cshtml.cs
│── Controllers/
│   ├── DynamoController.cs
│── Program.cs
│── README.md
```
