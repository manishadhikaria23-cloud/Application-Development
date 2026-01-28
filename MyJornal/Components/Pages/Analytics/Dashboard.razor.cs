using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using JournalApp.Services.Interfaces;
using JournalApp.Repositories.Interfaces;

namespace JournalApp.Components.Pages.Analytics
{
    public partial class Dashboard : ComponentBase
    {
        [Inject] public IStreakService StreakService { get; set; } = default!;
        [Inject] public IAnalyticsService Analytics { get; set; } = default!;
        [Inject] public NavigationManager Nav { get; set; } = default!;
        [Inject] public IJournalEntryRepository Repo { get; set; } = default!;

        protected int CurrentStreak { get; set; }
        protected int LongestStreak { get; set; }

        protected DateTime FromDate { get; set; } = DateTime.Today.AddDays(-30);
        protected DateTime ToDate { get; set; } = DateTime.Today;

        protected List<DateTime> MissedDays { get; set; } = new();
        protected Dictionary<string, int> MoodDist { get; set; } = new();
        protected string? MostFrequentMood { get; set; }
        protected Dictionary<string, int> TopTags { get; set; } = new();
        protected List<(DateTime date, int wordCount)> WordTrend { get; set; } = new();

        // Modal state
        protected bool ShowMissedModal { get; set; } = false;
        protected bool ShowCurrentStreakModal { get; set; } = false;
        protected bool ShowLongestStreakModal { get; set; } = false;

        // Detailed streak data
        protected List<DateTime> CurrentStreakDates { get; set; } = new();
        protected List<(DateTime Start, DateTime End, int Length)> LongestStreakRanges { get; set; } = new();

        protected override async Task OnInitializedAsync()
        {
            await LoadAsync();
        }

        protected async Task LoadAsync()
        {
            CurrentStreak = await StreakService.GetCurrentStreakAsync();
            LongestStreak = await StreakService.GetLongestStreakAsync();
            MissedDays = await StreakService.GetMissedDaysAsync(FromDate, ToDate) ?? new List<DateTime>();
            MoodDist = await Analytics.GetMoodCategoryDistributionAsync(FromDate, ToDate) ?? new Dictionary<string, int>();
            MostFrequentMood = await Analytics.GetMostFrequentMoodAsync(FromDate, ToDate);
            TopTags = await Analytics.GetMostUsedTagsAsync(FromDate, ToDate, 10) ?? new Dictionary<string, int>();
            WordTrend = await Analytics.GetWordCountTrendAsync(FromDate, ToDate) ?? new List<(DateTime, int)>();

            // compute detailed streaks from repository entries (for modal display)
            await ComputeStreakDetailsAsync();
        }

        private async Task ComputeStreakDetailsAsync()
        {
            var entries = await Repo.GetAllAsync();
            var dates = entries.Select(e => e.EntryDate.Date).Distinct().OrderBy(d => d).ToList();
            var set = new HashSet<DateTime>(dates);

            // Current streak dates (walk back from today while entries exist)
            CurrentStreakDates = new List<DateTime>();
            var day = DateTime.Today;
            while (set.Contains(day))
            {
                CurrentStreakDates.Add(day);
                day = day.AddDays(-1);
            }

            // Compute all consecutive ranges
            LongestStreakRanges = new List<(DateTime, DateTime, int)>();
            if (dates.Count > 0)
            {
                var rangeStart = dates[0];
                var prev = dates[0];
                for (int i = 1; i < dates.Count; i++)
                {
                    var d = dates[i];
                    if ((d - prev).Days == 1)
                    {
                        prev = d;
                        continue;
                    }

                    var length = (prev - rangeStart).Days + 1;
                    LongestStreakRanges.Add((rangeStart, prev, length));
                    rangeStart = d;
                    prev = d;
                }

                // add final range
                var finalLength = (prev - rangeStart).Days + 1;
                LongestStreakRanges.Add((rangeStart, prev, finalLength));
            }

            // Keep only ranges that match the longest length
            var maxLen = LongestStreakRanges.Any() ? LongestStreakRanges.Max(r => r.Length) : 0;
            if (maxLen > 0)
            {
                LongestStreakRanges = LongestStreakRanges.Where(r => r.Length == maxLen).ToList();
            }
            else
            {
                LongestStreakRanges = new List<(DateTime, DateTime, int)>();
            }
        }

        protected void OpenMissedModal()
        {
            ShowMissedModal = true;
        }

        protected void CloseMissedModal()
        {
            ShowMissedModal = false;
        }

        protected void OpenCurrentStreakModal()
        {
            ShowCurrentStreakModal = true;
        }

        protected void CloseCurrentStreakModal()
        {
            ShowCurrentStreakModal = false;
        }

        protected void OpenLongestStreakModal()
        {
            ShowLongestStreakModal = true;
        }

        protected void CloseLongestStreakModal()
        {
            ShowLongestStreakModal = false;
        }

        // Navigate to add-entry page with the date (adjust route if your add page expects different param)
        protected void CreateEntryForDate(DateTime date)
        {
            Nav.NavigateTo($"/add-entry?date={date:yyyy-MM-dd}");
        }

        // Open existing entry view for a date
        protected void OpenMissedDate(DateTime date)
        {
            Nav.NavigateTo($"/view/{date:yyyy-MM-dd}");
        }

        // Helper types and chart helpers (unchanged)
        // Use explicit hex colors to ensure SVG fills render even if CSS vars are missing
        private readonly Dictionary<string, string> MoodColors = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Positive"] = "#10B981", // emerald green
            ["Neutral"] = "#F59E0B", // amber
            ["Negative"] = "#EF4444"  // red
        };

        private class Slice { public string Path = ""; public string Color = ""; }

        private IEnumerable<Slice> GetDonutSlices(Dictionary<string, int> dist)
        {
            var total = dist?.Values.Sum() ?? 0;
            if (total == 0) yield break;

            double cx = 110, cy = 110, rOuter = 100, rInner = 55;
            double angle = 0;

            foreach (var kv in dist)
            {
                var portion = (double)kv.Value / total;
                var sweep = portion * 360.0;
                var start = angle;
                var end = angle + sweep;
                var path = ArcPath(cx, cy, rOuter, rInner, start, end);
                angle = end;
                var color = MoodColors.GetValueOrDefault(kv.Key, "#0891B2");
                yield return new Slice { Path = path, Color = color };
            }
        }

        private IEnumerable<Slice> GetPieSlices(Dictionary<string, int> dist)
        {
            var total = dist?.Values.Sum() ?? 0;
            if (total == 0) yield break;

            double cx = 110, cy = 110, r = 100;
            double angle = 0;

            static (double x, double y) P(double cx, double cy, double r, double deg)
            {
                var rad = (deg - 90) * Math.PI / 180.0;
                return (cx + r * Math.Cos(rad), cy + r * Math.Sin(rad));
            }

            foreach (var kv in dist)
            {
                var portion = (double)kv.Value / total;
                var sweep = portion * 360.0;
                var start = angle;
                var end = angle + sweep;

                var (sx, sy) = P(cx, cy, r, start);
                var (ex, ey) = P(cx, cy, r, end);
                var largeArc = (end - start) > 180 ? 1 : 0;

                // move to center, line to start, arc to end, close
                var path = $"M {cx:F2},{cy:F2} L {sx:F2},{sy:F2} A {r},{r} 0 {largeArc} 1 {ex:F2},{ey:F2} Z";

                angle = end;

                var color = MoodColors.GetValueOrDefault(kv.Key, "#0891B2");
                yield return new Slice { Path = path, Color = color };
            }
        }

        private record LegendItem(string Label, int Value, string Color);

        private IEnumerable<LegendItem> GetDonutLegend(Dictionary<string, int> dist)
        {
            if (dist == null) yield break;
            foreach (var kv in dist)
            {
                var color = MoodColors.GetValueOrDefault(kv.Key, "#0891B2");
                yield return new LegendItem(kv.Key, kv.Value, color);
            }
        }

        private string ArcPath(double cx, double cy, double outerR, double innerR, double startDeg, double endDeg)
        {
            static (double x, double y) P(double cx, double cy, double radius, double deg)
            {
                var rad = (deg - 90) * Math.PI / 180.0;
                return (cx + radius * Math.Cos(rad), cy + radius * Math.Sin(rad));
            }

            var largeArc = (endDeg - startDeg) > 180 ? 1 : 0;
            var (sx, sy) = P(cx, cy, outerR, startDeg);
            var (ex, ey) = P(cx, cy, outerR, endDeg);
            var (sx2, sy2) = P(cx, cy, innerR, endDeg);
            var (ex2, ey2) = P(cx, cy, innerR, startDeg);

            return $"M {sx:F2},{sy:F2} A {outerR},{outerR} 0 {largeArc} 1 {ex:F2},{ey:F2} L {sx2:F2},{sy2:F2} A {innerR},{innerR} 0 {largeArc} 0 {ex2:F2},{ey2:F2} Z";
        }

        private record BarRect(double X, double Y, double Width, double Height, string Label, int Value, string Color);

        private IEnumerable<BarRect> GetBarRects(Dictionary<string, int> topTags, double svgWidth, double svgHeight, double margin)
        {
            if (topTags == null) yield break;
            var items = topTags.ToList();
            var count = items.Count;
            if (count == 0) yield break;
            var max = Math.Max(1, items.Max(i => i.Value));
            var gap = 8;
            var totalGaps = (count + 1) * gap;
            var availWidth = svgWidth - totalGaps;
            var barW = availWidth / count;
            for (int i = 0; i < count; i++)
            {
                var x = gap + i * (barW + gap);
                var h = (items[i].Value / (double)max) * svgHeight;
                var y = svgHeight - h + margin;
                var color = i % 2 == 0 ? "var(--accent)" : "var(--accent2)";
                yield return new BarRect(x, y, barW, h, items[i].Key, items[i].Value, color);
            }
        }

        private record SparkResult(string Points, string Area, string Stroke, string Fill);

        private SparkResult GetSparklinePoints(List<(DateTime date, int wordCount)> rows, double width, double height, double padding)
        {
            if (rows == null || rows.Count == 0) return new SparkResult("", "", "var(--accent)", "var(--accent2)");
            var values = rows.Select(r => r.wordCount).ToList();
            var min = values.Min();
            var max = Math.Max(min + 1, values.Max());
            var count = values.Count;
            var xStep = (width - 2 * padding) / Math.Max(1, count - 1);
            var points = new List<string>();
            for (int i = 0; i < count; i++)
            {
                var x = padding + i * xStep;
                var y = padding + (1 - (values[i] - min) / (double)(max - min)) * (height - 2 * padding);
                points.Add($"{x:F2},{y:F2}");
            }
            var pts = string.Join(" ", points);
            var area = $"M {padding:F2},{height - padding:F2} L {pts} L {width - padding:F2},{height - padding:F2} Z";
            return new SparkResult(pts, area, "var(--accent)", "var(--accent2)");
        }
    }
}