using System;
using MongoDB.Bson.Serialization.Attributes;

namespace FadingFlame.Admin
{
    public class Season
    {
        [BsonId]
        public int SeasonId { get; set; }
        public bool IsPubliclyVisible { get; set; }
        public DateTime StartDate { get; set; } = DateTime.MaxValue;
        public DateTime ListSubmissionDeadline { get; set; } = DateTime.MaxValue;

        public bool ListSubmissionIsOver => DateTime.UtcNow > ListSubmissionDeadline;
    }
}