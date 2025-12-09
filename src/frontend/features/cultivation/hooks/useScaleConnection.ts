/**
 * useScaleConnection Hook
 * Manages WebSocket/SignalR connection to scale device service
 */

import { useState, useCallback, useEffect, useRef } from 'react';
import type { ScaleDevice, ScaleReading } from '@/features/inventory/types';

interface UseScaleConnectionOptions {
  /** Scale device to connect to */
  scaleDevice?: ScaleDevice | null;
  /** Base URL for the scale service */
  baseUrl?: string;
  /** Callback when weight reading is received */
  onWeightReading?: (reading: ScaleReading) => void;
  /** Callback when connection status changes */
  onConnectionChange?: (connected: boolean) => void;
  /** Auto-reconnect on disconnect */
  autoReconnect?: boolean;
  /** Reconnect interval in ms */
  reconnectIntervalMs?: number;
}

interface ScaleConnectionState {
  isConnected: boolean;
  isConnecting: boolean;
  error: string | null;
  lastReading: ScaleReading | null;
  connectionType: string | null;
}

interface UseScaleConnectionResult extends ScaleConnectionState {
  connect: () => Promise<boolean>;
  disconnect: () => Promise<void>;
  tare: () => Promise<boolean>;
  zero: () => Promise<boolean>;
}

export function useScaleConnection(options: UseScaleConnectionOptions = {}): UseScaleConnectionResult {
  const {
    scaleDevice,
    baseUrl = '/api/scales',
    onWeightReading,
    onConnectionChange,
    autoReconnect = true,
    reconnectIntervalMs = 3000,
  } = options;

  const [state, setState] = useState<ScaleConnectionState>({
    isConnected: false,
    isConnecting: false,
    error: null,
    lastReading: null,
    connectionType: null,
  });

  const wsRef = useRef<WebSocket | null>(null);
  const reconnectTimeoutRef = useRef<NodeJS.Timeout | null>(null);
  const shouldReconnectRef = useRef(autoReconnect);

  // Cleanup on unmount
  useEffect(() => {
    return () => {
      shouldReconnectRef.current = false;
      if (reconnectTimeoutRef.current) {
        clearTimeout(reconnectTimeoutRef.current);
      }
      if (wsRef.current) {
        wsRef.current.close();
      }
    };
  }, []);

  // Update shouldReconnect ref when prop changes
  useEffect(() => {
    shouldReconnectRef.current = autoReconnect;
  }, [autoReconnect]);

  // Schedule reconnection
  const scheduleReconnect = useCallback(() => {
    if (!shouldReconnectRef.current || !scaleDevice) return;

    if (reconnectTimeoutRef.current) {
      clearTimeout(reconnectTimeoutRef.current);
    }

    reconnectTimeoutRef.current = setTimeout(() => {
      connect();
    }, reconnectIntervalMs);
  }, [scaleDevice, reconnectIntervalMs]);

  // Connect to scale
  const connect = useCallback(async (): Promise<boolean> => {
    if (!scaleDevice) {
      setState(prev => ({ ...prev, error: 'No scale device selected' }));
      return false;
    }

    // Close existing connection
    if (wsRef.current) {
      wsRef.current.close();
    }

    setState(prev => ({ ...prev, isConnecting: true, error: null }));

    try {
      // Build WebSocket URL
      const wsProtocol = window.location.protocol === 'https:' ? 'wss:' : 'ws:';
      const wsUrl = `${wsProtocol}//${window.location.host}${baseUrl}/${scaleDevice.id}/stream`;

      const ws = new WebSocket(wsUrl);
      wsRef.current = ws;

      return new Promise((resolve) => {
        ws.onopen = () => {
          setState(prev => ({
            ...prev,
            isConnected: true,
            isConnecting: false,
            connectionType: scaleDevice.connectionType,
            error: null,
          }));
          onConnectionChange?.(true);
          resolve(true);
        };

        ws.onmessage = (event) => {
          try {
            const data = JSON.parse(event.data);
            
            if (data.type === 'weight') {
              const reading: ScaleReading = {
                id: data.id || crypto.randomUUID(),
                scaleDeviceId: scaleDevice.id,
                grossWeight: data.grossWeight,
                tareWeight: data.tareWeight,
                netWeight: data.netWeight,
                unitOfWeight: data.unit || 'Grams',
                isStable: data.isStable,
                stabilityDurationMs: data.stabilityDurationMs,
                readingTimestamp: data.timestamp || new Date().toISOString(),
                calibrationWasValid: data.calibrationValid,
                recordedBy: 'system',
                createdAt: new Date().toISOString(),
              };

              setState(prev => ({ ...prev, lastReading: reading }));
              onWeightReading?.(reading);
            }
          } catch (e) {
            console.error('Failed to parse scale message:', e);
          }
        };

        ws.onerror = (event) => {
          console.error('WebSocket error:', event);
          setState(prev => ({
            ...prev,
            error: 'Connection error',
            isConnecting: false,
          }));
        };

        ws.onclose = (event) => {
          setState(prev => ({
            ...prev,
            isConnected: false,
            isConnecting: false,
          }));
          onConnectionChange?.(false);
          wsRef.current = null;

          // Auto-reconnect if enabled and not a normal closure
          if (event.code !== 1000 && shouldReconnectRef.current) {
            scheduleReconnect();
          }
        };

        // Handle connection timeout
        setTimeout(() => {
          if (ws.readyState === WebSocket.CONNECTING) {
            ws.close();
            setState(prev => ({
              ...prev,
              error: 'Connection timeout',
              isConnecting: false,
            }));
            resolve(false);
          }
        }, 10000);
      });
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Failed to connect';
      setState(prev => ({
        ...prev,
        error: errorMessage,
        isConnecting: false,
      }));
      return false;
    }
  }, [scaleDevice, baseUrl, onWeightReading, onConnectionChange, scheduleReconnect]);

  // Disconnect from scale
  const disconnect = useCallback(async (): Promise<void> => {
    shouldReconnectRef.current = false;
    
    if (reconnectTimeoutRef.current) {
      clearTimeout(reconnectTimeoutRef.current);
    }

    if (wsRef.current) {
      wsRef.current.close(1000, 'User disconnected');
      wsRef.current = null;
    }

    setState(prev => ({
      ...prev,
      isConnected: false,
      lastReading: null,
    }));
  }, []);

  // Send tare command
  const tare = useCallback(async (): Promise<boolean> => {
    if (!wsRef.current || wsRef.current.readyState !== WebSocket.OPEN) {
      return false;
    }

    try {
      wsRef.current.send(JSON.stringify({ type: 'command', command: 'tare' }));
      return true;
    } catch (error) {
      console.error('Failed to send tare command:', error);
      return false;
    }
  }, []);

  // Send zero command
  const zero = useCallback(async (): Promise<boolean> => {
    if (!wsRef.current || wsRef.current.readyState !== WebSocket.OPEN) {
      return false;
    }

    try {
      wsRef.current.send(JSON.stringify({ type: 'command', command: 'zero' }));
      return true;
    } catch (error) {
      console.error('Failed to send zero command:', error);
      return false;
    }
  }, []);

  // Auto-connect when scale device changes
  useEffect(() => {
    if (scaleDevice && !state.isConnected && !state.isConnecting) {
      connect();
    }
  }, [scaleDevice?.id]);

  return {
    ...state,
    connect,
    disconnect,
    tare,
    zero,
  };
}
