# Entretien développeur — NovaMarket

**Durée : 50–60 min.** Poser les questions centrales 1–6 ; choisir une ou deux questions de réserve selon le poste. Le candidat ne doit connaître aucun dépôt interne.

## Le système fictif

NovaMarket est une boutique en ligne. PostgreSQL est la source de vérité ; l’outbox est écrite dans la même transaction métier. Redis accélère les lectures, Elasticsearch sert la recherche. Le parcours RAG est isolé du parcours commande.

```mermaid
flowchart LR
  U[Web / App] --> G[API Gateway] --> API[APIs .NET 10]
  API -->|Données + outbox, même transaction| PG[(PostgreSQL)]
  PG --> O[Outbox relay] --> K[(Kafka)] --> W[Projection workers]
  W --> R[(Redis cache)]
  W --> ES[(Elasticsearch)]
  G --> S[Search API]
  S --> R
  S --> ES
  D[Documents] --> I[Ingestion + embeddings] --> V[(Vector store / pgvector)]
  H[Support] --> Q[RAG API] --> V
  Q --> L[LLM externe]
  L --> Q --> H
```

**Carte d’incident fictive** (chiffres d’exercice, pas des données réelles) : campagne x8 prévue ; en test, le p99 passe de 250 ms à 2,4 s, le taux 5xx atteint 4 %, l’attente du pool PostgreSQL dépasse 800 ms et l’âge du message Kafka atteint 3 min.

## Questions centrales

| # | Scénario | Question principale | Relances |
|---|---|---|---|
| 1 | C# / annulation | Un client annule un `GET` pendant une lecture PostgreSQL. Tracez le `CancellationToken` depuis ASP.NET Core jusqu’au repository. Comment distingueriez-vous annulation client, timeout et panne de dépendance ? | Quelles signatures async faut-il vérifier ? Quel test prouverait que l’annulation traverse réellement la commande DB ? |
| 2 | Pic de charge | Pendant la campagne x8, comment localiseriez-vous le goulot avant de modifier le code ? | Quels logs, métriques et traces relieriez-vous ? Comment mesurer p95/p99, fan-out et attente du pool sous une charge reproductible ? |
| 3 | PostgreSQL / transaction | La commande est commitée, mais la publication de l’événement échoue. Que peut observer le client et quel état reste dans le système ? | Comment testeriez-vous un échec entre commit métier, relais outbox et réponse HTTP ? Que se passe-t-il lors d’un retry du client ? |
| 4 | Kafka / doublons | Un worker met à jour Elasticsearch puis s’arrête avant l’avancement durable de l’offset ; le message est relivré. Comment suivez-vous le parcours et vérifiez-vous le résultat ? | Que distinguez-vous entre message invalide, erreur temporaire et annulation ? Quel test couvre un effet de bord suivi d’un crash ? |
| 5 | Redis / Elasticsearch | PostgreSQL contient la nouvelle valeur, mais la recherche renvoie une ancienne entrée Redis ou un document Elasticsearch en retard. Comment diagnostiqueriez-vous et qualifieriez-vous la réponse ? | Quels indicateurs séparent cache stale, retard de projection et absence métier ? Quel comportement faut-il tester pendant le rétablissement ? |
| 6 | Tests / résilience C# | Proposez une suite de tests pour un timeout ou une réponse 503 d’une dépendance, puis son rétablissement. | Qu’est-ce qui relève du test unitaire, d’intégration, de contrat ou de charge ? Comment éviter qu’un retry masque l’échec ou amplifie la charge ? |

## Questions de réserve — IA en production

| # | Scénario | Question principale | Relances |
|---|---|---|---|
| 7 | Embeddings / RAG | Un document change ou est supprimé ; le retrieval devient lent et deux utilisateurs ont des droits différents. Comment suivre l’ingestion jusqu’aux citations de la réponse ? | Quelles métriques séparent embedding, retrieval et génération ? Quels tests couvrent suppression, permissions et qualité des citations ? |
| 8 | Harness d’agent | Un appel d’outil expire, mais l’action a peut-être déjà eu lieu ; le run attend une validation humaine puis reprend après redémarrage. Que faut-il tracer et tester ? | Comment éviter une répétition non voulue ? Où vérifier à nouveau permission et approbation ? |

Tout le contexte nécessaire à l’entretien est fourni dans cette fiche.
