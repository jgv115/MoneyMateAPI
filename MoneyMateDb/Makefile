start_db_alone:
	docker compose up -d db

run_db_migration:
	docker compose up dbmigration

run_standalone_db_migration:
	docker compose up dbmigration --no-deps

start_db: start_db_alone run_db_migration

stop_db:
	docker compose down