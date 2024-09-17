using TechNationTesteApp.Enums;

namespace TechNationTesteApp.Models
{
    public class FiltroNotasFiscais
    {
        public int? MesEmissao { get; set; }

        public int? MesCobranca { get; set; }

        public int? MesPagamento { get; set; }

        public StatusNota? Status { get; set; }

    }
}
