using System;
using System.Collections.Generic;
using System.Text;

namespace Lausanne20Km.Models
{
    public struct Participant : IEquatable<Participant>
    {
        public string FullName { get; set; }
        public string YearOfBirth { get; set; }
        public Gender Gender { get; set; }

        public Participant(string fullName, string yearOfBirth, Gender gender)
        {
            this.FullName = fullName;
            this.YearOfBirth = yearOfBirth;
            this.Gender = gender;
        }

        public bool Equals(Participant other)
            => (this.FullName == other.FullName 
                && this.YearOfBirth == other.YearOfBirth 
                && this.Gender == other.Gender);
    }
}
