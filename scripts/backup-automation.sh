#!/bin/bash
# Comprehensive backup automation script for Digital Twin Platform

set -euo pipefail

# Configuration
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
CONFIG_FILE="$SCRIPT_DIR/backup.conf"
LOG_FILE="/var/log/digitaltwin-backup.log"

# Load configuration
if [[ -f "$CONFIG_FILE" ]]; then
    source "$CONFIG_FILE"
else
    echo "Configuration file not found: $CONFIG_FILE"
    exit 1
fi

# Default configuration if not set in config file
: ${BACKUP_ROOT:="/backups"}
: ${RETENTION_DAYS:=30}
: ${AWS_BUCKET:="digitaltwin-backups"}
: ${AWS_REGION:="us-west-2"}
: ${ENCRYPTION_KEY:=""}
: ${SLACK_WEBHOOK:=""}

# Logging function
log() {
    local level=$1
    local message=$2
    local timestamp=$(date '+%Y-%m-%d %H:%M:%S')
    echo "[$timestamp] [$level] $message" | tee -a "$LOG_FILE"
}

# Error handling
error_exit() {
    log "ERROR" "$1"
    send_notification "Backup FAILED: $1" "danger"
    exit 1
}

# Send notifications
send_notification() {
    local message=$1
    local color=${2:-"good"}
    
    if [[ -n "$SLACK_WEBHOOK" ]]; then
        curl -X POST -H 'Content-type: application/json' \
            --data "{\"text\":\"Digital Twin Backup\",\"attachments\":[{\"color\":\"$color\",\"text\":\"$message\"}]}" \
            "$SLACK_WEBHOOK" >/dev/null 2>&1 || true
    fi
}

# Create backup directories
setup_directories() {
    local dirs=("postgresql" "application" "filesystem" "logs")
    
    for dir in "${dirs[@]}"; do
        mkdir -p "$BACKUP_ROOT/$dir"
    done
}

# Database backup
backup_postgresql() {
    log "INFO" "Starting PostgreSQL backup"
    
    local date_stamp=$(date +%Y%m%d_%H%M%S)
    local backup_file="$BACKUP_ROOT/postgresql/full_backup_$date_stamp.dump"
    local wal_dir="$BACKUP_ROOT/postgresql/wal_$date_stamp"
    
    # Create backup
    pg_dump -h "$DB_HOST" -U "$DB_USER" -d "$DB_NAME" \
        --format=custom \
        --compress=9 \
        --verbose \
        --file="$backup_file" || error_exit "PostgreSQL dump failed"
    
    # Backup WAL logs
    mkdir -p "$wal_dir"
    pg_basebackup -h "$DB_HOST" -U "$DB_USER" \
        --wal-method=stream \
        --format=tar \
        --gzip \
        --progress \
        --pgdata="$wal_dir" || error_exit "WAL backup failed"
    
    # Verify backup integrity
    pg_restore --list "$backup_file" >/dev/null || error_exit "Backup verification failed"
    
    log "INFO" "PostgreSQL backup completed: $backup_file"
}

# Application backup
backup_application() {
    log "INFO" "Starting application backup"
    
    local date_stamp=$(date +%Y%m%d_%H%M%S)
    local backup_dir="$BACKUP_ROOT/application/$date_stamp"
    
    mkdir -p "$backup_dir"
    
    # Backup Docker volumes
    docker volume ls -q | while read volume; do
        docker run --rm \
            -v "$volume:/volume" \
            -v "$backup_dir:/backup" \
            alpine tar czf "/backup/${volume}.tar.gz" -C /volume . || error_exit "Volume backup failed: $volume"
    done
    
    # Backup container configurations
    docker inspect $(docker ps -aq) > "$backup_dir/container_configs.json" || error_exit "Container config backup failed"
    
    # Backup application code
    if [[ -d "/app/src" ]]; then
        tar czf "$backup_dir/application_code.tar.gz" -C /app src/ || error_exit "Application code backup failed"
    fi
    
    log "INFO" "Application backup completed: $backup_dir"
}

# Filesystem backup
backup_filesystem() {
    log "INFO" "Starting filesystem backup"
    
    local date_stamp=$(date +%Y%m%d_%H%M%S)
    local backup_dir="$BACKUP_ROOT/filesystem/$date_stamp"
    
    mkdir -p "$backup_dir"
    
    # Directories to backup
    local backup_paths=(
        "/app/config"
        "/app/logs"
        "/etc/nginx"
        "/etc/ssl"
        "/home/appuser"
    )
    
    # Create rsync backup
    for path in "${backup_paths[@]}"; do
        if [[ -d "$path" ]]; then
            local dest_path="$backup_dir$(dirname "$path")"
            mkdir -p "$dest_path"
            rsync -avz --delete \
                --exclude='*.log' \
                --exclude='tmp/' \
                --exclude='cache/' \
                "$path/" "$dest_path/$(basename "$path")/" || error_exit "Filesystem backup failed: $path"
        fi
    done
    
    log "INFO" "Filesystem backup completed: $backup_dir"
}

# Encrypt backup files
encrypt_backups() {
    if [[ -n "$ENCRYPTION_KEY" ]]; then
        log "INFO" "Encrypting backup files"
        
        find "$BACKUP_ROOT" -name "*.dump" -o -name "*.tar.gz" -o -name "*.json" | while read file; do
            openssl enc -aes-256-cbc -salt -in "$file" -out "${file}.enc" -k "$ENCRYPTION_KEY" || error_exit "Encryption failed: $file"
            rm "$file"
        done
        
        log "INFO" "Backup encryption completed"
    fi
}

# Upload to cloud storage
upload_to_cloud() {
    log "INFO" "Uploading backups to cloud storage"
    
    # Sync to S3
    aws s3 sync "$BACKUP_ROOT" "s3://$AWS_BUCKET" \
        --region "$AWS_REGION" \
        --storage-class STANDARD_IA \
        --delete || error_exit "Cloud upload failed"
    
    log "INFO" "Cloud upload completed"
}

# Cleanup old backups
cleanup_old_backups() {
    log "INFO" "Cleaning up old backups"
    
    # Local cleanup
    find "$BACKUP_ROOT" -name "*.dump" -mtime +$RETENTION_DAYS -delete
    find "$BACKUP_ROOT" -name "*.tar.gz" -mtime +$RETENTION_DAYS -delete
    find "$BACKUP_ROOT" -name "*.json" -mtime +$RETENTION_DAYS -delete
    find "$BACKUP_ROOT" -type d -empty -delete
    
    # Cloud cleanup (using lifecycle policy)
    log "INFO" "Old backup cleanup completed"
}

# Health check
health_check() {
    log "INFO" "Running backup health check"
    
    # Check backup directory permissions
    if [[ ! -w "$BACKUP_ROOT" ]]; then
        error_exit "Backup directory not writable: $BACKUP_ROOT"
    fi
    
    # Check available disk space
    local available_space=$(df "$BACKUP_ROOT" | awk 'NR==2 {print $4}')
    local min_space=$((10 * 1024 * 1024)) # 10GB minimum
    
    if [[ $available_space -lt $min_space ]]; then
        error_exit "Insufficient disk space for backups"
    fi
    
    # Check database connectivity
    pg_isready -h "$DB_HOST" -U "$DB_USER" >/dev/null || error_exit "Database not accessible"
    
    log "INFO" "Health check passed"
}

# Main backup routine
main() {
    local start_time=$(date +%s)
    log "INFO" "=== Starting Digital Twin Platform Backup ==="
    
    # Run health check first
    health_check
    
    # Setup directories
    setup_directories
    
    # Execute backups
    backup_postgresql
    backup_application
    backup_filesystem
    
    # Post-processing
    encrypt_backups
    upload_to_cloud
    cleanup_old_backups
    
    local end_time=$(date +%s)
    local duration=$((end_time - start_time))
    
    log "INFO" "=== Backup completed successfully in ${duration} seconds ==="
    send_notification "✅ Backup completed successfully in ${duration} seconds" "good"
}

# Trap for cleanup on exit
trap 'log "ERROR" "Backup script interrupted"' INT TERM EXIT

# Run main function
main "$@"