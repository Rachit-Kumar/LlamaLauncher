# 🦙 LlamaLauncher

A modern, lightweight Windows GUI launcher & process manager for [llama.cpp](https://github.com/ggerganov/llama.cpp)'s `llama-server.exe`.  
Start and stop local AI model servers with a single click — featuring automatic CUDA detection, VRAM estimations, GGUF metadata inspection, and 1-click `llama-server.exe` downloading.

---

## 🌟 Key Features

- **⚡ 1-Click `llama-server.exe` Downloader & Extractor** — Automatically detects your hardware (`CUDA 12.x`, `CUDA 11.x`, or `CPU AVX2`), downloads the matching release zip from GitHub, and extracts `llama-server.exe` into a managed directory.
- **🔍 Windows 11 CUDA & GPU Inspector** — Automatically detects your NVIDIA GPU model, CUDA driver version, and VRAM capacity.
- **📊 GGUF Model Header Inspector** — Reads binary GGUF headers to display Architecture, Quantization type (`Q4_K_M`, `Q8_0`), Layer count, and file size without loading weights into RAM.
- **⚙️ VRAM Estimator & Health Indicator** — Real-time VRAM requirement calculation based on offload layers (`-ngl`), context size, and Flash Attention (`-fa`).
- **🎛️ Context Size Presets (8K to 512K)** — Quick dropdown for presets: `8K`, `16K`, `32K`, `64K`, `128K`, `256K`, `512K (Half Million)`, and `Custom...`.
- **🚩 Common CLI Flag Checkboxes** — Interactive toggles for Flash Attention (`-fa`), Disable mmap (`--no-mmap`), Lock RAM (`--mlock`), Embedding API (`--embedding`), and Disable Thinking (`--chat-template-kwargs`).
- **🌐 Web UI Launcher** — 1-click button to open `http://localhost:<port>` directly in your default browser.
- **📋 Live Log Console & System Tray** — Async non-blocking log output and tray icon status indicator.
- **🛡️ Process Safety** — Process lifecycle bound to Win32 Job Object (`JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE`), guaranteeing no orphaned processes if the launcher closes.

---

## 🚀 Getting Started

### 1. Download & Build
```powershell
# Build debug binary
dotnet build LlamaLauncher.csproj -c Debug

# Build self-contained release executable
dotnet publish LlamaLauncher.csproj -c Release
# Output: bin\Release\net8.0-windows\win-x64\publish\LlamaLauncher.exe
```

### 2. First-Time Setup
1. Run `LlamaLauncher.exe`.
2. Click **⚙ Settings** ➔ Click **⚡ 1-Click Download & Extract** to automatically download `llama-server.exe` matched to your GPU (or browse to an existing `llama-server.exe`).
3. Select or create a **Profile** with your GGUF model file.
4. Click **▶ Start Server**!

---

## 📁 Storage & Portability

LlamaLauncher stores settings locally in the application folder:
- `profiles.json` — Saved model profiles & presets
- `settings.json` — App settings and path configuration
- `llama-bin/` — Managed directory for `llama-server.exe`

No registry changes or external dependencies required.

---

## 📜 License

MIT License — free for personal and commercial use.
