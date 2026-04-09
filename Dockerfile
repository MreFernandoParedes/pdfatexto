FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY PDFATEXTO.Web/PDFATEXTO.Web.csproj PDFATEXTO.Web/
RUN dotnet restore PDFATEXTO.Web/PDFATEXTO.Web.csproj

COPY PDFATEXTO.Web/. PDFATEXTO.Web/
RUN dotnet publish PDFATEXTO.Web/PDFATEXTO.Web.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "PDFATEXTO.Web.dll"]
