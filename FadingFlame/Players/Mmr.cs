namespace FadingFlame.Players;

public class Mmr
{
    public double Rating { get; set; }
    public double RatingDeviation { get; set; }

    public static Mmr Create()
    {
        return new()
        {
            Rating = 1500,
            RatingDeviation = 350
        };
    }
}