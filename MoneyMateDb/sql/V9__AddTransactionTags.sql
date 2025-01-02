CREATE TABLE transactiontags
(
    transaction_id UUID NOT NULL REFERENCES transaction (id) ON DELETE CASCADE,
    tag_id         UUID NOT NULL REFERENCES tag (id) ON DELETE CASCADE,
    PRIMARY KEY (transaction_id, tag_id)
);