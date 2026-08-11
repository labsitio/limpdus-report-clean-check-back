import { Platform } from 'react-native';

/**
 * true  = celular físico Android (usa o IP da máquina na LAN)
 * false = emulador Android (10.0.2.2 aponta para localhost do host)
 */
export const USE_ANDROID_PHYSICAL_DEVICE = false;

/** Troque pelo IPv4 do seu PC (ipconfig), mesma rede Wi‑Fi do celular. */
export const LOCAL_MACHINE_IP = '192.168.0.0';

const API_PORT = 5234;

const androidBaseUrl = USE_ANDROID_PHYSICAL_DEVICE
  ? `http://${LOCAL_MACHINE_IP}:${API_PORT}`
  : `http://10.0.2.2:${API_PORT}`;

/**
 * iOS Simulator: localhost do Mac.
 * Android: emulador vs físico conforme flags acima.
 */
export const BASE_URL =
  Platform.OS === 'android'
    ? androidBaseUrl
    : `http://localhost:${API_PORT}`;
