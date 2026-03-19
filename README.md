🗺️ SemAGraf - Vizualizátor dopravních sítí a hledání alternativních tras

Tato desktopová aplikace (WPF/C#) slouží k interaktivní tvorbě, vizualizaci a analýze topologických sítí (např. dopravní infrastruktury). Umožňuje uživateli graficky i datově spravovat mapu měst a silnic a následně v ní vyhledávat optimální cesty s využitím pokročilých grafových algoritmů.

Aplikace vznikla jako semestrální práce s důrazem na čistý objektový návrh, efektivní datové struktury a striktní oddělení výpočetní logiky od prezentační vrstvy (UI).

* Interaktivní editor mapy: Přidávání, mazání a úprava uzlů (měst) a hran (silnic) přímo přes uživatelské rozhraní.
* Vykreslování v reálném čase: Topologie grafu je vizualizována na WPF Canvasu.
* Vyhledávání K-alternativních tras: Aplikace nehledá pouze jednu nejkratší cestu, ale dokáže nabídnout $K$ nejlepších alternativních tras pomocí Yenova algoritmu.
* Dynamické překážky (Problematic Edges): Uživatel může libovolné úseky označit za neprůjezdné (uzavírky). Algoritmy tyto překážky okamžitě reflektují a trasy dynamicky přepočítávají.
* Zálohování a sdílení (JSON): Kompletní topologii sítě lze ukládat a načítat do formátu JSON (včetně uživatelsky definovaných uzavírek).
* Zobrazení vektoru následníků: Transkripce grafu do přehledné tabulkové podoby pro rychlou analýzu sousednosti.

* Dijkstrův algoritmus: Slouží jako jádro pro hledání nejkratší cesty a je upraven tak, aby respektoval jak globální uživatelské uzavírky, tak dočasné restrikce od nadřazených algoritmů.
* Yenův algoritmus (K-Shortest Paths): Postaven jako "manažer" nad Dijkstrovým algoritmem. Využívá konceptu dočasného maskování uzlů a hran k nalezení striktně acyklických (loopless) alternativních cest ve správném matematickém pořadí.
* Seznam sousednosti (Adjacency List): Graf není v paměti držen jako matice ($O(V^2)$), ale jako efektivní slovník následníků ($O(V + E)$), což minimalizuje paměťovou stopu u řídkých sítí.
* Hashovací tabulky ($O(1)$ přístup): Vyhledávání uzlů a ověřování uzavírek při průchodu grafem běží v konstantním čase díky využití kolekcí `Dictionary<TKey, TValue>` a `HashSet<T>`.
