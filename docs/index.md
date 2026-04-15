<style>
*{box-sizing:border-box;margin:0;padding:0}
.doc{font-family:var(--font-sans);color:var(--color-text-primary);max-width:860px;margin:0 auto;padding:2rem 1.5rem}
.cover{text-align:center;padding:3rem 2rem 2.5rem;border-bottom:0.5px solid var(--color-border-tertiary);margin-bottom:2.5rem;position:relative}
.cover-badge{display:inline-flex;align-items:center;gap:6px;background:var(--color-background-secondary);border:0.5px solid var(--color-border-secondary);border-radius:20px;padding:4px 14px;font-size:12px;color:var(--color-text-secondary);margin-bottom:1.5rem}
.cover-symbol{font-size:2.8rem;display:block;margin-bottom:0.75rem;opacity:0.85}
.cover h1{font-family:var(--font-serif);font-size:2.6rem;font-weight:500;letter-spacing:-0.5px;line-height:1.15;margin-bottom:0.6rem}
.cover-sub{font-size:1rem;color:var(--color-text-secondary);line-height:1.6;max-width:560px;margin:0 auto 1.5rem}
.cover-tags{display:flex;flex-wrap:wrap;gap:8px;justify-content:center}
.tag{background:var(--color-background-info);color:var(--color-text-info);font-size:11px;font-weight:500;padding:3px 10px;border-radius:var(--border-radius-md)}
.toc{background:var(--color-background-secondary);border:0.5px solid var(--color-border-tertiary);border-radius:var(--border-radius-lg);padding:1.25rem 1.5rem;margin-bottom:2.5rem}
.toc-title{font-size:11px;font-weight:500;text-transform:uppercase;letter-spacing:0.08em;color:var(--color-text-secondary);margin-bottom:1rem}
.toc-grid{display:grid;grid-template-columns:repeat(auto-fill,minmax(200px,1fr));gap:3px}
.toc-item{font-size:13px;color:var(--color-text-secondary);padding:4px 8px;border-radius:6px;display:flex;align-items:center;gap:8px}
.toc-item:hover{background:var(--color-background-primary);color:var(--color-text-primary)}
.toc-num{font-size:11px;font-weight:500;color:var(--color-text-tertiary);min-width:18px}
.section{margin-bottom:2.5rem}
.section-header{display:flex;align-items:baseline;gap:12px;margin-bottom:1.25rem;padding-bottom:0.75rem;border-bottom:0.5px solid var(--color-border-tertiary)}
.section-num{font-family:var(--font-serif);font-size:1.8rem;font-weight:500;color:var(--color-text-tertiary);line-height:1;flex-shrink:0}
.section-title{font-family:var(--font-serif);font-size:1.35rem;font-weight:500;line-height:1.25}
.section-icon{font-size:1.1rem;opacity:0.7;flex-shrink:0}
.subsection{margin:1.25rem 0}
.subsection h3{font-size:0.9rem;font-weight:500;text-transform:uppercase;letter-spacing:0.07em;color:var(--color-text-secondary);margin-bottom:0.75rem;display:flex;align-items:center;gap:8px}
.subsection h3::after{content:'';flex:1;height:0.5px;background:var(--color-border-tertiary)}
p{font-size:0.9375rem;line-height:1.75;color:var(--color-text-secondary);margin-bottom:0.75rem}
.tbl{width:100%;border-collapse:collapse;font-size:0.875rem;margin-top:0.75rem}
.tbl thead tr{background:var(--color-background-secondary)}
.tbl th{text-align:left;font-weight:500;font-size:11px;text-transform:uppercase;letter-spacing:0.07em;color:var(--color-text-secondary);padding:9px 12px;border-bottom:0.5px solid var(--color-border-secondary)}
.tbl td{padding:9px 12px;border-bottom:0.5px solid var(--color-border-tertiary);color:var(--color-text-secondary);vertical-align:top;line-height:1.55}
.tbl tr:last-child td{border-bottom:none}
.tbl td:first-child{font-weight:500;color:var(--color-text-primary);white-space:nowrap}
.tbl tbody tr:hover td{background:var(--color-background-secondary)}
.priority-badge{display:inline-flex;align-items:center;gap:4px;font-size:11px;font-weight:500;padding:2px 8px;border-radius:4px}
.p-hoog{background:var(--color-background-danger);color:var(--color-text-danger)}
.p-mid{background:var(--color-background-warning);color:var(--color-text-warning)}
.callout{background:var(--color-background-secondary);border-left:2px solid var(--color-border-info);border-radius:0 var(--border-radius-md) var(--border-radius-md) 0;padding:0.875rem 1rem;margin:0.75rem 0;font-size:0.875rem;color:var(--color-text-secondary);line-height:1.6}
.callout strong{color:var(--color-text-primary)}
.arch-grid{display:grid;grid-template-columns:repeat(3,1fr);gap:10px;margin:0.75rem 0}
.arch-card{background:var(--color-background-secondary);border:0.5px solid var(--color-border-tertiary);border-radius:var(--border-radius-md);padding:0.875rem 1rem}
.arch-card-title{font-size:12px;font-weight:500;color:var(--color-text-primary);margin-bottom:4px}
.arch-card-body{font-size:12px;color:var(--color-text-secondary);line-height:1.5}
.stack-list{list-style:none;display:flex;flex-direction:column;gap:6px;margin:0.75rem 0}
.stack-item{display:flex;align-items:center;gap:10px;font-size:0.875rem}
.stack-dot{width:6px;height:6px;border-radius:50%;background:var(--color-border-info);flex-shrink:0}
.stack-name{font-weight:500;color:var(--color-text-primary);min-width:160px}
.stack-desc{color:var(--color-text-secondary)}
.risk-item{background:var(--color-background-secondary);border:0.5px solid var(--color-border-tertiary);border-radius:var(--border-radius-md);padding:0.875rem 1rem;margin-bottom:8px}
.risk-title{font-size:0.875rem;font-weight:500;color:var(--color-text-primary);margin-bottom:4px}
.risk-body{font-size:0.8125rem;color:var(--color-text-secondary);line-height:1.55}
.scenario-list{display:flex;flex-direction:column;gap:8px;margin-top:0.75rem}
.scenario{display:flex;gap:10px;align-items:flex-start;font-size:0.875rem}
.scenario-icon{width:20px;height:20px;border-radius:50%;background:var(--color-background-success);color:var(--color-text-success);font-size:11px;font-weight:500;display:flex;align-items:center;justify-content:center;flex-shrink:0;margin-top:1px}
.scenario-text{color:var(--color-text-secondary);line-height:1.55}
.divider{height:0.5px;background:var(--color-border-tertiary);margin:2rem 0}
@media(max-width:600px){.arch-grid{grid-template-columns:1fr}.cover h1{font-size:2rem}.toc-grid{grid-template-columns:1fr}}
</style>

<div class="doc">
<h2 class="sr-only">SharpChess arc42 architectuurdocument</h2>

<div class="cover">
  <div class="cover-badge">arc42 · Architectuurdocument</div>
  <span class="cover-symbol">♟</span>
  <h1>SharpChess</h1>
  <p class="cover-sub">Architectuuroverzicht van een full-stack schaakwebapplicatie met strikte scheiding van frontend, backend en infrastructuur.</p>
  <div class="cover-tags">
    <span class="tag">React + TypeScript</span>
    <span class="tag">ASP.NET Core</span>
    <span class="tag">EF Core</span>
    <span class="tag">JWT</span>
    <span class="tag">PostgreSQL</span>
    <span class="tag">Docker</span>
  </div>
</div>

<div class="toc">
  <div class="toc-title">Inhoudsopgave</div>
  <div class="toc-grid">
    <div class="toc-item"><span class="toc-num">1</span>Inleiding & Doelstellingen</div>
    <div class="toc-item"><span class="toc-num">2</span>Beperkingen</div>
    <div class="toc-item"><span class="toc-num">3</span>Context</div>
    <div class="toc-item"><span class="toc-num">4</span>Oplossingsstrategie</div>
    <div class="toc-item"><span class="toc-num">5</span>Bouwblokweergave</div>
    <div class="toc-item"><span class="toc-num">6</span>Runtimeweergave</div>
    <div class="toc-item"><span class="toc-num">7</span>Implementatieweergave</div>
    <div class="toc-item"><span class="toc-num">8</span>Concepten</div>
    <div class="toc-item"><span class="toc-num">9</span>Ontwerpbeslissingen</div>
    <div class="toc-item"><span class="toc-num">10</span>Kwaliteitsvereisten</div>
    <div class="toc-item"><span class="toc-num">11</span>Risico's</div>
    <div class="toc-item"><span class="toc-num">12</span>Woordenlijst</div>
  </div>
</div>

<div class="section">
  <div class="section-header">
    <span class="section-num">1</span>
    <span class="section-title">Inleiding & Doelstellingen</span>
    <span class="section-icon">♔</span>
  </div>
  <p>SharpChess heeft een client-serverarchitectuur en bestaat uit drie lagen. De frontend is de interface. De backend bevat de logica en de beveiliging. Het dataproject slaat de gegevens op via Entity Framework Core.</p>

  <div class="subsection">
    <h3>1.1 Vereistenoverzicht</h3>
    <table class="tbl">
      <thead><tr><th>Gebied</th><th>Beschrijving</th></tr></thead>
      <tbody>
        <tr><td>Gebruikersbeheer</td><td>Gebruikers moeten zich kunnen registreren, hun e-mailadres kunnen bevestigen en veilig kunnen inloggen.</td></tr>
        <tr><td>Spelinteractie</td><td>Gebruikers moeten een schaakbord kunnen bekijken, zetten kunnen indienen en actuele spelstatusinformatie kunnen ontvangen.</td></tr>
        <tr><td>Regelhandhaving</td><td>Schaakzetten moeten in de backend worden gevalideerd, zodat de browser nooit bepaalt of een zet geldig is.</td></tr>
        <tr><td>Persistentie</td><td>Relevante gegevens zoals accounts, tokens en partijen moeten consistent worden opgeslagen via de backend.</td></tr>
        <tr><td>Architectuur</td><td>De oplossing moet onderhoudbaar blijven door frontend-, backend- en data/infrastructuurverantwoordelijkheden te scheiden.</td></tr>
      </tbody>
    </table>
  </div>

  <div class="subsection">
    <h3>1.2 Kwaliteitsdoelen</h3>
    <table class="tbl">
      <thead><tr><th>Prioriteit</th><th>Kwaliteitsdoel</th><th>Reden</th></tr></thead>
      <tbody>
        <tr><td><span class="priority-badge p-hoog">Hoog</span></td><td>Onderhoudbaarheid</td><td>Het project moet begrijpelijk en aanpasbaar blijven tijdens ontwikkeling en beoordeling.</td></tr>
        <tr><td><span class="priority-badge p-hoog">Hoog</span></td><td>Correctheid</td><td>Schaakregels, authenticatiestromen en toestandsovergangen moeten consistent werken.</td></tr>
        <tr><td><span class="priority-badge p-hoog">Hoog</span></td><td>Beveiliging</td><td>Wachtwoorden, tokens en beveiligde endpoints moeten veilig worden behandeld.</td></tr>
        <tr><td><span class="priority-badge p-hoog">Hoog</span></td><td>Scheiding van verantwoordelijkheden</td><td>Elk project moet een duidelijke verantwoordelijkheid hebben om architectuurvervuiling te voorkomen.</td></tr>
        <tr><td><span class="priority-badge p-mid">Gemiddeld</span></td><td>Testbaarheid</td><td>Belangrijke logica moet verifieerbaar zijn via geautomatiseerde tests en API-tests.</td></tr>
        <tr><td><span class="priority-badge p-mid">Gemiddeld</span></td><td>Bruikbaarheid</td><td>De interface moet kernacties zoals registratie, inloggen en zetten indienen eenvoudig maken.</td></tr>
      </tbody>
    </table>
  </div>

  <div class="subsection">
    <h3>1.3 Stakeholders</h3>
    <table class="tbl">
      <thead><tr><th>Stakeholder</th><th>Belang in het systeem</th></tr></thead>
      <tbody>
        <tr><td>Eindgebruikers</td><td>Willen een duidelijke en betrouwbare manier om online te schaken.</td></tr>
        <tr><td>Studentontwikkelaar</td><td>Heeft een schone architectuur nodig die realistisch te bouwen, te verklaren en uit te breiden is.</td></tr>
        <tr><td>Docenten & beoordelaars</td><td>Hebben inzicht nodig in architectuurredenering, scheiding van verantwoordelijkheden en technische onderbouwing.</td></tr>
        <tr><td>Toekomstige beheerders</td><td>Hebben een systeem nodig dat gemakkelijk te begrijpen, debuggen en door te ontwikkelen is zonder kernregels te breken.</td></tr>
      </tbody>
    </table>
  </div>
</div>

<div class="divider"></div>

<div class="section">
  <div class="section-header">
    <span class="section-num">2</span>
    <span class="section-title">Beperkingen</span>
    <span class="section-icon">♖</span>
  </div>

  <div class="subsection">
    <h3>2.1 Technisch</h3>
    <table class="tbl">
      <thead><tr><th>Beperking</th><th>Impact op de architectuur</th></tr></thead>
      <tbody>
        <tr><td>React + TypeScript frontend</td><td>De client is geïmplementeerd als een aparte webapplicatie en communiceert uitsluitend via HTTP.</td></tr>
        <tr><td>ASP.NET Core Web API backend</td><td>Bedrijfslogica en API-endpoints zijn gecentraliseerd in een toegewijd backendproject.</td></tr>
        <tr><td>Entity Framework Core</td><td>Databasetoegang wordt uitsluitend afgehandeld via EF Core in de data/infrastructuurlaag.</td></tr>
        <tr><td>JWT-gebaseerde authenticatie</td><td>Beveiligde endpoints zijn afhankelijk van op tokens gebaseerde autorisatie in plaats van serversessies.</td></tr>
        <tr><td>Aparte projecten</td><td>Frontend, backend en data/infrastructuur blijven gescheiden om architectuurgrenzen te bewaken.</td></tr>
      </tbody>
    </table>
  </div>

  <div class="subsection">
    <h3>2.2 Organisatorisch</h3>
    <div class="callout">Het project wordt ontwikkeld in een <strong>onderwijscontext</strong> en vereist daarom verklaarbare ontwerpbeslissingen. De beschikbare ontwikkeltijd is beperkt, waardoor de scope realistisch moet blijven. De documentatie is geschreven voor docenten en medestudenten. Het systeem moet demonstraties, testen en incrementele oplevering ondersteunen.</div>
  </div>

  <div class="subsection">
    <h3>2.3 Conventies</h3>
    <div class="callout">Frontend en backend communiceren uitsluitend via <strong>REST/HTTP met JSON-payloads</strong>. De front-end heeft geen directe toegang tot de database. De backend geeft nooit EF Core-entiteiten direct terug - antwoorden worden gemapt naar DTO's. C4-diagrammen en een arc42-achtige documentstructuur worden gebruikt om architectuurbeslissingen toe te lichten.</div>
  </div>
</div>

<div class="divider"></div>

<div class="section">
  <div class="section-header">
    <span class="section-num">3</span>
    <span class="section-title">Context</span>
    <span class="section-icon">♗</span>
  </div>
  <p><strong>Bedrijfscontext:</strong> Gebruikers spelen online schaak via de browser. SharpChess gebruikt externe diensten voor e-mailverificatie en aanvullende schaakfuncties. Gebruikers communiceren met de front-end; de back-end valideert de schaakzetten en gebruikt een externe e-mailservice voor accountverificatie.</p>
  <p><strong>Implementatiecontext:</strong> De applicatie kan lokaal via containers draaien, of afzonderlijk in een cloudomgeving worden uitgerold.</p>
</div>

<div class="divider"></div>

<div class="section">
  <div class="section-header">
    <span class="section-num">4</span>
    <span class="section-title">Oplossingsstrategie</span>
    <span class="section-icon">♕</span>
  </div>
  <p>SharpChess heeft een klassieke web-architectuur met een strikte scheiding van verantwoordelijkheden. Het systeem bestaat uit drie delen.</p>
  <div class="arch-grid">
    <div class="arch-card">
      <div class="arch-card-title">Frontend</div>
      <div class="arch-card-body">React + TypeScript single-page applicatie. Toont de interface en verstuurt acties naar de backend.</div>
    </div>
    <div class="arch-card">
      <div class="arch-card-title">Backend</div>
      <div class="arch-card-body">ASP.NET Core Web API met applicatie- en domeinlogica. Valideert acties en spelregels.</div>
    </div>
    <div class="arch-card">
      <div class="arch-card-title">Data / Infrastructuur</div>
      <div class="arch-card-body">EF Core-configuratie, repositories, databasemigraties en externe service-implementaties.</div>
    </div>
  </div>
  <p style="margin-top:0.75rem">De back-end bevat de schaaklogica en is de <strong>enige bron van waarheid</strong>. De front-end en backend communiceren via REST en JSON, met JWT voor authenticatie.</p>
</div>

<div class="divider"></div>

<div class="section">
  <div class="section-header">
    <span class="section-num">5</span>
    <span class="section-title">Bouwblokweergave</span>
    <span class="section-icon">♘</span>
  </div>

  <div class="subsection">
    <h3>5.7 Authenticatiemodule</h3>
    <ul class="stack-list">
      <li class="stack-item"><span class="stack-dot"></span><span class="stack-name">Registratiestroom</span><span class="stack-desc">Aanmaken van nieuwe accounts.</span></li>
      <li class="stack-item"><span class="stack-dot"></span><span class="stack-name">E-mailverificatie</span><span class="stack-desc">Eenmalige tokens met verloopafhandeling.</span></li>
      <li class="stack-item"><span class="stack-dot"></span><span class="stack-name">Inlogstroom</span><span class="stack-desc">Tokens uitsluitend aan geldige en geverifieerde gebruikers.</span></li>
      <li class="stack-item"><span class="stack-dot"></span><span class="stack-name">Wachtwoordverwerking</span><span class="stack-desc">Veilige hashing in plaats van opslag als platte tekst.</span></li>
    </ul>
  </div>

  <div class="subsection">
    <h3>5.8 Spelmodule</h3>
    <ul class="stack-list">
      <li class="stack-item"><span class="stack-dot"></span><span class="stack-name">Bordtoestandsophaling</span><span class="stack-desc">Huidige spelstatus ophalen voor de frontend.</span></li>
      <li class="stack-item"><span class="stack-dot"></span><span class="stack-name">Zetindiening & -validatie</span><span class="stack-desc">Controle of gevraagde zet legaal is.</span></li>
      <li class="stack-item"><span class="stack-dot"></span><span class="stack-name">Speltoestandsvoortgang</span><span class="stack-desc">Voortgang en resultaatafhandeling.</span></li>
      <li class="stack-item"><span class="stack-dot"></span><span class="stack-name">Persistentie</span><span class="stack-desc">Opslag van partijeninformatie via backend-abstracties.</span></li>
    </ul>
  </div>
</div>

<div class="divider"></div>

<div class="section">
  <div class="section-header">
    <span class="section-num">6</span>
    <span class="section-title">Runtimeweergave</span>
    <span class="section-icon">♙</span>
  </div>
  <p><strong>Registratiestroom:</strong> Bij registratie stuurt de frontend de ingevulde gegevens naar de backend. De backend valideert het verzoek, hasht het wachtwoord, slaat de gebruiker op en verstuurt een verificatie-e-mail. De frontend toont daarna het resultaat.</p>
</div>

<div class="divider"></div>

<div class="section">
  <div class="section-header">
    <span class="section-num">7</span>
    <span class="section-title">Implementatieweergave</span>
    <span class="section-icon">♖</span>
  </div>
  <p>De codebasis is opgesplitst in afzonderlijke projecten voor API, applicatie, domein, infrastructuur en tests. Deze indeling dwingt architectuurgrenzen af en houdt afhankelijkheden controleerbaar.</p>
</div>

<div class="divider"></div>

<div class="section">
  <div class="section-header">
    <span class="section-num">8</span>
    <span class="section-title">Concepten</span>
    <span class="section-icon">♔</span>
  </div>
  <ul class="stack-list">
    <li class="stack-item"><span class="stack-dot"></span><span class="stack-name">DTO's</span><span class="stack-desc">Afgeschermde contracten tussen client en server.</span></li>
    <li class="stack-item"><span class="stack-dot"></span><span class="stack-name">JWT-authenticatie</span><span class="stack-desc">Stateless authenticatie voor beveiligde endpoints.</span></li>
    <li class="stack-item"><span class="stack-dot"></span><span class="stack-name">EF Core</span><span class="stack-desc">Persistentie en mapping naar de relationele database.</span></li>
    <li class="stack-item"><span class="stack-dot"></span><span class="stack-name">Gelaagde architectuur</span><span class="stack-desc">Scheiding tussen presentatie, applicatie, domein en infrastructuur.</span></li>
  </ul>
</div>

<div class="divider"></div>

<div class="section">
  <div class="section-header">
    <span class="section-num">9</span>
    <span class="section-title">Ontwerpbeslissingen</span>
    <span class="section-icon">♙</span>
  </div>
  <div class="callout"><strong>Belangrijk:</strong> De backend bewaakt spelregels en beveiliging centraal. Daardoor blijft de frontend dun en is kritieke logica niet afhankelijk van browsergedrag.</div>
</div>

<div class="divider"></div>

<div class="section">
  <div class="section-header">
    <span class="section-num">10</span>
    <span class="section-title">Kwaliteitsvereisten</span>
    <span class="section-icon">♗</span>
  </div>

  <div class="subsection">
    <h3>10.1 Utility Tree</h3>
    <table class="tbl">
      <thead><tr><th>Categorie</th><th>Sub-kwaliteit</th><th>Belang</th><th>Toelichting</th></tr></thead>
      <tbody>
        <tr><td>Onderhoudbaarheid</td><td>Duidelijke gelaagdheid</td><td><span class="priority-badge p-hoog">Hoog</span></td><td>Verantwoordelijkheden moeten gemakkelijk te lokaliseren en te wijzigen zijn.</td></tr>
        <tr><td>Correctheid</td><td>Regelhandhaving</td><td><span class="priority-badge p-hoog">Hoog</span></td><td>Illegale zetten en ongeldige stromen moeten betrouwbaar worden geblokkeerd.</td></tr>
        <tr><td>Beveiliging</td><td>Authenticatieveiligheid</td><td><span class="priority-badge p-hoog">Hoog</span></td><td>Inloggegevens en beveiligde bronnen mogen niet onzorgvuldig worden blootgesteld.</td></tr>
        <tr><td>Testbaarheid</td><td>Geïsoleerde logica</td><td><span class="priority-badge p-mid">Gemiddeld</span></td><td>Kernlogica moet testbaar zijn zonder de UI.</td></tr>
        <tr><td>Bruikbaarheid</td><td>Duidelijke feedback</td><td><span class="priority-badge p-mid">Gemiddeld</span></td><td>Gebruikers moeten begrijpen wat het systeem doet en waarom acties slagen of mislukken.</td></tr>
      </tbody>
    </table>
  </div>

  <div class="subsection">
    <h3>10.2 Kwaliteitsscenario's</h3>
    <div class="scenario-list">
      <div class="scenario"><div class="scenario-icon">1</div><div class="scenario-text">Wanneer een niet-geverifieerde gebruiker probeert in te loggen, wijst de backend het verzoek af met een duidelijk antwoord dat de status uitlegt.</div></div>
      <div class="scenario"><div class="scenario-icon">2</div><div class="scenario-text">Wanneer de frontend een illegale zet stuurt, wijst de backend deze af zonder de opgeslagen spelstatus te beschadigen.</div></div>
      <div class="scenario"><div class="scenario-icon">3</div><div class="scenario-text">Wanneer een toekomstige ontwikkelaar de registratielogica moet wijzigen, is de wijziging mogelijk zonder tegelijkertijd frontend-rendercode of persistentiedetails aan te passen.</div></div>
      <div class="scenario"><div class="scenario-icon">4</div><div class="scenario-text">Wanneer een onverwachte uitzondering optreedt, logt het systeem voldoende context voor diagnose en stuurt een veilig antwoord terug naar de client.</div></div>
    </div>
  </div>
</div>

<div class="divider"></div>

<div class="section">
  <div class="section-header">
    <span class="section-num">11</span>
    <span class="section-title">Risico's</span>
    <span class="section-icon">♕</span>
  </div>
  <div class="risk-item">
    <div class="risk-title">11.1 Frontend-verbinding</div>
    <div class="risk-body">SharpChess gebruikt expliciete DTO's en API-tests om de contracten tussen frontend en backend te borgen.</div>
  </div>
  <div class="risk-item">
    <div class="risk-title">11.2 Inspanning & Scope</div>
    <div class="risk-body">Een schaakplatform kan gemakkelijk in scope groeien bij functies zoals matchmaking, persistentie, timers en geavanceerde gameplay. Het project vereist bewuste scopebeheersing zodat architectuurkwaliteit niet wordt opgeofferd voor het aantal functies.</div>
  </div>
  <div class="risk-item">
    <div class="risk-title">11.3 Regelcorrectheid & Beveiliging</div>
    <div class="risk-body">De backend beheert de schaaklogica en authenticatie centraal. Strikte validatie en duidelijke autorisatie borgen de correctheid en veiligheid van het systeem.</div>
  </div>
</div>

<div class="divider"></div>

<div class="section">
  <div class="section-header">
    <span class="section-num">12</span>
    <span class="section-title">Woordenlijst</span>
    <span class="section-icon">♘</span>
  </div>
  <table class="tbl">
    <thead><tr><th>Term</th><th>Betekenis in SharpChess</th></tr></thead>
    <tbody>
      <tr><td>DTO</td><td>Een data transfer object dat wordt gebruikt voor communicatie tussen frontend en backend.</td></tr>
      <tr><td>EF Core</td><td>Entity Framework Core, gebruikt voor persistentie en databasetoegang in de infrastructuurlaag.</td></tr>
      <tr><td>JWT</td><td>JSON Web Token gebruikt voor stateless authenticatie en autorisatie.</td></tr>
      <tr><td>Zetvalidatie</td><td>De backend-controle die bepaalt of een gevraagde schaakzet legaal is.</td></tr>
      <tr><td>Spelstatus</td><td>De huidige toestand van een schaakpartij, inclusief bordgegevens en beurtvolgorde.</td></tr>
      <tr><td>Verificatietoken</td><td>Een eenmalig token dat wordt gebruikt om een nieuw aangemaakt gebruikersaccount te bevestigen.</td></tr>
    </tbody>
  </table>
</div>

</div>
