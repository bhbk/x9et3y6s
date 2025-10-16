import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ConfigService } from '../config.service';
import { DataStateQuery, PagedResult } from '../../models';

/**
 * Base service for Admin API CRUD operations
 */
export abstract class BaseAdminService<T, TCreate, TUpdate> {
  protected abstract readonly endpoint: string;

  constructor(
    protected readonly http: HttpClient,
    protected readonly config: ConfigService
  ) {}

  protected get baseUrl(): string {
    return `${this.config.adminApiUrl}${this.config.pathBase}`;
  }

  /**
   * Get all items with optional filtering/sorting/paging
   * .NET endpoints use POST {endpoint}/v1/page with [FromBody] DataStateV1
   */
  getAll(query?: DataStateQuery): Observable<PagedResult<T>> {
    return this.http.post<PagedResult<T>>(`${this.baseUrl}/${this.endpoint}/v1/page`, query ?? {});
  }

  /**
   * Get single item by ID
   */
  getById(id: string): Observable<T> {
    return this.http.get<T>(`${this.baseUrl}/${this.endpoint}/v1/${id}`);
  }

  /**
   * Create new item
   */
  create(item: TCreate): Observable<T> {
    return this.http.post<T>(`${this.baseUrl}/${this.endpoint}/v1`, item);
  }

  /**
   * Update existing item
   */
  update(item: TUpdate): Observable<T> {
    return this.http.put<T>(`${this.baseUrl}/${this.endpoint}/v1`, item);
  }

  /**
   * Delete item by ID
   */
  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${this.endpoint}/v1/${id}`);
  }
}
