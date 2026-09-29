<div id="top"></div>

[![Contributors][contributors-shield]][contributors-url]
[![Forks][forks-shield]][forks-url]
[![Stargazers][stars-shield]][stars-url]
[![Issues][issues-shield]][issues-url]
[![GNU Affero General Public License v3.0 License][license-shield]][license-url]

<div align="center">
  <h3 align="center">WinUia</h3>
  <p align="center">
    A .NET library for automating Windows applications through Microsoft UI Automation.
    <br />
    <a href="https://github.com/JelleBuning/win-uia/issues">Report Bug</a>
    ·
    <a href="https://github.com/JelleBuning/win-uia/issues">Request Feature</a>
  </p>
</div>

<!-- TABLE OF CONTENTS -->
<details>
  <summary>Table of Contents</summary>
  <ol>
    <li>
      <a href="#about-the-project">About The Project</a>
      <ul>
        <li><a href="#features">Features</a></li>
        <li><a href="#built-with">Built With</a></li>
      </ul>
    </li>
    <li>
      <a href="#getting-started">Getting Started</a>
      <ul>
        <li><a href="#installation">Installation</a></li>
      </ul>
    </li>
    <li>
      <a href="#usage">Usage</a>
      <ul>
        <li><a href="#limitations">Limitations</a></li>
      </ul>
    </li>
    <li><a href="#contributing">Contributing</a></li>
    <li><a href="#license">License</a></li>
  </ol>
</details>



## About The Project

WinUia is a .NET library for inspecting and automating Windows applications through the Microsoft UI Automation framework.

### Features

* **No dependencies:** talks to the native UIA3 COM API (`UIAutomationCore.dll`) through hand-written interop. No FlaUI, no interop packages.
* **Works across UI stacks:** WinForms, WPF, WinUI 3 (packaged and unpackaged), UWP and classic Win32 apps, x86 or x64.
* **Self-healing elements:** an element remembers how it was found and re-finds itself when a re-render makes it stale.
* **Waiting built in:** `Find` polls until the element appears (default 5 s), `TryFind` returns `null` instead of throwing.
* **Patterns first, mouse last:** `Click()` and `SetValue()` use UIA patterns (Invoke, Toggle, SelectionItem, ExpandCollapse, Value) and only fall back to `SendInput`.

### Built With

* [.NET 10](https://dotnet.microsoft.com/)
* [Microsoft UI Automation](https://learn.microsoft.com/windows/win32/winauto/entry-uiauto-win32)

## Getting Started
Setting up WinUia on your local machine is straightforward. Make sure the [.NET 10 SDK](https://dotnet.microsoft.com/download) is installed.

### Installation

1.  **Clone the repository:**

    ```bash
    git clone https://github.com/JelleBuning/win-uia.git
    cd win-uia
    ```

2.  **Build the solution:**

    ```bash
    dotnet build WinUia.slnx
    ```

3.  **Run the tests:**

    ```bash
    dotnet test WinUia.slnx
    ```

    The UI tests (`Category=UI`) launch the bundled `WinUia.Winforms` and need an interactive desktop. Run only the others with `dotnet test WinUia.slnx --filter "Category!=UI"`.

## Usage

```csharp
using WinUia;

using var app = Automation.Launch(@"C:\path\to\MyApp.exe");   // or Automation.LaunchPackaged("Publisher.App_hash!App"), Automation.Attach(pid)

app.Find("txtName").SetValue("Ada");                     // by AutomationId, then by Name
app.Find("btnSave").Click();

var status = app.MainWindow.FindByAutomationId("lblStatus", TimeSpan.FromSeconds(10));
Console.WriteLine(status.Name);
// Disposing a launched app closes it (Window pattern first, then kill).
```

### Project structure

WinUia is a modular monolith: one solution, one project per module, and one test project per module. Dependencies point one way, from the application layer down to the platform:

| Module | Layer | Contents | Depends on |
|---|---|---|---|
| `WinUia` | Application | `Automation` (page objects over a running app), `WinUia.Launchers` (`AppLauncher`, packaged-app activation) | `WinUia.Core` (public API only) |
| `WinUia.Core` | Domain | `AutomationContext`, `WinUia.Core.Elements` (`Element`, lambda searches such as `Find(e => e.Name == "OK")`), `WinUia.Core.Patterns`, `WinUia.Core.Exceptions`, the UIA COM interop | `WinUia.Input` |
| `WinUia.Input` | Platform | `Win32InputSimulator` (`SendInput`), DPI scope, native button clicks | — |
| `WinUia.NUnit` | Test integration | `[UiTest]`: one desktop per test, across test processes | NUnit |

Module rules:

* Each module's `Interop/` folder is private to that module. Other modules use what it offers (`PhysicalDpi`, `NativeButton`), never its P/Invoke or COM declarations.
* `WinUia` uses Core's public API only, the same API an application built on WinUia gets.
* Internals are shared only through `InternalsVisibleTo`, and only with the module's own `<Module>.UnitTests` project, plus `WinUia.Input` with `WinUia.Core`.
* One type per file.

Tests live next to their module as `tests/<Module>.UnitTests` and share `examples/WinUia.Winforms`, a WinForms fixture app (tested itself by `examples/WinUia.Winforms.Tests`), located through `tests/WinUia.Testing.Shared`. Tests use NUnit.

### Limitations

* **MTA only:** UI Automation must be called from an MTA thread. Creating an `AutomationContext` or `Automation` on an STA thread (for example a WinForms/WPF UI thread, or an `[STAThread]` `Main`) throws. Use `Task.Run` or a thread-pool thread.
* **Physical input needs a real desktop:** the mouse and keyboard fallbacks use `SendInput`, which does nothing on a locked workstation or a disconnected RDP session, and goes to whatever window is on top.
* **Elevation (UIPI):** a non-elevated process cannot send input to, and has limited UIA access to, an elevated app. Run the automation elevated when the target is.
* **DPI:** creating an `AutomationContext` makes the process per-monitor DPI aware (if it is not already) so UIA coordinates and `SendInput` both use physical pixels.

<!-- CONTRIBUTING -->
## Contributing

Contributions are what make the open source community such an amazing place to learn, inspire, and create. Any contributions you make are **greatly appreciated**.

If you have a suggestion that would make this better, please fork the repo and create a pull request. You can also simply open an issue with the tag "enhancement".
Don't forget to give the project a star! Thanks again!

1. Fork the Project
2. Create your Feature Branch (`git checkout -b features/feature-title`)
3. Commit your Changes (`git commit -m 'Added feature'`)
4. Push to the Branch (`git push origin features/feature-title`)
5. Open a Pull Request


<!-- LICENSE -->
## License
Distributed under the GNU Affero General Public License v3.0 License. See `LICENSE` for more information.


<p align="right">(<a href="#top">back to top</a>)</p>



<!-- MARKDOWN LINKS & IMAGES -->
<!-- https://www.markdownguide.org/basic-syntax/#reference-style-links -->
[contributors-shield]: https://img.shields.io/github/contributors/JelleBuning/win-uia.svg?style=for-the-badge
[contributors-url]: https://github.com/JelleBuning/win-uia/graphs/contributors
[forks-shield]: https://img.shields.io/github/forks/JelleBuning/win-uia.svg?style=for-the-badge
[forks-url]: https://github.com/JelleBuning/win-uia/network/members
[stars-shield]: https://img.shields.io/github/stars/JelleBuning/win-uia.svg?style=for-the-badge
[stars-url]: https://github.com/JelleBuning/win-uia/stargazers
[issues-shield]: https://img.shields.io/github/issues/JelleBuning/win-uia.svg?style=for-the-badge
[issues-url]: https://github.com/JelleBuning/win-uia/issues
[license-shield]: https://img.shields.io/github/license/JelleBuning/win-uia.svg?style=for-the-badge
[license-url]: https://github.com/JelleBuning/win-uia/blob/main/LICENSE
