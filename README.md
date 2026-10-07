# 📊 Dashboard de Vendas

Aplicação que importa um CSV de vendas pelo navegador, persiste os registros em uma API REST e exibe um dashboard interativo com tabela agregada por produto, filtro, gráfico de barras e modal de detalhes.

## 🚀 Tecnologias

| Camada | Tecnologias |
|---|---|
| API | .NET 8 (ASP.NET Core Web API), Entity Framework Core 8 + SQLite, Swagger/OpenAPI (Swashbuckle) |
| Padrões | Repository, Unit of Work, Service, Injeção de Dependência nativa do ASP.NET Core |
| Testes API | xUnit + Moq + SQLite em memória |
| Frontend | Angular 13.3.11, PrimeNG 13.0.0-rc.2 (Table, InputText, Chart), ngx-bootstrap 6.2.0 (Modal, Alert), Chart.js 3.9.1 |
| Testes Frontend | Jasmine + Karma |

> **Sobre ".NET Framework":** o enunciado cita ".NET Framework". Foi usado **.NET 8** (a plataforma atual, multiplataforma e com suporte do EF Core 8) em vez do .NET Framework 4.x legado. A arquitetura (controllers, repositório, DI, EF, xUnit) é a mesma.

## 📦 Estrutura

```
backend/
  Vendas.sln
  Vendas.Api/            Controllers, Services, Repositories, Data (DbContext + UnitOfWork), Dtos, Models
  Vendas.Api.Tests/      Testes unitários (serviço, repositório, controller)
frontend/
  src/app/
    core/models/venda.model.ts
    core/services/vendas.service.ts          acesso HTTP à API
    core/services/csv-parser.service.ts      parse manual com split + validação
    core/services/agregacao.service.ts       agrupamento por produto e filtro
    features/dashboard/                      dashboard.module.ts, dashboard.component.ts/.html
    features/upload/upload.component.ts      UPLOAD do CSV (FileReader)
    core/utils/decodificar-texto.ts          decodifica UTF-8 ou ANSI (Windows-1252)
    shared/pipes/currency-br.pipe.ts         formatação R$ pt-BR
    shared/components/page-header/           cabeçalho de página
    shared/shared.module.ts
  src/assets/vendas.csv                      arquivo de exemplo
```

## Onde fica o upload e quais componentes são usados

- **Upload:** `frontend/src/app/features/upload/upload.component.ts` (`<app-upload>`), exibido no topo do dashboard. Lê o arquivo com `FileReader` e emite o texto; o parse é feito por `CsvParserService`.
- **Dashboard:** `features/dashboard/dashboard.component.*` reúne os componentes abaixo.
- **PrimeNG:** `p-table` (ordenação e paginação), `pInputText` (filtro por produto), `p-chart` (barras), `pButton` (exportar).
- **ngx-bootstrap:** `bsModalService` (modal de detalhes do produto) e `<alert>` (avisos de erro/sucesso).
- **Próprios:** `app-page-header`, pipe `currencyBr`.

## 📋 Requisitos

- .NET SDK 8
- Node.js 16.x (>= 16.10) e npm 8
- Google Chrome/Chromium (para os testes do Angular)

## 🔧 Instalação e execução

**1. API** (terminal 1):

```bash
cd backend/Vendas.Api
dotnet restore
dotnet run
```

API em `http://localhost:5000` e Swagger em `http://localhost:5000/swagger`. O banco SQLite (`vendas.db`) é criado automaticamente.

**2. Frontend** (terminal 2):

```bash
cd frontend
npm install
npm start
```

Acesse `http://localhost:4200` e selecione o arquivo `frontend/src/assets/vendas.csv`. Se a API usar outra URL, altere `apiUrl` em `frontend/src/environments/environment.ts`.

> O `.npmrc` define `legacy-peer-deps=true` porque o ngx-bootstrap 6.2.0 (versão exigida) declara peer dependencies de Angular anteriores ao 13.
> O `@angular/cdk` (13.3.9) está listado explicitamente porque o PrimeNG 13 depende dele (`ScrollingModule`) e, com `legacy-peer-deps`, o npm não instala peer dependencies sozinho.

## 🧪 Testes

```bash
# API
cd backend
dotnet test

# Frontend
cd frontend
npm run test:ci      # headless, uma execução
npm test             # modo watch
```

O frontend cobre parsing (cabeçalho, tipos, datas, erros por linha, ids repetidos), agrupamento/filtro e o pipe monetário. A API cobre serviço (regras de negócio), repositório (filtros por produto, quantidade e data) com SQLite em memória, Unit of Work e controller.

## 📄 Formato do CSV

```
id_venda,produto,quantidade,preco_unitario,data_venda
1,Camiseta,3,49.90,06/09/2026
2,Calça,2,99.90,07/09/2026
3,Camiseta,1,49.90,06/09/2026
4,Tênis,1,699.90,10/09/2026
```

- A exportação gera CSV em UTF-8 com BOM, para o Excel exibir ç e acentos corretamente.
- Aceita CSV em UTF-8 ou ANSI/Windows-1252 (acentos e ç são preservados nos dois casos).
- Datas em `dd/MM/aaaa`; preço com ponto decimal; o nome do produto não pode conter vírgula (parse com `split`).
- Linhas inválidas geram avisos (com o número da linha) e são ignoradas; cabeçalho inválido interrompe a importação.

## 🗂 Endpoints

| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/vendas` | Lista; filtros: `produto`, `quantidadeMinima`, `quantidadeMaxima`, `dataInicio`, `dataFim` (`yyyy-MM-dd`) |
| GET | `/api/vendas/{id}` | Consulta por `id_venda` |
| POST | `/api/vendas` | Cria uma venda |
| POST | `/api/vendas/importacao` | Importa lote do CSV (atômico: se algum ID já existir, nada é gravado — `409`) |
| PUT | `/api/vendas/{id}` | Atualiza |
| DELETE | `/api/vendas/{id}` | Remove |

JSON em snake_case (`id_venda`, `preco_unitario`, `data_venda`), igual ao CSV. Erros de domínio retornam `ProblemDetails` (400/404/409).

## Decisões de projeto

- **Repository + Unit of Work:** `IVendaRepository` encapsula as consultas; `IUnitOfWork.CommitAsync()` confirma tudo em uma transação. O `VendaService` depende apenas das interfaces (injetadas via DI), o que permite testes unitários com Moq.
- **Tabela agregada:** a coluna "Data da Venda" mostra a data da venda mais recente do produto; o modal lista cada venda individual.
- **Fluxo de importação:** o CSV é exibido imediatamente e enviado à API em lote. Se a API estiver indisponível ou houver IDs já cadastrados, um aviso informa que os dados estão apenas locais.
- **Bônus implementados:** persistência do último CSV no `localStorage`, ordenação e paginação da tabela, exportação dos agregados para CSV.
- **Valores monetários:** acumulados em centavos no frontend para evitar erros de ponto flutuante.
