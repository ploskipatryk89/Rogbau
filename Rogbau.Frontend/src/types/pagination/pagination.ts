export type PaginationList<T> = {
items: T[];
pageNumber: number;
pageSize: number;
totalCount: number;
totalPages: number;
hasPreviousPage: boolean;
hasNextPage: boolean;
}

export type PaginatedRequest = {
    pageNumber: number;
    pageSize: number;
}