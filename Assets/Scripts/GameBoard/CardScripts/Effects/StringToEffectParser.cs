using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Unity.VisualScripting;

namespace TCGSim
{
    public static class StringToEffectsParser
    {
        public static List<Effects> Parser(string effectText)
        {
            List<Effects> effects = new List<Effects>();

            if (Regex.IsMatch(effectText, @"\[DON!! x(\d+)\] This Character gains \+(\d+) power"))
            {
                var match = Regex.Match(effectText, @"\[DON!! x(\d+)\] This Character gains \+(\d+) power");
                if (match.Success)
                {
                    int donCost = int.Parse(match.Groups[1].Value);
                    int powerBonus = int.Parse(match.Groups[2].Value);

                    effects.Add(
                        new Effects
                        {
                            triggerType = EffectTriggerTypes.DON,
                            cardEffect = new DonPowerBonus(donCost, powerBonus)
                        }
                    );
                }
            }

            if (Regex.IsMatch(effectText, @"\[(.+)\]\s+Give up to (\d+) rested DON!! (cards|card) to your Leader or (\d+) of your Characters\."))
            {
                var match = Regex.Match(effectText, @"\[(.+)\]\s+Give up to (\d+) rested DON!! (cards|card) to your Leader or (\d+) of your Characters\.");
                if (match.Success)
                {
                    string effectType = Convert.ToString(match.Groups[1].Value);
                    int upTo = int.Parse(match.Groups[2].Value);
                    int howMany = int.Parse(match.Groups[4].Value);
                    EffectTriggerTypes effectTrigger = EffectTriggerTypes.NOEFFECT;
                    bool oncePerTurn = false;
                    switch (effectType.Split(']')[0].ToLower())
                    {
                        case "on play":
                            effectTrigger = EffectTriggerTypes.OnPlay;
                            break;
                        case "activate: main":
                            effectTrigger = EffectTriggerTypes.ActivateMain;
                            if (effectType.Split('[')[1].ToLower().Contains("once per turn"))
                            {
                                oncePerTurn = true;
                            }
                            break;
                        default:
                            break;
                    }
                    if (effectTrigger == EffectTriggerTypes.NOEFFECT) { return null; }

                    effects.Add(
                        new Effects
                        {
                            triggerType = effectTrigger,
                            cardEffect = new GiveUpToRestedDonToLeaderOrCharacter(upTo, howMany, oncePerTurn, false)
                        }
                     );
                }

            }

            if (Regex.IsMatch(effectText, @"\[(.+)\]\s+Give this Leader or (\d+) of your Characters up to (\d+) rested DON!! card\."))
            {
                var match = Regex.Match(effectText, @"\[(.+)\]\s+Give this Leader or (\d+) of your Characters up to (\d+) rested DON!! card\.");
                if (match.Success)
                {
                    string effectType = Convert.ToString(match.Groups[1].Value);
                    int howMany = int.Parse(match.Groups[2].Value);
                    int upTo = int.Parse(match.Groups[3].Value);
                    EffectTriggerTypes effectTrigger = EffectTriggerTypes.NOEFFECT;
                    bool oncePerTurn = false;
                    switch (effectType.Split(']')[0].ToLower())
                    {
                        case "on play":
                            effectTrigger = EffectTriggerTypes.OnPlay;
                            break;
                        case "activate: main":
                            effectTrigger = EffectTriggerTypes.ActivateMain;
                            if (effectType.Split('[')[1].ToLower().Contains("once per turn"))
                            {
                                oncePerTurn = true;
                            }
                            break;
                        default:
                            break;
                    }
                    if (effectTrigger == EffectTriggerTypes.NOEFFECT) { return null; }

                    effects.Add(
                        new Effects
                        {
                            triggerType = effectTrigger,
                            cardEffect = new GiveUpToRestedDonToLeaderOrCharacter(upTo, howMany, oncePerTurn, false)
                        }
                     );
                }
            }

            if (Regex.IsMatch(effectText, @"\[DON!! x(\d+)\] \[When Attacking\] (.*)"))
            {
                var match = Regex.Match(effectText, @"\[DON!! x(\d+)\] \[When Attacking\] (.*)");
                if (match.Success)
                {
                    EffectTriggerTypes effectType = EffectTriggerTypes.WhenAttacking;
                    int donRequirement = int.Parse(match.Groups[1].Value);
                    string effectThatHappens = Convert.ToString(match.Groups[2].Value);

                    if (Regex.IsMatch(effectThatHappens, @"Your opponent cannot activate a \[Blocker]\ Character that has (\d+) or more power during this battle."))
                    {
                        var insideMatch = Regex.Match(effectThatHappens, @"Your opponent cannot activate a \[Blocker]\ Character that has (\d+) or more power during this battle\.");
                        int overThis = int.Parse(insideMatch.Groups[1].Value);

                        effects.Add(
                        new Effects
                        {
                            triggerType = effectType,
                            cardEffect = new WhenAttackingEnemyCantBlockOver(donRequirement, overThis, false)
                        }
                     );
                    }
                    if (Regex.IsMatch(effectThatHappens, @"Up to (\d+) of your Leader or Character cards other than this card gains \+(\d+) power during this turn."))
                    {
                        var insideMatch = Regex.Match(effectThatHappens, @"Up to (\d+) of your Leader or Character cards other than this card gains \+(\d+) power during this turn\.");
                        int upTo = int.Parse(insideMatch.Groups[1].Value);
                        int plusPower = int.Parse(insideMatch.Groups[2].Value);

                        effects.Add(
                        new Effects
                        {
                            triggerType = effectType,
                            cardEffect = new WhenAttackingPlusPower(upTo, plusPower, false)
                        }
                     );
                    }
                }
            }

            if (Regex.IsMatch(effectText, @"\[(.+) x(\d+)\]\s+This Character gains \[(.+)\.*]"))
            {
                var match = Regex.Match(effectText, @"\[(.+) x(\d+)\]\s+This Character gains \[(.+)\].*");
                if (match.Success)
                {
                    string effectType = Convert.ToString(match.Groups[1].Value);
                    int howManyDon = int.Parse(match.Groups[2].Value);
                    string getsThisEffect = Convert.ToString(match.Groups[3].Value);
                    EffectTriggerTypes effectTrigger = EffectTriggerTypes.NOEFFECT;
                    switch (effectType.Split(']')[0].ToLower())
                    {
                        case "on play":
                            effectTrigger = EffectTriggerTypes.OnPlay;
                            break;
                        case "don!!":
                            effectTrigger = EffectTriggerTypes.DON;
                            break;
                        default:
                            break;
                    }
                    if (effectTrigger == EffectTriggerTypes.NOEFFECT) { return null; }


                    switch (effectTrigger)
                    {
                        case EffectTriggerTypes.NOEFFECT:
                            break;
                        case EffectTriggerTypes.OnPlay:
                            break;
                        case EffectTriggerTypes.OnKO:
                            break;
                        case EffectTriggerTypes.WhenAttacking:
                            break;
                        case EffectTriggerTypes.ActivateMain:
                            break;
                        case EffectTriggerTypes.DON:
                            effects.Add(
                                new Effects
                                {
                                    triggerType = effectTrigger,
                                    cardEffect = new SimpleDonAttachedEffect(howManyDon, getsThisEffect)
                                }
                             );
                            break;
                        default:
                            UnityEngine.Debug.LogError("Missing effect trigger type in effect parser");
                            break;
                    }
                }
            }

            if (Regex.IsMatch(effectText, @"(.*)\r?\n(.*)"))
            {
                var match = Regex.Match(effectText, @"(.*)\r?\n(.*)");
                if (match.Success)
                {
                    string firstEffect = Convert.ToString(match.Groups[1].Value);
                    string secondEffect = Convert.ToString(match.Groups[2].Value);


                    if (Regex.IsMatch(firstEffect, @"\[(\w+)\]\s+(.*)"))
                    {
                        var insideMatch = Regex.Match(effectText, @"\[(\w+)\]\s+(.*)");
                        if (insideMatch.Success)
                        {
                            string effectType = Convert.ToString(insideMatch.Groups[1].Value);
                            EffectTriggerTypes? effectTrigger = effectType.FromEnumMemberValue<EffectTriggerTypes>();

                            switch (effectTrigger)
                            {
                                case EffectTriggerTypes.NOEFFECT:
                                    break;
                                case EffectTriggerTypes.OnPlay:
                                    break;
                                case EffectTriggerTypes.OnKO:
                                    break;
                                case EffectTriggerTypes.WhenAttacking:
                                    break;
                                case EffectTriggerTypes.ActivateMain:
                                    break;
                                case EffectTriggerTypes.Rush:
                                    effects.Add(
                                        new Effects
                                        {
                                            triggerType = (EffectTriggerTypes)effectTrigger,
                                            cardEffect = new Rush()
                                        }
                                     );
                                    break;
                                case EffectTriggerTypes.DON:
                                    break;
                                default:
                                    break;
                            }
                        }
                    }


                    if (Regex.IsMatch(secondEffect, @"\[DON!! x(\d+)\] \[(.*)\] (.*)"))
                    {
                        var insideMatch = Regex.Match(secondEffect, @"\[DON!! x(\d+)\] \[(.*)\] (.*)");
                        if (insideMatch.Success)
                        {
                            int donRequirement = -1;
                            string effectType = "NOEFFECT";
                            EffectTriggerTypes? effectTrigger = EffectTriggerTypes.NOEFFECT;
                            string invokedEffectText = "";
                            string[] splittedSecondEffect = secondEffect.Split(']');
                            if (splittedSecondEffect.Length == 4)
                            {
                                donRequirement = int.Parse(insideMatch.Groups[1].Value);
                                effectType = Convert.ToString(insideMatch.Groups[2].Value).Split(']')[0];
                                effectTrigger = effectType.FromEnumMemberValue<EffectTriggerTypes>();
                                invokedEffectText = Convert.ToString(insideMatch.Groups[2].Value).Split(']')[1]+"] "+ Convert.ToString(insideMatch.Groups[3].Value);
                            }
                            if (splittedSecondEffect.Length == 3)
                            {
                                donRequirement = int.Parse(insideMatch.Groups[1].Value);
                                effectType = Convert.ToString(insideMatch.Groups[2].Value);
                                effectTrigger = effectType.FromEnumMemberValue<EffectTriggerTypes>();
                                invokedEffectText = Convert.ToString(insideMatch.Groups[3].Value);
                            }

                            switch (effectTrigger)
                            {
                                case EffectTriggerTypes.NOEFFECT:
                                    break;
                                case EffectTriggerTypes.OnPlay:
                                    break;
                                case EffectTriggerTypes.OnKO:
                                    break;
                                case EffectTriggerTypes.WhenAttacking:
                                    if (Regex.IsMatch(invokedEffectText, @" Your opponent cannot activate \[Blocker\] during this battle."))
                                    {
                                        effects.Add(
                                        new Effects
                                        {
                                            triggerType = (EffectTriggerTypes)effectTrigger,
                                            cardEffect = new WhenAttackingEnemyCantBlockOver(donRequirement, -1, false)
                                        }
                                     );
                                    }
                                    break;
                                case EffectTriggerTypes.ActivateMain:
                                    break;
                                case EffectTriggerTypes.Rush:
                                    break;
                                case EffectTriggerTypes.DON:
                                    break;
                                default:
                                    break;
                            }
                        }
                    } 
                }
            }

            return effects;
        }

        private static T? FromEnumMemberValue<T>(this string value) where T : struct, Enum
        {
            foreach (var field in typeof(T).GetFields())
            {
                var attribute = field.GetCustomAttribute<EnumMemberAttribute>();
                if (attribute != null && attribute.Value == value)
                {
                    return (T)field.GetValue(null);
                }

                if (field.Name == value)
                {
                    return (T)field.GetValue(null);
                }
            }

            return null;
        }
    }
}