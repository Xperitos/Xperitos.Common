using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Xperitos.Common.Utils
{
    /// <summary>
    /// Helper functions for DisposableAsyncObject.
    /// </summary>
    /// <remarks>Some functions were taken from IReactiveObject.cs (of ReactiveUI)</remarks>
    public static class DisposableAsyncObjectMixins
    {
        /// <summary>
        /// RaiseAndSetIfChanged fully implements a Setter for a read-write
        /// property on a DisposableAsyncObject, using CallerMemberName to raise the notification
        /// and the ref to the backing field to set the property.
        /// </summary>
        /// <typeparam name="TObj">The type of the This.</typeparam>
        /// <typeparam name="TRet">The type of the return value.</typeparam>
        /// <param name="This"></param>
        /// <param name="backingField">A Reference to the backing field for this
        /// property.</param>
        /// <param name="newValue">The new value.</param>
        /// <param name="propertyName">The name of the property, usually 
        /// automatically provided through the CallerMemberName attribute.</param>
        /// <returns>The newly set value, normally discarded.</returns>
        public static TRet RaiseAndSetIfChanged<TObj, TRet>(
            this TObj This,
            ref TRet backingField,
            TRet newValue,
            [CallerMemberName] string propertyName = null) where TObj : DisposableAsyncObject
        {
            if (EqualityComparer<TRet>.Default.Equals(backingField, newValue))
                return newValue;

            This.RaisePropertyChanging(propertyName);
            backingField = newValue;
            This.RaisePropertyChanged(propertyName);
            return newValue;
        }
    }
}