import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { IdentityAdminClaimComponent } from './identity-admin-claim.component';

describe('IdentityAdminClaimComponent', () => {
  let component: IdentityAdminClaimComponent;
  let fixture: ComponentFixture<IdentityAdminClaimComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ IdentityAdminClaimComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(IdentityAdminClaimComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
