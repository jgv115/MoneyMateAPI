CREATE INDEX ON payerpayee using GIN (name gin_trgm_ops);