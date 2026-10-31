using System;

public static class TravelClaim
{
    static object owner;
    static Action cancel;

    public static void Claim(object next, Action onCancel)
    {
        if (owner == next)
            return;

        cancel?.Invoke();
        owner = next;
        cancel = onCancel;
    }

    public static void Release(object current)
    {
        if (owner != current)
            return;

        owner = null;
        cancel = null;
    }
}
