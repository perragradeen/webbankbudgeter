namespace Budgeter.Core.Entities
{
    public class LoadOrSaveResult
    {
        public int SkippedOrSaved { get; set; }
        public bool SomethingLoadedOrSaved { get; set; }
    }
}