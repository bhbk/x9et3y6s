import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { IdentityAdminIssuerComponent } from './identity-admin-issuer.component';

describe('IdentityAdminIssuerComponent', () => {
  let component: IdentityAdminIssuerComponent;
  let fixture: ComponentFixture<IdentityAdminIssuerComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ IdentityAdminIssuerComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(IdentityAdminIssuerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
