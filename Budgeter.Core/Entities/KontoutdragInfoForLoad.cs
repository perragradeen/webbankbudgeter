namespace Budgeter.Core.Entities
{
    public class KontoutdragInfoForLoad : KontoutdragInfoForSave
    {
        public string FilePath { get; set; }

        public bool ClearContentBeforeReadingNewFile { get; set; }
        public bool SomethingChanged { get; set; }
    }
}