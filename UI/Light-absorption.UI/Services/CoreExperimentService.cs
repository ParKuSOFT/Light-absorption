using Light_absorption.Core.Export;
using Light_absorption.Core.Models;
using Light_absorption.Core.Solvers;

namespace Light_absorption.UI.Services;

public sealed class CoreExperimentService : IExperimentService
{
    public RunData Calculate(double maxDepth, double step)
    {
        if (!double.IsFinite(maxDepth) || maxDepth < 0 || maxDepth > 10)
            throw new ArgumentOutOfRangeException(nameof(maxDepth));
        if (!double.IsFinite(step) || step < 0.01 || step > 0.1)
            throw new ArgumentOutOfRangeException(nameof(step));

        var initial = new ExperimentState(new[] { 1d, 1d, 1d });
        var history = new EulerSolver().Solve(initial, maxDepth, step);

        // Core uses a fixed grid and can append a point past maxDepth.
        // Use the same i * step convention as CsvExporter; share this subset with the chart and table.
        int count = (int)Math.Floor((decimal)maxDepth / (decimal)step) + 1;
        if (history.Count > count) history.RemoveRange(count, history.Count - count);
        var rows = history.Select((state, index) => new Reading(index * step,
            state.Values[0], state.Values[1], state.Values[2])).ToArray();
        return new RunData(maxDepth, step, history, rows);
    }

    public void Export(RunData run, string path) => new CsvExporter().ExportToCsv(run.History, run.Step, path);
}
