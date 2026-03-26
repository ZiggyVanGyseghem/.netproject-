\# To-Do List: Projectwerk .NET - ORW Bestelsysteem



\## Fase 1: Project Setup \& Structuur

\- \[x] \[cite\_start]Maak een leeg ASP.NET Core MVC project aan \[cite: 46]

\- \[x] Configureer een correcte `.gitignore` voor .NET en IDE's

\- \[x] \[cite\_start]Stel de basis layout in met het Bootstrap framework \[cite: 47]

\- \[x] Genereer een overzichtelijk database ERD (DBML/PNG)



\## Fase 2: Database \& Datamodellen (Persistentie)

\- \[x] Bepaal de database structuur (SQL/Tabellen) inclusief relaties en prijshistoriek

\- \[x] Maak de C# Entity Framework Core Modellen aan (Gebruikers, Rollen, Producten, Bestellingen, etc.)

\- \[x] Configureer de `AppDbContext` en connectiestring (appsettings.json)

\- \[x] Voer de eerste EF Core Migration uit om de database fysiek aan te maken

\- \[x] Voeg wat testdata (seeding) toe voor tafels en een paar testproducten



\## Fase 3: Bezoekers Applicatie (Mobiele Webpagina)

\- \[x] Bouw de statische Startpagina en het statische Menu

\- \[x] \[cite\_start]\*\*Dynamisch Menu:\*\* Lees het unieke tafelnummer uit de URL (gescand via QR) en toon dit op het scherm \[cite: 8, 12]

\- \[x] \[cite\_start]\*\*Dynamisch Assortiment:\*\* Haal de drank en versnaperingen rechtstreeks op uit de database \[cite: 13]

\- \[x] \[cite\_start]\*\*Winkelmandje (Overzicht):\*\* Maak een dynamisch overzicht met eenheidsprijs, aantal, subtotaal en totaalprijs \[cite: 15, 16]

\- \[ ] \[cite\_start]\*\*Mollie API Integratie:\*\* Zorg dat de bestelling pas definitief is na een succesvolle betaling via Mollie \[cite: 18, 19]

\- \[ ] \[cite\_start]\*\*Status Pagina:\*\* Laat de bezoeker live de status van hun bestelling volgen (in wachtrij, in bereiding, etc.) \[cite: 20]



\## Fase 4: Medewerker Dashboards

\- \[ ] \[cite\_start]\*\*Bar Dashboard:\*\* Toon alle bestelde dranken en voeg een knop toe om ze 'klaar' te zetten \[cite: 27, 28]

\- \[ ] \[cite\_start]\*\*Keuken Dashboard:\*\* Toon alle bestelde versnaperingen en voeg een knop toe om ze 'klaar' te zetten \[cite: 25, 26]

\- \[ ] \[cite\_start]\*\*Zaal Dashboard:\*\* Toon een overzicht van alle klaargezette bestellingen per tafel, zodat zaalmedewerkers ze kunnen rondbrengen \[cite: 29]



\## Fase 5: Admin Dashboard

\- \[ ] \[cite\_start]\*\*Beheer Assortiment:\*\* Maak CRUD-pagina's (Create, Read, Update, Delete) om producten toe te voegen, te verwijderen of prijzen aan te passen \[cite: 43]

\- \[ ] \[cite\_start]\*\*Bestelhistoriek:\*\* Maak een tabel met tijdstip, tafel, totaalprijs, gebruiker, bestelstatus en betaalstatus \[cite: 31]

\- \[ ] \[cite\_start]Voeg sorteer- en filterfunctionaliteit toe aan alle velden in de historiek (behalve de detail-link) \[cite: 33]

\- \[ ] \[cite\_start]\*\*Detailpagina Bestelling:\*\* Maak een pagina die de historiekgegevens toont plus alle bestelde producten (aantal, eenheidsprijs, subtotaal) \[cite: 32]

\- \[x] \[cite\_start]\*\*Gebruikersbeheer (Deel 1):\*\* Maak een admin-pagina om gebruikers aan te maken en te verwijderen \[cite: 38, 42]

\- \[ ] \[cite\_start]\*\*Gebruikersbeheer (Deel 2):\*\* Genereer een unieke URL/QR-code voor nieuwe gebruikers om hun registratie te voltooien (wachtwoord kiezen) \[cite: 38]



\## Fase 6: Authenticatie \& Autorisatie

\- \[x] Implementeer een Login/Logout systeem

\- \[x] \[cite\_start]Koppel de juiste rollen aan gebruikers (Bezoeker, Barmedewerker, Keukenmedewerker, Zaalmedewerker, Admin, Administrator) \[cite: 39]

\- \[x] \[cite\_start]Beveilig de routes: API voor iedereen, Dashboards enkel voor medewerkers, mobiele bestelpagina enkel voor bezoekers \[cite: 44]

\- \[ ] \[cite\_start]Zorg dat admins rollen kunnen toewijzen en wijzigen (meerdere rollen per gebruiker mogelijk) \[cite: 39, 40, 41]



\## Fase 7: JSON Web API

\- \[ ] \[cite\_start]Maak een API endpoint voor: Minst/meest bestelde drank en versnapering \[cite: 34, 35]

\- \[ ] \[cite\_start]Maak een API endpoint voor: Tafel met de meeste uitgaven (drank/versnapering) \[cite: 36]



\## Fase 8: Hosting \& Afronding

\- \[X] \[cite\_start]Host de database lokaal via een Docker omgeving (`docker-compose.yml`) \[cite: 48]

\- \[ ] \[cite\_start]Deploy de volledige website naar de cloud (bijv. Azure of vergelijkbare dienst) \[cite: 48]

\- \[ ] \[cite\_start]Test op crashbestendigheid en zorg dat tijdelijke stroomuitval geen dataverlies veroorzaakt \[cite: 45]

