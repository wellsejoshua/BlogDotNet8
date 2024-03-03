using System.ComponentModel;

namespace BlogDotNet8.Enums
{
    public enum ModerationType
    {
    [Description("Political propaganda")]
        Political,
    [Description("Offensive language")]
        Language,
    [Description("Drug references")]
        Drugs,
    [Description("Threatening speech")]
        Threatening,
    [Description("Sexual content")]
        Sexual,
    [Description("Hate speech")]
        HateSpeech,
    [Description("Targeted shaming")]
        Shaming
    }
}
