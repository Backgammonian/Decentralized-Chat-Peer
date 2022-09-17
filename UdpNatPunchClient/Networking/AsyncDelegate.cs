using System.Threading.Tasks;

namespace Networking
{
    public delegate Task AsyncEventHandler<TEventArgs>(object? sender, TEventArgs e);
}