using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;

namespace FadingFlame.Matchups;

public class PlayerResultDto
{
    public ObjectId Id { get; set; }
    [Range(0, 5100)]
    public int VictoryPoints { get; set; }
}