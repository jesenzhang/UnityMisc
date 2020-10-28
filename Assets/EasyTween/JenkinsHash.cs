/// <summary>
/// Simple implementation of Jenkin's hashing algorithm.
/// http://en.wikipedia.org/wiki/Jenkins_hash_function
/// I forget where I got these magic numbers from; supposedly they're good.
/// </summary>
public struct JenkinsHash
{
    private int _current;

#if !(UNITY || UNITY_5_4_OR_NEWER)
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public void Mixin(int hash)
    {
        unchecked
        {
            int num = _current;
            if(num == 0)
                num = 0x7e53a269;
            else
                num *= -0x5aaaaad7;
            num += hash;
            num += (num << 10);
            num ^= (num >> 6);
            _current = num;
        }
    }

#if !(UNITY || UNITY_5_4_OR_NEWER)
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public int GetValue()
    {
        unchecked
        {
            int num = _current;
            num += (num << 3);
            num ^= (num >> 11);
            num += (num << 15);
            return num;
        }
    }
}