using System.Runtime.Serialization;

namespace TCGSim.CardResources
{
    public enum Attributes
    {
        NONE=0,
        RANGED,
        STRIKE,
        SPECIAL,
        SLASH,
        WISDOM,
        [EnumMember(Value = "Strike/Special")]
        STRIKESPECIAL,
        [EnumMember(Value = "Strike/Ranged")]
        STRIKERANGED,
        [EnumMember(Value = "Slash/Special")]
        SLASHSPECIAL,
        [EnumMember(Value = "Slash/Strike")]
        SLASHSTRIKE,
        斬,
        特,
        知,
        打,
        射,
    }
}
