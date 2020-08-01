#
# VARIABLES
#

variable "region" {
    type = string
    default = "us-west-2"
}

variable "url_auth" {
  type = string
  default = ""
}

provider "aws" {
  region = var.region
  shared_credentials_file = "~/.aws/thecat.credentials"
}

# Additional provider configuration for aws default region
provider "aws" {
  alias  = "east"
  region = "us-east-1"
  shared_credentials_file = "~/.aws/thecat.credentials"
}

# retrieves the default vpc for this region
data "aws_vpc" "default" {
  default = true
}

# retrieves the subnet ids in the default vpc
data "aws_subnet_ids" "all" {
  vpc_id = data.aws_vpc.default.id
}

data "aws_ami" "ubuntu" {
  most_recent = true

  filter {
    name   = "name"
    values = ["ubuntu/images/hvm-ssd/ubuntu-disco-19.04-amd64-server-*"]
  }

  filter {
    name   = "virtualization-type"
    values = ["hvm"]
  }

  owners = ["099720109477"] # Canonical
}

#
# RESOURCES
#

resource "aws_security_group" "rumble-batch" {
  name = "aws-batch-rumble-security-group"
  description = "AWS Batch Security Group"
  vpc_id = data.aws_vpc.default.id

  egress {
    from_port       = 0
    to_port         = 65535
    protocol        = "tcp"
    cidr_blocks     = [ "0.0.0.0/0" ]
  }
}

resource "aws_security_group" "coin" {
  name = "coin-security-group"
  description = "AWS Batch Security Group"
  vpc_id = data.aws_vpc.default.id

  egress {
    from_port       = 0
    to_port         = 65535
    protocol        = "tcp"
    cidr_blocks     = [ "0.0.0.0/0" ]
  }
  ingress {
    from_port       = 22
    to_port         = 22
    protocol        = "tcp"
    cidr_blocks     = [ "0.0.0.0/0" ]
  }
  ingress {
    from_port       = 8332
    to_port         = 8332
    protocol        = "tcp"
    cidr_blocks     = [ "0.0.0.0/0" ]
  }
}

resource "aws_ecr_repository" "rumble-job-repo" {
  name = "thecat/rumble"
}

resource "aws_instance" "lambda-coin" {
  ami = data.aws_ami.ubuntu.id
  instance_type = "t2.micro"
  key_name = "ssh"
  vpc_security_group_ids = [
    aws_security_group.coin.id
  ]
}

resource "aws_route53_record" "coin" {
  zone_id = aws_route53_zone.rumbletoon.zone_id
  name = "coin.${var.domain}"
  type = "CNAME"
  ttl = 300
  records = [
    aws_instance.lambda-coin.public_dns
  ]
}


#
# OUTPUTS
#

output "ecr_repository" {
  value = aws_ecr_repository.rumble-job-repo.repository_url
}

output "rumble_bucket" {
  value = aws_s3_bucket.rumble-bucket.id
}

output "instance_dns" {
  value = aws_instance.lambda-coin.public_dns
}
