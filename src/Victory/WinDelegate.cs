using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using TOHTOR.Extensions;
using TOHTOR.Options;
using TOHTOR.Victory.Conditions;
using VentLib.Utilities.Extensions;
using VentLib.Logging;

namespace TOHTOR.Victory;

public class WinDelegate
{
    private readonly List<IWinCondition> winConditions = new() { new FallbackCondition() };
    private readonly List<Action<WinDelegate>> winNotifiers = new();

    private List<PlayerControl> winners = new();
    private WinReason winReason;
    private bool forcedWin;
    private bool forcedCancel;

    public WinReason GetWinReason() => winReason;
    public void SetWinReason(WinReason reason) => winReason = reason;
    public List<PlayerControl> GetWinners() => winners;
    public void SetWinners(List<PlayerControl> winners) => this.winners = winners;

    public bool IsGameOver()
    {
        if (forcedWin)
        {
            VentLogger.Info($"Triggering Game Win by Force, winners={winners.Select(p => p.UnalteredName()).Join()}, reason={winReason}", "WinCondition");
            winNotifiers.ForEach(notify => notify(this));
            return true;
        }

        IWinCondition? condition = winConditions.FirstOrDefault(con => con.IsConditionMet(out winners));
        if (condition == null) return false;
        winNotifiers.ForEach(notify => notify(this));

        if (forcedCancel) return false;

        winReason = condition.GetWinReason();
        VentLogger.Info($"Triggering Win by \"{condition.GetType()}\", winners={winners.Select(p => p.UnalteredName()).StrJoin()}, reason={winReason}", "WinCondition");
        return true;
    }

    /// <summary>
    /// Adds a consumer which gets triggered when the game has detected a possible win. This allows for pre-win interactions
    /// as well as the possibility to cancel a game win via CancelGameWin() or to modify the game winners
    /// </summary>
    /// <param name="consumer"></param>
    public void AddSubscriber(Action<WinDelegate> consumer)
    {
        winNotifiers.Add(consumer);
    }

    public void AddWinCondition(IWinCondition condition)
    {
        winConditions.Add(condition);
        winConditions.Sort();
    }

    public void ForceGameWin(List<PlayerControl> forcedWinners, WinReason reason)
    {
        this.winners = forcedWinners;
        this.winReason = reason;
        forcedWin = true;
    }

    public void CancelGameWin()
    {
        forcedCancel = true;
    }
}