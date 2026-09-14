# CrossApp
 Наскрізний проєкт з крос-платформного програмування.
 
 **Предметна область**: Склад
 
 **Сутності**: Product (товар), StockBatch (партія), Warehouse (склад), Movement (переміщення). 
 
**Призначення**: облік залишків товарів по партіях

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
    │   │   ├── Cli.csproj
    │   │   └── Program.cs
    │   └── Core/
    │       ├── Domain/
    │       │   └── .gitkeep
    │       ├── Dto/
    │       │   └── .gitkeep
    │       ├── Storage/
    │       │   └── .gitkeep
    │       ├── Core.csproj
    │       └── EnviromentInfo.cs
    ├── .gitignore
    ├── CrossApp.sln
    └── README.md
```


 ## Запуск
 ```
 dotnet build
 dotnet run --project src/Cli
 ```
 ### або
 ```
 dotnet publish src/Cli -c Release -r <platform> --self-contained true

 # Для Windows:
 .\src\Cli\bin\Release\net10.0\win-x64\publish\Cli.exe

 # Для Linux/MacOs:
 ./src/Cli/bin/Release/net10.0/linux-x64/publish/Cli
 ```
 ## Середовище
 .NET SDK 9.0, EndeavourOS Titan Nova x64 (Unix 7.2.3.1)
