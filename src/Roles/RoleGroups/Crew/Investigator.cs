using System;
using System.Collections.Generic;
using System.Linq;
using TOHTOR.API;
using TOHTOR.API.Odyssey;
using TOHTOR.Extensions;
using TOHTOR.Factions.Impostors;
using TOHTOR.GUI;
using TOHTOR.GUI.Name;
using TOHTOR.GUI.Name.Components;
using TOHTOR.GUI.Name.Holders;
using TOHTOR.GUI.Name.Impl;
using TOHTOR.GUI.Name.Interfaces;
using TOHTOR.Roles.Interactions;
using TOHTOR.Roles.Internals;
using TOHTOR.Roles.Internals.Attributes;
using TOHTOR.Roles.RoleGroups.Coven;
using TOHTOR.Roles.RoleGroups.Impostors;
using TOHTOR.Roles.RoleGroups.Madmates.Roles;
using TOHTOR.Roles.RoleGroups.Neutral;
using TOHTOR.Roles.RoleGroups.NeutralKilling;
using TOHTOR.Roles.RoleGroups.Vanilla;
using TOHTOR.Utilities;
using UnityEngine;
using VentLib.Logging;
using VentLib.Options.Game;
using VentLib.Utilities;

namespace TOHTOR.Roles.RoleGroups.Crew;

// This is going to be the longest option list :(
public class Investigator : Crewmate
{
    protected static readonly List<Tuple<Type, Color, InvestOptCategory>> InvestCategoryList = new()
    {
        new Tuple<Type, Color, InvestOptCategory>(typeof(Amnesiac), new Color(0.51f, 0.87f, 0.99f), InvestOptCategory.NeutralPassive),
        new Tuple<Type, Color, InvestOptCategory>(typeof(Executioner), new Color(0.55f, 0.17f, 0.33f), InvestOptCategory.NeutralPassive),
        new Tuple<Type, Color, InvestOptCategory>(typeof(Jester), new Color(0.93f, 0.38f, 0.65f), InvestOptCategory.NeutralPassive),
        new Tuple<Type, Color, InvestOptCategory>(typeof(Opportunist), Color.green, InvestOptCategory.NeutralPassive),
        new Tuple<Type, Color, InvestOptCategory>(typeof(Phantom), new Color(0.51f, 0.87f, 0.99f), InvestOptCategory.NeutralPassive),
        new Tuple<Type, Color, InvestOptCategory>(typeof(Survivor), new Color(1f, 0.9f, 0.3f), InvestOptCategory.NeutralPassive),

        new Tuple<Type, Color, InvestOptCategory>(typeof(Arsonist), new Color(1f, 0.4f, 0.2f), InvestOptCategory.NeutralKilling),
        new Tuple<Type, Color, InvestOptCategory>(typeof(BloodKnight), Utils.ConvertHexToColor("#630000"), InvestOptCategory.NeutralKilling),
        new Tuple<Type, Color, InvestOptCategory>(typeof(CrewPostor), Utils.ConvertHexToColor("#DC6601"), InvestOptCategory.NeutralKilling),
        new Tuple<Type, Color, InvestOptCategory>(typeof(Egoist), Color.green, InvestOptCategory.NeutralKilling),
        new Tuple<Type, Color, InvestOptCategory>(typeof(Glitch), Color.green, InvestOptCategory.NeutralKilling),
        new Tuple<Type, Color, InvestOptCategory>(typeof(Hacker), Color.green, InvestOptCategory.NeutralKilling),
        new Tuple<Type, Color, InvestOptCategory>(typeof(Jackal), new Color(0f, 0.71f, 0.92f), InvestOptCategory.NeutralKilling),
        new Tuple<Type, Color, InvestOptCategory>(typeof(Juggernaut), Color.green, InvestOptCategory.NeutralKilling),
        new Tuple<Type, Color, InvestOptCategory>(typeof(Marksman), Color.green, InvestOptCategory.NeutralKilling),
        new Tuple<Type, Color, InvestOptCategory>(typeof(NeutWitch), Color.green, InvestOptCategory.NeutralKilling),
        new Tuple<Type, Color, InvestOptCategory>(typeof(Pestilence), Color.green, InvestOptCategory.NeutralKilling),
        new Tuple<Type, Color, InvestOptCategory>(typeof(Pirate), Color.green, InvestOptCategory.NeutralKilling),
        new Tuple<Type, Color, InvestOptCategory>(typeof(PlagueBearer), Color.green, InvestOptCategory.NeutralKilling),
        new Tuple<Type, Color, InvestOptCategory>(typeof(Poisoner), Color.green, InvestOptCategory.NeutralKilling),
        new Tuple<Type, Color, InvestOptCategory>(typeof(Swapper), Color.green, InvestOptCategory.NeutralKilling),
        new Tuple<Type, Color, InvestOptCategory>(typeof(Vulture), Color.green, InvestOptCategory.NeutralKilling),
        new Tuple<Type, Color, InvestOptCategory>(typeof(Werewolf), Color.green, InvestOptCategory.NeutralKilling),

        new Tuple<Type, Color, InvestOptCategory>(typeof(Sheriff), new Color(0.97f, 0.8f, 0.27f), InvestOptCategory.CrewmateKilling),
        new Tuple<Type, Color, InvestOptCategory>(typeof(Veteran), new Color(0.6f, 0.5f, 0.25f), InvestOptCategory.CrewmateKilling),

        new Tuple<Type, Color, InvestOptCategory>(typeof(Conjuror), Color.red, InvestOptCategory.Coven),
        new Tuple<Type, Color, InvestOptCategory>(typeof(Coven.Coven), Color.red, InvestOptCategory.Coven),
        new Tuple<Type, Color, InvestOptCategory>(typeof(CovenWitch), Color.red, InvestOptCategory.Coven),
        new Tuple<Type, Color, InvestOptCategory>(typeof(HexMaster), Color.red, InvestOptCategory.Coven),
        new Tuple<Type, Color, InvestOptCategory>(typeof(Medusa), Color.red, InvestOptCategory.Coven),
        new Tuple<Type, Color, InvestOptCategory>(typeof(Mimic), Color.red, InvestOptCategory.Coven),
        new Tuple<Type, Color, InvestOptCategory>(typeof(PotionMaster), Color.red, InvestOptCategory.Coven),

        new Tuple<Type, Color, InvestOptCategory>(typeof(Madmate), Color.red, InvestOptCategory.Madmate),
        new Tuple<Type, Color, InvestOptCategory>(typeof(MadGuardian), Color.red, InvestOptCategory.Madmate),
        new Tuple<Type, Color, InvestOptCategory>(typeof(MadSnitch), Color.red, InvestOptCategory.Madmate),
        new Tuple<Type, Color, InvestOptCategory>(typeof(Parasite), Color.red, InvestOptCategory.Madmate),
    };

    [UIComponent(UI.Cooldown)]
    private Cooldown abilityCooldown;
    private NIOpt neutralPassiveRed;
    private NIOpt neutralKillingRed;
    private NIOpt crewmateKillingRed;
    private NIOpt covenPurple;
    private NIOpt madmateRed;

    private List<int> redRoles = new();
    private List<byte> investigated;

    protected override void Setup(PlayerControl player)
    {
        investigated = new List<byte>();
        base.Setup(player);
    }

    [RoleAction(RoleActionType.OnPet)]
    private void Investigate()
    {
        if (abilityCooldown.NotReady()) return;
        List<PlayerControl> players = MyPlayer.GetPlayersInAbilityRangeSorted().Where(p => !investigated.Contains(p.PlayerId)).ToList();
        if (players.Count == 0) return;

        abilityCooldown.Start();
        PlayerControl player = players[0];
        if (MyPlayer.InteractWith(player, DirectInteraction.NeutralInteraction.Create(this)) is InteractionResult.Halt) return;

        investigated.Add(player.PlayerId);
        CustomRole role = player.GetCustomRole();

        int categoryIndex = InvestCategoryList.FindIndex(tuple => tuple.Item1 == role.GetType());
        InvestOptCategory category = categoryIndex != -1 ? InvestCategoryList[categoryIndex].Item3 : InvestOptCategory.None;

        Color good = new(0.35f, 0.71f, 0.33f);
        Color bad = new(0.72f, 0.04f, 0f);
        Color purple = new(0.45f, 0.31f, 0.72f);

        bool roleIsInRoles = redRoles.Contains(categoryIndex);
        Color color = (category) switch
        {
            InvestOptCategory.None => role.Faction is ImpostorFaction ? bad : good,
            InvestOptCategory.NeutralPassive => neutralPassiveRed is NIOpt.All ? bad : neutralPassiveRed is NIOpt.None ? good : roleIsInRoles ? bad : good,
            InvestOptCategory.NeutralKilling => neutralKillingRed is NIOpt.All ? bad : neutralKillingRed is NIOpt.None ? good : roleIsInRoles ? bad : good,
            InvestOptCategory.CrewmateKilling => crewmateKillingRed is NIOpt.All ? bad : crewmateKillingRed is NIOpt.None ? good : roleIsInRoles ? bad : good,
            InvestOptCategory.Coven => covenPurple is NIOpt.All ? purple : covenPurple is NIOpt.None ? good : roleIsInRoles ? purple : good,
            InvestOptCategory.Madmate => madmateRed is NIOpt.All ? bad : madmateRed is NIOpt.None ? good : roleIsInRoles ? bad : good,
            _ => throw new ArgumentOutOfRangeException()
        };
        VentLogger.Old($"{player.GetNameWithRole()} is type {role.GetType()} and falls under category \"{category}\". Player is in redRoles list? {roleIsInRoles}. Player's name should be color: {color.ToTextColor()}", "InvestigateInfo");

        NameComponent nameComponent = new(new LiveString(player.name, color), GameStates.IgnStates, ViewMode.Replace, MyPlayer);
        player.NameModel().GetComponentHolder<NameHolder>().Add(nameComponent);
    }

    // This is the most complicated options because of all the individual settings
    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream)
    {
        GameOptionBuilder neutPassiveBuilder = new GameOptionBuilder()
            .Name("Neutral Passive are Red")
            .BindInt(v => neutralPassiveRed = (NIOpt)v)
            .ShowSubOptionPredicate(v => (int)v >= 2)
            .Value(v => v.Text("None").Value(1).Color(Color.red).Build())
            .Value(v => v.Text("All").Value(0).Color(Color.cyan).Build())
            .Value(v => v.Text("Individual").Value(2).Color(new Color(0.73f, 0.58f, 1f)).Build());
        GameOptionBuilder neutKillBuilder = new GameOptionBuilder()
            .Name("Neutral Killing are Red")
            .BindInt(v => neutralKillingRed = (NIOpt)v)
            .ShowSubOptionPredicate(v => (int)v >= 2)
            .Value(v => v.Text("None").Value(1).Color(Color.red).Build())
            .Value(v => v.Text("All").Value(0).Color(Color.cyan).Build())
            .Value(v => v.Text("Individual").Value(2).Color(new Color(0.73f, 0.58f, 1f)).Build());
        GameOptionBuilder crewmateKillBuilder = new GameOptionBuilder()
            .Name("Crewmate Killing are Red")
            .BindInt(v => crewmateKillingRed = (NIOpt)v)
            .ShowSubOptionPredicate(v => (int)v >= 2)
            .Value(v => v.Text("None").Value(1).Color(Color.red).Build())
            .Value(v => v.Text("All").Value(0).Color(Color.cyan).Build())
            .Value(v => v.Text("Individual").Value(2).Color(new Color(0.73f, 0.58f, 1f)).Build());
        GameOptionBuilder covenBuilder = new GameOptionBuilder()
            .Name("Coven are Purple")
            .BindInt(v => covenPurple = (NIOpt)v)
            .ShowSubOptionPredicate(v => (int)v >= 2)
            .Value(v => v.Text("None").Value(1).Color(Color.red).Build())
            .Value(v => v.Text("All").Value(0).Color(Color.cyan).Build())
            .Value(v => v.Text("Individual").Value(2).Color(new Color(0.73f, 0.58f, 1f)).Build());
        GameOptionBuilder madmateBuilder = new GameOptionBuilder()
            .Name("Madmate are Red")
            .BindInt(v => madmateRed = (NIOpt)v)
            .ShowSubOptionPredicate(v => (int)v >= 2)
            .Value(v => v.Text("None").Value(1).Color(Color.red).Build())
            .Value(v => v.Text("All").Value(0).Color(Color.cyan).Build())
            .Value(v => v.Text("Individual").Value(2).Color(new Color(0.73f, 0.58f, 1f)).Build());

        GameOptionBuilder[] builders = { neutPassiveBuilder, neutKillBuilder, crewmateKillBuilder, covenBuilder, madmateBuilder };

        for (int i = 0; i < InvestCategoryList.Count; i++)
        {
            Tuple<Type, Color, InvestOptCategory> item = InvestCategoryList[i];
            GameOptionBuilder builder = builders[(int)item.Item3 - 1];

            var i1 = i;
            builder.SubOption(sub => sub
                .Name(item.Item1.Name)
                .Color(item.Item2)
                .Bind(v =>
                {
                    if ((bool)v)
                        redRoles.Add(i1);
                    else
                        redRoles.Remove(i1);
                })
                .AddOnOffValues(false)
                .Build());
        }


        return base.RegisterOptions(optionStream)
            .SubOption(sub => sub
                .Name("Investigate Cooldown")
                .BindFloat(v => abilityCooldown.Duration = v)
                .AddFloatRange(2.5f, 120, 2.5f, 10, "s")
                .Build())
            .SubOption(_ => builders[0].Build())
            .SubOption(_ => builders[1].Build())
            .SubOption(_ => builders[2].Build())
            .SubOption(_ => builders[3].Build())
            .SubOption(_ => builders[4].Build());
    }

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier).RoleColor(new Color(1f, 0.79f, 0.51f));


    protected enum NIOpt
    {
        All,
        None,
        Individual
    }

    protected enum InvestOptCategory
    {
        None,
        NeutralPassive,
        NeutralKilling,
        CrewmateKilling,
        Coven,
        Madmate
    }
}