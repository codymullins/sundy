Cross-platform calendar app with intelligent cross-calendar time management.

## Why This Project?

I'm building Sundy because:
- **I want to** - it's a passion project that excites me
- **I need specific features** I've wanted for a while:
  - Seamless cross-calendar integration and management
  - ADHD-friendly features to help with time management
  - Smart features to prevent missing meetings
  - Reliable, configurable notifications
- **Daily use** - this solves real problems I face every day
- **Speed matters** - I want a fast, lightweight experience
- **Privacy-first** - my data stays mine, always

### The Problem

Here's my reality:
- I have a Gmail calendar
- I have multiple Outlook calendars
- Some things sneak into my Apple Calendar
- My wife uses Reminders and the paper calendar on our wall

And here's what happens:
1. Calendar A gets blocked, but B and C don't know about it
2. Calendar B gets scheduled later and silently conflicts with A
3. Calendar C's notifications are muted, or I only see Calendar A because that's my work computer
4. Where do I put family events? Doctor appointments for me, my wife, the kids... all of them?
5. No easy way to navigate all of this—add ADHD and object permanence issues and... *"Oops, sorry, can't make that meeting today"*
6. *"Hey, are you joining the meeting?"* — *"Oh crap! I didn't get that notification!"*

## Features
- **Offline-first** - works without an internet connection
- **Fast & responsive** - efficient rendering and sync
- **Privacy-focused** - your data stays yours
- **Clean, minimal UX** - no clutter, just what you need

## Demo

Try the browser version: [https://try.sundycal.com](https://try.sundycal.com)

**Note:**
- This is a very basic demo of the browser version, which is not as well tested as the desktop version
- The browser version may be slower and have a larger footprint than normal
- Data is stored in memory and will not be persisted

## Getting Started

### Prerequisites
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or later

### Running the Desktop App

1. Clone the repository:
   ```bash
   git clone https://github.com/codymullins/Sundy.git
   cd Sundy
   ```

2. Restore dependencies:
   ```bash
   dotnet restore
   ```

3. Run the desktop application:
   ```bash
   dotnet run --project src/Avalonia/Sundy.Desktop/Sundy.Desktop.csproj
   ```

Alternatively, you can navigate to the desktop project directory and run directly:
```bash
cd src/Avalonia/Sundy.Desktop
dotnet run
```



https://github.com/user-attachments/assets/1f9c9e79-1fa8-4561-a64b-7427b5a94873
