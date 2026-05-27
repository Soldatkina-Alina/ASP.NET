# 1. Используем официальный образ .NET SDK для сборки приложения
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /srcApp

# 2. Копируем все файлы проекта и восстанавливаем зависимости
COPY *.sln .
COPY */*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p ${file%.*} && mv $file ${file%.*}; done
RUN dotnet restore

# 3. Копируем остальные файлы проекта и публикуем приложение в режиме Release
COPY . ./app
WORKDIR /srcApp/app
RUN dotnet publish -c Release -o out

# 4. Используем официальный runtime-образ для запуска приложения
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /srcApp

# 5. Копируем собранное приложение из предыдущего этапа
COPY --from=build /srcApp/app/out ./

# 6. Открываем порт
EXPOSE 8080

# 7. Указываем команду запуска приложения
ENTRYPOINT ["dotnet", "PromoCodeFactory.WebHost.dll"]