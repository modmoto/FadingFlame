using MongoDB.Bson.Serialization.Attributes;

namespace FadingFlame.ReadModelBase;

public class HandlerVersion
{
    public string Version { get; set; }
        
    [BsonId]
    public string HandlerName { get; set; }

    public static HandlerVersion CreateFrom<T>(string version)
    {
        return new HandlerVersion
        {
            Version = version,
            HandlerName = typeof(T).Name
        };
    }
}