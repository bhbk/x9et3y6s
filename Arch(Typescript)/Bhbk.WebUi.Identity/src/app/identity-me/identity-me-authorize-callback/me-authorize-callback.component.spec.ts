import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { IdentityMeAuthorizeCallbackComponent } from './me-authorize-callback.component';

describe('IdentityMeAuthorizeCallbackComponent', () => {
  let component: IdentityMeAuthorizeCallbackComponent;
  let fixture: ComponentFixture<IdentityMeAuthorizeCallbackComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ IdentityMeAuthorizeCallbackComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(IdentityMeAuthorizeCallbackComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
