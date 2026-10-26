# Rogbau — Construction Warehouse Management System

A commercial web application developed for a construction company to manage equipment, warehouse operations, and resources across construction sites.

The application is deployed to a production server and remains under active development, with features and improvements driven by the company's operational needs.

**Production website:** [www.magazynrogbau.pl](https://www.magazynrogbau.pl)

> Access to the application's operational features requires an authorized account.

## Overview

Rogbau brings warehouse and construction site workflows into a single system. It helps employees track equipment availability, manage purchases and orders, and coordinate equipment transfers between locations.

The application includes a React frontend and an ASP.NET Core backend backed by a SQL Server database.

## Key Features

- **Equipment management** — maintain equipment records, categories, and availability.
- **Construction site management** — organize sites and track equipment assigned to each location.
- **Equipment transfers** — manage equipment movements, transfer documents, and receipt confirmations.
- **Order management** — create orders and track partial or complete fulfillment.
- **Purchase and supplier management** — record equipment purchases and maintain supplier information.
- **Dashboard** — access an overview of operational data.
- **Historical data** — review past records and equipment activity.
- **Real-time notifications** — receive updates through SignalR.
- **User and role management** — control access according to employee responsibilities.

## Technology Stack

| Area | Technologies |
| --- | --- |
| Frontend | React 19, TypeScript, Vite |
| Styling | Tailwind CSS |
| Routing | React Router |
| Backend | C#, ASP.NET Core, .NET 10 |
| Database | Microsoft SQL Server |
| Data access | Entity Framework Core |
| Authentication | ASP.NET Core Identity, JWT |
| Application logic | MediatR, FluentValidation |
| Real-time communication | SignalR |
| API documentation | Swagger / OpenAPI |

## Project Structure

- `Rogbau.Frontend/` — React and TypeScript application.
- `Rogbau.Api/Rogbau.Api/` — ASP.NET Core API, application features, and database integration.
- `Rogbau.Api/Tests/` — integration and workflow verification projects.

## Architecture

The frontend communicates with the backend through an HTTP API and uses SignalR for real-time notifications.

The backend organizes application logic into feature modules, including equipment, transfers, orders, purchases, construction sites, and users. Entity Framework Core handles persistence, while ASP.NET Core Identity and JWT provide authentication and role-based access control.

## Project Status

**Deployed to production · Under active development**

Rogbau is a commercial project built around the requirements of a construction company. Development continues alongside the deployed application, with ongoing improvements to workflows, functionality, and usability.
