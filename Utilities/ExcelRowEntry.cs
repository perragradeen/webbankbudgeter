namespace Utilities
{
    /// <summary>
    /// Represents a single row from an Excel file
    /// </summary>
    public class ExcelRowEntry
    {
        public ExcelRowEntry(object[] args)
        {
            Args = args;
        }

        public object[] Args { get; }
    }
}

