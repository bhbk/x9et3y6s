import { TestBed } from '@angular/core/testing';
import { IdentityMeService } from 'src/app/common/services';

describe('IdentityMeService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: IdentityMeService = TestBed.get(IdentityMeService);
    expect(service).toBeTruthy();
  });
});
