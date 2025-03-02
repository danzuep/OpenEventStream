namespace OpenEventStream.Extensions;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public static class DictionaryExtensions
{
    public static TValue? GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue? value) where TKey : notnull
    {
        ref var dictionaryValue = ref CollectionsMarshal.GetValueRefOrAddDefault(dict, key, out var exists);
        if (exists)
        {
            return dictionaryValue;
        }

        dictionaryValue = value;
        return value;
    }

    public static bool TryUpdate<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue? value) where TKey : notnull
    {
        ref var dictionaryValue = ref CollectionsMarshal.GetValueRefOrNullRef(dict, key);
        if (!Unsafe.IsNullRef(ref dictionaryValue))
        {
            dictionaryValue = value;
            return true;
        }

        return false;
    }
}
