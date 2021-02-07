using System;

namespace Utilities
{
    /// <summary>
    /// Summary description for TestInfo.
    /// </summary>
    public class TestInfo : Attribute
    {
        public string[] Columns { get; }
        public string Description { get; }
        public string InfoText { get; }

        public TestInfo(string description, string infoText, params string[] columns)
        {
            Description = description;
            InfoText = infoText;
            Columns = columns;
        }
    }
}