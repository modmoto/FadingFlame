using System.Threading.Tasks;

namespace FadingFlame.ReadModelBase;

public interface IAsyncUpdatable
{
    Task<HandlerVersion> Update(HandlerVersion currentVersion);
}