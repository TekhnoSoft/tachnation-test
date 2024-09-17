namespace TechNationTesteApp.Models
{
    public class IndicadoresDashboard
    {
        public decimal ValorTotalEmitido { get; set; }

        public decimal ValorTotalNaoCobrado { get; set; }

        public decimal ValorTotalInadimplencia { get; set; }

        public decimal ValorTotalAVencer { get; set; }

        public decimal ValorTotalPago { get; set; }

        public List<IndicadorMensal> InadimplenciaMensal { get; set; }

        public List<IndicadorMensal> ReceitaMensal { get; set; }
    }
}
