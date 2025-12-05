namespace Harvestry.Shared.Kernel.Errors;

/// <summary>
/// Centralized error code taxonomy for Harvestry platform.
/// 
/// Format: {DOMAIN}-{NUMBER}
/// - AUTH: Authentication and authorization errors
/// - TEL: Telemetry service errors
/// - SPA: Spatial service errors
/// - GEN: Genetics service errors
/// - TSK: Task and workflow errors
/// - INV: Inventory service errors
/// - INT: Integration errors (Slack, METRC, QBO, etc.)
/// - VAL: Validation errors
/// - SYS: System and infrastructure errors
/// </summary>
public static class ErrorCodes
{
    // =========================================================================
    // Authentication & Authorization (AUTH-xxx)
    // =========================================================================
    
    /// <summary>Badge not found in the system.</summary>
    public const string AuthBadgeNotFound = "AUTH-001";
    
    /// <summary>Badge has been revoked and cannot be used.</summary>
    public const string AuthBadgeRevoked = "AUTH-002";
    
    /// <summary>User session has expired.</summary>
    public const string AuthSessionExpired = "AUTH-003";
    
    /// <summary>Session token is invalid or malformed.</summary>
    public const string AuthSessionInvalid = "AUTH-004";
    
    /// <summary>Badge has expired and needs renewal.</summary>
    public const string AuthBadgeExpired = "AUTH-005";
    
    /// <summary>User account is locked due to failed login attempts.</summary>
    public const string AuthAccountLocked = "AUTH-006";
    
    /// <summary>User account is suspended or terminated.</summary>
    public const string AuthAccountSuspended = "AUTH-007";
    
    /// <summary>User does not have required permission for the action.</summary>
    public const string AuthPermissionDenied = "AUTH-008";
    
    /// <summary>Two-person approval required for this action.</summary>
    public const string AuthTwoPersonRequired = "AUTH-009";
    
    /// <summary>ABAC policy evaluation denied the request.</summary>
    public const string AuthPolicyDenied = "AUTH-010";
    
    /// <summary>User is not assigned to the requested site.</summary>
    public const string AuthSiteAccessDenied = "AUTH-011";
    
    // =========================================================================
    // Telemetry (TEL-xxx)
    // =========================================================================
    
    /// <summary>Ingest payload validation failed.</summary>
    public const string TelIngestValidation = "TEL-001";
    
    /// <summary>Sensor stream not found.</summary>
    public const string TelStreamNotFound = "TEL-002";
    
    /// <summary>Duplicate message detected (idempotency).</summary>
    public const string TelDuplicateMessage = "TEL-003";
    
    /// <summary>Unit normalization failed for the sensor value.</summary>
    public const string TelNormalizationFailed = "TEL-004";
    
    /// <summary>Sensor reading quality code indicates bad data.</summary>
    public const string TelBadQualityCode = "TEL-005";
    
    /// <summary>Alert rule not found.</summary>
    public const string TelAlertRuleNotFound = "TEL-006";
    
    /// <summary>Alert instance not found.</summary>
    public const string TelAlertNotFound = "TEL-007";
    
    /// <summary>Ingestion session not found or expired.</summary>
    public const string TelSessionNotFound = "TEL-008";
    
    /// <summary>Telemetry query validation failed.</summary>
    public const string TelQueryValidation = "TEL-009";
    
    /// <summary>Rollup data not available for the requested time range.</summary>
    public const string TelRollupNotAvailable = "TEL-010";
    
    // =========================================================================
    // Spatial (SPA-xxx)
    // =========================================================================
    
    /// <summary>Room not found.</summary>
    public const string SpaRoomNotFound = "SPA-001";
    
    /// <summary>Location not found in hierarchy.</summary>
    public const string SpaLocationNotFound = "SPA-002";
    
    /// <summary>Equipment not found.</summary>
    public const string SpaEquipmentNotFound = "SPA-003";
    
    /// <summary>Calibration record not found.</summary>
    public const string SpaCalibrationNotFound = "SPA-004";
    
    /// <summary>Valve-zone mapping conflict detected.</summary>
    public const string SpaValveMappingConflict = "SPA-005";
    
    /// <summary>Location hierarchy depth exceeded.</summary>
    public const string SpaHierarchyDepthExceeded = "SPA-006";
    
    /// <summary>Equipment is offline and cannot be accessed.</summary>
    public const string SpaEquipmentOffline = "SPA-007";
    
    /// <summary>Calibration is overdue for the equipment.</summary>
    public const string SpaCalibrationOverdue = "SPA-008";
    
    // =========================================================================
    // Genetics (GEN-xxx)
    // =========================================================================
    
    /// <summary>Genetics record not found.</summary>
    public const string GenGeneticsNotFound = "GEN-001";
    
    /// <summary>Strain not found.</summary>
    public const string GenStrainNotFound = "GEN-002";
    
    /// <summary>Batch not found.</summary>
    public const string GenBatchNotFound = "GEN-003";
    
    /// <summary>Invalid batch stage transition.</summary>
    public const string GenInvalidStageTransition = "GEN-004";
    
    /// <summary>Mother plant not found.</summary>
    public const string GenMotherPlantNotFound = "GEN-005";
    
    /// <summary>Propagation limit exceeded.</summary>
    public const string GenPropagationLimitExceeded = "GEN-006";
    
    /// <summary>Batch code already exists.</summary>
    public const string GenBatchCodeExists = "GEN-007";
    
    /// <summary>Phenotype not found.</summary>
    public const string GenPhenotypeNotFound = "GEN-008";
    
    // =========================================================================
    // Tasks & Workflow (TSK-xxx)
    // =========================================================================
    
    /// <summary>Task is blocked by gating requirements (SOP/training).</summary>
    public const string TskGatingBlocked = "TSK-001";
    
    /// <summary>Task not found.</summary>
    public const string TskTaskNotFound = "TSK-002";
    
    /// <summary>Invalid task state transition.</summary>
    public const string TskInvalidTransition = "TSK-003";
    
    /// <summary>Task dependencies not satisfied.</summary>
    public const string TskDependenciesNotMet = "TSK-004";
    
    /// <summary>Conversation not found.</summary>
    public const string TskConversationNotFound = "TSK-005";
    
    /// <summary>Task is already completed.</summary>
    public const string TskAlreadyCompleted = "TSK-006";
    
    /// <summary>Task is cancelled and cannot be modified.</summary>
    public const string TskCancelled = "TSK-007";
    
    // =========================================================================
    // Inventory (INV-xxx)
    // =========================================================================
    
    /// <summary>Inventory lot not found.</summary>
    public const string InvLotNotFound = "INV-001";
    
    /// <summary>Insufficient inventory balance.</summary>
    public const string InvInsufficientBalance = "INV-002";
    
    /// <summary>Invalid unit of measure conversion.</summary>
    public const string InvUomConversionFailed = "INV-003";
    
    /// <summary>Lot is on hold and cannot be moved.</summary>
    public const string InvLotOnHold = "INV-004";
    
    /// <summary>Barcode not found or invalid.</summary>
    public const string InvBarcodeNotFound = "INV-005";
    
    /// <summary>Split quantities do not match source lot.</summary>
    public const string InvSplitMismatch = "INV-006";
    
    // =========================================================================
    // Integrations (INT-xxx)
    // =========================================================================
    
    /// <summary>Slack message delivery failed.</summary>
    public const string IntSlackDeliveryFailed = "INT-001";
    
    /// <summary>Slack API rate limit exceeded.</summary>
    public const string IntSlackRateLimited = "INT-002";
    
    /// <summary>METRC sync failed.</summary>
    public const string IntMetrcSyncFailed = "INT-003";
    
    /// <summary>METRC API authentication failed.</summary>
    public const string IntMetrcAuthFailed = "INT-004";
    
    /// <summary>QuickBooks sync failed.</summary>
    public const string IntQboSyncFailed = "INT-005";
    
    /// <summary>QuickBooks OAuth token expired.</summary>
    public const string IntQboTokenExpired = "INT-006";
    
    /// <summary>Integration circuit breaker is open.</summary>
    public const string IntCircuitBreakerOpen = "INT-007";
    
    /// <summary>External API timeout.</summary>
    public const string IntApiTimeout = "INT-008";
    
    // =========================================================================
    // Validation (VAL-xxx)
    // =========================================================================
    
    /// <summary>Request body validation failed.</summary>
    public const string ValRequestInvalid = "VAL-001";
    
    /// <summary>Required field is missing.</summary>
    public const string ValFieldRequired = "VAL-002";
    
    /// <summary>Field value is out of allowed range.</summary>
    public const string ValFieldOutOfRange = "VAL-003";
    
    /// <summary>Field format is invalid.</summary>
    public const string ValFieldFormatInvalid = "VAL-004";
    
    /// <summary>Duplicate value detected.</summary>
    public const string ValDuplicateValue = "VAL-005";
    
    // =========================================================================
    // System (SYS-xxx)
    // =========================================================================
    
    /// <summary>Database connection failed.</summary>
    public const string SysDatabaseError = "SYS-001";
    
    /// <summary>Database query timeout.</summary>
    public const string SysQueryTimeout = "SYS-002";
    
    /// <summary>Service unavailable.</summary>
    public const string SysServiceUnavailable = "SYS-003";
    
    /// <summary>Internal server error.</summary>
    public const string SysInternalError = "SYS-004";
    
    /// <summary>Rate limit exceeded.</summary>
    public const string SysRateLimitExceeded = "SYS-005";
    
    /// <summary>Configuration error.</summary>
    public const string SysConfigurationError = "SYS-006";
}

