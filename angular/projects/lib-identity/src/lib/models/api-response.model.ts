/**
 * Common API Response Models
 */

// Paginated response wrapper (matches .NET DataStateV1Result<T>)
export interface PagedResult<T> {
  data: T[];
  total: number;
}

// Error response
export interface ApiError {
  status: number;
  message: string;
  errors?: Record<string, string[]>;
}

// Data state query model (for filtering/sorting/paging)
export interface DataStateQuery {
  filter?: DataStateFilter;
  sort?: DataStateSort[];
  skip?: number;
  take?: number;
}

export interface DataStateFilter {
  logic: 'and' | 'or';
  filters: DataStateFilterItem[];
}

export interface DataStateFilterItem {
  field: string;
  operator: FilterOperator;
  value: unknown;
}

export type FilterOperator =
  | 'eq'        // equals
  | 'neq'       // not equals
  | 'lt'        // less than
  | 'lte'       // less than or equal
  | 'gt'        // greater than
  | 'gte'       // greater than or equal
  | 'contains'  // contains substring
  | 'startswith'
  | 'endswith'
  | 'isnull'
  | 'isnotnull';

export interface DataStateSort {
  field: string;
  dir: 'asc' | 'desc';
}
