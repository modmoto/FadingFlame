using System.Threading.Tasks;

namespace FadingFlame.ReadModelBase;

public interface IVersionRepository
{
    Task<HandlerVersion> GetLastVersion<T>();
    Task SaveLastVersion<T>(HandlerVersion lastVersion);
}