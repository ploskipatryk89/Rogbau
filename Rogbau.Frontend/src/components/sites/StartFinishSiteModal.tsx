import type React from "react";
import { useState } from "react";
import { XMarkIcon } from "@heroicons/react/24/outline";
import { startSite, finishSite } from "../../api/siteService";
import type {
  StartSiteRequest,
  FinishSiteRequest,
} from "../../types/request/SiteRequest";

interface StartFinishSiteModalProps {
  siteId: string;
  mode: "start" | "finish";
  onClose: () => void;
  onSuccess: () => void;
}

export const StartFinishSiteModal: React.FC<StartFinishSiteModalProps> = ({
  siteId,
  mode,
  onClose,
  onSuccess,
}) => {
  const [date, setDate] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState("");

  const isStart = mode === "start";

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!date) return;

    try {
      setIsSubmitting(true);
      setError("");

      if (isStart) {
        const request: StartSiteRequest = {
          id: siteId,
          startDate: date,
        };

        await startSite(request);
      } else {
        const request: FinishSiteRequest = {
          id: siteId,
          endDate: date,
        };

        await finishSite(request);
      }

      await onSuccess();
      onClose();
    } catch (error) {
      setError(
        error instanceof Error
          ? error.message
          : "Wystąpił błąd podczas zapisywania."
      );
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
      <div className="w-full max-w-md rounded-xl border border-slate-200 bg-white shadow-xl">
        
        {/* Nagłówek */}
        <div className="flex items-center justify-between border-b border-slate-200 px-6 py-4">
          <div>
            <h2 className="text-lg font-semibold text-slate-900">
              {isStart ? "Rozpocznij budowę" : "Zakończ budowę"}
            </h2>

            <p className="mt-1 text-sm text-slate-500">
              {isStart
                ? "Podaj datę rozpoczęcia budowy."
                : "Podaj datę zakończenia budowy."}
            </p>
          </div>

          <button
            type="button"
            onClick={onClose}
            className="rounded-lg p-2 text-slate-400 transition hover:bg-slate-100 hover:text-slate-600"
          >
            <XMarkIcon className="h-5 w-5" />
          </button>
        </div>

        <form onSubmit={handleSubmit}>
          
          {/* Zawartość */}
          <div className="px-6 py-5">
            <label className="mb-2 block text-sm font-medium text-slate-700">
              {isStart ? "Data rozpoczęcia" : "Data zakończenia"}
            </label>

            <input
              type="date"
              value={date}
              onChange={(e) => setDate(e.target.value)}
              className="w-full rounded-lg border border-slate-300 px-3 py-2.5 text-sm text-slate-900 outline-none transition focus:border-amber-500 focus:ring-2 focus:ring-amber-500/20"
              required
            />

            {error && (
              <p className="mt-3 text-sm text-red-600">
                {error}
              </p>
            )}
          </div>

          {/* Przyciski */}
          <div className="flex justify-end gap-3 border-t border-slate-200 bg-slate-50 px-6 py-4">
            <button
              type="button"
              onClick={onClose}
              disabled={isSubmitting}
              className="rounded-lg border border-slate-300 bg-white px-4 py-2 text-sm font-medium text-slate-700 transition hover:bg-slate-50 disabled:opacity-50"
            >
              Anuluj
            </button>

            <button
              type="submit"
              disabled={!date || isSubmitting}
              className="rounded-lg bg-amber-500 px-4 py-2 text-sm font-semibold text-white transition hover:bg-amber-600 disabled:cursor-not-allowed disabled:opacity-50"
            >
              {isSubmitting
                ? "Zapisywanie..."
                : isStart
                  ? "Rozpocznij budowę"
                  : "Zakończ budowę"}
            </button>
          </div>

        </form>
      </div>
    </div>
  );
};