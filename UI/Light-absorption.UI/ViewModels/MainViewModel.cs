using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Light_absorption.UI.Services;
using OxyPlot;
using System.IO;

namespace Light_absorption.UI.ViewModels;

public sealed class MainViewModel : ObservableObject
{
    private readonly IExperimentService service;
    private readonly IExportDestination destination;
    private RunData? run;
    private double maxDepth = 2, step = 0.01;
    private int selectedIndex;
    private bool busy;
    private string status = "Задайте глубину и нажмите «Рассчитать».";
    private IntensityChart chart = new(Array.Empty<Reading>(), 2);

    public MainViewModel(IExperimentService service, IExportDestination destination)
    {
        this.service = service;
        this.destination = destination;
        CalculateCommand = new AsyncRelayCommand(Calculate, () => !busy);
        ExportCommand = new AsyncRelayCommand(Export, () => !busy && HasCurrentRun);
    }

    public IAsyncRelayCommand CalculateCommand { get; }
    public IAsyncRelayCommand ExportCommand { get; }
    public IReadOnlyList<double> Steps { get; } = Array.AsReadOnly(new[] { 0.01, 0.02, 0.05, 0.1 });
    public double MaxDepth { get => maxDepth; set { if (SetProperty(ref maxDepth, value)) InputsChanged(); } }
    public double Step { get => step; set { if (SetProperty(ref step, value)) InputsChanged(); } }
    public bool InputsEnabled => !busy;
    public bool HasResult => run != null;
    private bool HasCurrentRun => run != null && run.MaxDepth == MaxDepth && run.Step == Step;
    public string Status { get => status; private set => SetProperty(ref status, value); }
    public PlotModel Plot => chart.Model;
    public IReadOnlyList<Reading> Rows => run?.Rows ?? Array.Empty<Reading>();
    public int LastIndex => Math.Max(0, Rows.Count - 1);
    public int SelectedIndex
    {
        get => selectedIndex;
        set { if (value >= 0 && value <= LastIndex && SetProperty(ref selectedIndex, value)) SelectionChanged(); }
    }
    public Reading? Selected => Rows.Count == 0 ? null : Rows[selectedIndex];

    private void SelectionChanged()
    {
        OnPropertyChanged(nameof(Selected));
        if (Selected != null) chart.Select(Selected.Depth);
    }

    private void InputsChanged()
    {
        if (run != null) Status = HasCurrentRun ? $"Расчёт готов. Точек: {Rows.Count}." : "Параметры изменены. На экране предыдущий расчёт.";
        ExportCommand.NotifyCanExecuteChanged();
    }

    private void SetBusy(bool value)
    {
        busy = value;
        OnPropertyChanged(nameof(InputsEnabled));
        CalculateCommand.NotifyCanExecuteChanged();
        ExportCommand.NotifyCanExecuteChanged();
    }

    private async Task Calculate()
    {
        var depth = MaxDepth;
        var dz = Step;
        if (!double.IsFinite(depth) || depth < 0 || depth > 10 || !Steps.Contains(dz))
        {
            Status = "Проверьте глубину (от 0 до 10 м) и шаг расчёта.";
            return;
        }
        SetBusy(true);
        try
        {
            Status = "Выполняется расчёт…";
            var next = await Task.Run(() => service.Calculate(depth, dz));
            var nextChart = new IntensityChart(next.Rows, depth);
            run = next;
            chart = nextChart;
            selectedIndex = LastIndex;
            OnPropertyChanged(nameof(Rows));
            OnPropertyChanged(nameof(HasResult));
            OnPropertyChanged(nameof(LastIndex));
            OnPropertyChanged(nameof(SelectedIndex));
            OnPropertyChanged(nameof(Plot));
            SelectionChanged();
            Status = $"Расчёт завершён. Точек: {Rows.Count}.";
        }
        catch (Exception ex) { Status = "Не удалось выполнить расчёт: " + ex.Message; }
        finally { SetBusy(false); }
    }

    private async Task Export()
    {
        if (run is not { } snapshot || !HasCurrentRun) return;
        SetBusy(true);
        try
        {
            var path = destination.ChooseCsvFile();
            if (path == null) return;
            await Task.Run(() => service.Export(snapshot, path));
            Status = $"CSV сохранён: {Path.GetFileName(path)}";
        }
        catch (Exception ex) { Status = "Не удалось сохранить CSV: " + ex.Message; }
        finally { SetBusy(false); }
    }
}
