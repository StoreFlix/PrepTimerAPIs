namespace PrepTimerAPIs.Models
{
    // Per-company changes to shared (CompanyId = 1) items. Phase 1 only uses IsHidden.
    public class PTCompanyItemOverride
    {
        public int CompanyId { get; set; }
        public int ItemId { get; set; }
        public bool IsHidden { get; set; }
        public int? Duration { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
    }
}
