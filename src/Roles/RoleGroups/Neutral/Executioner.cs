#nullable enable
using System.Collections.Generic;
using System.Linq;
using TOHTOR.API;
using TOHTOR.Extensions;
using TOHTOR.Factions.Impostors;
using TOHTOR.Factions.Interfaces;
using TOHTOR.Factions.Neutrals;
using TOHTOR.GUI.Name.Components;
using TOHTOR.GUI.Name.Holders;
using TOHTOR.Managers;
using TOHTOR.Options;
using TOHTOR.Roles.Internals.Attributes;
using TOHTOR.Victory.Conditions;
using UnityEngine;
using VentLib.Logging;
using VentLib.Options.Game;
using VentLib.Utilities.Extensions;

namespace TOHTOR.Roles.RoleGroups.Neutral;

public class Executioner : CustomRole
{
    private bool canTargetImpostors;
    private bool canTargetNeutrals;
    private int roleChangeWhenTargetDies;

    private PlayerControl? target;

    [RoleAction(RoleActionType.RoundStart)]
    private void OnGameStart(bool gameStart)
    {
        if (!gameStart) return;
        target = Game.GetAllPlayers().Where(p =>
        {
            if (p.PlayerId == MyPlayer.PlayerId) return false;
            IFaction faction = p.GetCustomRole().Faction;
            if (!canTargetImpostors && faction is ImpostorFaction) return false;
            return canTargetNeutrals || faction is not Solo;
        }).ToList().GetRandom();
        VentLogger.Trace($"Executioner ({MyPlayer.UnalteredName()}) Target: {target}");

        target.NameModel().GetComponentHolder<NameHolder>().Add(new ColoredNameComponent(target, RoleColor, GameStates.IgnStates, MyPlayer));
    }

    [RoleAction(RoleActionType.OtherExiled)]
    private void CheckExecutionerWin(PlayerControl exiled)
    {
        if (target == null || target.PlayerId != exiled.PlayerId) return;
        List<PlayerControl> winners = new() { MyPlayer };
        if (target.GetCustomRole() is Jester) winners.Add(target);
        ManualWin win = new(winners, WinReason.SoloWinner);
        win.Activate();
    }

    [RoleAction(RoleActionType.AnyDeath)]
    private void CheckChangeRole(PlayerControl dead)
    {
        if (roleChangeWhenTargetDies == 0 || target == null || target.PlayerId != dead.PlayerId) return;
        switch ((ExeRoleChange)roleChangeWhenTargetDies)
        {
            case ExeRoleChange.Jester:
                Game.AssignRole(MyPlayer, CustomRoleManager.Static.Jester);
                break;
            case ExeRoleChange.Opportunist:
                Game.AssignRole(MyPlayer, CustomRoleManager.Static.Opportunist);
                break;
            case ExeRoleChange.SchrodingerCat:
                Game.AssignRole(MyPlayer, CustomRoleManager.Static.SchrodingerCat);
                break;
            case ExeRoleChange.Crewmate:
                Game.AssignRole(MyPlayer, CustomRoleManager.Static.Crewmate);
                break;
            case ExeRoleChange.None:
            default:
                break;
        }

        target = null;
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
        .Tab(DefaultTabs.NeutralTab)
            .SubOption(sub => sub
                .Name("Can Target Impostors")
                .Bind(v => canTargetImpostors = (bool)v)
                .AddOnOffValues(false).Build())
            .SubOption(sub => sub
                .Name("Can Target Neutrals")
                .Bind(v => canTargetNeutrals = (bool)v)
                .AddOnOffValues(false).Build())
            .SubOption(sub => sub
                .Name("Role Change When Target Dies")
                .Bind(v => roleChangeWhenTargetDies = (int)v)
                .Value(v => v.Text("Jester").Value(1).Color(new Color(0.93f, 0.38f, 0.65f)).Build())
                .Value(v => v.Text("Opportunist").Value(2).Color(Color.green).Build())
                .Value(v => v.Text("Schrodinger's Cat").Value(3).Color(Color.black).Build())
                .Value(v => v.Text("Crewmate").Value(4).Color(new Color(0.71f, 0.94f, 1f)).Build())
                .Value(v => v.Text("Off").Value(0).Color(Color.red).Build())
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        roleModifier.RoleColor(new Color(0.55f, 0.17f, 0.33f));

    private enum ExeRoleChange
    {
        None,
        Jester,
        Opportunist,
        SchrodingerCat,
        Crewmate
    }
}