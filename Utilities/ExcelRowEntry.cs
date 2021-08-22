namespace Utilities
{
    // Byt namn
    public class ExcelRowEntry
    {
        public ExcelRowEntry(object[] s)
        {
            Args = s;
        }

        public object[] Args { get; }
    }
}