using Lausanne20Km.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Lausanne20Km.Repositories
{
    class ParticipantRepository
    {
        private List<RaceResult> _raceResults;
        public ParticipantRepository(List<RaceResult> raceResults)
        {
            _raceResults = raceResults;
        }

        public List<Participant> GetAll(int distance)
            => _raceResults
                .Where(x => x.IsDistance(distance))
                .Select(x => x.participant)
                .Distinct()
                .ToList();

        public Dictionary<Participant, List<RaceResult>> GetResultsByParticipant(int distance)
            => _raceResults
                .Where(x => x.IsDistance(distance) && x.IsValidAge() && x.IsValidTime())
                .GroupBy(x => x.participant)
                .ToDictionary(x => x.Key, x => x.ToList());        

        public Dictionary<Participant, List<RaceResult>> GetAllCompletedNRaces(int distance, int minNumberOfCompletedRaces)
        {
            Console.WriteLine("Getting all years ...");
            var years = _raceResults
                .Where(x => x.IsDistance(distance))
                .Select(x => x.year)
                .OrderBy(x => x)
                .Distinct()
                .ToList();

            Console.WriteLine("Getting participants data ...");
            var resultsByParticipant = GetResultsByParticipant(distance);
            var fullResults = new Dictionary<Participant, List<RaceResult>>();
            var count = 0;
            foreach (var pair in resultsByParticipant)
            {
                count++;
                Console.Write($"\r{((double)count / resultsByParticipant.Count).ToString("0.00%")}");

                var participantResults = pair.Value;

                if(participantResults.Count >= minNumberOfCompletedRaces)
                {
                    fullResults.Add(
                        pair.Key,
                        participantResults.OrderBy(x => x.year).ToList()
                    );
                }
            }
            Console.WriteLine();
            return fullResults;
        }
    }
}
