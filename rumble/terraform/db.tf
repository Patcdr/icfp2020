
resource "aws_dynamodb_table" "rumble-job-table" {
  name = "RumbleJob"
  billing_mode = "PAY_PER_REQUEST"
  hash_key = "hash"
  range_key = "runtime"
  stream_enabled = true
  stream_view_type = "NEW_IMAGE"

  attribute {
    name = "hash"
    type = "S"
  }
  attribute {
    name = "entrant"
    type = "S"
  }
  attribute {
    name = "group"
    type = "S"
  }
  attribute {
    name = "problem"
    type = "S"
  }
  attribute {
    name = "runtime"
    type = "N"
  }
  attribute {
    name = "score"
    type = "N"
  }
  attribute {
    name = "id"
    type = "S"
  }

  global_secondary_index {
    name = "RumbleJobEntrant"
    hash_key = "entrant"
    range_key = "runtime"
    projection_type = "ALL"
  }

  global_secondary_index {
    name = "RumbleJobProblem"
    hash_key = "problem"
    range_key = "score"
    projection_type = "ALL"
  }

  global_secondary_index {
    name = "RumbleJobEntrantGroup"
    hash_key = "entrant"
    range_key = "group"
    projection_type = "ALL"
  }

  global_secondary_index {
    name = "RumbleJobGroup"
    hash_key = "group"
    range_key = "runtime"
    projection_type = "ALL"
  }

  global_secondary_index {
    name = "RumbleJobId"
    hash_key = "id"
    projection_type = "ALL"
  }
}

resource "aws_dynamodb_table" "rumble-leaderboard-table" {
  name = "RumbleLeaderboard"
  billing_mode = "PAY_PER_REQUEST"
  hash_key = "problem"
  range_key = "place"

  attribute {
    name = "problem"
    type = "S"
  }
  attribute {
    name = "place"
    type = "N"
  }
}
