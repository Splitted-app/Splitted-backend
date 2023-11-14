FROM mcr.microsoft.com/dotnet/sdk:6.0 as build-env
WORKDIR /app

COPY ./Splitted-backend/*.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish Splitted-backend -c Release -o ./Splitted-backend/out

# FROM debian:bullseye as build-AI
# WORKDIR /app

# COPY --from=build-env /app/AIService/requirements.txt .

# RUN apt-get update && apt-get upgrade && apt-get install -y python3.10 && apt-get install -y python3-pip
# RUN pip install -r requirements.txt

FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app

RUN mkdir -p ../ExternalServices/EmailSender/Templates
RUN mkdir -p ../AIService

COPY --from=build-env /app/Splitted-backend .
COPY --from=build-env /app/ExternalServices/EmailSender/Templates ../ExternalServices/EmailSender/Templates
COPY --from=build-env /app/AIService ../AIService
# COPY --from=build-AI /usr/lib/x86_64-linux-gnu/libpython3.9.so.1.0 ../usr/lib/x86_64-linux-gnu

RUN apt-get update && apt-get upgrade && apt-get install -y python3.10 && apt-get install -y python3-pip
RUN pip install -r ../AIService/requirements.txt

RUN apt-get update && apt-get install -y locales
RUN locale-gen pl_PL.UTF-8

ENTRYPOINT ["dotnet", "./out/Splitted-backend.dll"]

