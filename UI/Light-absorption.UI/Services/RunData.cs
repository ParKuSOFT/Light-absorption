using Light_absorption.Core.Models;
using System.Windows.Media;

namespace Light_absorption.UI.Services;

public sealed record Reading(double Depth, double R, double G, double B)
{
    // Display-only mapping of relative intensities to an RGB swatch.
    private static byte Channel(double value) => (byte)Math.Round(Math.Clamp(value, 0, 1) * 255);
    public Color SampleColor => Color.FromRgb(Channel(R), Channel(G), Channel(B));
}

public sealed record RunData(double MaxDepth, double Step, List<ExperimentState> History, IReadOnlyList<Reading> Rows);

public interface IExperimentService
{
    RunData Calculate(double maxDepth, double step);
    void Export(RunData run, string path);
}

public interface IExportDestination
{
    string? ChooseCsvFile();
}
