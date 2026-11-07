# Laboratorium 1: Pierwsza aplikacja MVC i protokół HTTP

---

## Część A – Struktura projektu

### Zadanie A1. Rola poszczególnych elementów projektu

| Element | Rola |
| :--- | :--- |
| `Program.cs` | Główny plik wejściowy aplikacji. Odpowiada za rejestrację usług w kontenerze Dependency Injection (`builder.Services`) oraz konfigurację potoku przetwarzania żądań HTTP / middleware (`app.Use...`, `app.MapControllerRoute`). |
| `Controllers/` | Katalog zawierający klasy kontrolerów, które obsługują przychodzące żądania HTTP, wykonują logikę akcji i zwracają odpowiedź (widok HTML, dane JSON lub kod statusu). |
| `Models/` | Katalog na klasy reprezentujące modele danych (np. encje, DTO) oraz modele widoków (ViewModels) przekazywane między kontrolerem a widokiem. |
| `Views/` | Katalog szablonów widoków Razor (`.cshtml`), odpowiedzialnych za warstwę prezentacji (generowanie kodu HTML). Zawiera podkatalogi powiązane z kontrolerami oraz katalog `Shared/` ze wspólnym layoutem strony. |
| `wwwroot/` | Jedyny publicznie dostępny katalog serwera, służący do bezpośredniego serwowania zasobów statycznych, takich jak arkusze stylów CSS, skrypty JavaScript, obrazy czy biblioteki zewnętrzne. |
| `appsettings.json` | Główny plik konfiguracyjny aplikacji w formacie JSON (przechowuje np. parametry połączenia z bazą danych, poziomy logowania, klucze API). |
| `Properties/launchSettings.json` | Plik konfiguracyjny środowiska uruchomieniowego używany wyłącznie lokalnie przez programistę i IDE (definiuje profile uruchomieniowe, numery portów HTTP/HTTPS, zmienne środowiskowe jak `ASPNETCORE_ENVIRONMENT`). |

---

## Część B – Podgląd HTTP w przeglądarce

### Tabela żądania strony głównej (`/`)

| Pytanie | Odpowiedź |
| :--- | :--- |
| **Metoda żądania** | `GET` |
| **Kod statusu odpowiedzi** | `200 OK` |
| **Content-Type odpowiedzi** | `text/html; charset=utf-8` |
| **Nagłówek User-Agent żądania** | Np. `Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36` *(ciąg identyfikujący przeglądarkę i system operacyjny klienta)* |
| **Ile żądań wykonała strona łącznie i dlaczego więcej niż jedno?** | Łącznie wykonano około 6–8 żądań. Dzieje się tak, ponieważ po pobraniu głównego dokumentu HTML przeglądarka analizuje jego treść i napotyka odwołania do zewnętrznych zasobów statycznych (arkusze CSS: Bootstrap i `site.css`, skrypty JS: jQuery, Bootstrap, `site.js` oraz `favicon.ico`). Aby je załadować, przeglądarka musi wysłać osobne żądanie HTTP dla każdego z tych plików. |

### Test nieistniejącego adresu (`/abc`)
* **Jaki kod statusu widzisz?** `404 Not Found`.

---

## Część C – BookController i kody statusu

### Zadanie C1. Analiza zachowania kodów statusu (`/Book/Status`)

* **Test `/Book/Status?code=404`:** Przeglądarka wyświetla swoją domyślną stronę błędu (np. w Chrome: *"Strona localhost nie znaleziona / HTTP ERROR 404"*).
* **Test `/Book/Status?code=418`:** Przeglądarka wyświetla pustą białą stronę (kod 418 "I'm a teapot").
* **Test `/Book/Status?code=500`:** Przeglądarka wyświetla błąd 500 (Internal Server Error) lub stronę błędu przeglądarki.

**Czy `StatusCode(404)` pokazuje stronę błędu aplikacji? Dlaczego nie?**
* **Odpowiedź:** Nie, `StatusCode(404)` zwraca sam nagłówek statusu HTTP z całkowicie pustym ciałem odpowiedzi (`Content-Length: 0`, brak zawartości HTML). ASP.NET Core domyślnie nie generuje strony HTML dla pustych kodów statusu, chyba że w potoku middleware zarejestrowano komponent `app.UseStatusCodePages()` lub `app.UseStatusCodePagesWithReExecute(...)` (temat rozwijany w Lab 3). Ponieważ serwer nie odesłał żadnej treści, przeglądarka Chrome decyduje się wyświetlić własną, wbudowaną stronę błędu dla użytkownika.

---

## Część D – Odczyt danych z żądania i Postman

### Wyniki testów w Postmanie

1. **Wysłanie żądania `POST /Book/Import` z nagłówkiem `Content-Type: text/plain`:**
   * **Kod statusu:** `415 Unsupported Media Type`
   * **Dlaczego:** Parametr akcji `Import` został oznaczony atrybutem `[FromBody] BookDto dto`. Mechanizm ASP.NET Core wymaga, aby format przesyłanych danych odpowiadał formaterowi wejściowemu (domyślnie JSON, czyli `Content-Type: application/json`). Przesłanie nagłówka `text/plain` oznacza niezgodność formatu, co skutkuje odrzuceniem żądania kodem 415.

2. **Wysłanie żądania `GET /Book/Import`:**
   * **Kod statusu:** `405 Method Not Allowed`
   * **Dlaczego:** Akcja `Import` posiada atrybut `[HttpPost]`, który ogranicza dopuszczalne metody HTTP wyłącznie do `POST`. Próba wywołania tego adresu metodą `GET` powoduje błąd 405 (serwer wie, że taka akcja istnieje, ale nie zezwala na użytą metodę HTTP).

---

## Zadania do samodzielnego wykonania

### 1. Akcja `Sum(int a, int b)`
* **Wywołanie:** `/Book/Sum?a=2&b=3` zwraca wynik: `5`.
* **Co się stanie po wywołaniu `/Book/Sum?a=x&b=3`?**
  * **Kod statusu:** `400 Bad Request`
  * **Dlaczego (Model Binding):** Mechanizm Model Bindingu w ASP.NET Core próbuje przekształcić parametr `a` z ciągu tekstowego `"x"` na 32-bitową liczbę całkowitą (`int`). Ponieważ `"x"` nie reprezentuje poprawnej liczby, walidacja i powiązanie modelu kończą się błędem (`ModelState.IsValid == false`), a framework automatycznie generuje odpowiedź z błędem walidacji 400 Bad Request.

### 2. Akcja `UserAgent()`
* **Implementacja:** Odczyt nagłówka klienta przez `Request.Headers.UserAgent`.
* **Porównanie wyników:**
  * **Przeglądarka (Chrome):** Wysyła pełny nagłówek przeglądarki, np. `Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 ... Chrome/...`.
  * **Postman:** Wysyła identyfikator klienta Postmana, np. `PostmanRuntime/7.43.0`.

### 3. Akcja `Health()`
* **Implementacja:** Zwraca obiekt JSON za pomocą metody `Json(new { status = "ok", time = DateTime.Now })`.
* **Nagłówek odpowiedzi:** `Content-Type: application/json; charset=utf-8`.

---

## Pytania kontrolne

### 1. Czym różnią się metody GET i POST z punktu widzenia idempotentności i miejsca przekazywania danych?
* **Idempotentność:**
  * Metoda **GET** jest **idempotentna** (oraz *bezpieczna* / *safe*). Oznacza to, że wielokrotne wykonanie tego samego żądania GET nie zmienia stanu serwera i daje identyczny efekt uboczny.
  * Metoda **POST** **nie jest idempotentna**. Każde ponowne wysłanie żądania POST może wywołać kolejną operację na serwerze (np. utworzyć kolejny duplikat zamówienia lub rekordu w bazie danych).
* **Miejsce przekazywania danych:**
  * W metodzie **GET** dane przekazywane są wyłącznie w adresie URL — poprzez ścieżkę (route values) lub ciąg zapytania (Query String: `?klucz=wartosc`). Długość danych jest ograniczona przez limit długości URL, a dane są widoczne w historii przeglądarki i logach serwera.
  * W metodzie **POST** dane przekazywane są przede wszystkim w **ciele żądania** (Request Body), np. jako JSON, XML lub formularz (`multipart/form-data`). Umożliwia to przesyłanie dużych wolumenów danych, plików binarnych oraz danych poufnych (np. haseł).

### 2. Co oznaczają kody statusu HTTP?
* **`200 OK`** – Sukces. Żądanie zostało pomyślnie przetworzone, a serwer zwrócił oczekiwaną zawartość.
* **`302 Found`** – Przekierowanie tymczasowe (Temporary Redirect). Klient powinien pobrać zasób z adresu wskazanego w nagłówku `Location`.
* **`400 Bad Request`** – Błąd klienta. Serwer nie jest w stanie przetworzyć żądania z powodu niepoprawnej składni (np. błąd parsowania parametrów w Model Bindingu).
* **`401 Unauthorized`** – Brak uwierzytelnienia. Dostęp do zasobu wymaga poprawnych poświadczeń (np. zalogowania się, podania tokena JWT).
* **`403 Forbidden`** – Brak uprawnień (zabronione). Serwer rozpoznał tożsamość użytkownika, ale użytkownik nie posiada odpowiednich uprawnień do tego zasobu.
* **`404 Not Found`** – Nie znaleziono zasobu pod wskazanym adresem URL.
* **`405 Method Not Allowed`** – Niedozwolona metoda HTTP dla tego endpointu (np. wysłanie żądania GET do akcji oznaczonej `[HttpPost]`).
* **`415 Unsupported Media Type`** – Niewspierany format danych. Serwer odrzuca żądanie, ponieważ format danych przesłanych w ciele (nagłówek `Content-Type`) nie jest obsługiwany przez dany endpoint.
* **`500 Internal Server Error`** – Wewnętrzny błąd serwera. Wskazuje na nieobsłużony wyjątek lub awarię w kodzie aplikacji backendowej.

### 3. Skąd model binding wie, czy `id` pochodzi ze ścieżki, czy z query string?
* Mechanizm Model Bindingu w ASP.NET Core przeszukuje domyślnie źródła danych w ściśle określonej kolejności:
  1. Dane z formularza (`Form values` przesyłane w żądaniu POST).
  2. Wartości z trasy / ścieżki routingu (`Route values`, dopasowane na podstawie wzorca trasy, np. `{controller}/{action}/{id?}`).
  3. Parametry zapytania z adresu URL (`Query string values`, np. `?id=5`).
* Jeżeli parametr `id` znajduje się w szablonie trasy w URL (`/Book/Details/5`), Model Binding powiąże go z trasy.
* Jeśli w szablonie trasy nie ma danego parametru, silnik sprawdza ciąg zapytania (`?id=5`).
* Dodatkowo programista może jawnie wymusić źródło za pomocą atrybutów: `[FromRoute] int id` lub `[FromQuery] int id`.

### 4. Dlaczego pliki z `wwwroot` są dostępne pod adresem, a pliki z `Controllers/` nie?
* Katalog **`wwwroot`** jest domyślnie skonfigurowany jako **WebRoot** aplikacji za pośrednictwem middleware obsługi plików statycznych (`app.UseStaticFiles()` w `Program.cs`). Każde żądanie o plik znajdujący się fizycznie w `wwwroot` jest przechwytywane przez ten komponent i natychmiast odsyłane do klienta jako surowy plik (CSS, JS, grafika).
* Katalog **`Controllers/`** (oraz wszystkie pozostałe pliki źródłowe C#) znajduje się poza `wwwroot`. Dostęp do kodu aplikacji ze względów bezpieczeństwa jest zablokowany przed bezpośrednim serwowaniem przez serwer WWW (aby nikt nie mógł podejrzeć kodu źródłowego czy tajemnic aplikacji). Kontrolery są dostępne wyłącznie poprzez mechanizm routingu (`app.MapControllerRoute`), który uruchamia skompilowany kod C# w środowisku .NET i zwraca wygenerowany wynik, a nie sam plik źródłowy.
