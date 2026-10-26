# Edycja WZ i przesunięć

Uruchomienie z głównego folderu projektu:

```powershell
dotnet build Rogbau.Api/Tests/MovementEditChecks/MovementEditChecks.csproj -p:UseAppHost=false -p:NuGetAudit=false -o tmp/movement-edit-checks
dotnet tmp/movement-edit-checks/MovementEditChecks.dll Rogbau.Api/Rogbau.Api
```

Testy korzystają z serwera wskazanego w konfiguracji API, ale tworzą osobną bazę
`Rogbau_MovementEditChecks_<losowy identyfikator>`. Baza jest usuwana po testach.
Nie używają bazy aplikacji do zapisu. Wymagają dostępu do SQL Server i prawa
utworzenia oraz usunięcia bazy testowej.

23 sprawdzenia obejmują dokumenty w transporcie i zakończone, sprzęt, ilości,
budowy, odwrócenie kierunku przesunięcia, przydziały, wycofanie błędnej korekty,
uprawnienia admina, walidację i powiązanie WZ z zamówieniem.

Edycja koryguje aktualne stany i aktywne przydziały od chwili korekty.
Nie odtwarza późniejszych dokumentów ani wcześniejszych okresów kosztów.
Zmiana powodująca ujemny aktualny stan jest odrzucana.
WZ można pozostawić przy istniejącym zamówieniu albo odłączyć; nie można
przepiąć go do innego zamówienia w tym formularzu. Status starego zamówienia
jest ponownie obliczany po zmianie ilości lub odłączeniu dokumentu.
