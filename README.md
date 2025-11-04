# PreciousPlan

## Overview
PreciousPlan is an intuitive web application for managing and simulating gold and silver savings plans. Users can register, create savings plans with custom monthly contributions, track investment growth, simulate historical performance, manage transactions, and securely view or edit their profile and bank details—all through a clean dashboard interface. The application displays portfolio metrics, charts for investment analysis, and supports plan management (creation, monitoring, closure) and transaction history review.

## Setup Instructions

1. **Clone or download the project from the repository:**


2. **Set your OpenExchangeRate APP_ID:**

- Replace the APP_ID in `Backend/MetalSavingsManager/MetalSavingsManager/Utils/Constants.cs`

3. **Run the application using Docker Compose:**

Navigate to the directory where your `docker-compose.yml` is located and run:
`docker compose up --build`

4. **Open the frontend in your browser:**

Go to [http://localhost:4200/register](http://localhost:4200/register) to register a new user.

> **Important:** Make sure your APP_ID is set correctly, otherwise, charts will not be visible on the dashboard.

## Usage

- After registration, log in to use the savings plan features and view metal price charts.
- Create a new savings plan for gold or silver, track its performance, and close or withdraw the plan when desired.
- Use the simulation feature on the dashboard to visualize historical performance based on real market data.

## Features
- User registration, login, and profile management
- Create and manage multiple gold/silver savings plans
- Dashboard showing key portfolio statistics and profit/loss
- Plan detail and transaction history pages
- Simulation and chart visualization of savings growth and returns
- Dockerized deployment for fast local setup

## Technologies Used
Angular & PrimeNG (Frontend), .NET Core (Backend API), OpenExchangeRates API, SQL Server (Database), Docker & Docker Compose
