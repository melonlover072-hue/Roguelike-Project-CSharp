using System;
using System.Windows.Forms;
using RoguelikeSkeleton.Rendering;

namespace RoguelikeSkeleton;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        Application.SetHighDpiMode(HighDpiMode.SystemAware);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new GameForm());
    }
}
