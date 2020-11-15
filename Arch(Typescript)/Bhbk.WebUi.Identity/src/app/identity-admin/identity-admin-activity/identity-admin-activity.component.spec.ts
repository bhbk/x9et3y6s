import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { IdentityAdminActivityComponent } from './identity-admin-activity.component';

describe('IdentityAdminActivityComponent', () => {
  let component: IdentityAdminActivityComponent;
  let fixture: ComponentFixture<IdentityAdminActivityComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ IdentityAdminActivityComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(IdentityAdminActivityComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
