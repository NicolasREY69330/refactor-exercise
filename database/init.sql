CREATE TABLE orders (
    id text PRIMARY KEY,
    is_premium_member boolean NOT NULL,
    total_price double precision NOT NULL DEFAULT 0,
    discount double precision NOT NULL DEFAULT 0
);

CREATE TABLE order_items (
    id integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    order_id text NOT NULL REFERENCES orders(id),
    category text NOT NULL,
    price double precision NOT NULL
);

CREATE TABLE coupons (
    code text PRIMARY KEY,
    discount_percentage double precision NOT NULL,
    is_valid boolean NOT NULL
);

CREATE TABLE order_events (
    id bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    order_id text NOT NULL REFERENCES orders(id),
    message text NOT NULL,
    created_at timestamp NOT NULL
);

INSERT INTO orders (id, is_premium_member, total_price) VALUES
    ('order-1001', false, 100),
    ('order-1002', true, 200);

INSERT INTO order_items (order_id, category, price) VALUES
    ('order-1001', 'Books', 40),
    ('order-1001', 'Electronics', 60),
    ('order-1002', 'Clothing', 80),
    ('order-1002', 'Electronics', 120);

INSERT INTO coupons (code, discount_percentage, is_valid) VALUES
    ('SAVE10', 0.1, true),
    ('EXPIRED', 0.5, false);
