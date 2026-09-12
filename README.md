# CrossApp
 Наскрізний проєкт з крос-платформного програмування.
 
 **Предметна область**: Склад
 
 **Сутності**: Product (товар), StockBatch (партія), Warehouse (склад), Movement (переміщення). 
 
**Призначення**: облік залишків товарів по партіях

 ## Таблиця розмірів публікацій

| RID | Режим | Розмір publish | Потрібен runtime |
| :--- | :--- | :--- | :--- |
| linux-x64 | self-contained | 77 МіБ | ні |
| linux-x64 | framework-dependent | 124 KіБ | так |
| win-x64 | self-contained | 75 МіБ | ні |
| win-x64 | framework-dependent | 204 KiB | так |

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
