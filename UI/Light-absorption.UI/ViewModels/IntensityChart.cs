using Light_absorption.UI.Services;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Annotations;

namespace Light_absorption.UI.ViewModels;

internal sealed class IntensityChart
{
    public PlotModel Model { get; } = new() { PlotAreaBorderColor = OxyColor.FromRgb(202, 211, 218), DefaultFont = "Segoe UI", DefaultFontSize = 12 };
    private readonly LineAnnotation cursor = new() { Type = LineAnnotationType.Vertical, Color = OxyColors.SlateGray, LineStyle = LineStyle.Dot };

    public IntensityChart(IReadOnlyList<Reading> rows, double maxDepth)
    {
        Model.Axes.Add(Axis(AxisPosition.Bottom, "Глубина z, м", Math.Max(0.1, maxDepth)));
        Model.Axes.Add(Axis(AxisPosition.Left, "Относительная интенсивность", 1.05));
        Add("R", "#BA3945", LineStyle.Solid, r => r.R);
        Add("G", "#18805A", LineStyle.Dash, r => r.G);
        Add("B", "#2665AC", LineStyle.DashDot, r => r.B);
        if (rows.Count > 0) Model.Annotations.Add(cursor);

        void Add(string label, string color, LineStyle style, Func<Reading, double> value)
        {
            var series = new LineSeries { Title = label, Color = OxyColor.Parse(color), StrokeThickness = 2.5, LineStyle = style,
                TrackerFormatString = "{0}\nГлубина: {2:0.00} м\nИнтенсивность: {4:0.0000}" };
            if (rows.Count == 1) { series.MarkerType = MarkerType.Circle; series.MarkerSize = 3; }
            foreach (var row in rows) series.Points.Add(new DataPoint(row.Depth, value(row)));
            Model.Series.Add(series);
        }
    }

    private static LinearAxis Axis(AxisPosition position, string title, double maximum) => new()
    {
        Position = position, Title = title, Minimum = 0, Maximum = maximum, IsZoomEnabled = false,
        IsPanEnabled = false, MajorGridlineStyle = LineStyle.Solid, MajorGridlineColor = OxyColor.FromRgb(231, 236, 240)
    };

    public void Select(double depth)
    {
        cursor.X = depth;
        Model.InvalidatePlot(false);
    }
}
