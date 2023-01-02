using AmongUs.GameOptions;
using TownOfHost.Factions;
using UnityEngine;

namespace TownOfHost.Roles;

public class Morphling: Impostor
{
    protected float? shapeshiftCooldown = null;
    protected float? shapeshiftDuration = null;

    [RoleAction(RoleActionType.AttemptKill, Subclassing = false)]
    public virtual bool TryKill(PlayerControl target) => base.TryKill(target);


    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        roleModifier.VanillaRole(RoleTypes.Shapeshifter)
            .RoleColor(Color.red)
            .Factions(Faction.Impostors)
            .OptionOverride(Override.ShapeshiftCooldown, shapeshiftCooldown)
            .OptionOverride(Override.ShapeshiftDuration, shapeshiftDuration);
}