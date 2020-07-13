workflow "Build and Deploy" {
  on = "push"
  resolves = ["Run smoke tests"]
}

# Deploy Filter

action "Deploy branch filter" {
  uses = "actions/bin/filter@master"
  args = "branch master"
}

# Build

action "Build Docker image" {
  needs = ["Deploy branch filter"]
  uses = "actions/docker/cli@master"
  args = ["build", "-f", "rumble/docker/Dockerfile", "--build-arg", "GIT_TAG=$GITHUB_SHA", "-t", "rumble", "."]
}

# AWS

action "Build Next export" {
  needs = ["Deploy branch filter"]
  uses = "actions/npm@master"
  runs = ["sh", "-c", "cd rumble/web && yarn install && npm run export"]
}

action "Sync static site" {
  needs = ["Build Next export"]
  uses = "actions/aws/cli@master"
  secrets = ["AWS_ACCESS_KEY_ID", "AWS_SECRET_ACCESS_KEY"]
  env = {
    AWS_DEFAULT_REGION = "us-west-2"
  }
  args = "s3 sync rumble/web/out s3://www.rumbletoon.com/"
}

action "Login to ECR" {
  needs = ["Deploy branch filter"]
  uses = "actions/aws/cli@master"
  secrets = ["AWS_ACCESS_KEY_ID", "AWS_SECRET_ACCESS_KEY"]
  env = {
    AWS_DEFAULT_REGION = "us-west-2"
  }
  args = "ecr get-login --no-include-email --region $AWS_DEFAULT_REGION | sh"
}

action "Tag image for ECR" {
  needs = ["Build Docker image"]
  uses = "actions/docker/tag@master"
  env = {
    CONTAINER_REGISTRY_PATH = "402072331520.dkr.ecr.us-west-2.amazonaws.com/thecat"
    IMAGE_NAME = "rumble"
  }
  args = ["$IMAGE_NAME", "$CONTAINER_REGISTRY_PATH/$IMAGE_NAME"]
}

action "Push image to ECR" {
  needs = ["Login to ECR", "Tag image for ECR"]
  uses = "actions/docker/cli@master"
  env = {
    CONTAINER_REGISTRY_PATH = "402072331520.dkr.ecr.us-west-2.amazonaws.com/thecat"
    IMAGE_NAME = "rumble"
  }
  args = ["push", "$CONTAINER_REGISTRY_PATH/$IMAGE_NAME"]
}

# Rumble

action "Run smoke tests" {
  uses = "actions/aws/cli@master"
  needs = ["Push image to ECR", "Sync static site"]
  secrets = ["AWS_ACCESS_KEY_ID", "AWS_SECRET_ACCESS_KEY"]
  env = {
    AWS_DEFAULT_REGION = "us-west-2"
  }
  runs = ["sh", "-c", "for i in rumble/docker/entrants/*; do aws lambda invoke --function-name rumble --payload '{\"entrants\": [\"'${i##*/}'\"], \"group\": \"smoke\", \"tag\": \"'$GITHUB_SHA'\"}' log; done;"]
}
