FROM mcr.microsoft.com/dotnet/sdk:6.0 as build-env
WORKDIR /app

COPY ./Splitted-backend/*.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish Splitted-backend -c Release -o ./Splitted-backend/out

FROM ubuntu:jammy as build-python
COPY --from=build-env /app/AIService/requirements.txt .

RUN apt-get -y update --fix-missing && apt-get install -y python3.10 && apt-get install -y python3-pip
RUN pip install -r requirements.txt

FROM mcr.microsoft.com/dotnet/aspnet:6.0-jammy
WORKDIR /app

RUN mkdir -p ../ExternalServices/EmailSender/Templates
RUN mkdir -p ../usr/lib/python3.10
RUN mkdir -p ../usr/local/lib/python3.10
RUN mkdir -p ../AIService

COPY --from=build-env /app/Splitted-backend .
COPY --from=build-env /app/ExternalServices/EmailSender/Templates ../ExternalServices/EmailSender/Templates
COPY --from=build-env /app/AIService ../AIService

COPY --from=build-python /usr/lib/x86_64-linux-gnu/libpython3.10.so ../usr/lib/x86_64-linux-gnu
COPY --from=build-python /usr/lib/python3.10 ../usr/lib/python3.10
COPY --from=build-python /usr/local/lib/python3.10 ../usr/local/lib/python3.10

RUN apt-get -y update && apt-get install -y locales && apt-get install -y locales libexpat1
RUN locale-gen pl_PL.UTF-8

ENV DllPath=../usr/lib/x86_64-linux-gnu/libpython3.10.so

ENTRYPOINT ["dotnet", "./out/Splitted-backend.dll"]

