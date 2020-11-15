import { TestBed } from '@angular/core/testing';
import { IdentityStsService } from 'src/app/common/services';

describe('IdentityStsService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: IdentityStsService = TestBed.get(IdentityStsService);
    expect(service).toBeTruthy();
  });
});
