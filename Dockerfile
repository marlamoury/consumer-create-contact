# Etapa 1: Construção do projeto
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copia os arquivos do projeto para o container
COPY . .

# Publica a aplicação em modo Release
RUN dotnet publish -c Release -o out

# Etapa 2: Configuração para execução
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copia os arquivos publicados para a imagem final
COPY --from=build /app/out .

# Define o ponto de entrada para executar a aplicação
ENTRYPOINT ["dotnet", "ContatosConsumers.dll"]
