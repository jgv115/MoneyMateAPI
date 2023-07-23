resource cockroach_database moneymate_database {
  cluster_id = var.cockroach_db_cluster_name
  name = "moneymate_db_${local.tags.environment}"
}