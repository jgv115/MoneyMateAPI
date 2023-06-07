CREATE TABLE Users
(
    id              UUID PRIMARY KEY DEFAULT GEN_RANDOM_UUID(),
    user_identifier VARCHAR(255) NOT NULL UNIQUE,
    INDEX index_user_identifier (user_identifier)
);

CREATE TABLE TransactionType
(
    id   UUID PRIMARY KEY DEFAULT GEN_RANDOM_UUID(),
    name STRING NOT NULL UNIQUE
);

INSERT INTO TransactionType (name)
VALUES ('Expense'),
       ('Income');

CREATE TABLE Category
(
    id                  UUID PRIMARY KEY DEFAULT GEN_RANDOM_UUID(),
    name                VARCHAR(255) NOT NULL,
    INDEX index_category_name (name),

    user_id             UUID         NOT NULL REFERENCES Users (id),
    transaction_type_id UUID         NOT NULL REFERENCES TransactionType (id),
    
    UNIQUE (name, user_id, transaction_type_id)
);

CREATE TABLE Subcategory
(
    id          UUID PRIMARY KEY DEFAULT GEN_RANDOM_UUID(),
    name        VARCHAR(255) NOT NULL,
    category_id UUID         NOT NULL,
    FOREIGN KEY (category_id) REFERENCES Category (id) ON DELETE CASCADE,
    UNIQUE (name, category_id)
);

CREATE VIEW categories_and_subcategories
            (USERID, user_identifier, CATEGORYID, CATEGORYNAME, SUBCATEGORYID, SUBCATEGORYNAME)
AS
SELECT u.id, user_identifier, c.id, c.name, sc.id, sc.name
FROM category c
         LEFT JOIN subcategory sc on sc.category_id = c.id
         JOIN Users u on c.user_id = u.id;

CREATE TABLE PayerPayeeType
(
    id   UUID PRIMARY KEY DEFAULT GEN_RANDOM_UUID(),
    name STRING NOT NULL UNIQUE
);

INSERT INTO PayerPayeeType (name)
VALUES ('Payer'),
       ('Payee');

CREATE TABLE PayerPayeeExternalLinkType
(
    payerpayeeexternallinktype_id UUID PRIMARY KEY DEFAULT GEN_RANDOM_UUID(),
    name                          STRING NOT NULL UNIQUE
);

INSERT INTO PayerPayeeExternalLinkType (name)
VALUES ('Google'),
       ('Custom');

CREATE TABLE PayerPayee
(
    id                    UUID PRIMARY KEY DEFAULT GEN_RANDOM_UUID(),
    user_id               UUID         NOT NULL REFERENCES Users (id),
    name                  VARCHAR(255) NOT NULL,
    payerPayeeType_id     UUID         NOT NULL REFERENCES PayerPayeeType (id),
    INDEX index_payerPayeeType (payerPayeeType_id),
    external_link_type_id UUID         NOT NULL,
    FOREIGN KEY (external_link_type_id) REFERENCES PayerPayeeExternalLinkType (payerpayeeexternallinktype_id),
    external_link_id      STRING       NOT NULL,
    UNIQUE (user_id, name, payerPayeeType_id)
);

CREATE VIEW Payers (payerPayeeId, user_id, user_identifier, name, external_link_type, external_link_id) AS
SELECT pp.id, pp.user_id, u.user_identifier, pp.name, ppelt.name, pp.external_link_id
FROM PayerPayee pp
         JOIN Users u ON pp.user_id = u.id
         JOIN payerpayeetype ppt ON ppt.id = pp.payerPayeeType_id
         JOIN PayerPayeeExternalLinkType ppelt on pp.external_link_type_id = ppelt.payerpayeeexternallinktype_id
WHERE ppt.name = 'Payer';

CREATE VIEW Payees (payerPayeeId, user_id, user_identifier, name, external_link_type, external_link_id) AS
SELECT pp.id, pp.user_id, u.user_identifier, pp.name, ppelt.name, pp.external_link_id
FROM PayerPayee pp
         JOIN Users u ON pp.user_id = u.id
         JOIN payerpayeetype ppt ON ppt.id = pp.payerPayeeType_id
         JOIN PayerPayeeExternalLinkType ppelt on pp.external_link_type_id = ppelt.payerpayeeexternallinktype_id
WHERE ppt.name = 'Payee';

CREATE VIEW PayersAndPayees
            (payerPayeeId, user_id, user_identifier, name, payerPayeeType, external_link_type, external_link_id) AS
SELECT pp.id,
       pp.user_id,
       u.user_identifier,
       pp.name,
       ppt.name,
       ppelt.name,
       pp.external_link_id
FROM PayerPayee pp
         JOIN Users u ON pp.user_id = u.id
         JOIN payerpayeetype ppt ON ppt.id = pp.payerPayeeType_id
         JOIN PayerPayeeExternalLinkType ppelt on pp.external_link_type_id = ppelt.payerpayeeexternallinktype_id;

CREATE TABLE Transaction
(
    id                    UUID PRIMARY KEY DEFAULT GEN_RANDOM_UUID(),
    user_id               UUID        NOT NULL REFERENCES Users (id),
    transaction_timestamp timestamptz NOT NULL,
    INDEX index_transaction_timestamp (transaction_timestamp),
    transaction_type_id   UUID        NOT NULL,
    FOREIGN KEY (transaction_type_id) REFERENCES TransactionType (id),
    amount                DECIMAL     NOT NULL,
    subcategory_id        UUID        NOT NULL,
    FOREIGN KEY (subcategory_id) REFERENCES Subcategory (id),
    payerpayee_id         UUID        NULL,
    FOREIGN KEY (payerpayee_id) REFERENCES PayerPayee (id),
    notes                 STRING
);

CREATE VIEW Transactions
            (
             transactionId, user_id, user_identifier, transaction_timestamp, transaction_type, amount, category,
             subcategory, payerPayeeName, notes)
AS
SELECT t.id,
       u.id,
       u.user_identifier,
       transaction_timestamp,
       tt.name,
       amount,
       cs.CATEGORYNAME,
       cs.SUBCATEGORYNAME,
       COALESCE(p.name, ''),
       notes
FROM Transaction t
         JOIN users u on t.user_id = u.id
         JOIN TransactionType tt on tt.id = t.transaction_type_id
         JOIN categories_and_subcategories cs ON cs.subcategoryid = t.subcategory_id
         LEFT JOIN payerpayee p on t.payerpayee_id = p.id;