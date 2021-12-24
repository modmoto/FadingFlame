using System;
using MongoDB.Bson.Serialization.Attributes;

namespace FadingFlame.Admin
{
    public class Season
    {
        public static Season Create(int seasonId)
        {
            return new Season
            {
                SeasonId = seasonId
            };
        }

        [BsonId]
        public int SeasonId { get; set; }
        public bool IsPubliclyVisible { get; set; }
        public DateTime StartDate { get; set; } = DateTime.MaxValue;
        public DateTime ListSubmissionDeadline { get; set; } = DateTime.MaxValue.AddDays(-1);
        public bool ListSubmissionIsOver => DateTime.UtcNow > ListSubmissionDeadline.AddDays(1);
    }
}