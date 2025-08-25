namespace WaxWorx.UI.ViewModels
{
    public class ConditionViewModel
    {
        public string ConditionGroup { get; set; } // e.g. "VG+"
        public string ConditionName { get; set; } // e.g. "VG+"
        public int AlbumCount { get; set; }
        public double PercentageOfTotal { get; set; }
    }
}
