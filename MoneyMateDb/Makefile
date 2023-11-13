start_db_alone:
	docker compose up -d db

start_db:
	docker compose up --wait

run_standalone_db_migration:
	docker compose up dbmigration --no-deps

stop_db:
	docker compose down -v