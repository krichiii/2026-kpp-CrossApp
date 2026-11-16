# CrossApp
Наскрізний проєкт з крос-платформного програмування.

**Предметна область**: Склад

**Сутності**: Product (товар), StockBatch (партія), Warehouse (склад), Movement (переміщення). 

**Призначення**: облік залишків товарів по партіях

**Сервіс**: CatalogService.cs

**Інтерфейс**: ICatalogStore.cs 

**Реалізації**:
- FileCatalogStore.cs
- InMemoryCatalogStore.cs

## Конфігурація та параметри (Lab 09)

Конфігурація будується як послідовність джерел (ConfigurationBuilder):
1. `appsettings.json` — базові налаштування;
2. `appsettings.{Environment}.json` — налаштування середовища (за замовчуванням `Development`);
3. `UserSecrets` (локальні секрети розробника поза git-репозиторієм);
4. Змінні середовища з префіксом `CROSSAPP_` (перекривають значення з файлів);
5. Спеціальна змінна середовища `CROSSAPP_DATA_PATH` (перекриває `Storage:DataPath`);
6. Аргументи командного рядка (`AddCommandLine(args)`).

### Профілі середовища
- **Development** (`appsettings.Development.json`): рівень логування `Debug`, детальне виведення.
- **Production** (`appsettings.json`): рівень логування `Information`.
Перемикання профілю здійснюється через змінну оточення `CROSSAPP_ENVIRONMENT` (наприклад, `CROSSAPP_ENVIRONMENT=Production`).

### Таблиця ключів конфігурації

| Ключ конфігурації | JSON-файл | Змінна середовища | Опис | Значення за замовчуванням |
| :--- | :--- | :--- | :--- | :--- |
| `EnvironmentName` | `appsettings.json` | `CROSSAPP_EnvironmentName` / `CROSSAPP_ENVIRONMENT` | Назва середовища | `Development` |
| `Logging:MinimumLevel` | `appsettings.json` / `appsettings.Development.json` | `CROSSAPP_Logging__MinimumLevel` | Мінімальний рівень логування | `Information` (Prod) / `Debug` (Dev) |
| `Storage:DataPath` | `appsettings.json` | `CROSSAPP_Storage__DataPath` або `CROSSAPP_DATA_PATH` | Каталог для збереження файлу даних | `data` |
| `Storage:FileName` | `appsettings.json` | `CROSSAPP_Storage__FileName` | Назва файлу даних | `items.json` |
| `Integration:ApiKey` | `secrets.json` (User Secrets) | `CROSSAPP_Integration__ApiKey` | Секретний ключ інтеграції (не зберігається в git) | `local-dev-key` |

### Робота з User Secrets

Ініціалізація та налаштування секрету для локальної розробки:
```bash
# Ініціалізація сховища секретів (додає UserSecretsId у Cli.csproj)
dotnet user-secrets init --project src/Cli

# Встановлення ключа
dotnet user-secrets set "Integration:ApiKey" "local-dev-key" --project src/Cli

# Перегляд наявних секретів
dotnet user-secrets list --project src/Cli
```

### Приклади перевизначення параметрів

#### PowerShell (Windows):
```powershell
# Зміна каталогу даних через спеціальну змінну:
$env:CROSSAPP_DATA_PATH = "C:\Users\User\data"
dotnet run --project src/Cli -- export

# Зміна назви файлу даних через префіксну змінну (подвійне підкреслення замість ':'):
$env:CROSSAPP_Storage__FileName = "custom-items.json"
dotnet run --project src/Cli

# Зміна середовища:
$env:CROSSAPP_ENVIRONMENT = "Production"
dotnet run --project src/Cli

# Задання API-ключа через змінну середовища (наприклад, для CI):
$env:CROSSAPP_Integration__ApiKey = "prod-super-secret-key"
dotnet run --project src/Cli

# Очищення змінних:
Remove-Item Env:\CROSSAPP_DATA_PATH
Remove-Item Env:\CROSSAPP_Storage__FileName
Remove-Item Env:\CROSSAPP_ENVIRONMENT
Remove-Item Env:\CROSSAPP_Integration__ApiKey
```

#### Bash (Linux / macOS):
```bash
# Зміна каталогу даних через спеціальну змінну:
CROSSAPP_DATA_PATH=/tmp/data dotnet run --project src/Cli -- export

# Зміна назви файлу даних:
CROSSAPP_Storage__FileName=custom-items.json dotnet run --project src/Cli

# Перевизначення середовища:
CROSSAPP_ENVIRONMENT=Production dotnet run --project src/Cli

# Задання API-ключа через змінну оточення:
CROSSAPP_Integration__ApiKey=ci-api-key dotnet run --project src/Cli
```

#### Аргументи командного рядка:
```
dotnet run --project src/Cli -- --Storage:DataPath=custom_data --Storage:FileName=items_cli.json
```

## Таблиця розмірів публікацій (+ Додаткові завдання 1,2 [lab02] )

| RID | Режим | Додаткові режими Publish | Розмір publish | К-сть файлів | Потрібен runtime |
| :--- | :--- | :--- | :--- | :--- | :--- |
| linux-x64 | self-contained | - | 77 MiB | 191 | ні |
| linux-x64 | fw-dependent | - | 124 KiB | 7 | так(.NET 9.0) |
| win-x64 | self-contained | - | 75 MiB | 191 | ні |
| win-x64 | fw-dependent | - | 204 KiB | 7 | так(.NET 9.0) |
| linux-x64 | self-contained | SingleFile | 68 MiB | 3 | ні |
| linux-x64 | fw-dependent | SingleFile | 112 KiB | 3 | так(.NET 9.0) |
| win-x64 | self-contained | SingleFile | 68 MiB | 3 | ні |
| win-x64 | fw-dependent | SingleFile | 200 KiB | 3 | так(.NET 9.0) |
| linux-x64 | self-contained | Trimmed | 23 MiB | 32 | ні |
| win-x64 | self-contained | Trimmed | 20 MiB | 31 | ні |

 ## Структура проекту
```
.
└── CrossApp/
    ├── src/
    │   ├── Cli/
    │   │   ├── appsettings.json
    │   │   ├── appsettings.Development.json
    │   │   ├── Cli.csproj
    │   │   └── Program.cs
    │   └── Core/
    |       ├── Abstractions/
    |       |   ├── ICatalogImporter.cs
    |       |   ├── ICatalogStore.cs
    |       |   └── INotificationSink.cs
    |       ├── Configuration/
    |       |   └── AppOptions.cs 
    │       ├── Domain/
    │       │   └── Product.cs
    │       ├── Dto/
    |       |   └── ProductDto.cs
    |       ├── Events/
    |       |   ├── CatalogChangedEventArgs.cs
    |       |   └── CatalogChangeKind.cs
    |       ├── Notifications/
    |       |   ├── CompositeNotificationSink.cs
    |       |   ├── ConsoleSink.cs
    |       |   └── FileSink.cs
    |       ├── Reports/
    |       |   ├── EnumerableExtensions.cs
    |       |   ├── ReportService.cs
    |       |   └── StockLine.cs
    |       ├── Services/
    |       |   └── CatalogService.cs
    │       ├── Storage/
    |       |   ├── CachingCatalogStore.cs
    |       |   ├── FileCatalogStore.cs
    |       |   ├── InMemoryCatalogStore.cs
    │       │   └── JsonCatalogImporter.cs
    │       ├── Core.csproj
    │       └── DomainException.cs
    ├── tests/
    |   └── Core.Tests/
    |       ├── CatalogServiceTests.cs
    |       ├── Core.Tests.csproj
    |       ├── DataTests.cs
    |       ├── ImportTests.cs
    |       └── ReportServiceTests.cs
    ├── .gitignore
    ├── CrossApp.sln
    └── README.md
```

 ## Запуск
 ```bash
 dotnet build
 dotnet run --project src/Cli -- export
 ```
 ### або
 ```bash
 dotnet publish src/Cli -c Release -r <platform> --self-contained true

 # Для Windows:
 .\src\Cli\bin\Release\net10.0\win-x64\publish\Cli.exe

 # Для Linux/MacOs:
 ./src/Cli/bin/Release/net10.0/linux-x64/publish/Cli
 ```
 ## Тестування
 ```bash
 dotnet test

 # Перевірка скасувань:
 - var dataPath = Path.Combine(Directory.GetCurrentDirectory(), "data", "non-existant.json");
 - var dataPath = Path.Combine(Directory.GetCurrentDirectory(), "data", "catalog-error.json");
 - using var timeoutCts = new CancellationTokenSource(TimeSpan.FromMilliseconds(5));
 ```
 
 ## Середовище
 .NET SDK 9.0, EndeavourOS Titan Nova x64 (Unix 7.2.3.1)

 ## Перелік реалізованих тестувань інваріантів
 - видача більша за залишок (InvalidOperationException)
 - порожній SKU (ArgumentException)
 - від'ємний залишок (ArgumentOutOfRangeException)
 - порожня назва (ArgumentException)
 - неприпустимий перехід стану замовлення OrderStatus (InvalidOperationException) (дод завдання №3)
 - перевищення кількості товару в замовленні (InvalidOperationException) (дод завдання №2)
 - недопустима кількість товару в замовленні (ArgumentOutOfRangeException) (дод завдання №2)
 - спроба додати товар до підтвержденого замовлення (InvalidOperationException) (дод завдання №3)