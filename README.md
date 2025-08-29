[![WaracleBooking API Deployed on Azure Web App](https://github.com/Bilal44/hotel-api/actions/workflows/waraclebooking.yml/badge.svg)](https://github.com/Bilal44/hotel-api/actions/workflows/waraclebooking.yml)

# Waracle Booking API

`A RESTful .NET 8 API facilitating hotel inquiries and room booking for Waracle tech task!`

The Waracle Booking API provides endpoints for searching hotels, finding available rooms and managing bookings. This README serves as a detailed guide on how to use the API, libraries used, dependencies, testing, Dockerisation and architecture.

## Table of Contents

- [Overview](#overview)
- [API](#api)
  - [Dependencies](#api-dependencies)
  - [Installation](#api-installation)
  - [Usage](#api-usage)
  - [Endpoints](#endpoints)
  - [Testing](#api-testing)
  - [Architecture](#api-architecture)
- [Design Decisions](#design-decisions)
- [Dockerisation](#dockerisation)

## Overview

### API

The RESTful API is built in C# (.NET 8) using Web API framework to provide booking management functionality. It allows users to retrieve hotels by name, check room availability in a hotel and then proceed to book and retrieve the booking. The API uses an in-memory database to complete the requests, which can be seeded and reset on demand.

**[Live Azure Demo with OpenAPI Documentation/Testing](https://waraclebooking.azurewebsites.net/swagger/index.html)**

<img width="1096" height="560" alt="image" src="https://github.com/user-attachments/assets/0c06c232-5c93-40d9-9f20-a7fd983136cd" />

### API Dependencies

The following are necessary to build and run the project:

- Visual Studio 2022/Rider/VSCode etc.
- .NET Core SDK 8.0 or later
- Entity Framework Core for database abstraction
- Microsoft InMemory Database for persistence
- Swashbuckle for Swagger/OpenAPI docs
- Serilog for logging
- xUnit and FluentAssertions for unit and integration tests
- FakeItEasy for stubbing dependencies
- Visual Studio Tools for Containers for running containerised service via Visual Studio 2022

Ensure that you have the required dependencies installed before proceeding with the installation.

### API Installation

To install and run the API, follow these steps:

1. Clone the repository:

   ```bash
   git clone https://github.com/Bilal44/hotel-api.git
   ```

2. Navigate to the project directory:

   ```bash
   cd WaracleBooking
   ```

3. Build the project:

   ```bash
   dotnet build
   ```

4. Run the API:

   ```bash
   dotnet run
   ```

The API will start and requests can be sent to https://localhost:7164 (or https://localhost:7164/swagger/index.html for OpenAPI docs).

### API Usage

Once the API is running, you can interact with it using API clients such as Postman or cURL. The API exposes the following endpoints for managing hotels, rooms and bookings:

#### Endpoints

`GET /api/hotels?name={name}`: Filters hotels by name, it is case-insensitive and allows partial matching. If no name is provided, it returns all hotels.

`GET /api/hotels/{id}/rooms`: Returns available rooms based on hotel ID, date range and guest count.

`POST /api/bookings`: Creates a new booking based on the selected hotel, guest details and room availability.

`GET /api/bookings/{id}`: Retrieves a booking by its GUID.

`POST /api/data`: Seeds testing data.

`DELETE /api/data`: Resets testing data.

Ensure you provide any required parameters in the query string or request body.

For detailed information on each endpoint, including request and response formats, refer to the [OpenAPI documentation](https://waraclebooking.azurewebsites.net/swagger/index.html).

### API Testing

The API supports unit and integration tests with `xUnit`, `FakeItEasy` and `FluentAssertions`. The tests are located in WaracleBooking.Tests project.

<img width="626" height="437" alt="image" src="https://github.com/user-attachments/assets/075a1c63-18ea-458f-90d7-bb7e194887e2" />

### API Architecture

The API follows an MVC layered architecture, separating concerns and providing modularity with OOP and SOLID principles.

## Design Decisions

I have taken some liberty with the design:

- I'm using in-memory database for easier cloud deployment, Azure charges for database hosting after the first year trial. It can be switched to a real database locally in a couple of minutes.
- The hotel name also allows partial matching and is case-insensitive for a better UX. If no name is provided, the same endpoint will return all hotels.
- Since the project brief mentioned _"night"_, the same-day check-in and check-out is deemed invalid. In a similar vein as real hotels, the assumption here is that there is an ample gap between person 1 checking out and person 2 checking in the same room. Therefore, it is possible for person 1 to book a room for 3rd-5th and person 2 to book 5th-6th as _"night"_ counting is the key here.
- The room availability is pre-filtered by the hotel ID in the route, although other approaches are also valid.
- The room IDs are globally unique and do not require hotelId + roomId composite key.
- Technically, the room type and capacity should be in a different table if we religiously follow normalisation. But with only three types, I've added additional validation and kept them in the same table to save an unnecessary join.
- I picked `GUID` for booking reference since it was especially mentioned to be unique at all times. An `int` can also work and is better for large table scans, especially if it becomes a foreign key.
- I have mitigated simultaneous booking with a `Pending` status, although it probably won't deal with sub-second race conditions very well.

## Dockerisation

Both API and Client support containerised deployments through Docker and Docker Compose. There are no prerequisites, libraries or dependencies required in order to run the containerised api apart from having Docker and Docker Compose installed on your system (and Git Bash if using Windows).

### Deployment

1. Clone the repository:

   ```bash
   git clone https://github.com/Bilal44/hotel-api.git
   ```

2. Either double-click the `deploy-api.sh` file or use the following commands on Linux from the root solution directory:

   ```bash
   chmod +x deploy-api.sh
   ./deploy-api.sh
   ```
Once built, it can be accessed at http://localhost (or http://localhost/swagger/index.html for OpenAPI docs)

#### Clean Up

1. Either double-click the `remove-api.sh` file or use the following commands on Linux from the root solution directory:

   ```bash
   chmod +x remove-api.sh
   ./remove-api.sh
   ```
