# Sprawdzenie edycji PZ

```powershell
dotnet run --project Rogbau.Api/Tests/PzEditChecks/PzEditChecks.csproj -- ./Rogbau.Api/Rogbau.Api
```

Uruchom z głównego katalogu Rogbau. Test korzysta z konfiguracji SQL API, ale
tworzy i usuwa wyłącznie osobną bazę `Rogbau_PzEditChecks_<losowy identyfikator>`.

Sprawdza korektę zgłoszenia, odbioru i złomu, zamianę budowy i sprzętu,
edycję dokumentu w transporcie, uprawnienia oraz wycofanie błędnego zapisu.

## Jak czytać handler

1. Weryfikacja uprawnień, dokumentu, budowy i ilości.
2. Wspólna lista sprzętu ze starej i nowej wersji.
3. Dla każdego sprzętu ustalenie starych i nowych ilości.
4. Korekta magazynu i złomu (zakończony PZ) lub transportu.
5. Korekta budowy: stara ilość wraca, nowa schodzi; przy tej samej budowie tylko różnica.
6. Aktualizacja pozycji dokumentu i zapis w jednej transakcji.

`ChangeSiteStock` dostosowuje otwarte przydziały od chwili korekty. Zamknięte
okresy pozostają bez zmian. Handler nie odtwarza wstecz późniejszych dokumentów
ani dawnych rozliczeń. Odrzuca zmianę, jeśli aktualne stany stałyby się ujemne.
