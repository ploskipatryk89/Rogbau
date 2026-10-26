import { createPortal } from 'react-dom';
import type { TransferDetailsDto } from '../../types/response/TransferResponseDto';

interface TransferPrintDocumentProps {
  transfer: TransferDetailsDto;
}

export const TransferPrintDocument = ({ transfer }: TransferPrintDocumentProps) => {
  const isReturn = transfer.type === 2;
  const isCompleted = transfer.status === 3;
  const typeName = isReturn ? 'PZ — zwrot do magazynu' : transfer.type === 1 ? 'WZ — wydanie sprzętu' : 'Przesunięcie sprzętu';
  const statusName = ({ 1: 'Szkic', 2: 'W transporcie', 3: 'Zakończony', 4: 'Anulowany' } as Record<number, string>)[transfer.status] ?? 'Nieznany';
  const cellClass = 'border border-black px-2 py-2 align-top';

  return createPortal(
    <article className="transfer-print-document hidden print:block bg-white text-black text-sm leading-normal">
      <header className="mb-6 border-b-2 border-black pb-4">
        <p className="text-lg font-bold">ROGBAU</p>
        <h1 className="mt-2 text-2xl font-bold">{transfer.number}</h1>
        <p className="mt-1">{typeName}</p>
        <p className="mt-1">Status: {statusName}</p>
      </header>

      <dl className="mb-6 grid grid-cols-2 gap-x-8 gap-y-3 break-inside-avoid">
        <div><dt className="font-semibold">Z:</dt><dd>{transfer.fromName || 'Magazyn'}</dd></div>
        <div><dt className="font-semibold">Do:</dt><dd>{transfer.toName || 'Magazyn'}</dd></div>
        <div><dt className="font-semibold">Data utworzenia:</dt><dd>{new Date(transfer.date).toLocaleDateString('pl-PL')}</dd></div>
        <div><dt className="font-semibold">Waga transportu:</dt><dd>{transfer.totalWeight ?? 0} kg</dd></div>
      </dl>

      <table className="w-full table-fixed border-collapse text-xs">
        <thead className="table-header-group">
          <tr>
            <th className={`${cellClass} w-8`}>Lp.</th>
            <th className={`${cellClass} text-left`}>Sprzęt</th>
            <th className={`${cellClass} w-12`}>J.m.</th>
            <th className={`${cellClass} w-20`}>{isReturn ? 'Zgłoszono' : 'Wysłano'}</th>
            {isReturn && <th className={`${cellClass} w-20`}>Zgłoszony złom / uszkodzone</th>}
            {(isReturn || !isCompleted) && <th className={`${cellClass} w-20`}>{isReturn ? 'Przyjęto' : 'Odebrano'}</th>}
            {isReturn && <th className={`${cellClass} w-20`}>W tym przyjęty złom / uszkodzone</th>}
          </tr>
        </thead>
        <tbody>
          {transfer.items.map((item, index) => (
            <tr key={item.equipmentId} className="break-inside-avoid">
              <td className={`${cellClass} text-center`}>{index + 1}</td>
              <td className={`${cellClass} wrap-break-word`}>{item.equipmentName}</td>
              <td className={`${cellClass} text-center`}>{item.unit}</td>
              <td className={`${cellClass} text-right`}>{item.quantity}</td>
              {isReturn && <td className={`${cellClass} text-right`}>{item.scrappedQuantity}</td>}
              {(isReturn || !isCompleted) && <td className={`${cellClass} text-right`}>{isCompleted && isReturn ? item.receivedQuantity ?? '—' : '\u00a0'}</td>}
              {isReturn && <td className={`${cellClass} text-right`}>{isCompleted ? item.receivedScrappedQuantity ?? '—' : '\u00a0'}</td>}
            </tr>
          ))}
        </tbody>
      </table>
      {transfer.items.length === 0 && <p className="mt-3">Dokument nie zawiera pozycji.</p>}
      {!isCompleted && <p className="mt-3 text-xs">Ilości faktycznie odebrane i rozbieżności należy wpisać przy odbiorze, a następnie potwierdzić w aplikacji.</p>}

      <section className="mt-6">
        <h2 className="font-semibold">Uwagi / rozbieżności:</h2>
        {transfer.note && <p className="mt-2 whitespace-pre-wrap wrap-break-word">{transfer.note}</p>}
        <div className="mt-4 h-8 border-b border-black" />
        <div className="h-8 border-b border-black" />
      </section>
      <footer className="mt-12 grid grid-cols-2 gap-12 break-inside-avoid text-center text-xs">
        <div className="border-t border-black pt-2">Data i podpis wydającego</div>
        <div className="border-t border-black pt-2">Data i podpis odbierającego</div>
      </footer>
    </article>,
    document.body,
  );
};
