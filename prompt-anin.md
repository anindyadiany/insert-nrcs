# INSERT NRCS — Development Prompt

You are a senior full-stack software engineer and broadcast newsroom system architect.

Build a modern web-based **NRCS (Newsroom Computer System)** called:

**INSERT NRCS**
**Integrated Newsroom System for Entertainment Reporting Teams**

INSERT NRCS is designed specifically for an entertainment newsroom and is intended to support the daily workflow of reporters, producers, editors, assignment desk operators, and newsroom staff.

Do not build a generic CRUD application.

The application must feel like a professional newsroom production system similar in concept to enterprise NRCS platforms such as ANN/Annova, but with a **modern web-based architecture and user experience.**

---

# 1. Project Background

Entertainment newsroom production involves many people and many types of information.

A typical story begins as an idea or assignment, followed by reporting, media acquisition, script writing, editing, review, approval, placement into a rundown, and eventually broadcast.

In a traditional workflow, information and media can become scattered across messaging applications, folders, local computers, spreadsheets, and different production systems.

INSERT NRCS is intended to bring the core newsroom workflow into one system.

The central concept is:

> **The Story is the center of the newsroom workflow.**

Everything related to a story should be connected to the Story:

* Assignment
* Reporter
* Producer
* Script
* Media
* Notes
* Approval
* Rundown
* Status
* History

---

# 2. Important Scope Decision

Do NOT implement PAM or MAM integration yet.

INSERT NRCS currently operates as an independent newsroom production system.

However, the architecture must leave clean extension points for future integration with:

* PAM
* MAM
* MOS
* Broadcast Automation
* Graphics
* Teleprompter systems
* Other broadcast systems

Do not over-engineer these future integrations.

Create clean interfaces and boundaries where necessary, but focus on the features that are actually required now.

---

# 3. AI

Do NOT implement AI features in this version.

The system must work completely without AI.

However, avoid architectural decisions that would make future AI integration difficult.

Possible future AI capabilities may include:

* Speech-to-text
* Automatic story summarization
* Semantic media search
* OCR
* Automatic metadata extraction
* Face recognition
* Story recommendation

These are future capabilities only.

---

# 4. Primary Goal

The first usable version must allow a newsroom to perform this workflow:

```text
Story / Assignment
        ↓
Reporter
        ↓
Media Ingest
        ↓
Story Media
        ↓
Script
        ↓
Review / Approval
        ↓
Rundown
        ↓
Ready for Broadcast
```

The system should be usable as soon as these core workflows are implemented.

Do not spend excessive effort building speculative features before the core workflow works.

---

# 5. Core Modules

Implement these modules first.

## Dashboard

Provide a role-aware dashboard.

The dashboard should show relevant information depending on the user's role.

Reporter:

* My assignments
* My stories
* Pending work
* Media ingest status
* Script status
* Deadlines

Producer:

* Today's stories
* Stories waiting for review
* Stories waiting for approval
* Rundown status
* Breaking stories

Ingest Operator:

* Ingest queue
* Processing
* Completed
* Failed
* Storage status

Administrator:

* System status
* Users
* Roles
* Configuration

---

# 6. Story Management

Story is the central entity.

A Story should contain at least:

* Story ID
* Slug
* Title
* Headline
* Category
* Program
* Priority
* Status
* Reporter
* Producer
* Assignment
* Deadline
* Created date
* Updated date
* Notes

Story statuses should be designed as a clear state machine.

Example:

```text
Idea
 ↓
Assigned
 ↓
Reporting
 ↓
Writing
 ↓
Review
 ↓
Revision
 ↓
Approved
 ↓
Ready
 ↓
On Air
 ↓
Completed
```

Do not blindly use this exact state list if a better newsroom workflow is appropriate. Explain the decision.

---

# 7. Assignment

Provide an Assignment workflow.

An assignment should be able to specify:

* Story
* Reporter
* Assignment desk
* Priority
* Location
* Event
* Deadline
* Brief
* Notes
* Status

The reporter should be able to see all assigned work from a dedicated workspace.

Assignment changes must be reflected in real time.

---

# 8. Media Ingest

Media Ingest is a core feature of Version 1.

Do NOT treat ingest as a simple web file upload.

Design it as a controlled newsroom media acquisition workflow.

Possible sources:

* Local file upload
* Drag and drop
* Network folder
* Watch folder
* Camera media
* FTP/SFTP
* Future SRT source

The ingest system must support:

* Queue
* Priority
* Progress
* Pause
* Resume
* Retry
* Cancel
* Error handling
* Checksum / verification
* File metadata
* Thumbnail
* Media duration
* Media format information
* Proxy generation after successful ingest
* Temporary media storage
* Retention policy

---

# 9. Important Ingest Rule

Do not assume that a file can be transcoded while it is still being copied.

For normal file-based media, the safe workflow is:

```text
Source
 ↓
Copy
 ↓
Copy Complete
 ↓
Verify
 ↓
Media Inspection
 ↓
Thumbnail
 ↓
Proxy Generation
 ↓
Available in Story
```

For formats that support growing-file workflows in the future, the architecture may support specialized processing, but Version 1 should prioritize reliability over premature optimization.

---

# 10. Temporary Media Storage

Do NOT treat ingest storage as permanent archive storage.

The purpose of the ingest area is temporary production storage.

For example:

```text
INGEST
 ├── Incoming
 ├── Processing
 ├── Ready
 ├── Failed
 └── Expired
```

Media that is not needed should eventually be removable according to retention rules.

The future MAM will become the permanent archive.

Do not implement MAM now.

---

# 11. Story Media Bin

Every Story should have a Media Bin.

Example:

```text
Story: Artist Interview

Media
 ├── Interview_01.mp4
 ├── Interview_02.mp4
 ├── B-Roll_01.mp4
 ├── B-Roll_02.mp4
 └── VO.wav
```

Each media asset should have:

* Asset ID
* Filename
* Original filename
* Storage path
* File size
* Duration
* Format
* Codec
* Resolution
* Frame rate
* Audio information
* Thumbnail
* Proxy path
* Ingest status
* Story ID
* Created time
* Retention status

---

# 12. Script Editor

Create a newsroom-oriented script editor.

Features:

* Autosave
* Draft
* Revision
* Version history
* Word count
* Estimated reading duration
* Script status
* Producer comments
* Approval
* Locking where necessary

The system should calculate estimated reading duration based on configurable words-per-minute settings.

Example:

```text
250 words
Average 150 WPM
Estimated duration: 01:40
```

---

# 13. Script Versioning

Never overwrite important script history.

A Story should maintain versions:

```text
Version 1
Version 2
Version 3
Approved Version
```

Users should be able to see:

* Who changed it
* When
* What version
* Optional change notes

---

# 14. Approval Workflow

Implement a simple newsroom approval workflow.

Example:

```text
Reporter
   ↓
Producer Review
   ↓
Revision
   ↓
Producer Approval
   ↓
Ready
```

Approval must record:

* User
* Date/time
* Decision
* Comment
* Version

Possible decisions:

* Approve
* Reject
* Request Revision

Do not build a complicated BPM engine for this.

Keep it practical.

---

# 15. Rundown

Create a newsroom rundown.

Features:

* Create rundown
* Program
* Date
* Air time
* Segment
* Story
* Order
* Duration
* Status
* Notes

The rundown must support drag-and-drop ordering.

Example:

```text
18:00:00  Opening
18:00:30  Story 01
18:02:45  Story 02
18:05:20  Story 03
18:08:10  Commercial
```

Automatically calculate:

* Story duration
* Start time
* End time
* Total duration

Changes to the rundown should be reflected to connected users in real time.

---

# 16. Real-Time Collaboration

Use SignalR where real-time communication provides clear value.

Examples:

* New assignment
* Assignment status change
* Story update
* Script update
* Approval
* Ingest progress
* Rundown changes
* Notifications

Do not use SignalR unnecessarily.

---

# 17. Search

Provide global search.

Search across:

* Stories
* Assignments
* Scripts
* Media metadata
* Reporters
* Rundowns

Start with normal database search.

Do not implement semantic/AI search yet.

---

# 18. User Roles

At minimum:

### Reporter

Can:

* View assignments
* Update assignments
* Create/edit stories
* Upload/ingest media
* Write scripts
* Submit stories for review

### Producer

Can:

* Manage stories
* Assign reporters
* Review scripts
* Approve/reject
* Manage rundown

### Assignment Desk

Can:

* Create assignments
* Assign reporters
* Monitor assignments
* Monitor incoming media

### Ingest Operator

Can:

* Manage ingest queue
* Monitor transfers
* Retry failed transfers
* Inspect media
* Manage temporary media

### Administrator

Can:

* Manage users
* Manage roles
* Manage configuration
* View system logs

Design permissions so additional roles can be added later.

---

# 19. Audit Trail

Important actions must be logged.

Examples:

* Story created
* Assignment changed
* Reporter changed
* Script edited
* Script approved
* Media ingested
* Media deleted
* Rundown changed
* User login

Store:

* User
* Timestamp
* Action
* Entity
* Entity ID
* Before value where appropriate
* After value where appropriate

---

# 20. UI / UX

The UI should feel like a professional newsroom system.

Do not create a generic admin dashboard full of cards.

Prioritize:

* Information density
* Fast navigation
* Keyboard shortcuts
* Split views
* Tables
* Queues
* Timeline
* Drag and drop
* Status indicators
* Contextual actions
* Minimal unnecessary dialogs

The application will be used for long periods by newsroom operators, so usability and speed are more important than visual decoration.

Use responsive layouts where practical, but prioritize desktop newsroom workflows.

---

# 21. Technology

Use:

### Backend

* ASP.NET Core
* .NET
* Entity Framework Core
* MySQL
* REST API
* SignalR
* Background Services

### Frontend

* Blazor Web App
* MudBlazor

### Media

Use FFmpeg / FFprobe or MediaInfo where appropriate.

Separate media-processing responsibilities from the main web application.

---

# 22. Architecture

Use a pragmatic Clean Architecture.

Suggested structure:

```text
src/
 ├── Insert.Web
 ├── Insert.Application
 ├── Insert.Domain
 ├── Insert.Infrastructure
 ├── Insert.Media
 └── Insert.Worker
```

Do not create unnecessary projects.

The architecture should remain understandable to a small development team.

---

# 23. Important Design Principle

Do not over-engineer.

This is an actual product that needs to start being used.

When choosing between:

```text
Complex theoretically perfect architecture
```

and

```text
Simple architecture that can evolve
```

prefer the second option unless the complexity provides a real operational benefit.

Build the minimum useful system first.

---

# 24. Development Approach

Work incrementally.

Do not generate the entire application at once.

Start with:

### Step 1

Application shell:

* Login
* Navigation
* Layout
* User identity
* Role

### Step 2

Story management.

### Step 3

Assignment.

### Step 4

Media Ingest.

### Step 5

Story Media Bin.

### Step 6

Script Editor.

### Step 7

Approval.

### Step 8

Rundown.

### Step 9

Notifications and realtime updates.

Each step must produce a working feature before moving to the next.

---

# 25. Coding Rules

Write production-quality code.

Use:

* Async/await
* CancellationToken
* Dependency Injection
* Structured logging
* Validation
* Error handling
* Configuration through appsettings/environment variables
* Database migrations
* DTOs for API boundaries
* Clear naming
* Unit tests for important business logic

Do not put business logic directly into Blazor components.

Do not put database access directly into UI components.

Do not hard-code storage paths.

Do not hard-code roles or permissions in dozens of places.

---

# 26. Future Compatibility

The architecture should make it possible to add:

```text
INSERT NRCS
     │
     ├── PAM
     │
     ├── MAM
     │
     ├── MOS
     │
     ├── Automation
     │
     ├── Graphics
     │
     └── AI
```

But none of these should be required for Version 1.

---

# 27. Most Important Requirement

Do not spend the first implementation phase creating documentation, theoretical architecture, or speculative features.

Start building the actual application.

The immediate objective is:

> **A newsroom user should be able to create an assignment, create a story, ingest media, write a script, approve the story, and put it into a rundown.**

If those workflows work reliably, INSERT NRCS has already become a useful product.

Everything else can evolve from there.

---

# First Task

Before writing large amounts of code, briefly describe the proposed database entities and relationships for:

* User
* Role
* Assignment
* Story
* Story Media
* Media Asset
* Script
* Script Version
* Approval
* Rundown
* Rundown Item
* Ingest Job
* Audit Log

Then create the initial project structure and implement **Story Management first**.

Do not proceed to Media Ingest until the Story Management foundation is functional.
