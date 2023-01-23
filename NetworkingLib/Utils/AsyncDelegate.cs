using System.Threading.Tasks;

namespace NetworkingLib.Utils
{
    public delegate Task AsyncEventHandler<TEventArgs>(object sender, TEventArgs e);
}
