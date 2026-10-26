export const EquipmentMovementTypeStatus = {
    wz: 1,
    pz: 2,
    transfer: 3
} as const;

export type EquipmentMovementTypeStatus = typeof EquipmentMovementTypeStatus[keyof typeof EquipmentMovementTypeStatus]