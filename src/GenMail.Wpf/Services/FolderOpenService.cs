using System.Diagnostics;
using System.IO;

namespace GenMail.Wpf.Services;

public sealed class FolderOpenService
{
    public void OpenFolder(string folderPath)
    {
        if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
        {
            return;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = folderPath,
            UseShellExecute = true
        });
    }
}
