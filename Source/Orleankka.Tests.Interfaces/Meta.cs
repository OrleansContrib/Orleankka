using System;
using System.Linq;

namespace Orleankka
{
    public interface Command
    {}

    public interface Query<TResult>
    {}

    public interface Event
    {}
}
