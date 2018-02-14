using System;
using System.Linq;
using System.Threading.Tasks;

namespace Orleankka
{
    public delegate Task<object> Receive(object message);
}