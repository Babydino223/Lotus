using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TOHTOR.Roles;
using TOHTOR.Roles.Debugger;
using TOHTOR.Roles.Extra;
using TOHTOR.Roles.RoleGroups.Coven;
using TOHTOR.Roles.RoleGroups.Crew;
using TOHTOR.Roles.RoleGroups.Crew.Snitch;
using TOHTOR.Roles.RoleGroups.Impostors;
using TOHTOR.Roles.RoleGroups.Madmates.Roles;
using TOHTOR.Roles.RoleGroups.Neutral;
using TOHTOR.Roles.RoleGroups.NeutralKilling;
using TOHTOR.Roles.RoleGroups.Undead.Roles;
using TOHTOR.Roles.RoleGroups.Vanilla;
using TOHTOR.Roles.Subroles;
using VentLib.Utilities.Attributes;
using VentLib.Utilities.Extensions;
using static TOHTOR.Roles.AbstractBaseRole;
using Impostor = TOHTOR.Roles.RoleGroups.Vanilla.Impostor;
using Medium = TOHTOR.Roles.RoleGroups.Crew.Medium;
using Necromancer = TOHTOR.Roles.RoleGroups.Undead.Roles.Necromancer;
using SerialKiller = TOHTOR.Roles.RoleGroups.Impostors.SerialKiller;

namespace TOHTOR.Managers;

[LoadStatic]
public static class CustomRoleManager
{
    public static Dictionary<byte, CustomRole> PlayersCustomRolesRedux = new();
    public static Dictionary<byte, CustomRole> LastRoundCustomRoles = new();

    public static Dictionary<byte, List<CustomRole>> PlayerSubroles = new();

    public static List<byte> RoleBlockedPlayers = new();
    public static StaticRoles Static = new();
    public static ExtraRoles Special = new();
    public static CustomRole Default = Static.Crewmate;

    public static readonly List<CustomRole> MainRoles = Static.GetType()
        .GetFields()
        .Select(f => (CustomRole)f.GetValue(Static))
        .ToList();

    public static readonly List<CustomRole> SpecialRoles = Special.GetType()
        .GetFields()
        .Select(f => (CustomRole)f.GetValue(Special))
        .ToList();

    public static readonly List<CustomRole> AllRoles = MainRoles.Concat(SpecialRoles).ToList();


    public static void AddRole(CustomRole staticRole)
    {
        AllRoles.Add(staticRole);
    }

    public static int GetRoleId(CustomRole role) => role == null ? 0 : GetRoleId(role.GetType());
    public static CustomRole GetRoleFromType(Type roleType) => GetRoleFromId(GetRoleId(roleType));

    public static int GetRoleId(Type roleType)
    {
        for (int i = 0; i < AllRoles.Count; i++)
            if (roleType == AllRoles[i].GetType())
                return i;
        return -1;
    }

    public static CustomRole GetRoleFromId(int id)
    {
        if (id == -1) id = 0;
        return AllRoles[id];
    }

    public static void AddPlayerSubrole(byte playerId, Subrole subrole)
    {
        if (!PlayerSubroles.ContainsKey(playerId)) PlayerSubroles[playerId] = new List<CustomRole>();
        PlayerSubroles[playerId].Add(subrole);
    }

    public static void AddPlayerSubrole(byte playerId, CustomRole subrole)
    {
        if (!PlayerSubroles.ContainsKey(playerId)) PlayerSubroles[playerId] = new List<CustomRole>();
        PlayerSubroles[playerId].Add(subrole);
    }

    internal static void LinkEditor(Type editorType)
    {
        if (!editorType.IsAssignableTo(typeof(RoleEditor)))
            throw new ArgumentException("Editor Type MUST be a subclass of AbstractBaseRole.RoleEditor");
        Type roleType = editorType.BaseType!.DeclaringType!;
        bool isStatic = typeof(StaticRoles).GetFields().Any(f => f.FieldType == roleType);
        bool isExtra = typeof(ExtraRoles).GetFields().Any(f => f.FieldType == roleType);

        CustomRole role = GetRoleFromType(roleType);
        ConstructorInfo editorCtor = editorType.GetConstructor(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic, new Type[] { roleType })!;
        RoleEditor editor = (RoleEditor)editorCtor.Invoke(new object?[] {role});
        CustomRole modified = (CustomRole)editor.StartLink();

        if (isStatic) {
            typeof(StaticRoles).GetField(roleType.Name)?.SetValue(Static, modified);
            MainRoles.Replace(role, modified);
        }

        if (isExtra) {
            typeof(ExtraRoles).GetField(roleType.Name)?.SetValue(Special, modified);
            SpecialRoles.Replace(role, modified);
        }

        AllRoles.Replace(role, modified);
    }

    internal static void RemoveEditor(Type editorType)
    {
        if (!editorType.IsAssignableTo(typeof(RoleEditor)))
            throw new ArgumentException("Editor Type MUST be a subclass of AbstractBaseRole.RoleEditor");
        Type roleType = editorType.BaseType!.DeclaringType!;
        bool isStatic = typeof(StaticRoles).GetFields().Any(f => f.FieldType == roleType);
        bool isExtra = typeof(ExtraRoles).GetFields().Any(f => f.FieldType == roleType);

        CustomRole role = GetRoleFromType(roleType);
        RoleEditor editor = role.Editor;

        if (isStatic) {
            typeof(StaticRoles).GetField(roleType.Name)?.SetValue(Static, editor.FrozenRole);
            MainRoles.Replace(role, (CustomRole)editor.FrozenRole);
        }

        if (isExtra) {
            typeof(ExtraRoles).GetField(roleType.Name)?.SetValue(Special, editor.FrozenRole);
            SpecialRoles.Replace(role, (CustomRole)editor.FrozenRole);
        }

        AllRoles.Replace(role, (CustomRole)editor.FrozenRole);
    }

    public class StaticRoles
    {

        public Impostor Impostor = new Impostor();
        public Morphling Morphling = new Morphling();
        public Miner Miner = new Miner();
        public Mafia Mafia = new Mafia();
        public Sniper Sniper = new Sniper();
        public BountyHunter BountyHunter = new BountyHunter();
        public Janitor Janitor = new Janitor();
        public Disperser Disperser = new Disperser();
        public Consort Consort = new Consort();
        public FireWorks FireWorks = new FireWorks();
        public Freezer Freezer = new Freezer();
        public Puppeteer Puppeteer = new Puppeteer();
        public Vampire Vampire = new Vampire();
        public SerialKiller SerialKiller = new SerialKiller();
        public Swooper Swooper = new Swooper();
        public Mare Mare = new Mare();
        public Camouflager Camouflager = new Camouflager();
        public Grenadier Grenadier = new Grenadier();
        public TimeThief TimeThief = new TimeThief();
        public Warlock Warlock = new Warlock();
        public Witch Witch = new Witch();
        public Coven Coven = new Coven();
        public BloodKnight BloodKnight = new BloodKnight();

        public LastImpostor LastImpostor = new LastImpostor();
        public Ninja Ninja = new Ninja();

        public Vigilante Vigilante = new Vigilante();
        public Crewmate Crewmate = new Crewmate();
        public Engineer Engineer = new Engineer();
        public Scientist Scientist = new Scientist();

        public Bastion Bastion = new Bastion();
        public Bodyguard Bodyguard = new Bodyguard();
        public Child Child = new Child();
        public Crusader Crusader = new Crusader();
        public Dictator Dictator = new Dictator();
        public Observer Observer = new Observer();
        public Oracle Oracle = new Oracle();
        public Sheriff Sheriff = new Sheriff();
        public Transporter Transporter = new Transporter();
        public Veteran Veteran = new Veteran();
        public Medic Medic = new Medic();
        public Investigator Investigator = new Investigator();
        public Mystic Mystic = new Mystic();
        public Mayor Mayor = new Mayor();
        public Jester Jester = new Jester();
        public CrewPostor CrewPostor = new CrewPostor();
        public Opportunist Opportunist = new Opportunist();
        public Medium Medium = new Medium();
        public SabotageMaster SabotageMaster = new SabotageMaster();


        public Glitch Glitch = new Glitch();
        public Jackal Jackal = new Jackal();
        public Arsonist Arsonist = new Arsonist();
        public Terrorist Terrorist = new Terrorist();
        public Executioner Executioner = new Executioner();
        public PlagueBearer PlagueBearer = new PlagueBearer();
        public Pestilence Pestilence = new Pestilence();
        public AgiTater AgiTater = new AgiTater();
        public Retributionist Retributionist = new Retributionist();

        public GuardianAngel GuardianAngel = new GuardianAngel();
        public Archangel Archangel = new Archangel();

        public Sidekick Sidekick = new Sidekick();
        public SchrodingerCat SchrodingerCat = new SchrodingerCat();
        public Demolitionist Demolitionist = new Demolitionist();
        public Vampiress Vampiress = new Vampiress();
        public YingYanger YingYanger = new YingYanger();


        public Amnesiac Amnesiac = new Amnesiac();
        public Survivor Survivor = new Survivor();
        public Snitch Snitch = new Snitch();
        public Speedrunner Speedrunner = new Speedrunner();
        public Trapper Trapper = new Trapper();
        public Phantom Phantom = new Phantom();
        public Hacker Hacker = new Hacker();

        public Egoist Egoist = new Egoist();

        public Necromancer Necromancer = new Necromancer();
        public Deathknight Deathknight = new Deathknight();

        public Madmate Madmate = new Madmate();
        public MadGuardian MadGuardian = new MadGuardian();
        public MadSnitch MadSnitch = new MadSnitch();
        public Parasite Parasite = new Parasite();
    }

    public class ExtraRoles
    {
        public GM GM = new GM();
        public Debugger Debugger = new Debugger();
        public Lovers Lovers = new Lovers();
        public Bait Bait = new Bait();
        public Bewilder Bewilder = new Bewilder();
        public Diseased Diseased = new Diseased();

        public Fox Fox = new();
    }
}