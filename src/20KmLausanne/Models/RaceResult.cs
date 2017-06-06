using System;
using System.Collections.Generic;
using System.Text;

namespace Lausanne20Km.Models
{
    public class RaceResult
    {
        public string year { get; set; }
        public string age { get; set; }

        public Participant participant { get; set; }

        public string categorie { get; set; }
        public string rang { get; set; }        
        public string equipe_ou_lieu { get; set; }
        public string temps { get; set; }

        public string temps_partiel_1 { get; set; }
        public string temps_partiel_2 { get; set; }

        public RaceResult()
        {
            // empty constructor
        }

        public TimeSpan? GetTotalTimeAsTimeSpan()
            => ParseAsTimeSpan(this.temps);

        public static TimeSpan? ParseAsTimeSpan(string input)
        {
            if (input.Contains("---"))
                return null;

            if (input.Contains(":"))
                return TimeSpan.ParseExact(input, "h\\:mm\\.ss", null);
            else
                return TimeSpan.ParseExact(input, "mm\\.ss", null);
        }

        public Gender GetGender()
            => (this.categorie.Contains("H") ? Gender.Male : Gender.Female);

        public bool IsDistance(int distance)
            => this.categorie.StartsWith(distance.ToString());

        public bool IsValidAge()
            => !string.IsNullOrEmpty(this.age);

        public bool IsValidTime() 
            => this.temps != "---";
    }
}
