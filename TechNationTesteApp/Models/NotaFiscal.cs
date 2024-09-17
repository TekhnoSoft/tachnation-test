using TechNationTesteApp.Enums;

namespace TechNationTesteApp.Models
{
    public class NotaFiscal
    {
        public int Id { get; set; }  

        public string NomePagador { get; set; }  

        public string NumeroNota { get; set; } 

        public DateTime DataEmissao { get; set; }  

        public DateTime? DataCobranca { get; set; }  

        public DateTime? DataPagamento { get; set; }  

        public decimal Valor { get; set; }  

        public string DocumentoNota { get; set; }  

        public string DocumentoBoleto { get; set; }  

        public StatusNota Status { get; set; }  
    }
}
