using Microsoft.Win32;

namespace GenMail.Wpf.Services;

public sealed class FileDialogService
{
    public string? PickInputTextFile()
    {
        OpenFileDialog dialog = new()
        {
            Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
            CheckFileExists = true,
            Multiselect = false,
            Title = "Select input text file"
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}
