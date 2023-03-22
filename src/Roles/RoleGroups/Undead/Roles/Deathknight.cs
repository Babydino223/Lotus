using System;
using System.Collections.Generic;
using System.Linq;
using TOHTOR.API;
using TOHTOR.Extensions;
using TOHTOR.GUI;
using TOHTOR.GUI.Name;
using TOHTOR.GUI.Name.Components;
using TOHTOR.GUI.Name.Holders;
using TOHTOR.GUI.Name.Impl;
using TOHTOR.Roles.Internals;
using TOHTOR.Roles.Internals.Attributes;
using UnityEngine;
using VentLib.Options.Game;
using VentLib.Utilities;

namespace TOHTOR.Roles.RoleGroups.Undead.Roles;

public class Deathknight : UndeadRole
{
    public bool CanBecomeNecromancer;
    private float influenceRange;
    private bool multiInfluence;

    private Cooldown influenceCooldown;
    private List<PlayerControl> inRangePlayers = new();
    private DateTime lastCheck = DateTime.Now;

    private const float UpdateTimeout = 0.25f;

    protected override void Setup(PlayerControl player)
    {
        base.Setup(player);
        LiveString liveString = new(inRangePlayers.Count > 0 ? "★" : "", Color.white);
        player.NameModel().GetComponentHolder<IndicatorHolder>().Add(new IndicatorComponent(liveString, GameState.Roaming, ViewMode.Additive, player));
    }


    [RoleAction(RoleActionType.FixedUpdate)]
    private void DeathknightFixedUpdate()
    {
        if (DateTime.Now.Subtract(lastCheck).TotalSeconds < UpdateTimeout) return;
        lastCheck = DateTime.Now;
        inRangePlayers.Clear();
        if (influenceCooldown.NotReady()) return;
        inRangePlayers = (influenceRange < 0
            ? MyPlayer.GetPlayersInAbilityRangeSorted().Where(IsUnconvertedUndead)
            : RoleUtils.GetPlayersWithinDistance(MyPlayer, influenceRange).Where(IsUnconvertedUndead))
            .ToList();
    }
    [RoleAction(RoleActionType.OnPet)]
    private void InitiatePlayer(ActionHandle handle)
    {
        if (influenceCooldown.NotReady()) return;
        int influenceCount = Math.Min(inRangePlayers.Count, multiInfluence ? int.MaxValue : 1);
        if (influenceCount == 0) return;
        influenceCooldown.Start();
        handle.Cancel();
        for (int i = 0; i < influenceCount; i++) InitiateUndead(inRangePlayers[i]);
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub.Name("Can Become Necromancer")
                .AddOnOffValues()
                .BindBool(b => CanBecomeNecromancer = b)
                .Build())
            .SubOption(sub => sub.Name("Influence Cooldown")
                .AddFloatRange(5f, 120f, 2.5f, 7, "s")
                .BindFloat(influenceCooldown.SetDuration)
                .Build())
            .SubOption(sub => sub.Name("Influence Range")
                .BindFloat(v => influenceRange = v)
                .Value(v => v.Text("Kill Distance").Value(-1f).Build())
                .AddFloatRange(1.5f, 3f, 0.1f, 4)
                .Build())
            .SubOption(sub => sub.Name("Influences Many")
                .BindBool(b => multiInfluence = b)
                .AddOnOffValues()
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) => base.Modify(roleModifier).RoleColor(new Color(0.34f, 0.34f, 0.39f));
}