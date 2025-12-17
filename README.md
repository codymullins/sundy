1| Cross-platform calendar app with intelligent cross-calendar time management.
2| 
3| ## Why This Project?
4| 
5| I'm building Sundy because:
6| - **I want to** - it's a passion project that excites me
7| - **I need specific features** I've wanted for a while:
8|   - Seamless cross-calendar integration and management
9|   - ADHD-friendly features to help with time management
10|   - Smart features to prevent missing meetings
11|   - Reliable, configurable notifications
12| - **Daily use** - this solves real problems I face every day
13| - **Speed matters** - I want a fast, lightweight experience
14| - **Privacy-first** - my data stays mine, always
15| 
16| ### The Problem
17| 
18| Here's my reality:
19| - I have a Gmail calendar
20| - I have multiple Outlook calendars
21| - Some things sneak into my Apple Calendar
22| - My wife uses Reminders and the paper calendar on our wall
23| 
24| And here's what happens:
25| 1. Calendar A gets blocked, but B and C don't know about it
26| 2. Calendar B gets scheduled later and silently conflicts with A
27| 3. Calendar C's notifications are muted, or I only see Calendar A because that's my work computer
28| 4. Where do I put family events? Doctor appointments for me, my wife, the kids... all of them?
29| 5. No easy way to navigate all of this—add ADHD and object permanence issues and... *"Oops, sorry, can't make that meeting today"*
30| 6. *"Hey, are you joining the meeting?"* — *"Oh crap! I didn't get that notification!"*
31| 
32| ## Features
33| - **Offline-first** - works without an internet connection
34| - **Fast & responsive** - efficient rendering and sync
35| - **Privacy-focused** - your data stays yours
36| - **Clean, minimal UX** - no clutter, just what you need
37| 
38| ## Demo
39| 
40| Try the browser version: [https://try.sundycal.com](https://try.sundycal.com)
41| 
42| **Note:**
43| - This is a very basic demo of the browser version, which is not as well tested as the desktop version
44| - The browser version may be slower and have a larger footprint than normal
45| - Data is stored in memory and will not be persisted
46| 
47| ## Getting Started
48| 
49| ### Prerequisites
50| - [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or later
51| 
52| ### Running the Desktop App
53| 
54| 1. Clone the repository:
55|    ```bash
56|    git clone https://github.com/codymullins/Sundy.git
57|    cd Sundy
58|    ```
59| 
60| 2. Restore dependencies:
61|    ```bash
62|    dotnet restore
63|    ```
64| 
65| 3. Run the desktop application:
66|    ```bash
67|    dotnet run --project src/Avalonia/Sundy.Desktop/Sundy.Desktop.csproj
68|    ```
69| 
70| Alternatively, you can navigate to the desktop project directory and run directly:
71| ```bash
72| cd src/Avalonia/Sundy.Desktop
73| dotnet run
74| ```
75| 
76| 
77| 
78| https://github.com/user-attachments/assets/1f9c9e79-1fa8-4561-a64b-7427b5a94873
79| 