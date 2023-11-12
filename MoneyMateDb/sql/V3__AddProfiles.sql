CREATE TABLE profile
(
    id           UUID PRIMARY KEY DEFAULT GEN_RANDOM_UUID(),
    display_name STRING NOT NULL
);

CREATE TABLE userprofile
(
    user_id    UUID REFERENCES users (id),
    profile_id UUID REFERENCES profile (id),
    CONSTRAINT userprofiles_pk PRIMARY KEY (user_id, profile_id)
);

-- Create a default profile for all users
INSERT INTO profile (id, display_name)
SELECT id, 'Default Profile'
FROM users;

-- Create a lookup for the default profile
INSERT INTO userprofile (user_id, profile_id)
SELECT id, id
FROM users;

-- Add profile_id to category and new profile_id unique constraint
ALTER table category
    ADD COLUMN profile_id UUID REFERENCES profile (id);

UPDATE category
SET profile_id = user_Id;

ALTER table category
    ALTER COLUMN profile_id SET NOT NULL;

ALTER TABLE category
    ADD UNIQUE (name, profile_id, transaction_type_id);

-- Add profile_id to payerpayee
ALTER table payerpayee
    ADD COLUMN profile_id UUID REFERENCES profile (id);

UPDATE payerpayee
SET profile_id = user_Id;

ALTER table payerpayee
    ALTER COLUMN profile_id SET NOT NULL;

ALTER TABLE payerpayee
    ADD UNIQUE (profile_id, payerPayeeType_id, external_link_id) where external_link_id != '';

ALTER TABLE payerpayee
    ADD UNIQUE (profile_id, payerPayeeType_id, name) where external_link_id = '';

-- Add profile_id to transaction
ALTER table transaction
    ADD COLUMN profile_id UUID REFERENCES profile (id);

UPDATE transaction
SET profile_id = user_Id;

ALTER table transaction
    ALTER COLUMN profile_id SET NOT NULL;