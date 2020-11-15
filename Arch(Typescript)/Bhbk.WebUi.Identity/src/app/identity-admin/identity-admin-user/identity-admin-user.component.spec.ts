import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { IdentityAdminUserComponent } from './identity-admin-user.component';

describe('IdentityAdminUserComponent', () => {
  let component: IdentityAdminUserComponent;
  let fixture: ComponentFixture<IdentityAdminUserComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ IdentityAdminUserComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(IdentityAdminUserComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
