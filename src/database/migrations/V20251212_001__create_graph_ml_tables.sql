-- Graph ML Schema for Harvestry Knowledge Graph
-- Creates tables for storing graph nodes and edges with support for
-- feature vectors, anomaly scores, and efficient graph traversal

-- Create ML schema if not exists
CREATE SCHEMA IF NOT EXISTS ml;

-- Graph Nodes table
-- Stores all entity nodes from various domains (packages, tasks, telemetry, genetics)
CREATE TABLE IF NOT EXISTS ml.graph_nodes (
    node_id VARCHAR(100) PRIMARY KEY,
    site_id UUID NOT NULL,
    node_type VARCHAR(50) NOT NULL,
    source_entity_id UUID NOT NULL,
    label VARCHAR(500) NOT NULL,
    feature_vector REAL[],
    properties JSONB,
    source_created_at TIMESTAMP NOT NULL,
    source_updated_at TIMESTAMP NOT NULL,
    snapshot_at TIMESTAMP NOT NULL DEFAULT NOW(),
    version BIGINT NOT NULL DEFAULT 1,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    anomaly_score REAL CHECK (anomaly_score >= 0 AND anomaly_score <= 1),
    anomaly_explanation JSONB
);

-- Graph Edges table
-- Stores relationships between nodes with weights and properties
CREATE TABLE IF NOT EXISTS ml.graph_edges (
    edge_id VARCHAR(300) PRIMARY KEY,
    site_id UUID NOT NULL,
    edge_type VARCHAR(50) NOT NULL,
    source_node_id VARCHAR(100) NOT NULL,
    target_node_id VARCHAR(100) NOT NULL,
    weight REAL NOT NULL DEFAULT 1.0,
    properties JSONB,
    relationship_created_at TIMESTAMP NOT NULL,
    snapshot_at TIMESTAMP NOT NULL DEFAULT NOW(),
    version BIGINT NOT NULL DEFAULT 1,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    anomaly_score REAL CHECK (anomaly_score >= 0 AND anomaly_score <= 1)
);

-- Indexes for graph_nodes
CREATE INDEX IF NOT EXISTS ix_graph_nodes_site_id 
    ON ml.graph_nodes(site_id);

CREATE INDEX IF NOT EXISTS ix_graph_nodes_site_type 
    ON ml.graph_nodes(site_id, node_type);

CREATE INDEX IF NOT EXISTS ix_graph_nodes_site_type_active 
    ON ml.graph_nodes(site_id, node_type, is_active);

CREATE INDEX IF NOT EXISTS ix_graph_nodes_type_source 
    ON ml.graph_nodes(node_type, source_entity_id);

CREATE INDEX IF NOT EXISTS ix_graph_nodes_anomaly 
    ON ml.graph_nodes(site_id, anomaly_score) 
    WHERE anomaly_score IS NOT NULL;

CREATE INDEX IF NOT EXISTS ix_graph_nodes_snapshot 
    ON ml.graph_nodes(snapshot_at);

-- GIN index for JSONB properties queries
CREATE INDEX IF NOT EXISTS ix_graph_nodes_properties 
    ON ml.graph_nodes USING GIN (properties);

-- Indexes for graph_edges
CREATE INDEX IF NOT EXISTS ix_graph_edges_site_id 
    ON ml.graph_edges(site_id);

CREATE INDEX IF NOT EXISTS ix_graph_edges_site_type 
    ON ml.graph_edges(site_id, edge_type);

CREATE INDEX IF NOT EXISTS ix_graph_edges_source_type 
    ON ml.graph_edges(source_node_id, edge_type);

CREATE INDEX IF NOT EXISTS ix_graph_edges_target_type 
    ON ml.graph_edges(target_node_id, edge_type);

CREATE INDEX IF NOT EXISTS ix_graph_edges_site_type_active 
    ON ml.graph_edges(site_id, edge_type, is_active);

CREATE INDEX IF NOT EXISTS ix_graph_edges_snapshot 
    ON ml.graph_edges(snapshot_at);

-- Unique constraint for edge uniqueness
CREATE UNIQUE INDEX IF NOT EXISTS ux_graph_edges_unique 
    ON ml.graph_edges(source_node_id, target_node_id, edge_type);

-- GIN index for JSONB properties
CREATE INDEX IF NOT EXISTS ix_graph_edges_properties 
    ON ml.graph_edges USING GIN (properties);

-- Graph snapshot jobs table
-- Tracks scheduled and completed snapshot operations
CREATE TABLE IF NOT EXISTS ml.graph_snapshot_jobs (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    site_id UUID NOT NULL,
    job_type VARCHAR(50) NOT NULL, -- 'full', 'partial', 'incremental'
    status VARCHAR(30) NOT NULL DEFAULT 'pending', -- 'pending', 'running', 'completed', 'failed'
    node_types VARCHAR(100)[], -- For partial snapshots
    started_at TIMESTAMP,
    completed_at TIMESTAMP,
    nodes_created INT DEFAULT 0,
    nodes_updated INT DEFAULT 0,
    nodes_deactivated INT DEFAULT 0,
    edges_created INT DEFAULT 0,
    edges_updated INT DEFAULT 0,
    edges_deactivated INT DEFAULT 0,
    error_message TEXT,
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS ix_graph_snapshot_jobs_site_status 
    ON ml.graph_snapshot_jobs(site_id, status);

CREATE INDEX IF NOT EXISTS ix_graph_snapshot_jobs_created 
    ON ml.graph_snapshot_jobs(created_at DESC);

-- Node embeddings table (for storing pre-computed ML embeddings)
CREATE TABLE IF NOT EXISTS ml.node_embeddings (
    node_id VARCHAR(100) PRIMARY KEY REFERENCES ml.graph_nodes(node_id) ON DELETE CASCADE,
    site_id UUID NOT NULL,
    embedding_type VARCHAR(50) NOT NULL, -- 'node2vec', 'graphsage', 'gat'
    embedding REAL[] NOT NULL,
    embedding_dim INT NOT NULL,
    model_version VARCHAR(50) NOT NULL,
    computed_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS ix_node_embeddings_site 
    ON ml.node_embeddings(site_id);

CREATE INDEX IF NOT EXISTS ix_node_embeddings_type 
    ON ml.node_embeddings(embedding_type);

-- Anomaly detection results table
CREATE TABLE IF NOT EXISTS ml.anomaly_results (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    site_id UUID NOT NULL,
    node_id VARCHAR(100) REFERENCES ml.graph_nodes(node_id) ON DELETE CASCADE,
    edge_id VARCHAR(300) REFERENCES ml.graph_edges(edge_id) ON DELETE CASCADE,
    anomaly_type VARCHAR(50) NOT NULL, -- 'movement', 'irrigation_response', 'task_pattern'
    score REAL NOT NULL CHECK (score >= 0 AND score <= 1),
    explanation JSONB,
    feature_attributions JSONB, -- SHAP/LIME feature importance
    model_version VARCHAR(50) NOT NULL,
    detected_at TIMESTAMP NOT NULL DEFAULT NOW(),
    acknowledged_at TIMESTAMP,
    acknowledged_by UUID,
    resolution_notes TEXT,
    CONSTRAINT chk_anomaly_target CHECK (node_id IS NOT NULL OR edge_id IS NOT NULL)
);

CREATE INDEX IF NOT EXISTS ix_anomaly_results_site 
    ON ml.anomaly_results(site_id);

CREATE INDEX IF NOT EXISTS ix_anomaly_results_score 
    ON ml.anomaly_results(site_id, score DESC);

CREATE INDEX IF NOT EXISTS ix_anomaly_results_type 
    ON ml.anomaly_results(anomaly_type);

CREATE INDEX IF NOT EXISTS ix_anomaly_results_detected 
    ON ml.anomaly_results(detected_at DESC);

CREATE INDEX IF NOT EXISTS ix_anomaly_results_unacknowledged 
    ON ml.anomaly_results(site_id, detected_at) 
    WHERE acknowledged_at IS NULL;

-- Comments for documentation
COMMENT ON TABLE ml.graph_nodes IS 'Canonical graph nodes representing entities across all Harvestry domains';
COMMENT ON TABLE ml.graph_edges IS 'Directed edges representing relationships between graph nodes';
COMMENT ON TABLE ml.node_embeddings IS 'Pre-computed node embeddings from graph ML models';
COMMENT ON TABLE ml.anomaly_results IS 'Detected anomalies from graph-based anomaly detection models';
COMMENT ON COLUMN ml.graph_nodes.feature_vector IS 'Node feature vector for ML (computed from properties)';
COMMENT ON COLUMN ml.graph_nodes.anomaly_score IS 'Anomaly score from 0 (normal) to 1 (highly anomalous)';
COMMENT ON COLUMN ml.graph_edges.weight IS 'Edge weight for weighted graph algorithms';
