using System;

namespace Cubeage
{
    public class Disposable : IDisposable
    {
        private readonly Action _action;

        public Disposable(Action action)
        {
            _action = action;
        }

        public void Dispose()
        {
            _action();
        }

        public static IDisposable Create(Action action)
        {
            return new Disposable(action);
        }
    }
}
