variable "domain" {
    type = string
    default = "rumbletoon.com"
}

variable "site-name" {
    type = string
    default = "www.rumbletoon.com"
}

resource "aws_route53_zone" "rumbletoon" {
  name = var.domain
}

resource "aws_route53_record" "rumbletoon" {
  allow_overwrite = true
  zone_id = aws_route53_zone.rumbletoon.id
  name = var.domain
  ttl = 300
  type = "NS"
  records = [
    "ns-142.awsdns-17.com.",
    "ns-1876.awsdns-42.co.uk.",
    "ns-1451.awsdns-53.org.",
    "ns-836.awsdns-40.net."
  ]
}

resource "aws_route53_record" "rumbletoon-mx" {
  allow_overwrite = true
  zone_id = aws_route53_zone.rumbletoon.id
  name = var.domain
  ttl = 300
  type = "MX"
  records = [
    "1 ASPMX.L.GOOGLE.COM.",
    "5 ALT1.ASPMX.L.GOOGLE.COM.",
    "5 ALT2.ASPMX.L.GOOGLE.COM.",
    "10 ASPMX2.GOOGLEMAIL.COM.",
    "10 ASPMX3.GOOGLEMAIL.COM."
  ]
}

resource "aws_route53_record" "rumbletoon-gsuite" {
  allow_overwrite = true
  zone_id = aws_route53_zone.rumbletoon.id
  name = var.domain
  ttl = 300
  type = "TXT"
  records = [
    "google-site-verification=lRLHMHc_bZVi0J_7FxsvvP5qILFvKIDS5PS2TdSoAEg"
  ]
}

resource "aws_route53_record" "www" {
  zone_id = aws_route53_zone.rumbletoon.zone_id
  name    = var.site-name
  type    = "A"

  alias {
    name                   = aws_cloudfront_distribution.www_distribution.domain_name
    zone_id                = aws_cloudfront_distribution.www_distribution.hosted_zone_id
    evaluate_target_health = false
  }
}

resource "aws_route53_record" "logs" {
  zone_id = aws_route53_zone.rumbletoon.zone_id
  name    = "logs.${var.domain}"
  type    = "A"

  alias {
    name                   = aws_cloudfront_distribution.logs_distribution.domain_name
    zone_id                = aws_cloudfront_distribution.logs_distribution.hosted_zone_id
    evaluate_target_health = false
  }
}

resource "aws_s3_bucket" "rumble-bucket" {
  bucket = var.site-name
  policy = <<EOF
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Action": [
        "s3:GetObject"
      ],
      "Effect": "Allow",
      "Resource": "arn:aws:s3:::${var.site-name}/*",
      "Principal": "*"
    }
  ]
}
EOF

  cors_rule {
    allowed_headers = ["*"]
    allowed_methods = ["GET", "HEAD"]
    allowed_origins = [
      "https://www.rumbletoon.com",
      "http://localhost:3000",
      "https://localhost:3000"
    ]
    max_age_seconds = 3000
  }

  website {
    index_document = "index.html"
    error_document = "404.html"
  }
}

resource "aws_s3_bucket" "logs-bucket" {
  bucket = "logs.${var.domain}"
  policy = <<EOF
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Action": [
        "s3:GetObject"
      ],
      "Effect": "Allow",
      "Resource": "arn:aws:s3:::logs.${var.domain}/*",
      "Principal": "*"
    }
  ]
}
EOF

  cors_rule {
    allowed_headers = ["*"]
    allowed_methods = ["GET", "HEAD"]
    allowed_origins = [
      "https://www.rumbletoon.com",
      "http://localhost:3000",
      "https://localhost:3000"
    ]
    max_age_seconds = 3000
  }

  website {
    index_document = "index.html"
    error_document = "404.html"
  }
}

resource "aws_acm_certificate" "certificate" {
  provider = aws.east
  domain_name = "*.${var.domain}"
  validation_method = "EMAIL"
  subject_alternative_names = [var.domain]
}

resource "aws_cloudfront_distribution" "www_distribution" {
  origin {
    domain_name = aws_s3_bucket.rumble-bucket.website_endpoint
    origin_id = var.site-name

    custom_origin_config {
      http_port              = "80"
      https_port             = "443"
      origin_protocol_policy = "http-only"
      origin_ssl_protocols   = ["TLSv1", "TLSv1.1", "TLSv1.2"]
    }
  }

  enabled = true
  default_root_object = "index.html"

  default_cache_behavior {
    viewer_protocol_policy = "redirect-to-https"
    compress = true
    target_origin_id = var.site-name
    min_ttl = 0
    default_ttl = 30
    max_ttl = 30

    allowed_methods = ["GET", "HEAD"]
    cached_methods = ["GET", "HEAD"]

    forwarded_values {
      query_string = false
      cookies {
        forward = "none"
      }
    }

    lambda_function_association {
      event_type   = "viewer-request"
      lambda_arn   = aws_lambda_function.auth-function.qualified_arn
      include_body = false
    }
  }

  aliases = [var.site-name]

  restrictions {
    geo_restriction {
      restriction_type = "none"
    }
  }

  // Here's where our certificate is loaded in!
  viewer_certificate {
    acm_certificate_arn = aws_acm_certificate.certificate.arn
    ssl_support_method  = "sni-only"
  }
}

resource "aws_cloudfront_distribution" "logs_distribution" {
  origin {
    domain_name = aws_s3_bucket.logs-bucket.website_endpoint
    origin_id = "logs.${var.domain}"

    custom_origin_config {
      http_port              = "80"
      https_port             = "443"
      origin_protocol_policy = "http-only"
      origin_ssl_protocols   = ["TLSv1", "TLSv1.1", "TLSv1.2"]
    }
  }

  enabled = true
  default_root_object = "index.html"

  default_cache_behavior {
    viewer_protocol_policy = "redirect-to-https"
    compress = true
    target_origin_id = "logs.${var.domain}"
    min_ttl = 0
    default_ttl = 30
    max_ttl = 30

    allowed_methods  = ["GET", "HEAD", "OPTIONS"]
    cached_methods = ["GET", "HEAD", "OPTIONS"]

    forwarded_values {
      query_string = false
      headers = ["Origin"]
      cookies {
        forward = "none"
      }
    }
  }

  aliases = ["logs.${var.domain}"]

  restrictions {
    geo_restriction {
      restriction_type = "none"
    }
  }

  // Here's where our certificate is loaded in!
  viewer_certificate {
    acm_certificate_arn = aws_acm_certificate.certificate.arn
    ssl_support_method  = "sni-only"
  }
}
