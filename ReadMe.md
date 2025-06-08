Projeto LDS

# Aplicação Demonstradora - Grupo 27

Este projeto é uma aplicação demonstradora desenvolvida em C# com WPF e ML.NET, como parte de um trabalho acadêmico. Ele apresenta sugestões de filmes utilizando técnicas básicas de aprendizado de máquina.

## 🛠 Funcionalidades
- Interface em WPF amigável ao usuário
- Sistema de recomendação de filmes com base em palavras-chave
- Classificação com ML.NET
- Geração de PDF com as sugestões
- Integração com API de e-mail

## 📁 Estrutura principal
- `Controller/` – Controladores da aplicação
- `Model/` – Classes de dados e lógica do domínio
- `View/` – Interface gráfica (WPF)
- `Resources/` – Serviços e utilitários como MovieService
- `RecommenderModel/` – Treinamento e uso do modelo de ML

## 🧰 Requisitos para funcionamento

Certifique-se de ter instalado:
- [.NET 6.0 SDK ou superior](https://dotnet.microsoft.com/en-us/download)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) com:
  - Suporte a .NET Desktop Development
  - ML.NET Model Builder (opcional, se for treinar o modelo)
- Pacotes NuGet:
  - `Microsoft.ML`
  - `Microsoft.ML.Data`
  - `Microsoft.ML.Transforms`
  - `Microsoft.ML.TextAnalytics`
  - `PdfSharp` (para exportar PDF)

Você pode restaurar os pacotes com:
```bash
dotnet restore
