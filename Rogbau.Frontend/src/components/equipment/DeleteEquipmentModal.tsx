import { DeleteConfirmModal } from '../ui/DeleteConfirmModal';

interface DeleteEquipmentModalProps {
  isOpen: boolean;
  equipmentName: string;
  isDeleting: boolean;
  onClose: () => void;
  onConfirm: () => void;
}

export function DeleteEquipmentModal(props: DeleteEquipmentModalProps) {
  return (
    <DeleteConfirmModal
      isOpen={props.isOpen}
      title="Usuń sprzęt trwale"
      message={`Czy na pewno chcesz trwale usunąć sprzęt "${props.equipmentName}"? Tej operacji nie można cofnąć. Sprzętu powiązanego z dokumentami nie można trwale usunąć.`}
      isDeleting={props.isDeleting}
      onClose={props.onClose}
      onConfirm={props.onConfirm}
    />
  );
}
