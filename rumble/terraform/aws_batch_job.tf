resource "aws_batch_compute_environment" "compute" {
  compute_environment_name = "rumble-spot"
  compute_resources {
    instance_role = aws_iam_instance_profile.ecs-instance-role.arn
    instance_type = ["optimal"] # ["m3", "m4"]
    max_vcpus = 48
    min_vcpus = 0
    security_group_ids = [aws_security_group.rumble-batch.id]
    subnets = data.aws_subnet_ids.all.ids

    # Spot-related configuration
    type = "SPOT"
    bid_percentage = 100
    spot_iam_fleet_role = aws_iam_role.spot-fleet-role.arn
  }
  service_role = aws_iam_role.aws-batch-service-role.arn
  type = "MANAGED"
  depends_on = [aws_iam_role_policy_attachment.aws-batch-service-role]
}

resource "aws_batch_compute_environment" "compute-z" {
  compute_environment_name = "rumble-spot-z"
  compute_resources {
    instance_role = aws_iam_instance_profile.ecs-instance-role.arn
    instance_type = ["z1d"] # ["m3", "m4"]
    max_vcpus = 16
    min_vcpus = 0
    security_group_ids = [aws_security_group.rumble-batch.id]
    subnets = data.aws_subnet_ids.all.ids

    # Spot-related configuration
    type = "SPOT"
    bid_percentage = 100
    spot_iam_fleet_role = aws_iam_role.spot-fleet-role.arn
  }
  service_role = aws_iam_role.aws-batch-service-role.arn
  type = "MANAGED"
  depends_on = [aws_iam_role_policy_attachment.aws-batch-service-role]
}

resource "aws_batch_compute_environment" "compute-coin" {
  compute_environment_name = "rumble-spot-coin"
  compute_resources {
    instance_role = aws_iam_instance_profile.ecs-instance-role.arn
    instance_type = ["z1d"] # ["m3", "m4"]
    max_vcpus = 4
    min_vcpus = 0
    security_group_ids = [aws_security_group.rumble-batch.id]
    subnets = data.aws_subnet_ids.all.ids

    # Spot-related configuration
    type = "SPOT"
    bid_percentage = 100
    spot_iam_fleet_role = aws_iam_role.spot-fleet-role.arn
  }
  service_role = aws_iam_role.aws-batch-service-role.arn
  type = "MANAGED"
  depends_on = [aws_iam_role_policy_attachment.aws-batch-service-role]
}

# AWS Batch Queue
resource "aws_batch_job_queue" "rumble-queue-default" {
  name = "rumble-queue-1"
  state = "ENABLED"
  priority = 1
  compute_environments = [
    aws_batch_compute_environment.compute-z.arn,
    aws_batch_compute_environment.compute.arn
  ]
  depends_on = [
    aws_batch_compute_environment.compute
  ]
}

resource "aws_batch_job_queue" "rumble-queue-smoke" {
  name = "rumble-queue-2"
  state = "ENABLED"
  priority = 2
  compute_environments = [
    aws_batch_compute_environment.compute-z.arn,
    aws_batch_compute_environment.compute.arn
  ]
  depends_on = [
    aws_batch_compute_environment.compute
  ]
}

resource "aws_batch_job_queue" "rumble-queue-coin" {
  name = "rumble-queue-200"
  state = "ENABLED"
  priority = 200
  compute_environments = [
    aws_batch_compute_environment.compute-coin.arn,
    aws_batch_compute_environment.compute-z.arn,
    aws_batch_compute_environment.compute.arn
  ]
  depends_on = [
    aws_batch_compute_environment.compute
  ]
}

resource "aws_batch_job_definition" "rumble-job-latest" {
    name = "rumble-job-latest"
    type = "container"
    retry_strategy {
      attempts = 3
    }
    depends_on = [
      aws_ecr_repository.rumble-job-repo,
      aws_s3_bucket.logs-bucket,
      aws_dynamodb_table.rumble-job-table
    ]
    parameters = {
      entrants = "eyex.Eyes.example",
      inputFile = "https://www.rumbletoon.com/problems/smoke.json"
    }
    timeout {
      attempt_duration_seconds = 600
    }
    container_properties = <<EOF
{
    "image": "${aws_ecr_repository.rumble-job-repo.repository_url}:latest",
    "memory": 2000,
    "vcpus": 1,
    "jobRoleArn": "${aws_iam_role.task-role.arn}",
    "environment": [
      {"name": "AWS_REGION", "value": "${var.region}"},
      {"name": "DYNAMO_TABLE", "value": "${aws_dynamodb_table.rumble-job-table.id}"},
      {"name": "S3_LOGS_BUCKET", "value": "${aws_s3_bucket.logs-bucket.id}"}
    ],
    "command": [
      "Ref::entrants",
      "Ref::inputFile"
    ]
}
EOF
}
