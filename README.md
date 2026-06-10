# YourMatter - Retro-Modern MySpace Clone

Welcome to **YourMatter**, a retro-modern social network clone built with ASP.NET Core 8.0. This project is developed to fulfill the requirements of the final exam project for the **"ASP.NET MVC"** course.

---

## 🚀 Key Features

* **Custom Profiles**: View, customize, and edit your profile details, including display name, bio, location, and a custom profile picture.
* **Classic Friends Grid**: A retro-style 3x3 friends showcase on user profiles, with the ability to search/browse members, send friend requests, and accept/decline incoming requests.
* **Interactive Feed**: Post thoughts and updates onto your wall or see the combined global feed.
* **AJAX Likes**: Express appreciation for posts instantly using a modern asynchronous API request without reloading the page.
* **Nested Comments**: Add feedback to any post to keep conversations going.
* **Admin Controls**: System administrators can view specific flags, manage all contents, and access special moderation capabilities.
* **Custom Error Handling**: Graceful error screens tailored for code errors, including custom pages for `400 Bad Request`, `401 Unauthorized`, and `404 Not Found`.

---

## 🛠️ Technology Stack

* **Core Framework**: ASP.NET Core 8.0 (MVC)
* **Database**: Microsoft SQL Server
* **ORM**: Entity Framework Core (with Migrations)
* **Authentication**: ASP.NET Core Identity System (with custom user properties)
* **Styling**: Bootstrap 5 + custom retro-modern CSS styling (`site.css`)
* **Unit Testing**: NUnit + InMemory Database for testing business logic layers with high code coverage.

---

## 📂 Project Structure

* **`YourMatter`**: The Web presentation layer containing Controllers, Views, Models, and static assets.
* **`YourMatter.Data`**: The Data access layer holding Entity Framework models, DB Context, Migrations, and database initialization logic.
* **`YourMatter.Services`**: The Business logic layer implementing decoupled service components (e.g. `UserService`, `PostService`, `CommentService`, `FriendService`, `LikeService`).
* **`YourMatter.Tests`**: NUnit testing project verifying core workflows across services.

---

## 🔑 Seeded Accounts & Credentials

To easily evaluate and test the system, the database is pre-seeded with the following accounts (all password rules are configured for development):

| Account Type | Email | Password | Role | Description |
|---|---|---|---|---|
| **Administrator** | `admin@yourmatter.com` | `AdminPass123!` | `Administrator` | Full moderation access. |
| **First Friend** | `tom@myspace.com` | `TomPass123!` | `User` | Your classic first friend "Tom". |
| **Standard User** | `kelly@yourmatter.com` | `KellyPass123!` | `User` | Community member. |
| **Standard User** | `bob@yourmatter.com` | `BobPass123!` | `User` | Community member. |

---

## ⚙️ Getting Started

1. **Configure Connection String**: Adjust the SQL Server connection string in [appsettings.json](file:///C:/Users/Orlin%20Rizov/source/repos/YourMatter/YourMatter/appsettings.json).
2. **Database Update & Seeding**: Running the web application automatically applies pending EF Core migrations and seeds initial database structures.
3. **Execute Unit Tests**:
   ```bash
   dotnet test
   ```
4. **Run Locally**:
   ```bash
   dotnet run --project YourMatter
   ```
