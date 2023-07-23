data cockroach_cluster "moneymate_cluster" {
  id = var.cockroach_db_cluster_id
}

resource cockroach_database moneymate_database {
  cluster_id = data.cockroach_cluster.moneymate_cluster.id
  name = "moneymate_db_${local.tags.environment}"
}