using TOHTOR.Extensions;
using TOHTOR.GUI;
using TOHTOR.GUI.Name;
using TOHTOR.Options;
using TOHTOR.Roles.Interactions.Interfaces;
using TOHTOR.Roles.Internals;
using TOHTOR.Roles.Internals.Attributes;
using UnityEngine;
using VentLib.Options.Game;
using VentLib.Utilities;

namespace TOHTOR.Roles.RoleGroups.NeutralKilling;

public class BloodKnight : NeutralKillingBase
{
    private float protectionAmt;
    private bool canVent;
    private bool isProtected;

    public override bool CanSabotage() => false;

    // Usually I use Misc but because the Blood Knight's color is hard to see I'm displaying this next to the player's name which requires a bit more hacky code
    [UIComponent(UI.Counter)]
    private string ProtectedIndicator() => isProtected ? RoleColor.Colorize("•") : "";

    [RoleAction(RoleActionType.RoundStart)]
    public void Reset() => isProtected = false;

    [RoleAction(RoleActionType.Attack)]
    public new bool TryKill(PlayerControl target)
    {
        // Call to Impostor.TryKill()
        bool killed = base.TryKill(target);
        // Possibly died due to veteran
        if (MyPlayer.Data.IsDead) return killed;

        isProtected = true;
        Async.Schedule(() => isProtected = false, protectionAmt);
        return killed;
    }

    [RoleAction(RoleActionType.Interaction)]
    private void InteractedWith(Interaction interaction, ActionHandle handle)
    {
        if (!isProtected) return;
        if (interaction.Intent() is not IFatalIntent) return;
        handle.Cancel();
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
         base.RegisterOptions(optionStream)
             .Tab(DefaultTabs.NeutralTab)
             .SubOption(opt =>
                opt.Name("Kill Cooldown")
                .BindFloat(v => KillCooldown = v)
                .AddFloatRange(2.5f, 180f, 2.5f, 11, "s")
                .Build())
            .SubOption(opt =>
                opt.Name("Protection Duration")
                .BindFloat(v => protectionAmt = v)
                .AddFloatRange(2.5f, 180, 2.5f, 5, "s")
                .Build())
            .SubOption(opt =>
                opt.Name("Can Vent")
                .BindBool(v => canVent = v)
                .AddOnOffValues()
                .Build());



    protected override RoleModifier Modify(RoleModifier roleModifier)
    {
        return base.Modify(roleModifier) // call base because we're utilizing some settings setup by NeutralKillingBase
            .RoleName("Blood Knight")
            .RoleColor(new Color(0.47f, 0f, 0f)) // Using Color() because it's easier to edit and get an idea for actual color
            .CanVent(canVent);
    }

}