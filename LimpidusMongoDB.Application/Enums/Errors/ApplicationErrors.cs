using System.ComponentModel;

namespace LimpidusMongoDB.Application.Enums.Errors
{
    public enum ApplicationErrors
    {
        [Description("Ocorreu um erro, por favor, tente novamente.")]
        Application_Error_General,
    }
}