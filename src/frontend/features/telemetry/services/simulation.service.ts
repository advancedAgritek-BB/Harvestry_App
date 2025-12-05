// src/frontend/features/telemetry/services/simulation.service.ts

// ============================================================================
// STREAM TYPE ENUM (matches backend Harvestry.Telemetry.Domain.Enums.StreamType)
// ============================================================================

export enum StreamType {
  // Climate sensors
  Temperature = 1,
  Humidity = 2,
  Co2 = 3,
  Vpd = 4,
  LightPar = 5,
  LightPpfd = 6,
  
  // Water quality sensors
  Ec = 10,
  Ph = 11,
  DissolvedOxygen = 12,
  WaterTemp = 13,
  WaterLevel = 14,
  
  // Substrate sensors
  SoilMoisture = 20,
  SoilTemp = 21,
  SoilEc = 22,
  
  // Flow and pressure
  Pressure = 30,
  FlowRate = 31,
  FlowTotal = 32,
  
  // Power and equipment health
  PowerConsumption = 40,
  EnergyConsumption = 41,
  EquipmentStatus = 42,
  
  // Environmental
  Airflow = 50,
  WindSpeed = 51,
  
  // Custom/Other
  Custom = 99
}

// Human-readable labels for StreamType
export const StreamTypeLabels: Record<StreamType, string> = {
  [StreamType.Temperature]: 'Temperature',
  [StreamType.Humidity]: 'Humidity',
  [StreamType.Co2]: 'CO₂',
  [StreamType.Vpd]: 'VPD',
  [StreamType.LightPar]: 'Light PAR',
  [StreamType.LightPpfd]: 'Light PPFD',
  [StreamType.Ec]: 'EC',
  [StreamType.Ph]: 'pH',
  [StreamType.DissolvedOxygen]: 'Dissolved O₂',
  [StreamType.WaterTemp]: 'Water Temp',
  [StreamType.WaterLevel]: 'Water Level',
  [StreamType.SoilMoisture]: 'Soil Moisture',
  [StreamType.SoilTemp]: 'Soil Temp',
  [StreamType.SoilEc]: 'Soil EC',
  [StreamType.Pressure]: 'Pressure',
  [StreamType.FlowRate]: 'Flow Rate',
  [StreamType.FlowTotal]: 'Flow Total',
  [StreamType.PowerConsumption]: 'Power',
  [StreamType.EnergyConsumption]: 'Energy',
  [StreamType.EquipmentStatus]: 'Equipment Status',
  [StreamType.Airflow]: 'Airflow',
  [StreamType.WindSpeed]: 'Wind Speed',
  [StreamType.Custom]: 'Custom'
};

// Group StreamTypes by category
export const StreamTypeGroups = {
  Climate: [
    StreamType.Temperature,
    StreamType.Humidity,
    StreamType.Co2,
    StreamType.Vpd,
    StreamType.LightPar,
    StreamType.LightPpfd
  ],
  Water: [
    StreamType.Ec,
    StreamType.Ph,
    StreamType.DissolvedOxygen,
    StreamType.WaterTemp,
    StreamType.WaterLevel
  ],
  Substrate: [
    StreamType.SoilMoisture,
    StreamType.SoilTemp,
    StreamType.SoilEc
  ],
  FlowPressure: [
    StreamType.Pressure,
    StreamType.FlowRate,
    StreamType.FlowTotal
  ],
  Power: [
    StreamType.PowerConsumption,
    StreamType.EnergyConsumption,
    StreamType.EquipmentStatus
  ],
  Environmental: [
    StreamType.Airflow,
    StreamType.WindSpeed
  ],
  Other: [
    StreamType.Custom
  ]
};

// ============================================================================
// UNIT ENUM (matches backend Harvestry.Telemetry.Domain.Enums.Unit)
// ============================================================================

export enum Unit {
  // Temperature
  DegreesFahrenheit = 1,
  DegreesCelsius = 2,
  Kelvin = 3,
  
  // Percentage
  Percent = 10,
  
  // Concentration
  PartsPerMillion = 20,
  PartsPerBillion = 21,
  MilligramsPerLiter = 22,
  
  // Pressure
  Kilopascals = 30,
  Psi = 31,
  Bar = 32,
  Pascals = 33,
  
  // Light
  Micromoles = 40,
  Lux = 41,
  Footcandles = 42,
  
  // Electrical Conductivity
  Microsiemens = 50,
  MillisiemensPerCm = 51,
  
  // pH (dimensionless)
  Ph = 60,
  
  // Volume
  Liters = 70,
  Gallons = 71,
  Milliliters = 72,
  CubicMeters = 73,
  CubicFeet = 74,
  
  // Flow Rate
  LitersPerMinute = 80,
  GallonsPerMinute = 81,
  LitersPerHour = 82,
  GallonsPerHour = 83,
  CubicMetersPerHour = 84,
  
  // Distance/Length
  Inches = 90,
  Centimeters = 91,
  Meters = 92,
  Feet = 93,
  
  // Power
  Watts = 100,
  Kilowatts = 101,
  Horsepower = 102,
  
  // Energy
  KilowattHours = 110,
  WattHours = 111,
  Joules = 112,
  
  // Speed
  MetersPerSecond = 120,
  FeetPerSecond = 121,
  MilesPerHour = 122,
  KilometersPerHour = 123,
  
  // Dimensionless/Status
  Boolean = 200,
  Count = 201,
  Ratio = 202
}

// Human-readable labels for Unit
export const UnitLabels: Record<Unit, string> = {
  [Unit.DegreesFahrenheit]: '°F',
  [Unit.DegreesCelsius]: '°C',
  [Unit.Kelvin]: 'K',
  [Unit.Percent]: '%',
  [Unit.PartsPerMillion]: 'ppm',
  [Unit.PartsPerBillion]: 'ppb',
  [Unit.MilligramsPerLiter]: 'mg/L',
  [Unit.Kilopascals]: 'kPa',
  [Unit.Psi]: 'PSI',
  [Unit.Bar]: 'bar',
  [Unit.Pascals]: 'Pa',
  [Unit.Micromoles]: 'µmol/m²/s',
  [Unit.Lux]: 'lux',
  [Unit.Footcandles]: 'fc',
  [Unit.Microsiemens]: 'µS/cm',
  [Unit.MillisiemensPerCm]: 'mS/cm',
  [Unit.Ph]: 'pH',
  [Unit.Liters]: 'L',
  [Unit.Gallons]: 'gal',
  [Unit.Milliliters]: 'mL',
  [Unit.CubicMeters]: 'm³',
  [Unit.CubicFeet]: 'ft³',
  [Unit.LitersPerMinute]: 'L/min',
  [Unit.GallonsPerMinute]: 'GPM',
  [Unit.LitersPerHour]: 'L/h',
  [Unit.GallonsPerHour]: 'GPH',
  [Unit.CubicMetersPerHour]: 'm³/h',
  [Unit.Inches]: 'in',
  [Unit.Centimeters]: 'cm',
  [Unit.Meters]: 'm',
  [Unit.Feet]: 'ft',
  [Unit.Watts]: 'W',
  [Unit.Kilowatts]: 'kW',
  [Unit.Horsepower]: 'hp',
  [Unit.KilowattHours]: 'kWh',
  [Unit.WattHours]: 'Wh',
  [Unit.Joules]: 'J',
  [Unit.MetersPerSecond]: 'm/s',
  [Unit.FeetPerSecond]: 'ft/s',
  [Unit.MilesPerHour]: 'mph',
  [Unit.KilometersPerHour]: 'km/h',
  [Unit.Boolean]: 'on/off',
  [Unit.Count]: 'count',
  [Unit.Ratio]: 'ratio'
};

// Map StreamType to appropriate Units
export const StreamTypeUnits: Record<StreamType, Unit[]> = {
  [StreamType.Temperature]: [Unit.DegreesFahrenheit, Unit.DegreesCelsius, Unit.Kelvin],
  [StreamType.Humidity]: [Unit.Percent],
  [StreamType.Co2]: [Unit.PartsPerMillion],
  [StreamType.Vpd]: [Unit.Kilopascals],
  [StreamType.LightPar]: [Unit.Micromoles],
  [StreamType.LightPpfd]: [Unit.Micromoles],
  [StreamType.Ec]: [Unit.MillisiemensPerCm, Unit.Microsiemens],
  [StreamType.Ph]: [Unit.Ph],
  [StreamType.DissolvedOxygen]: [Unit.MilligramsPerLiter, Unit.Percent],
  [StreamType.WaterTemp]: [Unit.DegreesFahrenheit, Unit.DegreesCelsius],
  [StreamType.WaterLevel]: [Unit.Inches, Unit.Centimeters, Unit.Percent],
  [StreamType.SoilMoisture]: [Unit.Percent],
  [StreamType.SoilTemp]: [Unit.DegreesFahrenheit, Unit.DegreesCelsius],
  [StreamType.SoilEc]: [Unit.MillisiemensPerCm, Unit.Microsiemens],
  [StreamType.Pressure]: [Unit.Psi, Unit.Kilopascals, Unit.Bar],
  [StreamType.FlowRate]: [Unit.GallonsPerMinute, Unit.LitersPerMinute, Unit.GallonsPerHour],
  [StreamType.FlowTotal]: [Unit.Gallons, Unit.Liters, Unit.CubicMeters],
  [StreamType.PowerConsumption]: [Unit.Watts, Unit.Kilowatts],
  [StreamType.EnergyConsumption]: [Unit.KilowattHours, Unit.WattHours],
  [StreamType.EquipmentStatus]: [Unit.Boolean, Unit.Count],
  [StreamType.Airflow]: [Unit.CubicMetersPerHour, Unit.CubicFeet],
  [StreamType.WindSpeed]: [Unit.MetersPerSecond, Unit.MilesPerHour],
  [StreamType.Custom]: [Unit.Count, Unit.Ratio, Unit.Boolean]
};

// Get default unit for a stream type
export const getDefaultUnit = (streamType: StreamType): Unit => {
  const units = StreamTypeUnits[streamType];
  return units?.[0] ?? Unit.Count;
};

// ============================================================================
// SIMULATION BEHAVIOR ENUM
// ============================================================================

export enum SimulationBehavior {
  SineWave24H = 0,
  InverseSineWave24H = 1,
  RandomWalk = 2,
  StaticWithNoise = 3,
  Sawtooth = 4
}

export const SimulationBehaviorLabels: Record<SimulationBehavior, string> = {
  [SimulationBehavior.SineWave24H]: 'Sine Wave (24h)',
  [SimulationBehavior.InverseSineWave24H]: 'Inverse Sine (24h)',
  [SimulationBehavior.RandomWalk]: 'Random Walk',
  [SimulationBehavior.StaticWithNoise]: 'Static + Noise',
  [SimulationBehavior.Sawtooth]: 'Sawtooth'
};

// ============================================================================
// INTERFACES
// ============================================================================

export interface SimulationProfile {
  behavior: SimulationBehavior;
  min: number;
  max: number;
  noise: number;
  volatility?: number;
}

export interface SimulationState {
  streamId: string;
  stream: {
    id: string;
    displayName: string;
    streamType: StreamType;
    unit: number;
    siteId: string;
    equipmentId: string;
  };
  profile: SimulationProfile;
  lastValue: number;
}

// ============================================================================
// SIMULATION SERVICE
// ============================================================================

const BASE_URL = '/api/v1/simulation';

export const simulationService = {
  async start(type: StreamType): Promise<string> {
    const res = await fetch(`${BASE_URL}/start?type=${type}`, { method: 'POST' });
    if (!res.ok) throw new Error('Failed to start simulation');
    return res.text();
  },

  async stop(type: StreamType): Promise<string> {
    const res = await fetch(`${BASE_URL}/stop?type=${type}`, { method: 'POST' });
    if (!res.ok) throw new Error('Failed to stop simulation');
    return res.text();
  },

  async toggle(streamId: string): Promise<string> {
    const res = await fetch(`${BASE_URL}/toggle/${streamId}`, { method: 'POST' });
    if (!res.ok) throw new Error('Failed to toggle simulation');
    return res.text();
  },

  async updateProfile(type: StreamType, profile: SimulationProfile): Promise<string> {
    const res = await fetch(`${BASE_URL}/config?type=${type}`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(profile)
    });
    if (!res.ok) throw new Error('Failed to update profile');
    return res.text();
  },

  async getActive(): Promise<SimulationState[]> {
    const res = await fetch(`${BASE_URL}/active`);
    if (!res.ok) throw new Error('Failed to fetch active simulations');
    return res.json();
  }
};
