import type { SupplierResponseDto } from '../../types/response/SupplierResponseDto';
import { AddSupplierModal } from './AddSupplierModal';

interface EditSupplierModalProps {
  supplier: SupplierResponseDto | null;
  onClose: () => void;
  onSuccess: () => void;
}

export const EditSupplierModal = ({ supplier, onClose, onSuccess }: EditSupplierModalProps) => (
  <AddSupplierModal isOpen={supplier !== null} supplier={supplier} onClose={onClose} onSuccess={onSuccess} />
);
