using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TCGSim
{
    public static class StringToEffectsParser
    {
        public static List<Effects> Parser(string effectText)
        {
            List<Effects> effects = new List<Effects>();

            if(Regex.IsMatch(effectText, @"\[DON!! x(\d+)\] This Character gains \+(\d+) power"))
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

            if(Regex.IsMatch(effectText, @"\[On Play\]\s+Give up to (\d+) rested DON!! cards to your Leader or (\d+) of your Characters\."))
            {
                var match = Regex.Match(effectText, @"\[On Play\]\s+Give up to (\d+) rested DON!! cards to your Leader or (\d+) of your Characters\.");
                if (match.Success)
                {
                    int upTo = int.Parse(match.Groups[1].Value);
                    int howMany = int.Parse(match.Groups[2].Value);
                    effects.Add(
                        new Effects
                        {
                            triggerType = EffectTriggerTypes.OnPlay,
                            cardEffect = new OnPlayGiveUpToRestedDonToLeaderOrCharacter(upTo,howMany)
                        }
                     );
                }

            }

            return effects;
        }

    }
}
