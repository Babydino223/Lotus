using System;
using System.Globalization;
using Lotus.Roles.Internals.Interfaces;
using UnityEngine;

namespace Lotus.GUI;

public class Cooldown: ICloneOnSetup<Cooldown>
{
    public float Duration;
    private float remaining;
    private DateTime lastTick = DateTime.Now;
    private Action? action;

    public Cooldown() {}

    public Cooldown(float duration)
    {
        this.Duration = duration;
    }

    public bool NotReady() => TimeRemaining() > 0;
    public bool IsReady() => TimeRemaining() <= 0;
    public void Start(float duration = float.MinValue)
    {
        remaining = duration == float.MinValue ? Duration : duration;
        lastTick = DateTime.Now;
    }

    public void Finish()
    {
        remaining = 0.01f;
    }

    public void StartThenRun(Action action, float duration = float.MinValue)
    {
        this.action = action;
        Start(duration);
    }

    public void SetDuration(float duration)
    {
        this.Duration = duration;
    }

    public float TimeRemaining()
    {
        remaining = Mathf.Clamp(remaining - TimeElapsed(), 0, float.MaxValue);
        if (remaining > 0 || action == null) return remaining;
        Action tempAction = action;
        action = null;
        tempAction();
        return remaining;
    }

    private float TimeElapsed()
    {
        TimeSpan elapsed = DateTime.Now - lastTick;
        lastTick = DateTime.Now;
        return (float)elapsed.TotalSeconds;
    }

    public Cooldown Clone() => (Cooldown)this.MemberwiseClone();
    public override string ToString() => Mathf.CeilToInt(TimeRemaining()).ToString();

    public string ToString(int decimals) => Math.Round((decimal)TimeRemaining(), decimals).ToString(CultureInfo.CurrentCulture);
}
