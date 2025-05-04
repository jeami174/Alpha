# Project Management Web Application – Alpha

## Overview

This is a project management web application built with **ASP.NET Core MVC**.  
It allows authenticated users to manage projects, assign members, and handle client data.  
The application follows a clean, layered architecture using **Entity Framework Core**,  
**ASP.NET Identity** for authentication and authorization, and **SignalR** for real-time notifications.

The application includes modern features such as:

- Role-based access control
- Dark/light mode toggle
- External authentication (Google)
- Tag-based UI for member selection

## Features

### Framework & Architecture

- ASP.NET Core MVC  
- Entity Framework Core (Code-First)  
- Layered architecture: data layer, business layer, web layer  
- ASP.NET Identity with individual user accounts

### Validation

- Client-side validation using JavaScript  
- Server-side validation using `ModelState`

### Cookie Consent

- GDPR-style cookie consent banner  
- Preferences stored via fallback logic:  
  - Server-side for logged-in users  
  - `localStorage` for guests  
  - `prefers-color-scheme` as fallback

### Role-Based Authorization

- Default role: **User**  
- Admin-only access for managing **clients** and **members**

### External Authentication

- Google Sign-In support  
- Automatically assigns role: **User**

### Dark/Light Theme

- Toggle available in UI  
- Theme preference stored per user or browser

### Real-Time Notifications

- Built with **SignalR**  
- Notifications for new projects (all users)  
- Notifications for new members (admins only)

### Project Media & Member Assignment

- Images for clients, members and projects can be uploaded  
- Members are assigned to projects via tag-style selection UI

---

## Admin Access (Initial User)

If no user exists in the system, a default admin account is seeded:

- **Email:** `admin@domain.com`  
- **Password:** `BytMig123!`  

> ⚠️ Be sure to log in and change this password immediately.

---

## Tech Stack

- ASP.NET Core MVC  
- Entity Framework Core  
- ASP.NET Identity  
- SignalR  
- JavaScript (vanilla)  
- HTML5 / CSS3  
- Google OAuth 2.0  
