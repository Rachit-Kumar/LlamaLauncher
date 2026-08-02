# 🦙 LlamaLauncher

A modern, standalone Windows GUI launcher and process manager for [llama.cpp](https://github.com/ggerganov/llama.cpp)'s `llama-server.exe`.  
Launch, configure, inspect, and manage local AI model servers with zero hassle — featuring automatic CUDA detection, real-time VRAM estimation, GGUF metadata parsing, and 1-click server downloading.

---

## 🌟 Key Features

- **⚡ 1-Click `llama-server.exe` Auto-Downloader & Extractor**  
  Queries GitHub releases API, automatically matches your hardware (`CUDA 12.x`, `CUDA 11.x`, or `CPU AVX2`), prompts for confirmation, and extracts `llama-server.exe` into a managed `llama-bin/` directory with progress tracking and cancel controls.
- **🔍 Windows 11 CUDA & GPU Hardware Inspector**  
  Detects your NVIDIA GPU model name (e.g. *GeForce RTX 4080 / RTX 3070*), installed CUDA version, and available VRAM.
- **📊 Binary GGUF Header Parser**  
  Inspects `.gguf` files to extract Architecture (`llama`, `qwen2`), Quantization (`Q4_K_M`, `Q8_0`), Layer count, and File Size without loading model weights into system RAM.
- **⚙️ Dynamic VRAM Estimator & Health Badge**  
  Calculates required VRAM (Model Size + KV Cache) based on offload layers (`-ngl`), context size, and Flash Attention (`-fa`), providing visual health indicators (`✓ ~X.X GB VRAM`, `⚡ High Memory Risk`).
- **🎛️ Context Size Presets (8K to 512K)**  
  Dropdown presets ranging from `8K` (`8,192`), `16K`, `32K`, `64K`, `128K`, `256K`, up to `512K` (`524,288 / Half Million`), alongside custom numerical overrides.
- **🚩 Common CLI Flag Toggles**  
  One-click checkboxes for Flash Attention (`-fa`), Disable mmap (`--no-mmap`), Lock RAM (`--mlock`), Embedding Endpoint (`--embedding`), and Disable Thinking / Reasoning (`--chat-template-kwargs "{\"reasoning_format\":\"none\"}"`).
- **🌐 1-Click Web UI Launcher**  
  Launches `http://localhost:<port>` directly in your default browser when the server is running.
- **📋 Non-Blocking Log Console & System Tray**  
  Periodic 50ms batch log renderer with line buffer truncation, tray icon state indicators, and startup tray minimization.
- **🛡️ Win32 Job Object Lifecycle Safety**  
  Child processes are attached to a Win32 Job Object with `JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE`, guaranteeing zero orphaned server processes if the app closes unexpectedly.

---

## 📖 Basic User Guide

### 1. Installation & Running
1. Download **`LlamaLauncher-v1.2.0-win-x64.zip`** from the [GitHub Releases](https://github.com/Rachit-Kumar/LlamaLauncher/releases) page.
2. Extract the archive to any folder on your PC.
3. Double-click **`LlamaLauncher.exe`**.

### 2. Auto-Installing `llama-server.exe`
1. Click **⚙ Settings** in the top right header.
2. View your detected GPU & CUDA information in the settings panel.
3. Click **⚡ 1-Click Download & Extract**.
4. Confirm the prompt — LlamaLauncher will download and extract the matching `llama-server.exe` binary into `llama-bin/` and configure the executable path automatically.

### 3. Creating & Managing Model Profiles
1. Click **Manage Models…** ➔ **Add**.
2. Enter a **Profile Name** (e.g. `Qwen 2.5 Coder 7B`).
3. Click **Browse…** to select your `.gguf` model file.
4. Select a **Context Size Preset** (e.g. `16,384 (16K)` or `32,768 (32K)`).
5. Configure GPU Layers (`-ngl`), Port (default `8080`), and check any desired flags (e.g. `⚡ Flash Attn`).
6. Click **OK** to save.

### 4. Running a Model Server
1. Select your profile from the main window dropdown menu.
2. Click **▶ Start Server**.
3. Once running, click **🌐 Web UI** to launch llama.cpp's built-in web browser UI (`http://localhost:<port>`).
4. Click **📋 Log Panel** to toggle the live console output stream.
5. Click **■ Stop Server** when finished.

---

## 🛠️ Developer Guide (Building from Source)

### Prerequisites
- Windows 10 or 11 (x64)
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Building & Publishing
```powershell
# Debug build (for local testing)
dotnet build LlamaLauncher.csproj -c Debug

# Publish standalone single-file executable (Release)
dotnet publish LlamaLauncher.csproj -c Release
# Binary location: bin\Release\net8.0-windows\win-x64\publish\LlamaLauncher.exe
```

---

## 📁 File Structure & Data Portability

LlamaLauncher is 100% portable and stores all configurations locally inside its directory:

| File / Folder | Description |
|---|---|
| `LlamaLauncher.exe` | Main executable application |
| `profiles.json` | Saved model profiles and flag configurations |
| `settings.json` | App settings, last-used profile, and server path |
| `llama-bin/` | Directory storing auto-downloaded `llama-server.exe` and version tags |

No Windows Registry modifications or hidden AppData folders are created.

---

## 📜 License

MIT License — free for personal, open-source, and commercial use.
