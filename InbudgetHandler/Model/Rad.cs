namespace InbudgetHandler.Model
{
    public class Rad
    {
        /// <summary>
        /// Ex. kategori "el"
        /// </summary>
        public string RadNamnY { get; set; }

        /// <summary>
        /// Ex. {2020-03, 150}, {2020-04, 2500}
        /// </summary>
        public Dictionary<string, double> Kolumner { get; set; } = new Dictionary<string, double>();

        public override string ToString()
        {
            return RadNamnY + " " + Kolumner.Count;
        }
    }
}