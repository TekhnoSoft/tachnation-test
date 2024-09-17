using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechNationTesteApp.Configs;
using TechNationTesteApp.Enums;
using TechNationTesteApp.Models;
using TechNationTesteApp.ViewModels;

namespace TechNationTesteApp.Controllers
{
    [ApiController]
    [Route("api/notas")]
    public class NotasFiscaisController : ControllerBase
    {
        private readonly ApplicationDbContext _context; 
       
        public NotasFiscaisController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> Dashboard([FromQuery] string periodo = "mes")
        {
            // Filtra as notas fiscais com base no período
            var notas = _context.NotasFiscais.AsQueryable();

            // Obtém a data atual
            var dataAtual = DateTime.Now;

            // Aplica o filtro de período
            switch (periodo.ToLower())
            {
                case "trimestre":
                    var trimestreAtual = (dataAtual.Month - 1) / 3 + 1;
                    notas = notas.Where(n => n.DataEmissao.Year == dataAtual.Year && (n.DataEmissao.Month - 1) / 3 + 1 == trimestreAtual);
                    break;

                case "ano":
                    notas = notas.Where(n => n.DataEmissao.Year == dataAtual.Year);
                    break;

                case "mes":
                default:
                    notas = notas.Where(n => n.DataEmissao.Year == dataAtual.Year && n.DataEmissao.Month == dataAtual.Month);
                    break;
            }

            var notasList = await notas.ToListAsync();

            var indicadores = new IndicadoresDashboard
            {
                ValorTotalEmitido = notas.Sum(n => n.Valor),
                ValorTotalNaoCobrado = notas.Where(n => !n.DataCobranca.HasValue).Sum(n => n.Valor),
                ValorTotalInadimplencia = notas.Where(n => n.Status == StatusNota.PagamentoEmAtraso).Sum(n => n.Valor),
                ValorTotalAVencer = notas.Where(n => n.Status == StatusNota.Emitida).Sum(n => n.Valor),
                ValorTotalPago = notas.Where(n => n.Status == StatusNota.PagamentoRealizado).Sum(n => n.Valor),
                InadimplenciaMensal = ObterIndicadoresMensais(notas, StatusNota.PagamentoEmAtraso),
                ReceitaMensal = ObterIndicadoresMensais(notas, StatusNota.PagamentoRealizado)
            };

            return Ok(indicadores);
        }

        [HttpGet("listar")]
        public async Task<IActionResult> ListarNotas([FromQuery] FiltroNotasFiscais filtros)
        {
            var notas = _context.NotasFiscais.AsQueryable();

            if (filtros.MesEmissao.HasValue)
                notas = notas.Where(n => n.DataEmissao.Month == filtros.MesEmissao.Value);

            if (filtros.MesCobranca.HasValue)
                notas = notas.Where(n => n.DataCobranca.HasValue && n.DataCobranca.Value.Month == filtros.MesCobranca.Value);

            if (filtros.MesPagamento.HasValue)
                notas = notas.Where(n => n.DataPagamento.HasValue && n.DataPagamento.Value.Month == filtros.MesPagamento.Value);

            if (filtros.Status.HasValue)
                notas = notas.Where(n => n.Status == filtros.Status.Value);

            var resultado = notas.Select(n => new NotaFiscalViewModel
            {
                NomePagador = n.NomePagador,
                NumeroNota = n.NumeroNota,
                DataEmissao = n.DataEmissao,
                DataCobranca = n.DataCobranca,
                DataPagamento = n.DataPagamento,
                Valor = n.Valor,
                DocumentoNota = n.DocumentoNota,
                DocumentoBoleto = n.DocumentoBoleto,
                Status = n.Status
            }).ToList();

            return Ok(resultado);
        }

        [HttpPost("criar")]
        public async Task<IActionResult> CriarNota([FromBody] NotaFiscal novaNota)
        {
            if (ModelState.IsValid)
            {
                _context.NotasFiscais.Add(novaNota);
                _context.SaveChanges();

                return Ok(new { mensagem = "Nota Fiscal criada com sucesso!" });
            }

            return BadRequest("Dados inválidos");
        }

        [HttpPut("editar/{id}")]
        public async Task<IActionResult> EditarNota(int id, [FromBody] NotaFiscal notaEditada)
        {
            var nota = _context.NotasFiscais.FirstOrDefault(n => n.Id == id);

            if (nota == null)
                return NotFound("Nota Fiscal não encontrada");

            nota.NomePagador = notaEditada.NomePagador;
            nota.NumeroNota = notaEditada.NumeroNota;
            nota.DataEmissao = notaEditada.DataEmissao;
            nota.DataCobranca = notaEditada.DataCobranca;
            nota.DataPagamento = notaEditada.DataPagamento;
            nota.Valor = notaEditada.Valor;
            nota.DocumentoNota = notaEditada.DocumentoNota;
            nota.DocumentoBoleto = notaEditada.DocumentoBoleto;
            nota.Status = notaEditada.Status;

            _context.SaveChanges();

            return Ok(new { mensagem = "Nota Fiscal editada com sucesso!" });
        }

        [HttpDelete("deletar/{id}")]
        public async Task<IActionResult> DeletarNota(int id)
        {
            var nota = _context.NotasFiscais.FirstOrDefault(n => n.Id == id);

            if (nota == null)
                return NotFound("Nota Fiscal não encontrada");

            _context.NotasFiscais.Remove(nota);
            _context.SaveChanges();

            return Ok(new { mensagem = "Nota Fiscal deletada com sucesso!" });
        }
        private List<IndicadorMensal> ObterIndicadoresMensais(IEnumerable<NotaFiscal> notas, StatusNota status)
        {
            return notas
                .Where(n => n.Status == status)
                .GroupBy(n => n.DataEmissao.Month)
                .Select(grupo => new IndicadorMensal
                {
                    Mes = grupo.Key,
                    Valor = grupo.Sum(n => n.Valor)
                })
                .ToList();
        }
    }
}
