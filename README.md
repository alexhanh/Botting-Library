GR Botting Library
==================
This projects contains parts of an automated gameplay (ie. botting) framework. It has been used for various problems and games, such as Poker, Backgammon and Blackjack. Parts of the code has been used by the author to create bots for fun for Bejeweled and Tetris Battle as well.

Large part of the code has been last time in use in 2010 and hasn't been maintained or tested after that.

Examples
--------

Initial steps before each example:

- Create a new C# console application
- Add the dependencies to the project

### Hello world example
Writes 'hello world' to a running notepad. Dependencies: GR.Interop.Win32 and GR.Input.

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GR.Input;
using GR.Win32;
using System.Threading;
using System.Diagnostics;

namespace notepadtest {
    class Program {
        static void Main(string[] args) {
            Window window = Window.FindWindow("Untitled - Notepad");

            for (int tries = 0; tries < 2 && window == null; tries++) {
                Console.WriteLine("Notepad not found.. launching notepad.");
                Process.Start("notepad.exe");
                Thread.Sleep(1000); // Give 1 second for the notepad to launch and appear in the process list
                window = Window.FindWindow("Untitled - Notepad");
            }

            if (window == null) {
                Console.WriteLine("Couldn't launch notepad or wasn't able to find it. :(");
                return;
            }

            window.SetForeground(); // Set the Notepad window to foreground

            Thread.Sleep(2000); // Wait for 2 seconds before typing

            Keyboard.Write("hello world!");
        }
    }
}
```

Description of the projects
---------------------------

### Bopycat
Play against the AI and the program learns your playstyle: how long it takes you to make moves, how often and when you make undos, etc.

### GR.Common.Logging
ConsoleCapturer can be used to capture the application's console's STDOUT and STDERROR.

### GR.Cryptography
MD5 has a helper method for generating a checksum from a stream.

### GR.Data
PersistantStorage is a simple key-value storage.

### GR.Gambling
Contains abstract classes, VenueClient and GameWindow, which serve as a base for building an automated bot.

VenueClient serves as an abstraction between whatever is managing the overall gameplay and the actual venue program.

### GR.Gambling.Backgammon
Framework for representing and analysing backgammon games.

HintModule abstracts solvers for backgammon. There's a concrete implementation for GnuBg.

### GR.Gambling.Backgammon.Analysis
BoardStatistics can calculate how many rolls a player needs to win.

### GR.Gambling.Backgammon.Conversion
BGConverter converts game state to GnuBg ASCII format and imports from FIBS id string.

### GR.Gambling.Backgammon.HCI
Different models to represent human-like gameplay with time pauses and undos.

NeuralThinker lears from a defined set of actions, which can be recorded with Bopycat.

### GR.Gambling.Backgammon.Utils
GameStateRenderer renders a backgammon position to a Bitmap.

### GR.Gambling.Backgammon.Venue
Classes for abstracting interaction with online backgammon gambling venues.

### GR.Gambling.Bot
Misc classes useful for botting purposes. Allows to create and queue jobs (uses Quartz).

### GR.Imaging
Contains various classes for dealing with image data.

BitmapAnalyzer: methods for analyzing, comparing and modifying FastBitmaps.

BitmapIO: save and load FastBitmaps to and from disk.

BitmapMask: sometimes you need to test if an image is inside another image. BitmapMask stores the image and the location and can test whether it is inside another image.

BucketImage: used to create multiple buckets from a FastBitmap. This is used by an OCR engine.

ColorRange: range of colors from one to another.

FastBitmap: the default Bitmap's .GetPixel() and .SetPixel() methods are very slow because .Net locks the entire Bitmap data on each call. FastBitmap locks the Bitmap data from a regular Bitmap object once, thus pixel manipulation is extremely fast.

ImageFilter: contains a binary filter which converts image data to black and white pixels depending on lumenance.

PixelMask: same as BitmapMask but with a single color as the match data instead.

PixelMaskCollection: a collection of PixelMask to hold and match multiple PixelMasks at once.

Screenshot: for taking screenshots of the entire desktop. It can also capture the mouse cursor.

### GR.Imaging.OCR
OCR engine for pixel-perfect text recognition. It generates its internal matching library on the fly from the given font names and font sizes.

### GR.Input
For simulating keyboard and mouse actions. Mouse can be set to use a click distribution to emulate human like click distribution inside a region. ScarMouse tries to emulate more human-like mouse paths by deviating from a straight line.

### GR.Interop.Win32
ExternalProcess is used for managing external process's memory: reading, writing, allocating and releasing.

User32 and Interop just contain a bunch of imports from different Windows DLLs, mainly User32.dll and gdi32.dll.

Window is wrapper class that can interact with windows through window handles, hWnds. It can query window's properties like title and rectangle,
take a screenshot, bring the window to foreground, move it around, etc. There are also a way to find windows based on their titles.

### GR.IO
PathHelper generates auto-incrementing unique filename in a given folder.

### GR.Net.Mail
Contains a simple GMail client for sending emails.

### GR.Math
Combin and Perm allow enumerating combinations and permutations with integers.

CombinBuilder<T> and PermBuilder<T> allow enumerating combinations and permutations of any type of collection using generics.

Combin, Perm, CombinBuiler and PermBuilder all remember the current state of the enumeration and compute the next combination or permutation based on the current one. PermBuilder uses lexicographic ordering.

Gaussian implements the Box-Muller algorithm for generating normal distribution numbers.

Prime contains very naive and slow method for checking primeness of a number. It also can generate prime numbers.

### GR.Rootkit
Uses a rootkit to hide a running process.