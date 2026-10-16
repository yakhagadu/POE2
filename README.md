# POE2
Yakha Qhayiya Gadu
# RaceDay — Event Management System for South African Road Events

A full-stack web-based event management platform built for South Africa's road running, walking, and cycling community. RaceDay allows Event Organisers to create and manage events, categories, and participant results, while Participants can browse events, enter events, track their performance history, and prepare for race day.

**Project:** Portfolio of Evidence (PoE) — Part 2 of 3  
**Stack:** ASP.NET Core 8 Web API · Entity Framework Core · SQL Server · JWT Authentication · xUnit  
**Author:** [Your Name]  
**Student Number:** [Your Number]

---

## Table of Contents

1. [Project Overview](#project-overview)
2. [User Roles](#user-roles)
3. [System Architecture](#system-architecture)
4. [Repository Structure](#repository-structure)
5. [Prerequisites](#prerequisites)
6. [Setup Instructions](#setup-instructions)
7. [Running the Application](#running-the-application)
8. [API Endpoints](#api-endpoints)
9. [Testing](#testing)
10. [CI/CD](#cicd)
11. [Video Presentation](#video-presentation)
12. [Part 1 Artefacts](#part-1-artefacts)
13. [Troubleshooting](#troubleshooting)

---

## Project Overview

RaceDay is built progressively across three parts:

| Part | Focus | Status |
|------|-------|--------|
| **Part 1** | System planning — ERD, API endpoint plan, SQL schema |  Complete |
| **Part 2** | RESTful API in C# (this submission) |  Complete |
| **Part 3** | MVC web app + Azure Blob Storage + Docker |  Upcoming |

The RaceDay API powers the entire system. It handles:
- Authentication (register, login, JWT sessions)
- Role-based access control (Organiser vs Participant)
- Event management (CRUD)
- Category management per event
- Participant enrolments
- Result capture and retrieval

---

## User Roles

The system supports **two distinct user roles**, enforced at the API level:

###  Organiser
- Register an account with an organisation name and contact details
- Create, edit, and delete events
- Manage event categories (distances, age groups, gender restrictions)
- View all enrolments for their events
- Capture and manage participant results

###  Participant
- Register an account with personal and emergency contact details
- Browse and view upcoming events
- Enter an event by selecting a category
- View their own enrolments
- Track their personal results history

Role enforcement is applied via `[Authorize(Roles = "Organiser")]` / `[Authorize(Roles = "Participant")]` attributes on every protected endpoint. Participants **cannot** create events. Organisers **cannot** enrol in events.

---

## System Architecture

<img width="484" height="570" alt="image" src="https://github.com/user-attachments/assets/2e045142-2e41-47cf-860f-71374b028c0d" />
<img width="279" height="728" alt="image" src="https://github.com/user-attachments/assets/c97ba13b-820b-4600-bb22-547cc36cf649" />



---

## Prerequisites

Before running this project, ensure you have:

| Tool | Version | Notes |
|------|---------|-------|
| **.NET SDK** | 8.0 or later | [Download](https://dotnet.microsoft.com/download/dotnet/8.0) |
| **SQL Server** | LocalDB or Express | Comes with VS Installer |
| **Visual Studio 2022** | 17.8+ | With "ASP.NET and web development" workload |
| **SSMS** | Latest | For verifying the database |
| **Git** | Latest | |

---


