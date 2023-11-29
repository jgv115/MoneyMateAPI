-- Drop existing categories view
DROP VIEW categories_and_subcategories CASCADE;

-- Recreate categories and transactions view
CREATE VIEW categories_and_subcategories
            (PROFILEID, CATEGORYID, CATEGORYNAME, SUBCATEGORYID, SUBCATEGORYNAME)
AS
SELECT p.id, c.id, c.name, sc.id, sc.name
FROM category c
         LEFT JOIN subcategory sc on sc.category_id = c.id
         JOIN profile p on p.id = c.profile_id;

CREATE VIEW Transactions
            (transactionId, user_id, user_identifier, profile_id, transaction_timestamp, transaction_type, amount,
             category,
             subcategory, payerPayeeName, notes)
AS
SELECT t.id,
       u.id,
       u.user_identifier,
       profile.id,
       transaction_timestamp,
       tt.name,
       amount,
       cs.CATEGORYNAME,
       cs.SUBCATEGORYNAME,
       COALESCE(p.name, ''),
       notes
FROM Transaction t
         JOIN users u on t.user_id = u.id
         JOIN profile  on t.profile_id = profile.id
         JOIN TransactionType tt on tt.id = t.transaction_type_id
         JOIN categories_and_subcategories cs ON cs.subcategoryid = t.subcategory_id
         LEFT JOIN payerpayee p on t.payerpayee_id = p.id;