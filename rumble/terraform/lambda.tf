## lambda resource + iam

# helper to package the lambda function for deployment
data "archive_file" "lambda-rumble-zip" {
  type = "zip"
  source_dir = "../lambda/rumble"
  output_path = "../lambda/rumble.zip"
}

data "archive_file" "lambda-aggregate-zip" {
  type = "zip"
  source_dir = "../lambda/aggregate"
  output_path = "../lambda/aggregate.zip"
}

data "archive_file" "lambda-validate-zip" {
  type = "zip"
  source_dir = "../lambda/validate"
  output_path = "../lambda/validate.zip"
}

data "archive_file" "lambda-auth-zip" {
  type = "zip"
  source_dir = "../lambda/auth"
  output_path = "../lambda/auth.zip"
}

data "archive_file" "lambda-grader-zip" {
  type = "zip"
  source_dir = "../lambda/grader"
  output_path = "../lambda/grader.zip"
}

resource "aws_iam_role" "lambda-role" {
  name = "aws-batch-rumble-function-role"
  path = "/BatchSample/"
  assume_role_policy = <<EOF
{
    "Version": "2012-10-17",
    "Statement":
    [
      {
          "Action": "sts:AssumeRole",
          "Effect": "Allow",
          "Principal": {
            "Service": "lambda.amazonaws.com"
          }
      }
    ]
}
EOF
}

resource "aws_iam_policy" "lambda-policy" {
  name = "aws-batch-rumble-function-policy"
  path = "/BatchSample/"
  policy = <<EOF
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Action": [
        "batch:SubmitJob",
        "batch:Describe*",
        "batch:List*",
        "batch:Register*",
        "batch:Create*"
      ],
      "Effect": "Allow",
      "Resource": "*"
    },
    {
      "Effect": "Allow",
      "Action": [
        "s3:ListBucket"
      ],
      "Resource": ["${aws_s3_bucket.rumble-bucket.arn}"]
    },
    {
      "Effect": "Allow",
      "Action": [
        "s3:GetObject",
        "s3:PutObject"
      ],
      "Resource": ["${aws_s3_bucket.rumble-bucket.arn}/*"]
    },
    {
      "Effect": "Allow",
      "Action": [
          "iam:GetRole",
          "iam:PassRole"
      ],
      "Resource": "${aws_iam_role.task-role.arn}"
    },
    {
      "Effect": "Allow",
      "Action": [
          "lambda:InvokeFunction"
      ],
      "Resource": "*"
    },
    {
        "Effect": "Allow",
        "Action": [
            "dynamodb:Describe*",
            "dynamodb:Get*",
            "dynamodb:Put*",
            "dynamodb:Query",
            "dynamodb:Update*"
        ],
        "Resource": "arn:aws:dynamodb:*"
    }
  ]
}
EOF
}

resource "aws_iam_role" "lambda-edge-role" {
  name = "lambda-edge-role"

assume_role_policy = <<EOF
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Sid": "",
      "Effect": "Allow",
      "Principal": {
        "Service": [
          "lambda.amazonaws.com",
          "edgelambda.amazonaws.com"
        ]
      },
      "Action": "sts:AssumeRole"
    }
  ]
}
EOF
}

resource "aws_iam_role_policy_attachment" "lambda-service" {
  role = aws_iam_role.lambda-role.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AWSLambdaBasicExecutionRole"
}

resource "aws_iam_role_policy_attachment" "lambda-policy" {
  role = aws_iam_role.lambda-role.name
  policy_arn = aws_iam_policy.lambda-policy.arn
}

resource "aws_lambda_function" "rumble-function" {
  function_name = "rumble"
  filename = "../lambda/rumble.zip"
  role = aws_iam_role.lambda-role.arn
  handler = "index.handler"
  source_code_hash = data.archive_file.lambda-rumble-zip.output_base64sha256
  runtime = "nodejs10.x"
  timeout = 900
  depends_on = [aws_iam_role_policy_attachment.lambda-policy]
  environment {
    variables = {
      ECR_REPO = aws_ecr_repository.rumble-job-repo.repository_url
      S3_BUCKET_NAME = aws_s3_bucket.rumble-bucket.id
      S3_BUCKET_DOMAIN = aws_s3_bucket.rumble-bucket.bucket_domain_name
      DYNAMO_TABLE = aws_dynamodb_table.rumble-job-table.id
    }
  }
}

resource "aws_lambda_function" "aggregate-function" {
  function_name = "aggregate"
  filename = "../lambda/aggregate.zip"
  role = aws_iam_role.lambda-role.arn
  handler = "index.handler"
  source_code_hash = data.archive_file.lambda-aggregate-zip.output_base64sha256
  runtime = "nodejs10.x"
  timeout = 900
  depends_on = [aws_iam_role_policy_attachment.lambda-policy]
  environment {
    variables = {
      DYNAMO_TABLE = aws_dynamodb_table.rumble-job-table.id
    }
  }
}

resource "aws_lambda_event_source_mapping" "lambda_event_maps_dynamodb" {
  event_source_arn  = aws_dynamodb_table.rumble-job-table.stream_arn
  function_name     = aws_lambda_function.aggregate-function.arn
  starting_position = "LATEST"
}

resource "aws_lambda_function" "auth-function" {
  publish = true
  function_name = "auth"
  filename = "../lambda/auth.zip"
  provider = aws.east
  role = aws_iam_role.lambda-edge-role.arn
  handler = "index.handler"
  source_code_hash = data.archive_file.lambda-auth-zip.output_base64sha256
  runtime = "nodejs10.x"
  depends_on = [aws_iam_role_policy_attachment.lambda-policy]
  # environment {
  #   variables = {
  #   }
  # }
}

resource "aws_lambda_function" "grader-function" {
  publish = true
  function_name = "grader"
  filename = "../lambda/grader.zip"
  role = aws_iam_role.lambda-role.arn
  handler = "grader.lambda"
  source_code_hash = data.archive_file.lambda-grader-zip.output_base64sha256
  runtime = "nodejs10.x"
  depends_on = [aws_iam_role_policy_attachment.lambda-policy]
  timeout = 900
  memory_size = 512
  environment {
    variables = {
      URL_AUTH = var.url_auth
    }
  }
}

resource "aws_lambda_function" "validate-function" {
  function_name = "validate"
  filename = "../lambda/validate.zip"
  role = aws_iam_role.lambda-role.arn
  handler = "index.handler"
  source_code_hash = data.archive_file.lambda-validate-zip.output_base64sha256
  runtime = "nodejs10.x"
  timeout = 900
  depends_on = [aws_iam_role_policy_attachment.lambda-policy]
  environment {
    variables = {
      DYNAMO_TABLE = aws_dynamodb_table.rumble-job-table.id
    }
  }
}

# resource "aws_lambda_permission" "allow-cloudfront" {
#   provider = aws.east
#   statement_id = "AllowExecutionFromCloudFront"
#   action = "lambda:GetFunction"
#   function_name = aws_lambda_function.auth-function.arn
#   principal = "edgelambda.amazonaws.com"
# }

resource "aws_cloudwatch_event_rule" "every-minute" {
    name = "every-minute"
    description = "Fires every minute"
    schedule_expression = "rate(1 minute)"
}

resource "aws_cloudwatch_event_target" "coin-every-minute" {
    rule = aws_cloudwatch_event_rule.every-minute.name
    target_id = "rumble"
    input = jsonencode(map("action", "coin"))
    arn = aws_lambda_function.rumble-function.arn
}

resource "aws_lambda_permission" "allow_cloudwatch_to_rumble" {
    statement_id = "AllowExecutionFromCloudWatch"
    action = "lambda:InvokeFunction"
    function_name = aws_lambda_function.rumble-function.function_name
    principal = "events.amazonaws.com"
    source_arn = aws_cloudwatch_event_rule.every-minute.arn
}