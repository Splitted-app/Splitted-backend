FROM mcr.microsoft.com/dotnet/sdk:6.0 as build-env
WORKDIR /app

COPY ./Splitted-backend/*.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish Splitted-backend -c Release -o ./Splitted-backend/out

FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
RUN mkdir -p ../ExternalServices/EmailSender/Templates
COPY --from=build-env /app/Splitted-backend .
COPY --from=build-env /app/ExternalServices/EmailSender/Templates ../ExternalServices/EmailSender/Templates
ENTRYPOINT ["dotnet", "./out/Splitted-backend.dll"]

RUN apt-get clean && apt-get update && apt-get install -y locales
RUN locale-gen pl_PL.UTF-8