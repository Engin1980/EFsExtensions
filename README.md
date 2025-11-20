# EFsExtensions

*A set of tools for Microsoft Flight Simulator (MSFS) 2020*

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

## 🔧 What is EFsExtensions?

EFsExtensions is a collection of extensions and tools designed for integration with Microsoft Flight Simulator (MSFS). It includes modules such as:

* **Checklist Reader** – in-sim checklist display
* **Copilot Announcements** – AI copilot voice notifications
* **Failures** – random aircraft system failures simulation
* **RaaS (Repair as a Service)** – maintenance and repair management system
* **Flight Log** - a simple flight log with many statistical data (departure/arrival delay, fuel consumption, real touchdown rate, ...)

> **Work in Progress:** The project is under active development. Feedback and contributions are welcome.

## 📚 Documentation

Full documentation is available at:
[https://engin.gitbook.io/efs-extensions-wiki/](https://engin.gitbook.io/efs-extensions-wiki/) The documentation is also **work in progress**.

## ✅ Requirements

* Microsoft Flight Simulator 2020 (2024 is not tested yet)
* .NET Framework / .NET Core 8.0

## 🚀 Installation

Will be released.
If you are interested in pre-testing, please contact me.

## 🧩 Modules Overview

Below is a summary of key modules included in the repository:

| Module                 | Description                                               |
| ---------------------- | --------------------------------------------------------- |
| ChecklistReader      | Reads checklists directly inside MSFS. The reading is invoked manually, or by an event (e.g., parking brake release). The checklist content is fully customizable via XML files.     |
| CopilotAnnouncements | Generates copilot voice messages based on scenario logic, e.g. acceleration/decceleration speed announcements, FL 100 pass announcements, etc. Announcements are fully configurable via XML files. |
| Failures             | Simulates random aircraft system failures (for all planes in general). In progress, needs deeper research. Some failures are a simple tweaks over the airplane (but better than nothing, as FS2020 support in this field is tragic). Failures are configurable via XML files.               |
| RaaS                 | Provides *Runway Awareness and Advisory system* announcing approaching (in taxi) or landing on the runway. Basic functionality, in progress. |
| Flight Log | Provides simple flight log for completed flights. Captures basic properties, like flight length, departure/destination location/time, delays, touchdown speed/angle/yaw, rollout and take-off distance etc.

## 🧪 Usage Example

Simply start the application on the computer with FS2020 running. Network connection is not supported (yet, at least).

## 🐛 Issues & Contributions

* Found a bug or have an idea? Please open an **Issue** on GitHub.
* Pull requests are welcome! To contribute:

  1. Fork the repository.
  2. Create a feature branch (e.g., `feature-myNewModule`).
  3. Add tests and update the documentation.
  4. Submit a Pull Request.
* For contribution rules, see [CONTRIBUTING.md](./CONTRIBUTING.md) (if available).

## ℹ️ FAQ

Nothing here so far.

## 📝 License

This project is licensed under the MIT License — see [LICENSE](./LICENSE) for details.

## ✉️ Contact

Author: **Marek Vajgl** – [GitHub Profile](https://github.com/Engin1980)

Thanks for your interest and contributions to this project!
