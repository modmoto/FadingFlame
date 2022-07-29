namespace FadingFlame.Leagues;

internal class PointTuple
{
    public PointTuple(int player1, int player2)
    {
        Player1 = player1;
        Player2 = player2;
    }

    public int Player1 { get; }
    public int Player2 { get; }
}