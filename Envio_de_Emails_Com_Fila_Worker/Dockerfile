FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Envio_de_Emails_Com_Fila_Worker/ Envio_de_Emails_Com_Fila_Worker/
COPY Envio_de_Emails_Com_Fila_Shared/ Envio_de_Emails_Com_Fila_Shared/

# limpa build antigo
RUN rm -rf /src/**/bin /src/**/obj

WORKDIR /src/Envio_de_Emails_Com_Fila_Worker

RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:10.0
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Envio_de_Emails_Com_Fila_Worker.dll"]