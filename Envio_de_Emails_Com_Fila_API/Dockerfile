FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Envio_de_Emails_Com_Fila_API/ Envio_de_Emails_Com_Fila_API/
COPY Envio_de_Emails_Com_Fila_Shared/ Envio_de_Emails_Com_Fila_Shared/

# limpa build antigo
RUN rm -rf /src/**/bin /src/**/obj

WORKDIR /src/Envio_de_Emails_Com_Fila_API

RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Envio_de_Emails_Com_Fila_API.dll"]