export const EquipmentMovementStatus = {
    draft: 1,
    inTransit: 2,
    Completed: 3,
    Cancelled: 4
} as const;

export type EquipmentMovementStatus = typeof EquipmentMovementStatus[keyof typeof EquipmentMovementStatus]