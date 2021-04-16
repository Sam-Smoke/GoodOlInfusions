using BepInEx;
using BepInEx.Configuration;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.Orbs;
using UnityEngine;

namespace SamSmoke
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("net.rgh-group.goodolinfusions", "Good Ol' Infusions", "1.0.0")]
    public class GoodOlInfusions : BaseUnityPlugin
    {
        public static ConfigEntry<int> MaxInfusionHealthLimit { get; set; }
        public static ConfigEntry<int> OrbHealthGain { get; set; }
        public static ConfigFile GoodOlInfusionsConfig { get; set; } 

        public void Awake()
        {
            GoodOlInfusionsConfig = new ConfigFile(Paths.ConfigPath + "\\GoodOlInfusions.cfg", true);

            MaxInfusionHealthLimit = GoodOlInfusionsConfig.Bind<int>("Values", "MaxHealthLimitForInfusions", 0, "The maximum amount of health per infusion a character gets, set to 0 for no limit.");
            OrbHealthGain = GoodOlInfusionsConfig.Bind<int>("Values", "OrbHealthGainMultiplier", 1, "The amount each infusion orb adds to the max health of a character.");


            IL.RoR2.GlobalEventManager.OnCharacterDeath += (il) =>
            {
                ILCursor c = new ILCursor(il);

                c.GotoNext(
                    x => x.MatchLdloc(36),
                    x => x.MatchLdcI4(100),
                    x => x.MatchMul(),
                    x => x.MatchStloc(50)
                    );

                if (MaxInfusionHealthLimit.Value != 0)
                {
                    c.Index += 3;

                    c.Emit(OpCodes.Ldc_I4, MaxInfusionHealthLimit.Value / 100);
                    c.Emit(OpCodes.Mul);
                }
                else
                {
                    c.Index += 4;

                    c.RemoveRange(5);

                    c.Emit(OpCodes.Nop);
                    c.Emit(OpCodes.Nop);
                    c.Emit(OpCodes.Nop);
                    c.Emit(OpCodes.Ldc_I4, 0);
                    c.Emit(OpCodes.Ldc_I4, 1);

                }
                
                c.GotoNext(
                    x => x.MatchLdloc(51),
                    x => x.MatchLdloc(36),
                    x => x.MatchStfld<InfusionOrb>("maxHpValue")
                    );

                c.Index += 2;

                c.Emit(OpCodes.Ldc_I4, OrbHealthGain.Value);
                c.Emit(OpCodes.Mul);
            };

            Logger.LogMessage("Initialization completed!");
        }
    }
}