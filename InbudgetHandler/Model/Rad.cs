using System.Collections.Generic;

namespace InbudgetHandlle.Model
{
    public class Rad
    {
        /// <summary>
        /// Ex. kategori "el"
        /// </summary>
        public string RadNamnY { get; set; }

        /// <summary>
        /// Ex. 150, 2500, 10000
        /// </summary>
        public Dictionary<string, double> Kolumner { get; set; } = new Dictionary<string, double>();

        public override string ToString()
        {
            return RadNamnY + " " + Kolumner.Count;
        }
    }
}
