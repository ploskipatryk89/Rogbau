# Kontrola zamówień i ruchu sprzętu

Testy korzystają z połączenia SQL Server z konfiguracji API, ale tworzą osobną bazę
`Rogbau_WorkflowChecks_<losowy identyfikator>`. Po zakończeniu usuwają wyłącznie tę bazę.
Nie zmieniają danych w bazie projektu. Konto SQL musi mieć prawo tworzenia baz.

Uruchom z katalogu `Rogbau.Api`:

```powershell
dotnet build Tests/WorkflowChecks/WorkflowChecks.csproj -p:UseAppHost=false -p:OutputPath=bin/WorkflowChecks
dotnet Tests/WorkflowChecks/bin/WorkflowChecks/WorkflowChecks.dll ./Rogbau.Api
```

Testy sprawdzają tworzenie zamówień, częściową i pełną realizację przez WZ,
powiązane dokumenty, stany sprzętu, wagę, zapis powiadomień i ich wysyłkę przez
SignalR (z zastępczym klientem), dostęp kierowników do dokumentów swoich budów,
blokadę wysyłania z cudzej budowy oraz potwierdzanie odbioru.

Klienci SignalR i bieżący użytkownik są zastępowani na potrzeby testów; połączenie
przeglądarki z rzeczywistym hubem nie jest częścią tej kontroli.

Kontrola obejmuje także zapis stawek dziennych z czterema miejscami po przecinku
i rozliczenia stawek poniżej grosza, blokadę odebrania roli oraz usunięcia ostatniego
administratora, zachowanie danych i sesji po odrzuconej zmianie oraz równoczesne
próby odebrania roli dwóm administratorom.
