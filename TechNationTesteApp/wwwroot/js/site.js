$(document).ready(function () {


    const api = new Api('/api/');

    const toggleButton = document.querySelector("#sidebar-toggle");
    const sidebar = document.querySelector("#sidebar");
    const showSidebar = (e) => {
        sidebar.classList.contains("show")
            ? (sidebar.classList.remove("show"),
                toggleButton.classList.remove("position-fixed"))
            : (sidebar.classList.add("show"),
                toggleButton.classList.add("position-fixed"));
    };
    toggleButton.addEventListener("click", showSidebar, false);

    function popularFiltrosMeses() {
        const meses = [
            "Janeiro", "Fevereiro", "Março", "Abril", "Maio", "Junho",
            "Julho", "Agosto", "Setembro", "Outubro", "Novembro", "Dezembro"
        ];
        const filtrosMes = $('#filtroMesEmissao, #filtroMesCobranca, #filtroMesPagamento');
        filtrosMes.each(function () {
            $(this).empty().append('<option value="">Todos</option>');
            meses.forEach((mes, index) => {
                $(this).append(`<option value="${index + 1}">${mes}</option>`);
            });
        });
    }

    popularFiltrosMeses();

    $('#filtroPeriodo').on('change', function () {
        getDashboard();
    });

    let inadimplenciaChart = null;
    let receitaChart = null;

    function createOrUpdateChart(ctx, data, options) {
        if (ctx.chart) {
            ctx.chart.destroy();
        }
        ctx.chart = new Chart(ctx, {
            type: 'line',
            data: data,
            options: options
        });
    }


    async function getDashboard() {
        try {

            const periodo = $('#filtroPeriodo').val();

            const dashboard = await api.get(`notas/dashboard?periodo=${periodo}`);

            $('#valorTotalEmitido').text(`R$ ${dashboard.valorTotalEmitido.toLocaleString('pt-br', { style: 'currency', currency: 'BRL' }) }`);
            $('#valorTotalNaoCobrado').text(`R$ ${dashboard.valorTotalNaoCobrado.toLocaleString('pt-br', { style: 'currency', currency: 'BRL' }) }`);
            $('#valorTotalInadimplencia').text(`R$ ${dashboard.valorTotalInadimplencia.toLocaleString('pt-br', { style: 'currency', currency: 'BRL' }) }`);
            $('#valorTotalAVencer').text(`R$ ${dashboard.valorTotalAVencer.toLocaleString('pt-br', { style: 'currency', currency: 'BRL' }) }`);
            $('#valorTotalPago').text(`R$ ${dashboard.valorTotalPago.toLocaleString('pt-br', { style: 'currency', currency: 'BRL' }) }`);

            const inadimplenciaData = {
                labels: dashboard.inadimplenciaMensal.map(ind => `Mês ${ind.mes}`),
                datasets: [{
                    label: 'Inadimplência',
                    data: dashboard.inadimplenciaMensal.map(ind => ind.valor),
                    borderColor: 'rgba(255, 99, 132, 0.2)',
                    backgroundColor: 'rgba(255, 99, 132, 0.2)',
                }]
            };

            const receitaData = {
                labels: dashboard.receitaMensal.map(ind => `Mês ${ind.mes}`),
                datasets: [{
                    label: 'Receita',
                    data: dashboard.receitaMensal.map(ind => ind.valor),
                    borderColor: 'rgba(54, 162, 235, 0.2)',
                    backgroundColor: 'rgba(54, 162, 235, 0.2)',
                }]
            };

            const chartOptions = {
                responsive: true,
                scales: {
                    x: {
                        beginAtZero: true
                    },
                    y: {
                        beginAtZero: true
                    }
                }
            };

            const inadimplenciaChartCtx = document.getElementById('inadimplenciaChart').getContext('2d');
            createOrUpdateChart(inadimplenciaChartCtx, inadimplenciaData, chartOptions);

            const receitaChartCtx = document.getElementById('receitaChart').getContext('2d');
            createOrUpdateChart(receitaChartCtx, receitaData, chartOptions);

        } catch (error) {
            console.error('Failed to fetch dashboard data:', error);
        }
    }

    const converterStatusParaString = (statusNumero) => {
        const statusMap = {
            0: "Emitida",
            1: "Cobrança Realizada",
            2: "Pagamento em Atraso",
            3: "Pagamento Realizado"
        };
        return statusMap[statusNumero] || "Desconhecido";
    }

    async function buscarNotas() {
        const filtros = {
            MesEmissao: $('#filtroMesEmissao').val() || null,
            MesCobranca: $('#filtroMesCobranca').val() || null,
            MesPagamento: $('#filtroMesPagamento').val() || null,
            Status: $('#filtroStatus').val() || null
        };

        try {
            const notas = await api.get(`notas/listar?${$.param(filtros)}`);

            const tabelaNotas = $('#tabelaNotas tbody');
            tabelaNotas.empty();

            notas.forEach(nota => {
                tabelaNotas.append(`
                    <tr>
                        <td>${nota.nomePagador}</td>
                        <td>${nota.numeroNota}</td>
                        <td>${nota.dataEmissao ? new Date(nota.dataEmissao).toLocaleDateString('pt-BR') : '(não emitido)'}</td>
                        <td>${nota.dataCobranca ? new Date(nota.dataCobranca).toLocaleDateString('pt-BR') : '(não cobrado)'}</td>
                        <td>${nota.dataPagamento ? new Date(nota.dataPagamento).toLocaleDateString('pt-BR') : '(não pago)'}</td>
                        <td>${nota?.valor?.toLocaleString('pt-br', { style: 'currency', currency: 'BRL' }) }</td>
                        <td><a href="${nota.documentoNota}" target="_blank">Ver Documento</a></td>
                        <td><a href="${nota.documentoBoleto}" target="_blank">Ver Boleto</a></td>
                        <td>${converterStatusParaString(nota.status)}</td>
                    </tr>
                `);
            });

        } catch (error) {
            console.error('Failed to fetch notas fiscais:', error);
        }
    }

    $('#btnAdicionarNota').on('click', function () {
        $('#modalAdicionarNota').modal('show');
    });

    $('#formAdicionarNota').on('submit', async function (e) {
        e.preventDefault();

        const novaNota = {
            nomePagador: $('#nomePagador').val(),
            numeroNota: $('#numeroNota').val(),
            dataEmissao: $('#dataEmissao').val(),
            valor: parseFloat($('#valor').val()),
            documentoNota: $('#documentoNota').val(),
            documentoBoleto: $('#documentoBoleto').val(),
            status: parseInt($('#status').val())
        };

        try {
            const response = await api.post('notas/criar', novaNota);
            alert(response.mensagem);
            $('#modalAdicionarNota').modal('hide');
            buscarNotas(); 
        } catch (error) {
            console.error('Failed to add nota fiscal:', error);
            alert('Erro ao adicionar nota fiscal');
        }
    });

    getDashboard();
    buscarNotas();

    $('#btnBuscarNotas').on('click', function () {
        buscarNotas();
    });

})