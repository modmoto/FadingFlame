using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FadingFlame.Players;

public interface IMmrRepository
{
    Task<List<Mmr>> UpdateMmrs(UpdateMmrRequest updateMmrRequest);
}
    
public class MmrRepository : IMmrRepository
{
    private readonly HttpClient _httpClient;

    public MmrRepository(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<Mmr>> UpdateMmrs(UpdateMmrRequest updateMmrRequest)
    {
        var response = await _httpClient.PostAsJsonAsync("/mmr/update", updateMmrRequest);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<UpdateMmrResponse>();
            return TransformResponse(result);
        }

        return null;
    }

    private List<Mmr> TransformResponse(UpdateMmrResponse response) {
        var ret = response.RatingsList.Select((rating, index) =>
            new Mmr
            {
                Rating = rating,
                RatingDeviation = response.RatingDeviationsList[index],
            }
        ).ToList();

        return ret;
    }
}

public class UpdateMmrResponse
{
    [JsonPropertyName("ratings_list")]
    public List<double> RatingsList { get; set; }

    [JsonPropertyName("rds_list")]
    public List<double> RatingDeviationsList { get; set; }
}

public class UpdateMmrRequest
{
    [JsonPropertyName("ratings_list")]
    public List<double> RatingsList { get; set; }

    [JsonPropertyName("rds_list")]
    public List<double> RdsList { get; set; }

    [JsonPropertyName("winning_team")]
    public int WinningTeam { get; set; }

    [JsonPropertyName("number_of_teams")]
    public int NumberOfTeams { get; init; }

    [JsonPropertyName("beta")]
    public double Beta { get; init; }

    [JsonPropertyName("p_mean")]
    public double PmEan { get; init; }

    [JsonPropertyName("min_rating_deviation")]
    public double MinRatingDeviation { get; init; }

    public static UpdateMmrRequest Create(List<Mmr> mmrs, int winningTeam)
    {
        var ratingsList = mmrs.Select(t => t.Rating).ToList();
        var rdsList = mmrs.Select(t => t.RatingDeviation).ToList();

        return new UpdateMmrRequest
        {
            RatingsList = ratingsList,
            RdsList = rdsList,
            WinningTeam = winningTeam,

            NumberOfTeams = 2,
            Beta = 0.85,
            MinRatingDeviation = 80,
            PmEan = 1
        };
    }
}