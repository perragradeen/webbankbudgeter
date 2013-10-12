namespace Utilities
{
    // Byt namn
    public class ExcelRowEntry
    {
        private readonly object[] args;

        public ExcelRowEntry(int i, object[] s)
        {
            Row = i;

            args = s;
        }

        // Byt namn till rownumber
        public int Row { get; private set; }

        public object[] Args
        {
            get
            {
                return args;
            }
        }
    }
}