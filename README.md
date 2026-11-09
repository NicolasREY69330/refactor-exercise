# Order discount refactoring exercise

This is an interview exercise, not a production-ready application. Review the code,
explain what you would change and why, then refactor a small, useful slice. You are
not expected to rewrite everything or introduce a particular architecture.

## The feature

An order's subtotal is the sum of its item prices. A valid coupon adds its percentage
discount, and premium members receive another 10 percentage points off. For example,
a premium member using `SAVE10` receives 20% off the subtotal. The resulting total
and discount are stored, along with an order event. There are no seasonal or
day-of-week rules.

The implementation may not always meet that description. Discuss ambiguous or
unexpected behavior before deciding what to preserve or fix.

## Run locally

Prerequisites: .NET 10 SDK and Docker with the Compose plugin, or an existing PostgreSQL
instance.

```sh
docker compose up -d --wait
dotnet run --project refactor-exercise.csproj --launch-profile http
```

Swagger is available at http://localhost:5108/swagger. Try the requests in
`refactor-exercise.http`, or:

```sh
curl -X POST http://localhost:5108/applyDiscount/order-1001/SAVE10
```

Seed data:

| Order | Membership | Subtotal |
| --- | --- | --- |
| `order-1001` | Standard | 100 |
| `order-1002` | Premium | 200 |

Coupons: `SAVE10` (10%, valid), `EXPIRED` (50%, invalid).

The development database is `orders` on `localhost:5432`, with username/password
`orders`/`orders`. These credentials are for this local exercise only. For an existing
database, run `database/init.sql` once against an empty database and override
`ConnectionStrings__Orders` when starting the API.

To inspect stored results:

```sh
docker compose exec postgres psql -U orders -d orders -c 'SELECT * FROM orders;'
docker compose exec postgres psql -U orders -d orders -c 'SELECT * FROM order_events;'
```

To reset **this exercise's database**, stop the API, run the following commands,
then restart the API. This deletes the Compose project's PostgreSQL volume.

```sh
docker compose down --volumes
docker compose up -d --wait
```

## Tests

```sh
dotnet test refactor-exercise.sln
```

The included tests run without PostgreSQL. Decide how much confidence they provide
and add tests that protect the behavior you change.

## During the interview

- Identify issues across the project and prioritize them by impact.
- Explain your approach and tradeoffs as you work.
- Improve a small end-to-end slice, using SOLID and clean-code principles where helpful.
- Use tests to make your changes safe, rather than aiming for a particular test count.
- Discuss what you would do next with more time.

## Interview questionnaires

The bilingual fictional NovaMarket interview pack is available in [`interviews/`](interviews/README.md).
