namespace StaticLeakLab.Views
{
    /// <summary>One line of the leak dashboard: a label, a value and whether it looks wrong.</summary>
    public readonly struct DashboardRow
    {
        public readonly string Label;
        public readonly string Value;
        public readonly bool IsAlarm;
        public readonly bool IsHeader;

        public DashboardRow(string label, string value, bool isAlarm = false)
        {
            Label = label;
            Value = value;
            IsAlarm = isAlarm;
            IsHeader = false;
        }

        private DashboardRow(string header)
        {
            Label = header;
            Value = string.Empty;
            IsAlarm = false;
            IsHeader = true;
        }

        public static DashboardRow Header(string text) => new DashboardRow(text);
    }
}
