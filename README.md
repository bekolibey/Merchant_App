# Merchant App

Angular frontend ve .NET Web API backend içeren full stack merchant uygulaması.

## Yapı

- `MerchantAppClient/`: Angular istemci
- `MerchantApp.WebAPI/`: ana Web API
- `merchantapp.application/`: application katmanı
- `merchantapp.Infrastructure/`: EF Core ve altyapı katmanı
- `merchant.domain/`: domain modelleri
- `son_calisma_merchantapp.sln`: solution dosyası

## Gereksinimler

- Node.js + npm
- .NET 10 SDK

## Frontend

```bash
cd MerchantAppClient
npm install
npm run build
npm start
```

## Backend

```bash
dotnet build son_calisma_merchantapp.sln
dotnet run --project MerchantApp.WebAPI/MerchantApp.WebAPI.csproj
```

## Doğrulama

Bu repo GitHub'a alınmadan önce aşağıdaki kontroller çalıştırıldı:

- `npm run build` (`MerchantAppClient`)
- `dotnet build son_calisma_merchantapp.sln`
- `dotnet test son_calisma_merchantapp.sln`

Not: repo içinde şu an ayrı frontend spec dosyası veya .NET test projesi bulunmuyor; bu yüzden temel doğrulama build ve solution test komutlarıyla yapıldı.
