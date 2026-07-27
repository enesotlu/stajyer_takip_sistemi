# ---- 1. asama: derleme (SDK imaji - buyuk ama sadece build sirasinda kullanilir) ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Once sadece .csproj dosyalarini kopyalayip restore ediyoruz. Boylece sadece
# .cs/.cshtml dosyalari degistiginde (csproj/paketler ayni kalinca) Docker bu
# restore adimini tekrar calistirmaz, cache'ten okur - build cok hizlanir.
COPY ["StajyerTakip.sln", "."]
COPY ["src/StajyerTakip.Web/StajyerTakip.Web.csproj", "src/StajyerTakip.Web/"]
COPY ["src/StajyerTakip.Business/StajyerTakip.Business.csproj", "src/StajyerTakip.Business/"]
COPY ["src/StajyerTakip.DataAccess/StajyerTakip.DataAccess.csproj", "src/StajyerTakip.DataAccess/"]
COPY ["src/StajyerTakip.Core/StajyerTakip.Core.csproj", "src/StajyerTakip.Core/"]
RUN dotnet restore "src/StajyerTakip.Web/StajyerTakip.Web.csproj"

# Simdi geri kalan tum kaynak kodu kopyalayip yayinlanabilir (publish) hale getiriyoruz.
COPY . .
WORKDIR /src/src/StajyerTakip.Web
RUN dotnet publish -c Release -o /app/publish --no-restore

# ---- 2. asama: calistirma (kucuk runtime imaji - SDK yok, sadece calistirmak icin gerekenler) ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# .NET 8'in resmi container imajlari varsayilan olarak 8080 portunu dinler.
EXPOSE 8080
ENTRYPOINT ["dotnet", "StajyerTakip.Web.dll"]
