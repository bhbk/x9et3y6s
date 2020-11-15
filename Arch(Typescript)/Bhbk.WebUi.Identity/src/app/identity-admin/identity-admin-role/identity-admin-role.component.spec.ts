import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { IdentityAdminRoleComponent } from './identity-admin-role.component';

describe('IdentityAdminRoleComponent', () => {
  let component: IdentityAdminRoleComponent;
  let fixture: ComponentFixture<IdentityAdminRoleComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ IdentityAdminRoleComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(IdentityAdminRoleComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
