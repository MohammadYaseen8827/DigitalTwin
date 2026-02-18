# Terraform variables for Digital Twin Platform

variable "project_name" {
  description = "Project name prefix"
  type        = string
  default     = "digitaltwin"
}

variable "environment" {
  description = "Environment name (dev, staging, prod)"
  type        = string
  default     = "prod"
}

variable "region" {
  description = "AWS region"
  type        = string
  default     = "us-west-2"
}

variable "availability_zones" {
  description = "List of availability zones"
  type        = list(string)
  default     = ["us-west-2a", "us-west-2b", "us-west-2c"]
}

variable "vpc_cidr" {
  description = "VPC CIDR block"
  type        = string
  default     = "10.0.0.0/16"
}

# EKS Variables
variable "eks_cluster_name" {
  description = "EKS cluster name"
  type        = string
  default     = ""
}

variable "eks_version" {
  description = "EKS Kubernetes version"
  type        = string
  default     = "1.28"
}

variable "eks_node_groups" {
  description = "EKS node group configurations"
  type = map(object({
    instance_types = list(string)
    min_size       = number
    max_size       = number
    desired_size   = number
    capacity_type  = string
  }))
  default = {
    general = {
      instance_types = ["t3.medium"]
      min_size       = 2
      max_size       = 10
      desired_size   = 3
      capacity_type  = "SPOT"
    }
    compute = {
      instance_types = ["c5.large"]
      min_size       = 1
      max_size       = 5
      desired_size   = 2
      capacity_type  = "ON_DEMAND"
    }
  }
}

# Database Variables
variable "db_name" {
  description = "Database name"
  type        = string
  default     = "digitaltwin"
}

variable "db_username" {
  description = "Database username"
  type        = string
  default     = "digitaltwin"
}

variable "db_instance_class" {
  description = "Database instance class"
  type        = string
  default     = "db.t4g.medium"
}

variable "db_allocated_storage" {
  description = "Allocated storage in GB"
  type        = number
  default     = 20
}

variable "db_max_allocated_storage" {
  description = "Maximum allocated storage in GB"
  type        = number
  default     = 100
}

# Redis Variables
variable "redis_node_type" {
  description = "Redis node type"
  type        = string
  default     = "cache.t4g.micro"
}

variable "redis_engine_version" {
  description = "Redis engine version"
  type        = string
  default     = "6.2"
}

variable "redis_num_cache_nodes" {
  description = "Number of cache nodes"
  type        = number
  default     = 1
}

# Networking Variables
variable "public_subnets" {
  description = "Public subnet CIDR blocks"
  type        = list(string)
  default     = ["10.0.101.0/24", "10.0.102.0/24", "10.0.103.0/24"]
}

variable "private_subnets" {
  description = "Private subnet CIDR blocks"
  type        = list(string)
  default     = ["10.0.1.0/24", "10.0.2.0/24", "10.0.3.0/24"]
}

# Security Variables
variable "allowed_ingress_cidrs" {
  description = "CIDR blocks allowed to access the application"
  type        = list(string)
  default     = ["0.0.0.0/0"]
}

variable "ssh_access_cidrs" {
  description = "CIDR blocks allowed SSH access"
  type        = list(string)
  default     = ["10.0.0.0/8"]
}

# Monitoring Variables
variable "enable_monitoring" {
  description = "Enable monitoring stack (Prometheus, Grafana)"
  type        = bool
  default     = true
}

variable "enable_logging" {
  description = "Enable logging stack (ELK)"
  type        = bool
  default     = true
}

# Backup Variables
variable "backup_retention_days" {
  description = "Backup retention period in days"
  type        = number
  default     = 30
}

variable "snapshot_retention_limit" {
  description = "Snapshot retention limit for Redis"
  type        = number
  default     = 7
}

# Cost Optimization Variables
variable "enable_spot_instances" {
  description = "Enable spot instances for cost optimization"
  type        = bool
  default     = true
}

variable "spot_max_price" {
  description = "Maximum spot instance price"
  type        = string
  default     = "0.05"
}

# Tags
variable "tags" {
  description = "Common tags for all resources"
  type        = map(string)
  default = {
    Project     = "DigitalTwinPlatform"
    Owner       = "DevOps"
    ManagedBy   = "Terraform"
  }
}

# Environment-specific overrides
locals {
  environment_tags = {
    Environment = var.environment
    Name        = "${var.project_name}-${var.environment}"
  }
  
  merged_tags = merge(var.tags, local.environment_tags)
  
  # Environment-specific defaults
  eks_cluster_name_default = var.eks_cluster_name != "" ? var.eks_cluster_name : "${var.project_name}-${var.environment}"
}