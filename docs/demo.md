# FoodOrder – 5 perces demó

## Előkészítés (a demó előtt, kb. 5 perc)

1. Indítsd el az smtp4dev-et: `smtp4dev --smtpport 2525 --urls=http://localhost:5050`, és a böngészőben nyisd meg a http://localhost:5050 oldalt. Ha vannak benne régi levelek, töröld őket.
2. Indítsd el az alkalmazást Visual Studióban (FoodOrder projekt, **https** profil, F5).
3. Nyiss két böngészőablakot egymás mellé:
   - **A ablak (normál):** https://localhost:7060, kijelentkezve. Ez lesz Kiss Anna, a vásárló.
   - **B ablak (InPrivate/inkognitó):** jelentkezz be `dolgozo@foodorder.hu` / `Demo123!` fiókkal, és nyisd meg a *Beérkezett rendelések* oldalt.
4. Tarts kéznél egy harmadik lapot az smtp4dev-vel és a README architektúraábrájával.
5. Ha a kritikus szabályt API-n is meg akarod mutatni, nyisd meg a Visual Studióban a `FoodOrder/FoodOrder/FoodOrder.http` fájlt.

## Forgatókönyv

| Idő | Ablak | Lépés | Mit mondj / mire mutass |
|---|---|---|---|
| 0:00–0:30 | README | Az architektúraábra és az állapotgép | Clean Architecture: a Domain nem ismeri a többi réteget; az állapotváltás szabályai a Domainben vannak, a kliensben nincs átmenettábla. |
| 0:30–1:15 | A | Az étlap bejelentkezés nélkül is látszik. *Kosárba* → a bejelentkező oldalra visz. Belépés: `anna@foodorder.hu` / `Demo123!`. Gulyásleves 2 db kosárba, *Kosár*, cím megadása, megjegyzésbe: `<b>teszt</b>`, *Rendelés leadása*. | A vásárló neve és e-mail-címe nem a kérésből, hanem a bejelentkezésből jön. A kosár felhasználónként külön tárolódik. |
| 1:15–1:45 | B | Újratöltés nélkül megjelenik a „Új rendelés érkezett: #N” értesítés és a zölden kiemelt kártya. | Valós idejű push SignalR-rel; csak az azonosító és a verzió megy ki, a részleteket a kliens az API-ból kéri le. |
| 1:45–2:00 | smtp4dev | A visszaigazoló levél: ékezetek rendben, a megjegyzés `<b>teszt</b>` **szövegként** jelenik meg. | Outbox: a levél a rendeléssel egy mentésben jön létre, a háttérszolgáltatás küldi el; a felhasználói adat HTML-kódolva kerül a levélbe. |
| 2:00–2:15 | A | *Rendelés követése*: az idővonalon Leadva, látszik a *Rendelés lemondása* gomb. | Vásárló csak Leadva állapotban mondhat le. |
| 2:15–3:00 | B | → Elfogadva. | Az A ablakban azonnal frissül az idővonal, eltűnik a lemondás gomb, és felugrik a „A(z) #N rendelésed új állapota: Elfogadva” értesítés. |
| | B | → Készül → Kiszállításra kész. Futár hozzárendelése (*Vincze Gábor* → *Hozzárendelés*). | Futár nélkül a „Kiszállítás alatt” gomb meg sem jelenik. |
| | B | → Kiszállítás alatt → Kézbesítve. | Az A ablakban megjelenik a futár neve és telefonszáma, majd a Kézbesítve állapot. A kézbesített kártyán nincs több állapotgomb. |
| 3:00–3:15 | smtp4dev | Minden állapotváltásról pontosan egy levél; a „Kiszállítás alatt” levélben szerepel a futár. | A futár-hozzárendelésről nem megy levél, mert az állapot nem változott. |
| 3:15–3:45 | VS | **Kritikus szabály:** a `.http` fájlban a dolgozó bejelentkezése, majd a Kézbesítve rendelésre `Preparing` kérés. Vagy a Test Explorerben a „Kézbesített rendelés nem kerülhet vissza előkészítésbe” teszt (*Group By → Traits*, `CriticalRule`). | 409 Conflict, magyar üzenet: „Nem engedélyezett állapotváltás: Kézbesítve → Készül”. A szabályt a Domain, az Application és az API szintű teszt is ellenőrzi. |
| 3:45–4:15 | A | A címsorba: `https://localhost:7060/etterem/rendelesek`. | „Nincs jogosultságod ehhez az oldalhoz” – a jogosultságot a szerver dönti el, a felület csak követi. |
| | A | Kijelentkezés, belépés `bela@foodorder.hu`-ként, majd a címsorba Anna rendelésének címe (`/rendeleseim/N`). | „A rendelés nem található” – más vásárló rendelésének még a létezése sem derül ki. |
| 4:15–4:45 | VS | Test Explorer: *Run All Tests* (130 teszt zöld), kiemelve az `ArchitectureTests`, a `SecurityTests` és a `NotificationTests`. | Az integrációs tesztek memóriabeli SQLite-on futnak, LocalDB és SMTP nélkül; az architektúrateszt a rétegszabályokat ellenőrzi. |
| 4:45–5:00 | – | Zárás: az AI-fejlesztési napló és az ismert korlátok (README). | A heti promptok, a pontosító promptok és az ellenőrzés lépései a naplóban vannak. |

## Ha valami nem a terv szerint megy

- **Nincs bejelentkezés, vagy kilépteti a felhasználót:** a http profillal indult; állítsd a **https** profilra.
- **Nem jön levél:** fut-e az smtp4dev a 2525-ös porton? A levél nem vész el: indítás után néhány másodpercen belül megérkezik (SSMS-ben az `OutboxMessages` táblában látszik a próbálkozások száma).
- **Nem frissül azonnal a másik ablak:** a felső sávban „Kapcsolat helyreállítása…” jelzés látszik? Várj pár másodpercet; újracsatlakozás után az oldal magától újratölti az adatokat.
- **Nincsenek tesztfiókok:** az alkalmazás nem Development környezetben fut (`ASPNETCORE_ENVIRONMENT=Development` a launchSettings.json-ban).
