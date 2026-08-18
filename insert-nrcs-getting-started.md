# INSERT NRCS — Getting Started Guide

A practical, ordered path from zero to a working Story Management module, following the incremental approach the prompt requires (no big-bang generation, no premature PAM/MAM/AI work).

---

## 0. Before Writing Code: Lock In the Domain Model

The prompt explicitly asks for this as the "First Task." Do this on paper/Miro/dbdiagram.io before touching a project template — it will save you from schema churn later.

### Core entities and how they relate

```
User ──< UserRole >── Role

Story
 ├── ReporterId → User
 ├── ProducerId → User
 ├── AssignmentId → Assignment (1:1 or 1:0..1)
 ├── StoryStatus (state machine, see §1)
 ├── has many → StoryMedia (join to MediaAsset)
 ├── has many → Script
 │        └── has many → ScriptVersion
 ├── has many → Approval
 └── belongs to (optional) → RundownItem

Assignment
 ├── StoryId → Story
 ├── ReporterId → User
 └── Status (Draft / Assigned / Accepted / InProgress / Done)

MediaAsset
 ├── IngestJobId → IngestJob (nullable — asset may exist without an active job once complete)
 ├── StoryId → Story (nullable until attached to a story's media bin)
 └── technical metadata (duration, codec, resolution, checksum, proxy path…)

IngestJob
 ├── Source type (upload / watch folder / FTP…)
 ├── Status (Queued / Copying / Verifying / Inspecting / ProxyGen / Ready / Failed / Expired)
 └── produces → MediaAsset (1:1 once complete)

Script
 ├── StoryId → Story
 └── has many → ScriptVersion (immutable snapshots)

ScriptVersion
 ├── ScriptId → Script
 ├── VersionNumber
 ├── AuthorId → User
 └── Content, WordCount, EstimatedDuration

Approval
 ├── StoryId → Story
 ├── ScriptVersionId → ScriptVersion (which version was reviewed)
 ├── ReviewerId → User
 └── Decision (Approve / Reject / RequestRevision), Comment, Timestamp

Rundown
 ├── Program, Date, AirTime
 └── has many → RundownItem

RundownItem
 ├── RundownId → Rundown
 ├── StoryId → Story
 ├── Order, StartTime, Duration, Status

AuditLog
 ├── UserId, Timestamp, Action, EntityType, EntityId
 └── BeforeValue / AfterValue (JSON snapshot, not full diffing engine)
```

### Key modeling decisions to make explicit up front

- **Story status is a real state machine, not a free-text field.** Model it as an enum + a small `StoryStatusTransition` rule table (or just a switch in application logic) so illegal jumps (e.g. `Idea → OnAir`) are rejected server-side. The prompt says don't blindly copy its example list — a workable simplification for entertainment news is:
  `Idea → Assigned → InProgress → Draft → Review → Approved → Rundowned → OnAir → Completed` (+ `Revision` as a side-branch off `Review`, not a separate linear stage). Write down your reasoning in a short ADR (Architecture Decision Record) file — the prompt asks you to justify deviations.
- **MediaAsset is decoupled from Story** via a nullable `StoryId` and an `IngestJobId`, so ingest can start before anyone decides which story it belongs to (common in real newsrooms — footage arrives before assignment is finalized).
- **ScriptVersion is append-only.** Never update a version row in place; every save that matters creates a new version. Autosave drafts can be a separate lightweight `ScriptDraft` (single mutable row) that gets promoted to a `ScriptVersion` on explicit "save version" / submit-for-review.
- **Approval references a specific ScriptVersion**, not just the Story — this is what lets you show "approved version 3, but reporter has since edited to version 4."
- **AuditLog is generic** (EntityType + EntityId + JSON before/after) rather than one table per entity — keeps it from multiplying tables for every module.

---

## 1. Project Scaffolding

```
src/
 ├── Insert.Web            (Blazor Web App + MudBlazor)
 ├── Insert.Application    (use cases / services, DTOs, validation)
 ├── Insert.Domain         (entities, enums, state machine rules — no EF references)
 ├── Insert.Infrastructure (EF Core, MySQL, repositories, SignalR hubs)
 ├── Insert.Media          (FFmpeg/FFprobe wrappers — isolated so it can later become its own worker/service)
 └── Insert.Worker         (background services: ingest processing, proxy generation)
```

- `Insert.Domain` has zero EF/infrastructure dependencies — this is what keeps the state machine and business rules testable without spinning up a database.
- `Insert.Media` stays a separate project from day one specifically because the prompt calls out not coupling media processing to the web app — even though Version 1 might run it in-process, the seam should already exist.
- Set up the solution, EF Core + MySQL provider (`Pomelo.EntityFrameworkCore.MySql`), and an empty initial migration before writing any feature code.

---

## 2. Step 1 — Application Shell

Goal: log in, see role-appropriate navigation, nothing else.

1. Auth (ASP.NET Core Identity or a simple JWT/cookie scheme — Identity is the pragmatic default with MySQL).
2. Role model: `Reporter`, `Producer`, `AssignmentDesk`, `IngestOperator`, `Administrator` — stored as data, not hardcoded strings scattered through the UI. Centralize role checks behind a small `IPermissionService` or policy-based authorization so adding a role later doesn't mean hunting through components.
3. MudBlazor shell layout: nav menu that renders differently per role, a placeholder dashboard page per role (empty state is fine — just prove routing + auth + role-gating works).
4. **Done when:** four different test users (one per role, skip Admin for now) can log in and see a different nav menu / empty dashboard.

---

## 3. Step 2 — Story Management (the foundation everything else attaches to)

This is the module the prompt says not to skip past. Build it fully before touching Assignment.

1. `Story` CRUD (create/edit/list/detail) scoped by role: Reporter creates/edits their own, Producer sees all.
2. Implement the state machine as a domain service (`StoryWorkflowService.CanTransition(current, next)` + `Transition(...)`) — not as inline `if` statements in a Blazor page.
3. Story detail page becomes the hub: even before Assignment/Media/Script exist, leave clearly-labeled empty sections for them ("No media yet", "No script yet") — this is where those modules will attach in later steps.
4. Wire up AuditLog writes on create/status-change as the first real use of the generic audit table.
5. **Done when:** a Producer can create a story, change its status through legal transitions only, and see it in a list; illegal transitions are rejected with a clear error.

---

## 4. Step 3 — Assignment

1. Assignment CRUD linked 1:1 (or 1:0..1) to a Story.
2. Reporter's "My Assignments" workspace (a real dashboard query, not a stub).
3. SignalR: pushing assignment-created / status-changed to the assigned reporter in real time — this is your first justified use of SignalR (the prompt lists this as a clear-value case).
4. **Done when:** Assignment Desk assigns a reporter, the reporter sees it appear live without refreshing, and updates its status.

---

## 5. Step 4 — Media Ingest

Build the pipeline as a state machine too, mirroring the story workflow pattern:

```
Queued → Copying → CopyComplete → Verifying → Inspecting → ThumbnailGenerated → ProxyGenerating → Ready
                                                                                        ↘ Failed (from any step)
```

1. Start with local file upload only (defer watch folder / FTP / SRT — the prompt lists them as "possible sources," not Version-1 requirements).
2. `Insert.Worker` background service processes the queue: copy → checksum → ffprobe metadata → thumbnail → proxy (ffmpeg). Each step updates `IngestJob.Status` and is resumable/retryable.
3. Ingest Operator dashboard: queue / processing / completed / failed, with retry and cancel actions.
4. Store ingested media under a **temporary** ingest area (`Incoming/Processing/Ready/Failed/Expired`) — explicitly not the final story storage location yet.
5. **Done when:** a file uploaded through the UI ends up as a `MediaAsset` in `Ready` status with a real thumbnail and proxy, visible to the Ingest Operator, with progress shown live via SignalR.

---

## 6. Step 5 — Story Media Bin

1. Attach a `Ready` `MediaAsset` to a `Story` (sets the previously-nullable `StoryId`).
2. Media Bin UI on the Story detail page: list of assets with thumbnail, duration, format, ingest status.
3. **Done when:** ingested media can be pulled into a specific story and shows up in that story's bin.

---

## 7. Step 6 — Script Editor

1. `Script` + `ScriptVersion` per story; autosave to a mutable draft, explicit "save version" creates an immutable `ScriptVersion` row.
2. Word count + estimated duration from configurable WPM (store WPM as config, not a magic number).
3. Version history view: who, when, version number, optional note.
4. **Done when:** a reporter can write, autosave, and explicitly version a script; version history is visible and past versions are read-only.

---

## 8. Step 7 — Approval

1. `Approval` records tied to a specific `ScriptVersion`.
2. Producer review UI: Approve / Reject / Request Revision + comment, on the current version.
3. Rejection/revision-request moves the Story back to `Revision` via the state machine service from Step 2 — don't build a second, separate workflow engine for this.
4. **Done when:** a producer approves or rejects a specific script version, the story status updates accordingly, and the decision is logged with a comment.

---

## 9. Step 8 — Rundown

1. `Rundown` + `RundownItem`, drag-and-drop ordering (MudBlazor has drag-and-drop primitives; a dedicated JS interop drag library is a fallback if needed).
2. Auto-calculate start/end times and total duration from item durations + air time.
3. Only stories in an approved/ready status should be addable to a rundown — enforce via the state machine, not a UI-only check.
4. **Done when:** a producer builds a rundown by dragging approved stories into order, and times recalculate automatically.

---

## 10. Step 9 — Notifications & Realtime Polish

1. Consolidate the SignalR hubs used ad hoc in Steps 3/4/8 into a coherent notification pattern (toast/badge) across the app.
2. Add real-time push for: story update, script update, approval decision, rundown changes — reusing the same hub infrastructure rather than one hub per feature.
3. **Done when:** a change made by one user (status change, approval, rundown edit) is visible to relevant other logged-in users without a page refresh.

---

## Ongoing / Cross-Cutting (do a little at each step, don't defer to the end)

- **Global search** (Story/Assignment/Script/Media/Reporter/Rundown) — start as soon as 2–3 of these entities exist; plain EF `LIKE`/full-text queries, no semantic search.
- **Audit trail** — wire into each module as you build it (already started in Step 2), not bolted on later.
- **Unit tests** — prioritize the state machine services (Story workflow, Ingest pipeline) since they're the business-critical logic the prompt calls out.
- **Extension seams for later** — keep `Insert.Media` isolated, keep `MediaAsset`/`IngestJob` decoupled from any specific storage backend (interface + local-disk implementation now, swappable later for PAM/MAM), but don't build actual PAM/MAM/MOS/AI code paths yet.

---

## Suggested First Week

| Day | Focus |
|---|---|
| 1 | Entity/relationship diagram finalized + ADR on story state machine; solution scaffolding + initial migration |
| 2 | Auth, roles, shell layout, role-based nav (Step 1) |
| 3–4 | Story CRUD + state machine service + audit logging (Step 2) |
| 5 | Story detail page polish, tests for the state machine, demo-able "create story → change status" flow |

Everything after that follows the same one-module-at-a-time rhythm through Steps 3–9.
