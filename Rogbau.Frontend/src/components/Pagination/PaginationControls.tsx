import { PageSizeSelector } from "./PageSizeSelector";
import type { PageSizeSelectorProps } from "./PageSizeSelector";
import { PageNavigation } from "./PageNavigation";
import type { PageNavigationProps } from "./PageNavigation";

type PaginationControlsProps =
  PageSizeSelectorProps & PageNavigationProps;

export function PaginationControls(props: PaginationControlsProps) {
  return (
    <div className="flex flex-wrap items-center justify-between gap-4 py-4">
      <PageSizeSelector
        pageSize={props.pageSize}
        totalCount={props.totalCount}
        onPageSizeChange={props.onPageSizeChange}
        isLoading={props.isLoading}
      />

      <PageNavigation
        pageNumber={props.pageNumber}
        totalPages={props.totalPages}
        hasPreviousPage={props.hasPreviousPage}
        hasNextPage={props.hasNextPage}
        onPageChange={props.onPageChange}
        isLoading={props.isLoading}
      />
    </div>
  );
}