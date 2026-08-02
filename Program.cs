using LlamaLauncher.Forms;

namespace LlamaLauncher;

internal static class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// [STAThread] is CRITICAL — OpenFileDialog and other COM-based Windows
    /// dialogs require a Single-Threaded Apartment. Without it, file browse
    /// dialogs hang or appear behind other windows, freezing the app.
    /// Top-level statements do NOT apply [STAThread], which is why this must
    /// be an explicit Main method.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // Single-instance guard: prevents running two copies simultaneously
        // (which would cause port conflicts and VRAM double-loading)
        using var mutex = new System.Threading.Mutex(true, "LlamaLauncher_SingleInstance", out bool isFirstInstance);

        if (!isFirstInstance)
        {
            MessageBox.Show(
                "LlamaLauncher is already running.\n\nCheck the system tray.",
                "Already Running", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}
