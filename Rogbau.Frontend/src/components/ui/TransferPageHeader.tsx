import { ArrowPathIcon, PlusIcon } from "@heroicons/react/24/outline";

interface TransferPageHeaderProps {
  title: string;
  subTitle: string;

  onClickFetchButton: () => void;
  onClickWzButton: () => void;
  onClickPzButton: () => void;
  onClickTransferButton: () => void;

  loadingItems: boolean;
  actionButtonsVisible: boolean;
  warehouseButtonsVisible: boolean;
  orderButtonVisible: boolean;
  onClickOrderButton: () => void;
}

export const TransferPageHeader: React.FC<TransferPageHeaderProps> = ({
  title,
  subTitle,
  onClickFetchButton,
  onClickWzButton,
  onClickPzButton,
  onClickTransferButton,
  loadingItems,
  actionButtonsVisible,
  warehouseButtonsVisible,
  orderButtonVisible,
  onClickOrderButton,
}) => {
  return (
    <div>
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <div className="flex flex-row">
            <h1 className="text-2xl font-bold text-slate-900">
              {title}
            </h1>
          </div>

          <p className="text-sm text-slate-500">
            {subTitle}
          </p>
        </div>

        <div className="flex flex-wrap items-center gap-3">
          {/* Odśwież */}
          <button
            type="button"
            onClick={onClickFetchButton}
            className="p-2.5 text-slate-600 bg-white border border-slate-200 rounded-xl hover:bg-slate-50 transition-colors shadow-sm cursor-pointer"
            title="Odśwież"
          >
            <ArrowPathIcon
              className={`w-5 h-5 ${
                loadingItems ? "animate-spin" : ""
              }`}
            />
          </button>

          {actionButtonsVisible && (
            <>
              {/* WZ */}
              {warehouseButtonsVisible && (
                <>
              <button
                type="button"
                onClick={onClickWzButton}
                className="flex items-center gap-2 bg-amber-500 hover:bg-amber-600 text-slate-950 px-4 py-2.5 rounded-xl font-bold text-sm shadow-sm transition-all active:scale-95 cursor-pointer"
              >
                <PlusIcon className="w-5 h-5 stroke-[2.5]" />
                WZ
              </button>
                </>
              )}
              {/* PZ */}
              {orderButtonVisible && <button
                type="button"
                onClick={onClickPzButton}
                className="flex items-center gap-2 bg-amber-500 hover:bg-amber-600 text-slate-950 px-4 py-2.5 rounded-xl font-bold text-sm shadow-sm transition-all active:scale-95 cursor-pointer"
              >
                <PlusIcon className="w-5 h-5 stroke-[2.5]" />
                Zgłoś zwrot (PZ)
              </button>}
              {/* Przesunięcie */}
              <button
                type="button"
                onClick={onClickTransferButton}
                className="flex items-center gap-2 bg-amber-500 hover:bg-amber-600 text-slate-950 px-4 py-2.5 rounded-xl font-bold text-sm shadow-sm transition-all active:scale-95 cursor-pointer"
              >
                <PlusIcon className="w-5 h-5 stroke-[2.5]" />
                Przesunięcie
              </button>
              {orderButtonVisible && <button type="button" onClick={onClickOrderButton}
                className="flex items-center gap-2 bg-amber-500 hover:bg-amber-600 text-slate-950 px-4 py-2.5 rounded-xl font-bold text-sm shadow-sm cursor-pointer">
                <PlusIcon className="w-5 h-5" /> Zamów sprzęt
              </button>}
            </>
          )}
        </div>
      </div>
    </div>
  );
};
