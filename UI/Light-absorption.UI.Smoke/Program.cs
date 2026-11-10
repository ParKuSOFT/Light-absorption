using System.IO;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Light_absorption.UI;
using Light_absorption.UI.Services;
using Light_absorption.UI.ViewModels;
using OxyPlot.Series;
using OxyPlot.Wpf;

internal static class Program
{
    [STAThread]
    static int Main(string[] args)
    {
        int exitCode = 1;
        var app = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
        string csv = Path.Combine(Path.GetTempPath(), $"light-{Guid.NewGuid():N}.csv");
        app.Startup += async (_, _) =>
        {
            try
            {
                var window = new MainWindow { ShowInTaskbar = false, ShowActivated = false,
                    WindowStartupLocation = WindowStartupLocation.Manual, Left = -16000, Top = -16000 };
                var actual = (MainViewModel)window.DataContext;
                Check(actual.MaxDepth == 2, "Window must create the production view model");
                // Use the real model and CSV writer; replace only the native save dialog.
                var vm = new MainViewModel(new CoreExperimentService(), new Destination(csv));
                window.DataContext = vm;
                window.Show();
                await Idle();
                var calculate = (Button)window.FindName("CalculateButton");
                var export = (Button)window.FindName("ExportButton");
                var slider = (Slider)window.FindName("SampleSlider");
                var input = (Slider)window.FindName("DepthInput");
                var graph = (PlotView)window.FindName("Graph");
                var table = (DataGrid)window.FindName("ResultsTable");
                Check(!export.IsEnabled && calculate.IsEnabled, "Initial command states");
                await Click(calculate, () => vm.HasResult && vm.InputsEnabled);
                Check(vm.Rows.Count == 201 && vm.Selected?.Depth == 2, "Default depth and point count");
                Check(graph.Model == vm.Plot && graph.Model.Series.Count == 3, "Bound RGB plot");
                Check(graph.Model.Series.Cast<OxyPlot.Series.LineSeries>().All(s => s.Points.Count == 201), "Graph point count");
                Check(table.Items.Count == 201 && slider.Maximum == 200, "Bound table and slider");
                slider.Value = 0;
                await Idle();
                Check(vm.Selected is { Depth: 0, R: 1, G: 1, B: 1 }, "Initial white light");
                slider.Value = 100;
                await Idle();
                Check(vm.Selected is { Depth: 1 } r && r.R < r.G && r.G < r.B, "Selection and attenuation");
                if (args.Length == 1)
                {
                    window.UpdateLayout();
                    var content = (FrameworkElement)window.Content;
                    var bitmap = new RenderTargetBitmap((int)Math.Ceiling(content.ActualWidth), (int)Math.Ceiling(content.ActualHeight), 96, 96, PixelFormats.Pbgra32);
                    bitmap.Render(content);
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmap));
                    using var file = File.Create(args[0]);
                    encoder.Save(file);
                }
                await Click(export, () => File.Exists(csv) && vm.InputsEnabled);
                var lines = File.ReadAllLines(csv);
                Check(lines.Length == 202 && lines[0] == "z,R,G,B", "CSV format and row count");
                var last = lines[^1].Split(',').Select(v => double.Parse(v, System.Globalization.CultureInfo.InvariantCulture)).ToArray();
                var final = vm.Rows[^1];
                Check(last.SequenceEqual(new[] { final.Depth, final.R, final.G, final.B }), "Export matches displayed results");
                input.Value = 0;
                await Idle();
                Check(!export.IsEnabled, "Changed inputs invalidate export");
                await Click(calculate, () => vm.Rows.Count == 1 && vm.InputsEnabled);
                Check(vm.Selected is { R: 1, G: 1, B: 1 } && slider.Maximum == 0, "Zero depth");
                var service = new CoreExperimentService();
                foreach (double dz in vm.Steps)
                {
                    var run = service.Calculate(10, dz);
                    Check(run.Rows.Count == (int)Math.Round(10 / dz) + 1 && run.Rows[^1].Depth == 10, "Last physical sample");
                }
                window.Close();
                Console.WriteLine("PASS: WPF window, commands, RGB plot, selection, CSV, zero depth and all step sizes.");
                exitCode = 0;
            }
            catch (Exception ex) { Console.Error.WriteLine(ex); }
            finally { if (File.Exists(csv)) File.Delete(csv); app.Shutdown(); }
        };
        app.Run();
        return exitCode;
    }

    static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    static async Task Idle() => await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);

    static async Task Click(Button button, Func<bool> done)
    {
        Check(button.IsEnabled, $"Disabled button: {button.Content}");
        var peer = new ButtonAutomationPeer(button);
        ((IInvokeProvider)peer.GetPattern(PatternInterface.Invoke)!).Invoke();
        var timer = System.Diagnostics.Stopwatch.StartNew();
        do
        {
            await Task.Delay(15);
            await Idle();
            if (timer.Elapsed.TotalSeconds > 12) throw new TimeoutException(button.Content?.ToString());
        } while (!done());
    }

    private sealed record Destination(string Path) : IExportDestination
    {
        public string ChooseCsvFile() => Path;
    }
}
