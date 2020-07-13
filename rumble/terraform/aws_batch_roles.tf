resource "aws_iam_role" "ecs-instance-role" {
  name = "ecs-instance-role"
  assume_role_policy = <<EOF
{
    "Version": "2012-10-17",
    "Statement": [
    {
        "Action": "sts:AssumeRole",
        "Effect": "Allow",
        "Principal": {
        "Service": "ec2.amazonaws.com"
        }
    }
    ]
}
EOF
}

resource "aws_iam_role_policy_attachment" "ecs-instance-role" {
  role       = aws_iam_role.ecs-instance-role.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AmazonEC2ContainerServiceforEC2Role"
}


resource "aws_iam_role" "spot-fleet-role" {
  name = "batch-spot-fleet-role"
  assume_role_policy = <<EOF
{
    "Version": "2012-10-17",
    "Statement": [
    {
        "Action": "sts:AssumeRole",
        "Effect": "Allow",
        "Principal": {
        "Service": "spotfleet.amazonaws.com"
        }
    }
    ]
}
EOF
}

resource "aws_iam_role_policy_attachment" "spot-fleet-policy" {
  role       = aws_iam_role.spot-fleet-role.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AmazonEC2SpotFleetRole"
}

resource "aws_iam_instance_profile" "ecs-instance-role" {
  name  = "ecs-instance-role"
  role = aws_iam_role.ecs-instance-role.name
}

resource "aws_iam_role" "aws-batch-service-role" {
  name = "aws-batch-service-role"
  assume_role_policy = <<EOF
{
    "Version": "2012-10-17",
    "Statement": [
    {
        "Action": "sts:AssumeRole",
        "Effect": "Allow",
        "Principal": {
        "Service": "batch.amazonaws.com"
        }
    }
    ]
}
EOF
}

resource "aws_iam_role_policy_attachment" "aws-batch-service-role" {
  role       = aws_iam_role.aws-batch-service-role.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AWSBatchServiceRole"
}


# AWS Batch Job
resource "aws_iam_role" "task-role" {
  name = "aws-batch-rumble-task-role"
  assume_role_policy = <<EOF
{
    "Version": "2012-10-17",
    "Statement": [
    {
        "Action": "sts:AssumeRole",
        "Effect": "Allow",
        "Principal": {
          "Service": "ecs-tasks.amazonaws.com"
        }
    }
    ]
}
EOF
}

resource "aws_iam_policy" "task-policy" {
  name = "aws-batch-rumble-task-policy"
  path = "/BatchSample/"
  policy = <<EOF
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Action": [
        "dynamodb:DescribeTable",
        "dynamodb:DescribeGlobalTable",
        "dynamodb:PutItem"
      ],
      "Effect": "Allow",
      "Resource": "${aws_dynamodb_table.rumble-job-table.arn}"
    },
    {
      "Action": [
        "s3:Get*",
        "s3:Put*"
      ],
      "Effect": "Allow",
      "Resource": [
        "${aws_s3_bucket.rumble-bucket.arn}",
        "${aws_s3_bucket.rumble-bucket.arn}/*",
        "${aws_s3_bucket.logs-bucket.arn}",
        "${aws_s3_bucket.logs-bucket.arn}/*"
      ]
    },
    {
      "Action": [
        "lambda:InvokeFunction"
      ],
      "Effect": "Allow",
      "Resource": "*"
    }
  ]
}
EOF
}

resource "aws_iam_role_policy_attachment" "task-role" {
  role = aws_iam_role.task-role.name
  policy_arn = aws_iam_policy.task-policy.arn
}
