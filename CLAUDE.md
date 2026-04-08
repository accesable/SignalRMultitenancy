# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

A real-time notification demo with two parts:
- **`Api/`** — ASP.NET Core (.NET 10) backend with SignalR hub and REST endpoint
- **`chatapp/`** — Vue 3 + TypeScript frontend using `@microsoft/signalr`

## Running the Project

**Backend** (from `Api/`):
```bash
dotnet run
```
Runs on `http://localhost:5223`. Uses the `http` launch profile by default.

**Frontend** (from `chatapp/`):
```bash
npm install
npm run dev
```
Runs on `http://localhost:5173`.

**Build frontend:**
```bash
npm run build   # runs vue-tsc type check + vite build
```

## Architecture

### Flow
1. The frontend generates random `resourceId` and `userId` (UUIDs) on page load.
2. It connects to the SignalR hub at `/hubs/notifications?resourceId=...&userId=...`.
3. `NotificationHub.OnConnectedAsync` extracts query params and registers the mapping `userId → connectionId` in the in-memory `AvailableConnection` static dictionary.
4. When the user submits a comment, the frontend POSTs to `/api/v1/feedback` with `{ resourceId, userId, comment }`.
5. The API looks up the `connectionId` for that `userId` via `AvailableConnection` and sends a `"Notification"` message directly to that client via `IHubContext<NotificationHub>`.
6. The frontend receives the message and appends it to the `notifications` list.

### Key files
- `Api/Program.cs` — service registration, CORS policy (allows `http://localhost:5173`), hub mapping, and the `/api/v1/feedback` endpoint
- `Api/NotificationHub.cs` — SignalR hub; manages connect/disconnect lifecycle
- `Api/AvailableConnection.cs` — in-memory `ConcurrentDictionary<userId, connectionId>` store (no persistence)
- `chatapp/src/components/NotificationPage.vue` — all frontend logic (SignalR connection, axios POST, UI)

### Limitations to be aware of
- `AvailableConnection` is a static in-memory store — connections are lost on server restart and do not scale across multiple server instances.
- One `userId` maps to one `connectionId` (last-writer-wins); multiple tabs for the same user will override each other.
