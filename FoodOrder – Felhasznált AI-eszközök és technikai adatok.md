# FoodOrder – Felhasznált AI-eszközök és technikai adatok

## Összefoglaló

A FoodOrder kódját két nagy nyelvi modell írta a heti promptokból: az 1–3. héten a Google **Gemini Pro**, a 4–7. héten az Anthropic **Claude** (Claude Opus 5.5). A technológiai döntéseket, a Visual Studio-beli lépéseket (projektek, migrációk, futtatás, commit) és a végső ellenőrzést a fejlesztő végezte. A heti fő promptok, a pontosító promptok és az ellenőrzés lépései az AI-fejlesztési naplóban vannak.

## A használt AI-eszközök technikai adatai

| Jellemző | Gemini Pro | Claude Opus 5.5 |
| --- | --- | --- |
| Gyártó | Google | Anthropic |
| Modellazonosító | Gemini Pro (a pontos verzió nincs rögzítve) | `claude-opus-5-5`, megjelenés: 2026. szeptember 22. |
| Kontextusablak | – | 1M token |
| Maximális kimenet | – | 128K token |
| Tudáshatár | – | 2026. június |
| Bemenet | szöveg, csatolt forrásfájlok | szöveg és kép, csatolt fájlok |
| Hozzáférés | Gemini csevegőfelület, hetente új beszélgetés | Claude alkalmazás (claude.ai), ügynöki munkamenet felhőbeli Linux-munkaterülettel |
| Használat | 1–3. hét | 4–7. hét (2026. október) |
| Feladat | fájlfa és teljes fájltartalmak generálása a heti promptból | megvalósítás, fordítás, tesztelés, csomagolás, a napló frissítése |
| Kimenet | kód a csevegésben; a fájlokat a fejlesztő másolta a projektbe | teljes solution zip, PowerShell telepítő szkript, commitüzenet, naplóbejegyzés |
| Kódfuttatás | nem; fordítás és futtatás a fejlesztő gépén, Visual Studióban | igen, a saját munkaterületén (.NET 10 SDK); a WebAssembly klienst és a LocalDB-t a fejlesztő gépe ellenőrizte |

A Claude adatai a beszélgetéshez beállított modellre vonatkoznak ([Anthropic modelldokumentáció](https://platform.claude.com/docs/pt-BR/models/opus-5-5/overview)). A Gemini-oszlop üres sorait a Gemini felületén látható modellnév alapján lehet kitölteni.

## Munkamenet és promptolás

Minden hetet egy fő prompt vitt előre, utána 3–6 pontosító prompt rögzítette azokat a szabályokat, amelyeket az első generálás jellemzően elnagyol. A fő prompt felépítése minden héten azonos:

- **Szerep és stack:** pl. „Tapasztalt .NET fejlesztőként…”, Blazor Web App, .NET 10, WebAssembly kliens, Clean Architecture, Minimal API, EF Core + SQL Server LocalDB.
- **KONTEXTUS:** a meglévő projektek, a telepített NuGet-csomagok, a tesztfiókok; a forráskód csatolva.
- **Számozott követelmények:** rétegenként, pontos osztály-, végpont- és üzenetnevekkel.
- **KIMENET ÉS SZABÁLYOK:** .NET 10 és nullable; a meglévő nevek maradnak; csak a felsorolt NuGet-csomagok; projektfájl (.csproj, .slnx) nem jön vissza; előbb fájlfa, aztán teljes fájlok kihagyás nélkül („FOLYTATÁS KÖVETKEZIK” jelzéssel); magyar felületi szövegek, angol kódnevek; a végén futtatás és kézi tesztforgatókönyv.

### Heti menet a Geminivel (1–3. hét)

1. Új Gemini Pro csevegés minden héten, mert egy hosszabb beszélgetés végén a modell pontatlanul emlékszik a korai részletekre.
2. Az Előkészítés lépései kézzel, Visual Studióban: új projektek, hivatkozások, NuGet-csomagok.
3. A forráskód csatolása, majd a fő prompt változtatás nélkül.
4. A generált fájlok a helyükre kerültek, fordítás és futtatás. A generált .csproj és .slnx fájlt a fejlesztő nem másolta be, mert a mappaszerkezet miatt a Gemini rossz relatív útvonalat írt bele.
5. Fordítási hibánál a hibajavító minta, utána a pontosító promptok.
6. Emberi ellenőrzés, Git commit, naplóbejegyzés a kézi javításokról.

### Heti menet a Claude-dal (4–7. hét)

1. A fejlesztő csatolja a hét fő promptját („Folytassuk a 7. héttel.”).
2. Claude a naplóból a hét pontosító promptjait is elvégzi.
3. Megvalósítás a forrás másolatán, majd fordítás, automatikus tesztek és böngészős végigjátszás a munkaterületen.
4. Átadás: teljes solution zip (`.git` és `Migrations` nélkül) és PowerShell telepítő szkript, amely a Git-előzményt és a fejlesztő gépén készült migrációkat a régi mappából másolja vissza.
5. A naplóban a hét „Megvalósítás és ellenőrzés” része frissül.
6. A fejlesztő telepít, Visual Studióban létrehozza a migrációt (`Add-Migration … -ErrorAction Continue`), végigmegy az ellenőrzőlistán és commitol.

## Claude futtató és ellenőrző környezete

Claude egy elszigetelt, felhőbeli Linux-munkaterületen dolgozott: itt fordította és tesztelte a kódot, mielőtt átadta. A NuGet.org innen nem volt elérhető, ezért a külső csomagokat a GitHubon lévő forrásukból fordította le, és egy helyi csomagforrásból használta, a valódi csomagnevekkel és verziókkal.

| Elem | Mire szolgált | Technikai részlet |
| --- | --- | --- |
| Munkaterület | fordítás, futtatás, tesztelés | felhőbeli Linux-konténer, .NET 10 SDK, Python, Node.js, Chromium |
| Forrásból fordított csomagok | a projekt függőségei NuGet.org nélkül | EF Core 10.0.14 (SQLite-szolgáltatóval), SQLitePCLRaw 2.1.11, ASP.NET Core 10.0 (TestHost, Mvc.Testing, Identity.EntityFrameworkCore, SignalR-kliens), MailKit és MimeKit 4.18.1, NetArchTest.Rules 1.3.2 + Mono.Cecil 0.11.6, xunit.analyzers 1.18.0 |
| Tesztfuttató | a három tesztprojekt futtatása | minimális xUnit-kompatibilis futtató; az xUnit-analizátorok és a fordító figyelmeztetései is futottak |
| API-ellenőrző szerver | jogosultság, hibaválaszok, sütik | a valódi `Program.cs` HTTPS-en, memóriabeli adattárral; Python-szkript 53 esettel |
| E-mail-ellenőrzés | outbox és levelek | helyi SMTP-fogadó, valódi MailKit-küldés, 15 eset |
| SignalR-ellenőrzés | valós idejű értesítés | a valódi C# SignalR-klienssel, 14 eset |
| Böngészős végigjátszás | a felület kipróbálása | Playwright + Chromium; a kliens komponensei szerveroldali tesztgazdában futottak |
| Akadálymentesség | WCAG 2.1 AA | axe-core, 14 oldal |
| Diagramok | a README Mermaid-ábrái | Mermaid CLI (mmdc) renderelés |

Itt nem volt futtatható, ezért a fejlesztő gépén ellenőrizendő: a WebAssembly kliens fordítása és futása, az SQL Server LocalDB és a migrációk, valamint a Visual Studio Test Explorer.

## AI-használat és ellenőrzés hetenként

A 4. héttől minden átadást automatikus ellenőrzés előzött meg; az ellenőrzés által talált hibák még átadás előtt javítva lettek.

| Hét | AI | Eredmény | Automatikus ellenőrzés | Ellenőrzés közben javított hibák |
| --- | --- | --- | --- | --- |
| 1. | Gemini Pro | böngészős prototípus: étlap, kosár, rendelés, szerepkörváltó | fordítás és futtatás a fejlesztő gépén | a napló 1. heti fülén |
| 2. | Gemini Pro | domain modell (Money, OrderStatus, Dish, Order, OrderItem, Courier), unit tesztek | fordítás és tesztek a fejlesztő gépén | a napló 2. heti fülén |
| 3. | Gemini Pro | Application és Infrastructure réteg, SQL Server LocalDB, migráció, Minimal API | fordítás, migráció, `.http`-kérések a fejlesztő gépén | a napló 3. heti fülén |
| 4. | Claude Opus 5.5 | kliens az API-n, állapot-workflow, futárkezelés | 0 hiba, 0 figyelmeztetés; 80 domain-teszt; 9 lépéses böngészős végigjátszás | mobilon kilógó szerepkörváltó, rejtett táblázatfejléc, két „Mégse” gomb |
| 5. | Claude Opus 5.5 | Identity, bejelentkezés, szerepkörök | 80 domain-teszt; 53 API-eset; 13 + 4 böngészős lépés | űrlapkontextus-ütközés (RZ9999), hiányos regisztrációs hibák |
| 6. | Claude Opus 5.5 | e-mail outbox mintával (MailKit), SignalR élő követés | 15 e-mail-eset; 9 outbox-eset; 14 hub-eset; 12 kliens-eset; 11 böngészős lépés | láthatatlan kiemelés, számkódolt ékezetek a levélben |
| 7. | Claude Opus 5.5 | tesztek, biztonsági átnézés, csiszolás, README, demó, CI | 130 teszt (80 + 24 + 26); 53 API-, 15 e-mail-, 14 SignalR-eset; 28 böngészős lépés; axe 14 oldalon | üres 404-oldal teljes betöltésnél, SQLite dátumrendezés, előzmények sorrendje, gyenge kontraszt |

## Emberi szerep

Az AI kódot írt és ellenőrzött, a projekt irányáról és az átvett kódról a fejlesztő döntött.

- **Döntések:** Blazor Web App Visual Studio 2026-ban; SQL Server LocalDB és SSMS az SQLite helyett; a heti promptlánc; a 4. héttől váltás Claude-ra.
- **Kézi lépések Visual Studióban:** projektek, hivatkozások és NuGet-csomagok (az 1–3. héten); a migrációk létrehozása (InitialCreate, AddIdentity, AddOutbox); user-secrets; futtatás.
- **Ellenőrzés:** a napló heti „Emberi ellenőrzés” listája, a 7. heti végső kézi ellenőrzőlista, a demó végigjátszása.
- **Verziókezelés:** hetenkénti Git commit, a naplóban rögzített kézi javításokkal.

## Adatkezelés és korlátok

Az AI csak a projekt forráskódját, a heti promptokat és a fordítási hibákat, képernyőképeket kapta meg; valódi személyes adatot és titkot nem.

- **Titkok:** a valódi SMTP-fiók adatai csak user-secretsbe kerülhetnek, a promptokba és a repóba nem. A `Demo123!` jelszavú tesztfiókok csak Development környezetben jönnek létre.
- **Hibalehetőség:** a generált kód hibázhat, ezért minden hét fordításon, automatikus teszteken és kézi ellenőrzésen ment át.
- **Környezet:** Claude munkaterületén nem fordult a WebAssembly kliens, nem futott SQL Server és Visual Studio Test Explorer; ezek a fejlesztő gépén ellenőrizendők.
- **Naprakészség:** a modellek tudása egy dátumig tart (Claude Opus 5.5: 2026. június), ezért a külső eszközök aktuális adatait (pl. az smtp4dev .NET-verzióját) Claude webes kereséssel ellenőrizte.
- **Modellazonosítás:** a Claude-adatok a beszélgetéshez beállított modellre vonatkoznak; a Gemini Pro pontos verziója nincs rögzítve.

## Használt promptok:
FoodOrder 

## 1.Hét

Tapasztalt .NET / Blazor fejlesztőként segíts egy egyetemi féléves projekt első heti prototípusában.

Készíts egy reszponzív, magyar nyelvű FoodOrder prototípust ételrendeléshez Blazor Web App alapon (.NET 10).

KIINDULÓ ÁLLAPOT (Visual Studio 2026-ban már létrehoztam; a .csproj és solution fájlokat NE generáld újra):
- „Blazor Web App” sablon: Interactive render mode = WebAssembly, Interactivity location = Global, Authentication = None, sample pages bekapcsolva.
- Projektek: FoodOrder (szerver host), FoodOrder.Client (Razor oldalak, a böngészőben futnak), FoodOrder.Shared (.NET 10 osztálykönyvtár, a Client és a szerver is hivatkozza).
- Az App.razor a FoodOrder/Components mappában, a Routes.razor, a Layout és a Pages a FoodOrder.Client projektben van.

FUNKCIÓK:
1. Szerepkörváltó a felső sávban két mintafelhasználóval: „Kiss Anna – Vásárló” és „Nagy Péter – Éttermi dolgozó”.
2. Étlap (/): az elérhető ételek kártyákon, névvel, leírással, kategóriával és forintárral (pl. „2 490 Ft”). Vásárlóként „Kosárba” gomb mennyiségválasztóval.
3. Kosár (/kosar): tételek mennyiségének módosítása, tétel törlése, végösszeg, szállítási cím és opcionális megjegyzés, „Rendelés leadása” gomb. Sikeres leadás után a rendelés sorszáma jelenjen meg (pl. #1001), és a kosár ürüljön ki.
4. Rendeléseim (/rendeleseim): a Vásárló saját rendelései, legújabb elöl, tételekkel, végösszeggel és állapottal.
5. Étlapkezelés (/etterem/etlap): az Éttermi dolgozó új ételt vesz fel (név, leírás, kategória, ár), és bármely ételt elérhetővé vagy nem elérhetővé tehet.
6. Beérkezett rendelések (/etterem/rendelesek): az Éttermi dolgozó minden rendelést lát, legújabb elöl, vásárlóval, címmel, tételekkel, végösszeggel és állapottal.
7. Új rendelés állapota „Leadva”. Állapotváltás ebben a fázisban NINCS.
8. Üres tároló esetén töltődjön be 6 magyar mintaétel (leves, főétel, desszert, ital kategóriákban).
9. Az étlap, a rendelések, a kosár és a kiválasztott szerepkör maradjon meg oldalfrissítés után localStorage-ban, a következő kulcsokon: foodorder.dishes, foodorder.orders, foodorder.cart, foodorder.role.

TECHNIKAI ELŐÍRÁSOK:
- A közös modellek a FoodOrder.Shared projektbe kerüljenek: DishDto (étel), OrderDto (rendelés), OrderItemDto (rendelési tétel), CartItemDto (kosártétel), UserRole enum (Customer, Staff), OrderStatusCode enum (egyelőre egyetlen értékkel: Placed). Ezek a kliens és a szerver közös adatközlő modelljei; a domain entitások később külön projektbe kerülnek, ezért itt a Dto utótag. Az ár decimal, forintban. Nullable reference types legyen bekapcsolva.
- Az állapotkezelés egy injektálható szolgáltatásban legyen a Client projektben (FoodOrderStore), a Client Program.cs-ben scoped-ként regisztrálva; változáskor értesítse a komponenseket (event Action? OnChange), a komponensek iratkozzanak le (IDisposable).
- localStorage elérés IJSRuntime-on keresztül (localStorage.getItem / setItem), szerializálás System.Text.Json-nal. Külső NuGet-csomagot ne használj.
- Kapcsold ki az előrenderelést: az App.razor-ban a Routes és a HeadOutlet render módja legyen new InteractiveWebAssemblyRenderMode(prerender: false), mert a localStorage csak a böngészőben érhető el.
- Az ár formázása („2 490 Ft”) a FoodOrder.Shared projekt HufFormatter.Format(decimal) metódusában legyen, és ne függjön a böngésző nyelvétől (ne a CultureInfo("hu-HU")-ra támaszkodj, mert WebAssemblyben nem garantált a magyar kultúraadat).
- Űrlapokhoz EditForm és DataAnnotations validáció, magyar hibaüzenetekkel.
- A sablon Bootstrapját használd; az oldal keskeny (mobil) nézetben és billentyűzettel is legyen kezelhető.
- Töröld a sablon Counter és Weather oldalát; a NavMenu az aktuális szerepkörhöz illő menüpontokat mutassa.
- Ne készíts Web API-t, adatbázist vagy valódi bejelentkezést; a szerverprojekt ebben a fázisban csak kiszolgálja az alkalmazást.

KIMENET:
1. Fájlfa, amely jelöli, melyik fájl új, módosított vagy törlendő.
2. Minden új vagy módosított fájl TELJES tartalma, projekten belüli útvonallal (pl. FoodOrder.Client/Pages/Menu.razor). Ne használj „...”-ot vagy kihagyást.
3. A végén: milyen függőségek kellenek, és hogyan kell futtatni Visual Studio 2026-ban.

## 2. Hét

Tapasztalt .NET fejlesztőként folytasd a FoodOrder egyetemi projektet (Blazor Web App, .NET 10). Ez a 2. hét: domain modell és refaktorálás.

KONTEXTUS (csatolom a forráskódot):
- Projektek: FoodOrder (szerver host), FoodOrder.Client (WebAssembly, globális interaktivitás, prerender kikapcsolva), FoodOrder.Shared (DishDto, OrderDto, OrderItemDto, CartItemDto, UserRole, OrderStatusCode, HufFormatter).
- Az 1. heti prototípus működik: étlap, kosár, rendelés leadása, szerepkörváltó; minden adat localStorage-ban, egyetlen FoodOrderStore szolgáltatásban.
- Új, üres projektek (én hoztam létre): FoodOrder.Domain (osztálykönyvtár, nincs hivatkozása) és FoodOrder.Domain.Tests (xUnit, hivatkozza a Domaint).

1. DOMAIN MODELL (FoodOrder.Domain, külső függőség nélkül)
Value objectek:
- Money: immutable sealed record, Amount (decimal) és Currency ("HUF"). Gyártó: Money.Huf(decimal), Money.Zero. Szabály: nem negatív, csak egész forint. Műveletek: összeadás (csak azonos pénznem), Multiply(int quantity). ToString() → „2 490 Ft”, kultúrafüggetlenül.
- OrderStatus: smart enum osztály statikus példányokkal: Placed „Leadva”, Accepted „Elfogadva”, Preparing „Készül”, ReadyForPickup „Kiszállításra kész”, OutForDelivery „Kiszállítás alatt”, Delivered „Kézbesítve”, Cancelled „Lemondva”. Tulajdonságok: Code (string), DisplayName, IsFinal, AllowedNext. Metódusok: CanTransitionTo(OrderStatus), FromCode(string) – ismeretlen kódra DomainException. Érték szerinti egyenlőség.
- Megengedett átmenetek, minden más tilos: Placed → Accepted, Cancelled; Accepted → Preparing, Cancelled; Preparing → ReadyForPickup; ReadyForPickup → OutForDelivery; OutForDelivery → Delivered. A Delivered és a Cancelled végállapot.

Entitások (privát setterek, gyártó metódusok, invariánsok ellenőrzése):
- Dish: Id (int), Name (1–80 karakter, trimmelt, nem csak szóköz), Description (max. 300), Category (DishCategory: Soup, MainCourse, Dessert, Drink), Price (Money, 1–100 000 Ft), IsAvailable. Metódusok: Create, ChangePrice, MakeAvailable, MakeUnavailable.
- Courier: Id, Name (1–80), Phone (magyar mobilszám: +36 vagy 06 előtag, utána 9 számjegy, szóköz megengedett), IsActive. Metódusok: Create, Activate, Deactivate.
- OrderItem: Id, DishId, DishName és UnitPrice pillanatkép a leadás pillanatából, Quantity (1–20), LineTotal = UnitPrice × Quantity. Csak az Order hozhatja létre (internal konstruktor).
- OrderStatusChange: Id, FromStatus (az elsőnél null), ToStatus, ChangedAt (DateTimeOffset), ChangedBy (string).
- Order (aggregátumgyökér): Id, CustomerId, CustomerName, CustomerEmail, DeliveryAddress (1–200), Note (max. 300, opcionális), PlacedAt, Status, CourierId (int?), Items és StatusHistory (kívülről csak olvasható, privát listával), Total (a tételekből számolva, nem tárolt mező), Version (int, minden módosításkor nő).
  - Order.Place(customerId, customerName, customerEmail, deliveryAddress, note, IEnumerable<(Dish Dish, int Quantity)> lines, DateTimeOffset now): legalább egy tétel; csak elérhető étel; ugyanaz az étel egy tétellé vonódik össze (összesen max. 20); állapot Placed; első történetbejegyzés.
  - ChangeStatus(OrderStatus next, string changedBy, DateTimeOffset now): tiltott átmenetre InvalidStatusTransitionException ilyen üzenettel: „Nem engedélyezett állapotváltás: Kézbesítve → Készül”. OutForDelivery csak hozzárendelt futárral. Történetbejegyzés, Version++.
  - AssignCourier(Courier courier): csak Accepted, Preparing vagy ReadyForPickup állapotban, csak aktív futárral; Version++.
- Kivételek: DomainException (alap) és InvalidStatusTransitionException : DomainException, magyar üzenetekkel.
- Az időpont mindig paraméterként érkezik; a domainben nincs DateTime.Now.

2. UNIT TESZTEK (FoodOrder.Domain.Tests, xUnit, Arrange–Act–Assert)
- Money: negatív és tört összeg elutasítása, összeadás, szorzás, formázás.
- OrderStatus: [Theory] MemberData-val mind a 7×7 állapotpár; a megengedettek sikerülnek, minden más InvalidStatusTransitionException. Külön, beszédes nevű teszt a Delivered → Preparing tiltására.
- Order: üres rendelés, elérhetetlen étel, tételösszevonás, árpillanatkép (az étel későbbi árváltozása nem hat a rendelésre), Total, futár nélküli OutForDelivery, inaktív futár, Version növekedése.
- Dish és Courier validációk.

3. KLIENS-REFAKTOR (FoodOrder.Client – a működés NEM változik)
- A FoodOrderStore-t bontsd szét: IMenuService, ICartService, IOrderService, ISessionService (szerepkör). Minden metódus aszinkron (Task), és Shared DTO-t ad vissza, hogy később API-implementáció is kerülhessen mögé. Implementációk: LocalStorageMenuService, LocalStorageCartService stb.
- A localStorage-elérés egy IKeyValueStorage interfész mögé kerüljön (LocalStorageKeyValueStorage: IJSRuntime + System.Text.Json); a kulcsnevek egy helyen, konstansként.
- A nagy oldalakat bontsd komponensekre (pl. DishCard, CartLine, OrderCard, RoleSwitcher).
- A kliens NE hivatkozzon a FoodOrder.Domainre: a Domain később csak a szerveren fut.

KIMENET ÉS SZABÁLYOK
- .NET 10, C#, nullable bekapcsolva; a meglévő projekt- és mappaneveket tartsd meg; új NuGet-csomagot ne használj; a .csproj fájlokat ne írd át.
- Először fájlfa (új / módosított / törlendő jelöléssel), majd minden új vagy módosított fájl TELJES tartalma útvonallal, kihagyás nélkül. Ha a válasz túl hosszú lenne, egy fájl végén állj meg „FOLYTATÁS KÖVETKEZIK” jelzéssel; a „folytasd” szóra a következő fájllal indulj.
- Felületi szövegek és hibaüzenetek magyarul, kódnevek angolul.
- A végén: tesztfuttatás Visual Studióban (Test Explorer) és parancssorban (dotnet test), valamint rövid kézi forgatókönyv arra, hogy a kliens ugyanúgy működik.

## 3. Hét

Tapasztalt .NET fejlesztőként folytasd a FoodOrder egyetemi projektet (Blazor Web App, .NET 10). Ez a 3. hét: Clean Architecture rétegek, a négy use case, SQL Server (LocalDB) adatbázis és Web API.

KONTEXTUS (csatolom a forráskódot):
- FoodOrder.Domain kész, tesztekkel: Money, OrderStatus (átmenettáblával), Dish, Courier, Order, OrderItem, OrderStatusChange, DomainException, InvalidStatusTransitionException.
- FoodOrder.Shared: DishDto, OrderDto, OrderItemDto, CartItemDto, UserRole, OrderStatusCode, HufFormatter.
- FoodOrder.Client: IMenuService, ICartService, IOrderService, ISessionService localStorage-implementációkkal.
- Új projektek (én hoztam létre, hivatkozásokkal és csomagokkal):
  FoodOrder.Application → Domain, Shared; csomag: Microsoft.Extensions.DependencyInjection.Abstractions.
  FoodOrder.Infrastructure → Application; csomag: Microsoft.EntityFrameworkCore.SqlServer; FrameworkReference: Microsoft.AspNetCore.App.
  FoodOrder (szerver) → Application, Infrastructure; csomagok: Microsoft.EntityFrameworkCore.Design, Microsoft.EntityFrameworkCore.Tools, Microsoft.AspNetCore.OpenApi.
- Függőségi szabály: a Domain semmire nem hivatkozik; az Application csak a Domainre és a Shared-re; az Infrastructure az Application interfészeit valósítja meg; a szerverprojekt a kompozíciós gyökér.

1. SHARED – DTO-K BŐVÍTÉSE (a kliens maradjon fordítható: a localStorage-szolgáltatásait minimálisan igazítsd, a működése ne változzon)
- OrderStatusCode kapja meg mind a 7 értéket: Placed, Accepted, Preparing, ReadyForPickup, OutForDelivery, Delivered, Cancelled.
- DishDto(Id, Name, Description, Category, Price, IsAvailable); CreateDishRequest; SetDishAvailabilityRequest(IsAvailable).
- OrderDto(Id, PlacedAt, CustomerName, DeliveryAddress, Note, Status, StatusDisplayName, Items, Total, CourierId, CourierName, CourierPhone, Version, AllowedNextStatuses: List<StatusOptionDto>, History: List<StatusHistoryEntryDto>).
- StatusOptionDto(Code, DisplayName); StatusHistoryEntryDto(Status, DisplayName, ChangedAt, ChangedBy).
- PlaceOrderRequest(Items: List<PlaceOrderItemRequest(DishId, Quantity)>, DeliveryAddress, Note, CustomerName, CustomerEmail) – a vásárló adatai az 5. hétig a kérésből jönnek.
- ChangeOrderStatusRequest(NewStatus, ExpectedVersion); CourierDto; CreateCourierRequest; AssignCourierRequest(CourierId, ExpectedVersion).
- A kérés-DTO-kon DataAnnotations validáció magyar üzenetekkel, ugyanazokkal a határokkal, mint a domainben.

2. APPLICATION (FoodOrder.Application)
- Portok: IDishRepository, IOrderRepository (a rendelést tételekkel és történettel együtt tölti), ICourierRepository, IUnitOfWork (SaveChangesAsync), IClock.
- Use case-ek egyszerű handler osztályként, egyetlen HandleAsync(…, CancellationToken) metódussal, MediatR nélkül:
  AddDishHandler (Étel felvétele), SetDishAvailabilityHandler, GetMenuHandler(onlyAvailable);
  PlaceOrderHandler (Rendelés leadása: ételek betöltése, hiányzó étel → NotFoundException, majd Order.Place);
  ChangeOrderStatusHandler (Állapotváltás: ha az ExpectedVersion eltér, ConflictException „A rendelést közben módosították, frissítsd az oldalt.”);
  TrackOrderHandler (Rendelés követése: OrderDto történettel, futárral és AllowedNextStatuses listával);
  ListOrdersHandler, AddCourierHandler, ListCouriersHandler, AssignCourierHandler.
- Saját kivételek: NotFoundException, ConflictException. Kézi mapping (ToDto bővítő metódusok), AutoMapper nélkül.
- services.AddApplication() bővítő metódus.

3. INFRASTRUCTURE (FoodOrder.Infrastructure)
- AppDbContext (EF Core, SQL Server), konfigurációk külön IEntityTypeConfiguration osztályokban:
  Money EF Core complex type-ként (ComplexProperty), az Amount decimal(18,2) oszlopban (HasPrecision(18, 2)), a Currency nvarchar(3);
  OrderStatus string kódként, value converterrel (OrderStatus.FromCode);
  Order.Items és Order.StatusHistory a privát listamezőkön (backing field) keresztül, kaszkádolt törléssel; Order.Version concurrency token.
- Ha az EF Core-nak kell, adj a domain entitásokhoz privát, paraméter nélküli konstruktort – ezen kívül a Domain ne változzon.
- Repository-k és UnitOfWork; a DbUpdateConcurrencyException ConflictException-né alakul.
- DbSeeder: üres adatbázisba 6 magyar étel és 2 aktív futár.
- services.AddInfrastructure(IConfiguration) bővítő metódus; connection string az appsettings.json-ban DefaultConnection néven: „Server=(localdb)\MSSQLLocalDB;Database=FoodOrder;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True” (a JSON-fájlban a \ jelet duplán kell írni). UseSqlServer, átmeneti hibákra EnableRetryOnFailure.
- Az első migráció neve InitialCreate; a parancsot én futtatom, add meg pontosan.

4. WEB API (FoodOrder szerverprojekt)
- Minimal API végpontcsoportok külön fájlokban (Endpoints mappa):
  GET /api/dishes?onlyAvailable=true, POST /api/dishes, PATCH /api/dishes/{id}/availability;
  GET /api/orders, GET /api/orders/{id}, POST /api/orders, POST /api/orders/{id}/status, POST /api/orders/{id}/courier;
  GET /api/couriers, POST /api/couriers.
- Kérésvalidáció endpoint filterrel (System.ComponentModel.DataAnnotations.Validator); hiba esetén 400 ValidationProblem.
- Hibakezelés IExceptionHandler-rel, ProblemDetails magyar title/detail mezővel: DomainException → 400; InvalidStatusTransitionException és ConflictException → 409; NotFoundException → 404; minden más 500, belső részletek nélkül.
- AddOpenApi/MapOpenApi csak Development környezetben.
- Indításkor, Developmentben: Database.MigrateAsync, majd seed.
- FoodOrder.http fájl minden végponthoz, köztük: teljes állapotlánc Leadva → Kézbesítve futárral; egy tiltott Kézbesítve → Készül kísérlet (409); két kérés ugyanazzal az ExpectedVersion-nel (a második 409).

KIMENET ÉS SZABÁLYOK
- .NET 10, C#, nullable; a meglévő neveket tartsd meg; csak a fent felsorolt NuGet-csomagokat használd; a .csproj fájlokat ne írd át.
- Először fájlfa (új / módosított / törlendő jelöléssel), majd minden új vagy módosított fájl TELJES tartalma útvonallal, kihagyás nélkül. Ha a válasz túl hosszú lenne, egy fájl végén állj meg „FOLYTATÁS KÖVETKEZIK” jelzéssel; a „folytasd” szóra a következő fájllal indulj.
- Felületi szövegek és hibaüzenetek magyarul, kódnevek angolul.
- A végén: migrációs parancs Package Manager Console-ban és dotnet ef CLI-vel, futtatás, és kézi tesztforgatókönyv a .http fájllal.

## 4. Hét

Tapasztalt Blazor fejlesztőként folytasd a FoodOrder egyetemi projektet (Blazor Web App, .NET 10, WebAssembly kliens). Ez a 4. hét: a kliens átállítása a szerver API-jára, az állapot-workflow és a futárkezelés felülete, rendeléskövetés.

KONTEXTUS (csatolom a forráskódot):
- A szerver kész: Minimal API (/api/dishes, /api/orders, /api/couriers), ProblemDetails hibák (400, 404, 409), SQL Server LocalDB, a négy use case az Application rétegben.
- Az OrderDto tartalmazza: Status, StatusDisplayName, AllowedNextStatuses, History, Version és a futár adatait.
- A kliens még localStorage-implementációkat használ: IMenuService, ICartService, IOrderService, ISessionService.
- A szerepkör továbbra is demó-váltóval megy (Kiss Anna – Vásárló, Nagy Péter – Éttermi dolgozó); a bejelentkezés az 5. héten jön.

1. API-KLIENS
- A Client Program.cs-ben HttpClient, BaseAddress = builder.HostEnvironment.BaseAddress.
- Új implementációk: ApiMenuService : IMenuService, ApiOrderService : IOrderService, valamint új ICourierService és ApiCourierService; a DI-regisztráció ezekre váltson. Az ICartService maradjon localStorage-ban (a kosár kliensoldali állapot).
- Közös ApiClient segéd: sikertelen válaszból kiolvassa a ProblemDetails-t, és ApiException-t dob a magyar detail szöveggel; 400-as ValidationProblem esetén a mezőhibákat is továbbadja.
- Töröld a már nem használt LocalStorageMenuService-t és LocalStorageOrderService-t; induláskor töröld a régi foodorder.dishes és foodorder.orders kulcsot.
- A PlaceOrderRequest vásárlói adatai a demó-szerepkörből jönnek (Kiss Anna, anna@example.com).

2. ÉTTERMI DOLGOZÓ FELÜLETE
- /etterem/rendelesek: rendeléskártyák állapot szerinti szűrővel (Aktív / Kézbesített / Lemondott / Mind), legújabb elöl. Kártyánként: állapotjelvény, tételek, végösszeg, futár.
- Állapotváltó gombok KIZÁRÓLAG az AllowedNextStatuses alapján – a kliens ne tartalmazzon saját átmenettáblát. A kérésben az aktuális Version megy ExpectedVersion-ként.
- Lemondás előtt megerősítő párbeszédablak.
- Futár hozzárendelése legördülővel (csak aktív futárok), Elfogadva, Készül vagy Kiszállításra kész állapotban.
- 409-es válasznál magyar figyelmeztetés és az érintett rendelés újratöltése.
- /etterem/futarok: futárlista, új futár felvétele (név, telefonszám) validációval, aktiválás/inaktiválás.
- /etterem/etlap: az étlapkezelés már az API-t használja.

3. VÁSÁRLÓ FELÜLETE
- /rendeleseim: a demó-vásárló rendelései a szerverről.
- /rendeleseim/{id}: rendeléskövetés – idővonal (lépésjelző) a teljes állapotsorral, a megtett lépéseknél időponttal; Lemondva esetén ezt jelezze. Kiszállítás alatt állapotban a futár neve és telefonszáma; tételek és végösszeg.
- A követőoldal 10 másodpercenként frissüljön (a 6. héten SignalR váltja ki), és az oldal elhagyásakor álljon le az időzítő (IDisposable).
- Lemondás gomb csak akkor, ha az AllowedNextStatuses tartalmazza a Cancelled értéket.

4. ÁLTALÁNOS
- Betöltésjelző, üres állapot szövege és hibaüzenet-sáv minden listán.
- Kérés közben a gomb legyen letiltva (dupla beküldés ellen).
- A prototípus-sáv szövege: „Fejlesztői változat – a szerepkörváltó még nem bejelentkezés”.

KIMENET ÉS SZABÁLYOK
- .NET 10, C#, nullable; a meglévő neveket tartsd meg; új NuGet-csomagot ne használj; projektfájlt (.csproj, .slnx) NE adj vissza; ha csomag vagy hivatkozás hiányzik, csak írd le, melyik projekthez mit adjak hozzá.
- Először fájlfa (új / módosított / törlendő jelöléssel), majd minden új vagy módosított fájl TELJES tartalma útvonallal, kihagyás nélkül. Ha a válasz túl hosszú lenne, egy fájl végén állj meg „FOLYTATÁS KÖVETKEZIK” jelzéssel; a „folytasd” szóra a következő fájllal indulj.
- Felületi szövegek és hibaüzenetek magyarul, kódnevek angolul.
- A végén: futtatás, és kézi tesztforgatókönyv két böngészőablakkal (egy normál és egy InPrivate ablak, mert a szerepkör localStorage-ban van).

## 5. Hét

Fiók	Név	Szerepkör
dolgozo@foodorder.hu	Nagy Péter	Éttermi dolgozó
anna@foodorder.hu	Kiss Anna	Vásárló
bela@foodorder.hu	Kovács Béla	Vásárló

A jelszó mindháromnál Demo123!.

Tapasztalt ASP.NET Core fejlesztőként folytasd a FoodOrder egyetemi projektet (Blazor Web App, .NET 10, WebAssembly kliens, Minimal API, EF Core SQL Server LocalDB). Ez az 5. hét: valódi bejelentkezés és szerepkör-alapú jogosultság.

KONTEXTUS (csatolom a forráskódot):
- Rétegek: Domain, Application (use case handlerek), Infrastructure (AppDbContext, repository-k), szerver (Minimal API, ProblemDetails), Client (Api*Service implementációk), Shared (DTO-k).
- Jelenleg a szerepkör egy demó-váltó a kliensben (ISessionService, foodorder.role), az API mindenkit kiszolgál, és a PlaceOrderRequest tartalmazza a vásárló nevét és e-mail-címét.
- Telepített csomagok: Infrastructure: Microsoft.AspNetCore.Identity.EntityFrameworkCore; Client: Microsoft.AspNetCore.Components.Authorization.

1. IDENTITY A SZERVEREN
- AppUser : IdentityUser, FullName mezővel; az AppDbContext örököljön az IdentityDbContext<AppUser>-ből, a meglévő konfigurációk maradnak. Új migráció: AddIdentity.
- AddIdentity<AppUser, IdentityRole>() EF Core tárolóval; jelszó: min. 8 karakter, kis- és nagybetű, szám; 5 hibás próbálkozás után 5 perc zárolás.
- ConfigureApplicationCookie: név „FoodOrder.Auth”, HttpOnly, SameSite=Strict, SecurePolicy=Always, LoginPath „/bejelentkezes”, AccessDeniedPath „/hozzaferes-megtagadva”. Az /api kezdetű kéréseknél NE legyen átirányítás: OnRedirectToLogin → 401, OnRedirectToAccessDenied → 403.
- Fontos: a Blazor oldalakon lévő [Authorize] attribútumot a szerver is érvényesíti teljes oldalbetöltéskor, ezért a LoginPath egy létező, névtelenül elérhető kliensoldalra mutasson.
- Szerepkörök: „Customer” (Vásárló) és „Staff” (Éttermi dolgozó). Policy-k: StaffOnly, CustomerOnly.
- Seed csak Developmentben: a két szerepkör és három felhasználó – dolgozo@foodorder.hu (Nagy Péter, Staff), anna@foodorder.hu (Kiss Anna, Customer), bela@foodorder.hu (Kovács Béla, Customer); jelszó mindháromnál: Demo123!

2. AUTH VÉGPONTOK (Shared: LoginRequest, RegisterRequest, UserInfoDto(Email, FullName, Roles))
- POST /api/auth/register – csak Customer szerepkörű fiókot hoz létre; foglalt e-mail-címre és gyenge jelszóra magyar hiba (400 ValidationProblem).
- POST /api/auth/login – SignInManager.PasswordSignInAsync; hibás adatra 401 általános üzenettel: „Hibás e-mail-cím vagy jelszó.”
- POST /api/auth/logout; GET /api/auth/me → UserInfoDto vagy 401.

3. JOGOSULTSÁG AZ API-N ÉS AZ APPLICATION RÉTEGBEN
- ICurrentUser interfész az Application-ben (UserId, Email, FullName, IsStaff, IsCustomer); implementáció a szerverben IHttpContextAccessor-ral.
- Végpontok: GET /api/dishes névtelenül is elérhető; étel létrehozása és elérhetőség: StaffOnly; POST /api/orders: CustomerOnly; futárvégpontok: StaffOnly; a többi rendelésvégpont bejelentkezéshez kötött.
- PlaceOrder: a vásárló azonosítója, neve és e-mail-címe az ICurrentUser-ből jön; ezeket a mezőket töröld a PlaceOrderRequest-ből.
- ListOrders: a Staff mindent lát, a Customer csak a sajátját. TrackOrder: Customer más rendelésére 404 (ne derüljön ki, hogy létezik).
- ChangeOrderStatus: a Staff minden megengedett átmenetet végrehajthat; a Customer csak a saját rendelését mondhatja le, és csak Leadva állapotban; minden más esetben 403. Ezt egy OrderStatusPolicy osztály döntse el az Application rétegben.
- Az OrderDto AllowedNextStatuses listája szerepkörre szűrve érkezzen (a vásárló legfeljebb a Cancelled-et kapja).

4. KLIENS
- CookieAuthenticationStateProvider: GET /api/auth/me, a válasz gyorsítótárazása, belépés és kilépés után NotifyAuthenticationStateChanged. Regisztráció: AddAuthorizationCore, AddCascadingAuthenticationState.
- Routes.razor: AuthorizeRouteView. Bejelentkezetlen felhasználót a /bejelentkezes?returnUrl=… oldalra irányít, a bejelentkezett, de jogosulatlan felhasználónak „Nincs jogosultságod ehhez az oldalhoz” üzenetet mutat.
- Oldalak: /bejelentkezes, /regisztracio, /hozzaferes-megtagadva. Éttermi oldalak: [Authorize(Roles = "Staff")]; vásárlói oldalak: [Authorize(Roles = "Customer")]; az étlap névtelenül is látható, a Kosárba gomb bejelentkezésre visz.
- A NavMenu AuthorizeView-val a szerepkörhöz illő menüpontokat mutassa; a felső sávban a felhasználó neve és Kijelentkezés gomb.
- A kosár kulcsa felhasználónként: foodorder.cart.{email}.
- Töröld a szerepkörváltót, az ISessionService-t, a foodorder.role kulcsot és a prototípus-sávot.
- Bármely 401-es API-válasz a bejelentkező oldalra vigyen.

KIMENET ÉS SZABÁLYOK
- .NET 10, C#, nullable; a meglévő neveket tartsd meg; csak a felsorolt NuGet-csomagokat használd; projektfájlt (.csproj, .slnx) NE adj vissza; ha csomag vagy hivatkozás hiányzik, csak írd le, melyik projekthez mit adjak hozzá.
- Először fájlfa (új / módosított / törlendő jelöléssel), majd minden új vagy módosított fájl TELJES tartalma útvonallal, kihagyás nélkül. Ha a válasz túl hosszú lenne, egy fájl végén állj meg „FOLYTATÁS KÖVETKEZIK” jelzéssel; a „folytasd” szóra a következő fájllal indulj.
- Felületi szövegek és hibaüzenetek magyarul, kódnevek angolul.
- A végén: migrációs parancs (AddIdentity), futtatás, és kézi tesztforgatókönyv a három tesztfiókkal, valamint .http példák bejelentkezés nélkül, vásárlóként és dolgozóként.

## 6. Hét

Tapasztalt ASP.NET Core fejlesztőként folytasd a FoodOrder egyetemi projektet (Blazor Web App, .NET 10, WebAssembly kliens, Minimal API, EF Core SQL Server LocalDB, Identity cookie-val). Ez a 6. hét: külső e-mail-értesítés és valós idejű rendeléskövetés.

KONTEXTUS (csatolom a forráskódot):
- A PlaceOrderHandler és a ChangeOrderStatusHandler az Application rétegben van; a rendelés tárolja a vásárló e-mail-címét (CustomerEmail). A rendelés azonosítóját az adatbázis adja mentéskor.
- A kliens követőoldala (/rendeleseim/{id}) és a dolgozói rendeléslista jelenleg 10 másodperces pollinggal frissül.
- Telepített csomagok: Infrastructure: MailKit; Client: Microsoft.AspNetCore.SignalR.Client. Fejlesztéshez smtp4dev fut: SMTP localhost:2525, webes felület http://localhost:5050.

1. E-MAIL ÉRTESÍTÉS OUTBOX MINTÁVAL
- Application: INotificationOutbox.EnqueueOrderEvent(Order order, OrderEventType type), ahol a típus Placed vagy StatusChanged; a bejegyzés a rendelés akkori állapotkódját rögzíti.
- A PlaceOrderHandler és a ChangeOrderStatusHandler ugyanabban a SaveChanges-ben írja az outbox-bejegyzést, mint a rendelést: így nem keletkezhet rendelés értesítés nélkül, sem értesítés rendelés nélkül.
- Infrastructure: OutboxMessage entitás (Id, OrderId, EventType, StatusCode, CreatedAt, ProcessedAt, Attempts, LastError), navigációval az Order-re, hogy új rendelésnél a mentéskor generált OrderId-t az EF Core töltse ki. Új migráció: AddOutbox.
- Application: EmailTemplates osztály magyar sablonokkal (rendelés beérkezett; állapot megváltozott – Kiszállítás alatt esetén a futár neve és telefonszáma; kézbesítve; lemondva), HTML és sima szöveges változattal. A felhasználói adatokat (név, cím, megjegyzés) HTML-kódold. Tárgy: „FoodOrder #12 – Kiszállítás alatt”.
- OutboxProcessor : BackgroundService (Infrastructure): 5 másodpercenként legfeljebb 20 feldolgozatlan, 5-nél kevesebb próbálkozású bejegyzést dolgoz fel CreatedAt szerint; betölti a rendelést (és a futárt), a levelet KÜLDÉSKOR rendereli az EmailTemplates-szel, a címzett a rendelés CustomerEmail-je. Siker: ProcessedAt. Hiba: Attempts++, LastError, figyelmeztető log. A szolgáltatás kivétel miatt soha ne álljon le; scope-ot IServiceScopeFactory-val nyit.
- IEmailGateway (Application) + MailKitEmailGateway (Infrastructure). Beállítások SmtpOptions osztályban („Smtp” szekció: Host, Port, UseStartTls, UserName, Password, FromAddress, FromName), ValidateOnStart-tal. Az appsettings.Development.json az smtp4dev-re mutasson (localhost, 2525, StartTls és hitelesítés nélkül); jelszó csak user-secretsben lehet.

2. VALÓS IDEJŰ PUSH SIGNALR-REL
- OrderHub a /hubs/orders címen, [Authorize]. Csatlakozáskor a felhasználó a „user-{UserId}” csoportba kerül, a Staff szerepkörűek a „staff” csoportba is. A csoporttagságot kizárólag a szerver dönti el; a hub ne fogadjon csoportváltó hívást.
- Application: IOrderUpdatePublisher.PublishOrderChangedAsync(orderId, customerId, version); implementáció a szerverben IHubContext<OrderHub>-bal: „OrderChanged” üzenet a vásárló csoportjának és a staff csoportnak. Csak az azonosító és a verzió menjen ki; a részleteket a kliens az API-ból kéri le, így a jogosultság-ellenőrzés egy helyen marad.
- A publikálás a sikeres SaveChanges UTÁN történjen; ha hibázik, csak naplózzon, a kérés ne bukjon el.
- Kliens: OrderUpdatesClient szolgáltatás (HubConnection, WithAutomaticReconnect, OrderChanged esemény), bejelentkezés után indul, kijelentkezéskor leáll.
- A követőoldal és a dolgozói lista iratkozzon fel, frissítse az érintett rendelést; a pollingot távolítsd el.
- Felugró értesítés (toast) a vásárlónak: „A(z) #12 rendelésed új állapota: Kiszállítás alatt”, aria-live régióval.

KIMENET ÉS SZABÁLYOK
- .NET 10, C#, nullable; a meglévő neveket tartsd meg; csak a felsorolt NuGet-csomagokat használd; projektfájlt (.csproj, .slnx) NE adj vissza; ha csomag vagy hivatkozás hiányzik, csak írd le, melyik projekthez mit adjak hozzá.
- Először fájlfa (új / módosított / törlendő jelöléssel), majd minden új vagy módosított fájl TELJES tartalma útvonallal, kihagyás nélkül. Ha a válasz túl hosszú lenne, egy fájl végén állj meg „FOLYTATÁS KÖVETKEZIK” jelzéssel; a „folytasd” szóra a következő fájllal indulj.
- Felületi szövegek és hibaüzenetek magyarul, kódnevek angolul.
- A végén: migrációs parancs (AddOutbox), az smtp4dev indítása, futtatás, és kézi tesztforgatókönyv két böngészőablakkal és az smtp4dev postafiókkal.

## 7.Hét
Tapasztalt .NET fejlesztőként és tesztelőként zárd le a FoodOrder egyetemi projektet (Blazor Web App, .NET 10, WebAssembly kliens, Clean Architecture, Minimal API, EF Core SQL Server LocalDB, Identity, MailKit outbox, SignalR). Ez a 7., utolsó hét: tesztek, csiszolás, dokumentáció és demó.

KONTEXTUS (csatolom a forráskódot):
- Projektek: FoodOrder.Domain, FoodOrder.Application, FoodOrder.Infrastructure, FoodOrder.Shared, FoodOrder (szerver), FoodOrder.Client, FoodOrder.Domain.Tests (kész).
- Új, üres tesztprojektek (én hoztam létre): FoodOrder.Application.Tests (→ Application, Domain) és FoodOrder.Api.Tests (→ FoodOrder szerver; csomagok: Microsoft.AspNetCore.Mvc.Testing, Microsoft.EntityFrameworkCore.Sqlite, NetArchTest.Rules).
- Tesztfiókok: dolgozo@foodorder.hu, anna@foodorder.hu, bela@foodorder.hu – jelszó: Demo123!

1. APPLICATION TESZTEK (FoodOrder.Application.Tests, kézi fake-ekkel, mock-könyvtár nélkül)
- PlaceOrder: elérhetetlen étel; nem létező étel (NotFound); a vásárló adatai az ICurrentUser-ből jönnek; keletkezik outbox-esemény.
- ChangeOrderStatus: Staff teljes lánc; Customer csak a saját, Leadva állapotú rendelését mondhatja le, minden másra Forbidden; ExpectedVersion eltérés → ConflictException; sikeres váltásnál outbox-esemény és publikálás.
- TrackOrder: más vásárló rendelésére NotFound; AllowedNextStatuses szerepkör szerint.

2. API INTEGRÁCIÓS TESZTEK (FoodOrder.Api.Tests, WebApplicationFactory<Program>)
- A szerver Program.cs végére: public partial class Program { }.
- Saját factory „Testing” környezettel. Az alkalmazás SQL Server LocalDB-t használ, a tesztek viszont memóriabeli SQLite-ot (nyitva tartott SqliteConnection, minden tesztosztálynak saját adatbázis): a ConfigureTestServices-ben távolítsd el a DbContextOptions<AppDbContext> és az IDbContextOptionsConfiguration<AppDbContext> regisztrációt, majd regisztráld újra UseSqlite-tal. Mivel a migrációk SQL Server-specifikusak, Testing környezetben a Program ne migráljon és ne seedeljen; a factory EnsureCreated-et hív, majd kézzel futtatja a seedert (szerepkörök, tesztfiókok, ételek, futárok). A valódi IEmailGateway helyett üzeneteket rögzítő fake; az OutboxProcessor tesztből kézzel futtatható egy körre.
- Bejelentkező segédmetódus: POST /api/auth/login, a cookie-t a tesztkliens kezeli.
- Esetek: bejelentkezés nélkül POST /api/dishes → 401, vásárlóként → 403, dolgozóként → 201. Teljes életút Leadva → Kézbesítve futárral. A kritikus szabály: Kézbesítve → Készül → 409 ProblemDetails magyar detail-lel. Futár nélkül „Kiszállítás alatt” → elutasítva. Anna nem látja Béla rendelését → 404. Anna lemondja a saját Leadva rendelését → 200; Elfogadva állapotban → 403. Ugyanazzal az ExpectedVersion-nel két kérés → a második 409. Leadás és egy outbox-kör után a fake gateway pontosan egy levelet kapott Anna címére.
- A tesztek egymástól függetlenek; a futtatási sorrend nem számít.

3. ARCHITEKTÚRATESZTEK (NetArchTest.Rules, a FoodOrder.Api.Tests-ben)
- A Domain nem függ az Application, Infrastructure és Shared projekttől, sem a Microsoft.EntityFrameworkCore névtértől.
- Az Application nem függ az Infrastructure-től és az EF Core-tól.
- A FoodOrder.Client nem hivatkozik a Domainre, az Application-re és az Infrastructure-re.

4. CSISZOLÁS
- Fordítási figyelmeztetések (nullable is) nullára; nem használt kód és fájl törlése.
- Akadálymentesség: minden űrlapmezőnek label, ikongombnak szöveges címke, látható fókusz, a toast aria-live régióban.
- Egységes, magyar nyelvű 404-es és váratlanhiba-oldal.
- Strukturált naplózás a use case-ekben (rendelés azonosító, felhasználó, régi és új állapot).

5. DOKUMENTÁCIÓ
- README.md a solution gyökerébe: rövid leírás; követelmény-lefedési táblázat; architektúra Mermaid-diagrammal (projektek és hivatkozások); állapotgép Mermaid stateDiagram-mal; futtatás Visual Studio 2026-ban és VS Code-ban; LocalDB és SSMS-kapcsolódás; migrációk; smtp4dev és user-secrets; tesztfiókok; tesztek futtatása; ismert korlátok; AI-használat (hivatkozás a naplóra).
- docs/demo.md: 5 perces demóforgatókönyv lépésenként (két böngészőablak + smtp4dev).

KIMENET ÉS SZABÁLYOK
- .NET 10, C#, nullable; a meglévő neveket tartsd meg; csak a felsorolt NuGet-csomagokat használd; projektfájlt (.csproj, .slnx) NE adj vissza; ha csomag vagy hivatkozás hiányzik, csak írd le, melyik projekthez mit adjak hozzá.
- Először fájlfa (új / módosított / törlendő jelöléssel), majd minden új vagy módosított fájl TELJES tartalma útvonallal, kihagyás nélkül. Ha a válasz túl hosszú lenne, egy fájl végén állj meg „FOLYTATÁS KÖVETKEZIK” jelzéssel; a „folytasd” szóra a következő fájllal indulj.
- Felületi szövegek és hibaüzenetek magyarul, kódnevek angolul.
- A végén: a teljes tesztkészlet futtatása (Test Explorer és dotnet test) és a végső kézi ellenőrzőlista.
