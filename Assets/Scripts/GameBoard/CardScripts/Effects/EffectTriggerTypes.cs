using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TCGSim
{
    public enum EffectTriggerTypes
    {
        NOEFFECT,
        [EnumMember(Value = "On Play")]
        OnPlay,
        [EnumMember(Value = "On K.O.")]
        OnKO,
        [EnumMember(Value = "When Attacking")]
        WhenAttacking,
        [EnumMember(Value = "Activate: Main")]
        ActivateMain,
        [EnumMember(Value = "RUSH")]
        Rush,
        [EnumMember(Value = "DON!!")]
        DON
    }
}
