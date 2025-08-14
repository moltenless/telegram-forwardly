# Telegram Forwardly

**Telegram Forwardly** is a multi-component system that automatically forwards the most important messages from active Telegram group chats to a dedicated private Telegram forum group, based on user-defined keywords.  
It is built with **ASP.NET Core Web API**, **Flask + Telethon**, **MS SQL Server**, and **Docker**.

---

## 🚀 Features

- **Keyword-based filtering** — Forward only messages that match specific keywords.
- **Chat selection** — Choose which chats to monitor.
- **Flexible grouping** — Group forwarded messages by keyword or by source chat.
- **Two-component architecture**:
  - **Telegram Bot (ASP.NET Core)** — User interaction, chat and keyword management, database operations.
  - **User Bot (Flask + Telethon)** — Real-time message listening, filtering, and forwarding.
- **Multi-user support** with session-based storage (string sessions in DB).
- **Logging system** for errors, warnings, and events across all modules.
- **Docker-first development & deployment**.
- **Nginx reverse proxy** with **Let's Encrypt SSL** for secure production.
- **Scalable concurrency handling** — From simple asyncio to worker pools and microservices.

---

**Key points:**
- **Telegram Bot**: Handles commands, manages users, chats, and keywords, communicates with DB and User Bot.
- **User Bot**: Listens for new messages via Telethon, filters by keywords, forwards to user’s forum group.
- **Database**: Stores user data, chat settings, keywords, and session strings.
- **Inter-service Communication**: Via internal Docker network or HTTPS.
- **Deployment**: Monorepo with Docker Compose + Nginx reverse proxy.

---

## 📦 Tech Stack

- **Backend (Telegram Bot)**: ASP.NET Core Web API
- **ORM**: Entity Framework Core (Code First)
- **Database**: MS SQL Server
- **User Bot**: Python, Flask, Telethon
- **Containerization**: Docker, Docker Compose
- **Reverse Proxy**: Nginx + Let's Encrypt SSL
- **Logging**: Centralized logging across all components

---

## ⚙️ Installation & Setup

Use bot via [@forwardly_bot](https://t.me/forwardly_bot) 
Also visit our site at [telegram-forwardly.com](https://telegram-forwardly.com)
