import { TestBed } from '@angular/core/testing';
import { IdentityAdminService } from 'src/app/common/services';

describe('IdentityAdminService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: IdentityAdminService = TestBed.get(IdentityAdminService);
    expect(service).toBeTruthy();
  });
});
