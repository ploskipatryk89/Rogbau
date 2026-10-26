import { Link } from 'react-router-dom';
import type { OrderDetails } from '../../api/orderService';

interface Props {
  transfers: OrderDetails['transfers'];
}

export function OrderTransfersList({ transfers }: Props) {
  return (
    <div className="bg-white border border-slate-200 rounded-xl p-5">
      <h2 className="font-bold text-slate-900 mb-3">Powiązane WZ</h2>
      {transfers.length === 0 ? (
        <p className="text-sm text-slate-500">Nie utworzono jeszcze WZ dla tego zamówienia.</p>
      ) : transfers.map(transfer => (
        <div key={transfer.id} className="flex justify-between py-3 border-b border-slate-100 last:border-0">
          <Link to={`/transfers/${transfer.id}`} className="text-amber-700 font-semibold hover:underline">
            {transfer.number}
          </Link>
          <span className="text-sm text-slate-500">
            {transfer.status === 2 ? 'W transporcie' : transfer.status === 3
              ? 'Odbiór potwierdzony' : transfer.status === 4 ? 'Anulowany' : 'Szkic'}
          </span>
        </div>
      ))}
    </div>
  );
}
