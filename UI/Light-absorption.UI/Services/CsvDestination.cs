using Microsoft.Win32;
using System.Windows;

namespace Light_absorption.UI.Services;

internal sealed class CsvDestination : IExportDestination
{
    public string? ChooseCsvFile()
    {
        var dialog = new SaveFileDialog
        {
            Title = "Сохранить результаты эксперимента", FileName = "light-absorption.csv",
            Filter = "Таблица CSV (*.csv)|*.csv", DefaultExt = ".csv", AddExtension = true,
            OverwritePrompt = true
        };
        return dialog.ShowDialog(Application.Current.MainWindow) == true ? dialog.FileName : null;
    }
}
