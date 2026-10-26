# Edycja zakupów

Uruchom z katalogu głównego projektu:

```powershell
dotnet run --project Rogbau.Api/Tests/PurchaseEditChecks/PurchaseEditChecks.csproj -- ./Rogbau.Api/Rogbau.Api
```

Test tworzy osobną bazę `Rogbau_PurchaseEditChecks_<losowy identyfikator>`
na serwerze SQL z konfiguracji API i usuwa ją po zakończeniu. Nie zmienia
danych aplikacji. Konto SQL musi mieć uprawnienia tworzenia baz.

Sprawdza zwiększenie i zmniejszenie ilości, zamianę sprzętu, przeliczenie cen,
zapis pozycji, daty i sumy zakupu oraz wycofanie całej niedozwolonej korekty.
