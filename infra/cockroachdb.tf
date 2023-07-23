#resource "cockroach_cluster" "moneymate_cluster" {
#  name           = var.cockroach_db_cluster_name
#  cloud_provider = "AWS"
#  serverless = {
#    spend_limit = 0
#  }
#  regions = [{
#    name = "ap-southeast-1"
#  }]
#}
#
#resource cockroach_database moneymate_database {
#  cluster_id = cockroach_cluster.moneymate_cluster.id
#  name = "moneymate_db_${local.tags.environment}"
#}