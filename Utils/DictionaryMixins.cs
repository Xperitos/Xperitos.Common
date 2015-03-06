using System.Collections.Generic;

namespace Xperitos.Common.Utils
{
    public static class DictionaryMixins
    {
        /// <summary>
        /// Return default value if the requested key wasn't found.
        /// </summary>
        public static TValue GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key, TValue defaultValue = default(TValue))
        {
            TValue result;
            if (dic.TryGetValue(key, out result))
                return result;

            return defaultValue;
        }

        /// <summary>
        /// Try to get a value from the dictionary in the target type.
        /// </summary>
        /// <returns>false if result is null or can't be converted (or not found)</returns>
        public static bool TryGetValueAs<TKey, TValue, TTarget>(this Dictionary<TKey, TValue> dic, TKey key, out TTarget targetValue)
            where TTarget : class
        {
            TValue midValue;
            if ( !dic.TryGetValue(key, out midValue) )
            {
                targetValue = default( TTarget );
                return false;
            }

            targetValue = midValue as TTarget;
            return targetValue != null;
        }
    }
}
