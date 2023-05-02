using System;
using TOHTOR.API;
using TOHTOR.API.Odyssey;
using TOHTOR.Extensions;
using TOHTOR.Roles;
using TOHTOR.Roles.Internals.Attributes;
using VentLib.Options.Game;
using static TOHTOR.Roles.RoleGroups.Impostors.SerialKiller;
using SerialKiller = TOHTOR.Roles.RoleGroups.Impostors.SerialKiller;

namespace TOHTOR.Gamemodes.Colorwars;

public class CwPainter: SerialKillerModifier
{
    internal CwPainter(SerialKiller role) : base(role) { }
    public override void OnLink() { }

    [ModifiedAction(RoleActionType.FixedUpdate)]
    public void FixedUpdate()
    {
        double timeElapsed = ((DateTime.Now - Game.StartTime).TotalSeconds);
        if (timeElapsed < ColorwarsGamemode.GracePeriod) {
            this.DeathTimer.Start();
            return;
        }
        CheckForSuicide();
    }

    [ModifiedAction(RoleActionType.Attack)]
    public void ColorwarsKill(PlayerControl target)
    {
        double timeElapsed = ((DateTime.Now - Game.StartTime).TotalSeconds);
        if (timeElapsed < ColorwarsGamemode.GracePeriod) return;
        if (ColorwarsGamemode.ConvertColorMode) SplatoonConvert(target);
        else {
            MyPlayer.RpcMurderPlayer(target);
            this.DeathTimer.Start();
        }
    }

    private void SplatoonConvert(PlayerControl target)
    {
        int killerColor = MyPlayer.cosmetics.bodyMatProperties.ColorId;
        if (killerColor == target.cosmetics.bodyMatProperties.ColorId) return;

        target.RpcSetColor((byte)killerColor);
        MyPlayer.RpcGuardAndKill(target);
        this.DeathTimer.Start();
    }

    public override GameOptionBuilder HookOptions(GameOptionBuilder optionStream) =>
        base.HookOptions(optionStream)
            .Tab(ColorwarsGamemode.ColorwarsTab);

    public override AbstractBaseRole.RoleModifier HookModifier(AbstractBaseRole.RoleModifier modifier) =>
        base.HookModifier(modifier)
            .RoleName("Painter");
}



