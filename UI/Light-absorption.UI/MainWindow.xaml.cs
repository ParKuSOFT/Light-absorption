using System.Windows;
using Light_absorption.UI.Services;
using Light_absorption.UI.ViewModels;

namespace Light_absorption.UI;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel(new CoreExperimentService(), new CsvDestination());
    }
}
