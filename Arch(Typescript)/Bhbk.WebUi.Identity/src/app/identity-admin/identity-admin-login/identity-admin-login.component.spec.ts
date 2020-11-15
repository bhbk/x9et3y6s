import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { IdentityAdminLoginComponent } from './identity-admin-login.component';

describe('IdentityAdminLoginComponent', () => {
  let component: IdentityAdminLoginComponent;
  let fixture: ComponentFixture<IdentityAdminLoginComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ IdentityAdminLoginComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(IdentityAdminLoginComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
