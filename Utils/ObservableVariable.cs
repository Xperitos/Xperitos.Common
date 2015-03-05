using System;

namespace Xperitos.Common.Utils
{
    /// <summary>
    /// Similar to "ObservableAsPropertyHelper{T}" but without the notifications.
    /// </summary>
    public class ObservableVariable<T> : IObservable<T>
    {
        public ObservableVariable( IObservable<T> valuesObservable, T initialValue = default( T ) )
        {
            m_valuesObservable = valuesObservable;
            Value = initialValue;
            valuesObservable.Subscribe(( v ) => Value = v);
        }

        public T Value { get; private set; }

        private readonly IObservable<T> m_valuesObservable;

        #region Implementation of IObservable<out T>

        public IDisposable Subscribe( IObserver<T> observer )
        {
            return m_valuesObservable.Subscribe(observer);
        }

        #endregion
    }

    public static class ObservableVariableMixins
    {
        public static ObservableVariable<T> ToVariable<T>(this IObservable<T> This, T initialValue = default(T))
        {
            return new ObservableVariable<T>(This, initialValue);
        }
    }
}
