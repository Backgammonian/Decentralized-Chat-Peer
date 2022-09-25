using System.Threading.Tasks;

namespace Networking
{
    //I don't know if it makes event handlers actually awaitable but it works
    public delegate Task AsyncEventHandler<TEventArgs>(object? sender, TEventArgs e);
}