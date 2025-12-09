# Edge Security Model (v3.3)

## 1. Threat Model
We operate in "Hostile Environments" (Shared WiFi, Physical Access available, potential rogue devices).
**Core Threats:**
*   **Rogue Command Injection:** Attacker turning on valves/pumps.
*   **Device Cloning:** Copying credentials to unauthorized hardware.
*   **Man-in-the-Middle:** Intercepting telemetry or OTA updates.

---

## 2. Device Identity (mTLS)
Every Harvestry Edge device (HydroCore, RoomHub) must have a cryptographically strong identity.

### 2.1 Provisioning Process (Factory)
1.  **Key Generation:** Device generates a Key Pair (ECC P-256) inside its Secure Element (TPM / ATECC608). *Private Key never leaves the chip.*
2.  **CSR:** Device generates a Certificate Signing Request (CSR).
3.  **Signing:** Factory CA signs the CSR, returning a device-specific X.509 Certificate (`CN={serial}`).
4.  **Registration:** The Certificate Thumbprint is registered in the Harvestry Cloud Device Registry.

### 2.2 Runtime Auth
*   **Protocol:** MQTT over TLS 1.3.
*   **Mutual Auth:** Server verifies Device Cert; Device verifies Server Cert.
*   **Rejection:** If a device is reported stolen, its Cert is revoked via CRL/OCSP.

---

## 3. Secure Boot & Integrity
**Goal:** Prevent running modified/malicious firmware.

*   **Boot Chain:**
    1.  **ROM:** Verifies Bootloader signature.
    2.  **Bootloader (U-Boot):** Verifies OS Kernel signature.
    3.  **Kernel:** Verifies Root Filesystem (dm-verity).
*   **Runtime:** The Container Runtime verifies the signature of the `Harvestry.Edge` docker image before pulling/running.

---

## 4. Command Signing
To prevent a compromised cloud service from damaging the facility, critical commands (e.g., "Unlock Door", "Update Firmware") require a secondary signature.

*   **Mechanism:** Critical commands contain a JWT signed by a specific "Operations Key" which is kept offline/restricted, not just the standard TLS session key.
*   **Replay Protection:** All commands include a `nonce` and `timestamp`. Commands >5 seconds old are rejected.

---

## 5. Network Segmentation
For Enterprise deployments, we enforce strict VLAN separation.

### 5.1 OT VLAN (Operational Technology)
*   **Members:** HydroCore, EdgePods, HE Anderson Skids.
*   **Access:** NO Internet Access (except HydroCore via specific outbound port).
*   **Isolation:** Isolated from the "Guest WiFi" and "Office LAN".

### 5.2 Encryption on the Wire
*   **EdgePods:** CoAP payloads are encrypted with DTLS (Pre-Shared Key derived during pairing).
*   **SkidLink:** If Modbus/TCP is unencrypted (standard), it *must* stay on the physical OT VLAN.
