import { ArrowPathIcon } from "@heroicons/react/24/outline";
import {PlusIcon} from "@heroicons/react/24/outline";
interface PageHeaderProps{
    title: string;
    subTile: string;
    onClickFetchButton: () => void;
    onclickActionButton: () => void;
    actionButtonText: string;
    loadingItems: boolean;
    actionButtonVisible: boolean;
}

export const PageHeader: React.FC<PageHeaderProps> = ({title, subTile, onClickFetchButton, onclickActionButton, actionButtonText, loadingItems, actionButtonVisible}) => {
    return(
        <div>
             <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
            
          <div className="flex flex-row">

            <h1 className="text-2xl font-bold text-slate-900">{title}</h1>
        </div>
          <p className="text-sm text-slate-500">{subTile}</p>
        </div>
        <div className="flex items-center gap-3">
          <button
            type="button"
            onClick={onClickFetchButton}
            className="p-2.5 text-slate-600 bg-white border border-slate-200 rounded-xl hover:bg-slate-50 transition-colors shadow-sm cursor-pointer"
            title="Odśwież"
          >
            <ArrowPathIcon className={`w-5 h-5 ${loadingItems ? 'animate-spin' : ''}`} />
          </button>
          
          {actionButtonVisible &&

          
           <button 
            type="button"
            onClick={onclickActionButton}
            className="flex items-center gap-2 bg-amber-500 hover:bg-amber-600 text-slate-950 px-4 py-2.5 rounded-xl font-bold text-sm shadow-sm transition-all active:scale-95 cursor-pointer"
          >
            <PlusIcon className="w-5 h-5 stroke-[2.5]" />
            {actionButtonText}
          </button>
          }
         
        </div>

        </div>
        </div>
    )}
