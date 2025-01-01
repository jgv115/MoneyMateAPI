CREATE TABLE tag
(
    id         UUID PRIMARY KEY DEFAULT GEN_RANDOM_UUID(),
    name       STRING NOT NULL,
    profile_id UUID   NOT NULL REFERENCES profile (id) ON DELETE CASCADE,
    created_at TIMESTAMPTZ      DEFAULT CURRENT_TIMESTAMP,

    UNIQUE (name, profile_id)
)