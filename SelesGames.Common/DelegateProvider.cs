using System;

namespace SelesGames.Common
{
    public class DelegateProvider<T> : IProvider<T>
    {
        Func<T> generator;

        public DelegateProvider(Func<T> generator)
        {
            this.generator = generator;
        }

        public T Get()
        {
            return generator();
        }
    }

    public class DelegateProvider
    {
        public static DelegateProvider<T> Create<T>(Func<T> generator)
        {
            return new DelegateProvider<T>(generator);
        }
    }
}
