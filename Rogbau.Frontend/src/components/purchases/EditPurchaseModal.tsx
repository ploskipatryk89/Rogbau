import type { PurchaseListDto } from '../../types/response/PurchaseResponse';
import { CreatePurchaseModal } from './CreatePurchaseModal';

interface EditPurchaseModalProps {
  purchase: PurchaseListDto | null;
  onClose: () => void;
  onSuccess: () => void;
}

export const EditPurchaseModal = ({ purchase, onClose, onSuccess }: EditPurchaseModalProps) => (
  <CreatePurchaseModal isOpen={purchase !== null} purchase={purchase} onClose={onClose} onSuccess={onSuccess} />
);
