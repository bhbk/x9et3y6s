import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { IdentityAdminClientComponent } from './identity-admin-client.component';

describe('IdentityAdminClientComponent', () => {
  let component: IdentityAdminClientComponent;
  let fixture: ComponentFixture<IdentityAdminClientComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ IdentityAdminClientComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(IdentityAdminClientComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
